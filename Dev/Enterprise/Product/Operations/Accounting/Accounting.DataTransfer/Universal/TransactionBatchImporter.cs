using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public class TransactionBatchImporter
	{
		public TransactionBatchImporter(UniversalTransactionBatch universalTransactionBatch, IXmlSessionTracker logger, UniversalObjectFactory universalObjectFactory)
		{
			UniversalTransactionBatch = universalTransactionBatch;
			Logger = logger;
			UniversalObjectFactory = universalObjectFactory;
			TxnHeaderNotification = new ImportLoggerNotificationManager(logger);
		}

		readonly UniversalTransactionBatch UniversalTransactionBatch;
		readonly IXmlSessionTracker Logger;
		readonly UniversalObjectFactory UniversalObjectFactory;
		readonly ImportLoggerNotificationManager TxnHeaderNotification;

		public bool ImportTransactionBatch()
		{
			var result = false;

			var txnHeaderCollection = MapImport();

			VaildateTxnHeaderCollection(txnHeaderCollection);

			if (!Logger.HasErrors() && txnHeaderCollection.Count > 0)
			{
				result = PerformTransactionBatchImport(txnHeaderCollection);
			}

			return result;
		}

		#region Vaildate TxnHeader Collection

		void VaildateTxnHeaderCollection(TxnHeaderCollection txnHeaderCollection)
		{
			var thirdPartyReferenceTxnHeaders = txnHeaderCollection.Cast<TxnHeader>().Where(x => !x.ThirdPartyReference.IsEmpty);
			if (thirdPartyReferenceTxnHeaders.Any())
			{
				var thirdPartyReferenceList = new List<ThirdPartyReferenceForGroup>();

				foreach (var txnHeader in thirdPartyReferenceTxnHeaders)
				{
					thirdPartyReferenceList.Add(new ThirdPartyReferenceForGroup(UniversalObjectFactory.BOFactory, txnHeader));
				}

				var thirdPartyReferenceGroups = thirdPartyReferenceList.GroupBy(x => new { x.ThirdPartyReference, x.Ledger, x.TransactionType, x.OrgCode, x.CompanyPK });
				var groupsWithDuplicateOTI = thirdPartyReferenceGroups.Where(x => x.Count() > 1);
				if (groupsWithDuplicateOTI.Any())
				{
					var duplicateThirdPartyReferences = string.Join(", ", groupsWithDuplicateOTI.Select(y => y.Key.ThirdPartyReference).Distinct());
					TxnHeaderNotification.AddErrorToNotifications(Res.GetString("3cb68213-43d8-4a20-abcd-6536565cb39d", "Transaction ID {0} is already in use by another transaction in the XML file with the same organization, ledger and transaction type.", duplicateThirdPartyReferences));
				}
			}

			var checkNumberGroups = txnHeaderCollection.Cast<TxnHeader>().Where(x => x.TxnType == Xsd.TxnType.PAY &&
																					x.ReceiptPaymentType == TxnHeaderReceiptPaymentType.CHQ &&
																					!x.ChequeBook.IsEmpty &&
																					!x.ChequeOrReference.IsEmpty).GroupBy(y => new { y.ChequeBook, y.ChequeOrReference });
			var groupsWithDuplicateCheckNumber = checkNumberGroups.Where(x => x.Count() > 1);
			if (groupsWithDuplicateCheckNumber.Any())
			{
				var duplicateCheckNumbers = string.Join(", ", groupsWithDuplicateCheckNumber.Select(y => y.Key.ChequeOrReference).Distinct());
				TxnHeaderNotification.AddErrorToNotifications(Res.GetString("ccc1a561-bbd7-4194-9dd7-6306eadadd35", "Cheque number {0} is already in use by another transaction in the XML file.", duplicateCheckNumbers));
			}
		}

		class ThirdPartyReferenceForGroup
		{
			public ThirdPartyReferenceForGroup(BusinessObjectFactory factory, TxnHeader txnHeader)
			{
				ThirdPartyReference = txnHeader.ThirdPartyReference;
				Ledger = txnHeader.Ledger;
				TransactionType = txnHeader.TxnType;
				OrgCode = txnHeader.DebtorOrCreditor.EDICode;
				CompanyPK = GetComanyPKFromBranchCode(factory, txnHeader.Branch);
			}

			ZGuid GetComanyPKFromBranchCode(BusinessObjectFactory factory, string branchCode)
			{
				var result = ZGuid.Empty;
				var branch = BusinessObjectRetriever.GetBranchFromBranchCode(factory, branchCode);
				if (branch != null)
				{
					result = branch.GB_GC;
				}

				return result;
			}

			public ZString ThirdPartyReference { get; private set; }
			public TxnLedgerType Ledger { get; private set; }
			public TxnType TransactionType { get; private set; }
			public ZString OrgCode { get; private set; }
			public ZGuid CompanyPK { get; private set; }
		}

		#endregion

		#region Map

		TxnHeaderCollection MapImport()
		{
			var txnHeaderCollection = new TxnHeaderCollection();

			var toMapTransactionCollection = UniversalTransactionBatch.TransactionCollection.Where(x => AccountingConstants.PaymentReceiptXUBTypes.GetTypes().Contains(x.DataContext?.DataTargetCollection?.FirstOrDefault()?.Type.GetValueOrDefault() ?? ZString.Empty));

			foreach (var receiptPayment in toMapTransactionCollection)
			{
				if (TxnHeaderNotification.ErrorsHaveBeenReported)
				{
					break;
				}

				var txnHeader = txnHeaderCollection.AddNew();
				var matchLines = receiptPayment.MatchLineCollection;

				if (receiptPayment.DataContext.DataTargetCollection.FirstOrDefault().Type.GetValueOrDefault() == AccountingConstants.PaymentReceiptXUBTypes.AccountingMatching)
				{
					if (matchLines != null && matchLines.Count > 1)
					{
						var fullyPaidDate = TxnHeaderBuilder.GetZDateTimeFromField(receiptPayment.PostDate.GetValueOrDefault(), TxnHeaderNotification);
						ProcessPaidTransactions(matchLines, txnHeader, fullyPaidDate, UniversalTransactionBatch.TransactionCollection);
					}
					else
					{
						TxnHeaderNotification.AddErrorToNotifications(Res.GetString("6f14cf64-c0c0-430d-a7c6-f1031b72b0a6", "The Accounting Matching type must be accompanied by at least two match lines."));
					}
				}
				else
				{
					var orgAddress = TxnHeaderBuilder.GetOrgAddress(UniversalObjectFactory, receiptPayment.OrganizationAddress, Logger);
					var paymentReceiptHeader = new PaymentReceiptUniversalBatchHeader(receiptPayment, orgAddress);
					TxnHeaderBuilder.BuildReceiptPaymentHeader(txnHeader, paymentReceiptHeader, UniversalObjectFactory.BOFactory, TxnHeaderNotification);

					if (matchLines != null)
					{
						ProcessPaidTransactions(matchLines, txnHeader, ZDateTime.Empty, UniversalTransactionBatch.TransactionCollection);
					}
				}
			}

			return txnHeaderCollection;
		}

		void ProcessPaidTransactions(IEnumerable<MatchLine> matchLines, TxnHeader txnHeader, ZDateTime fullyPaidDate, IEnumerable<TransactionInfo> transactionCollection = null)
		{
			ValidateSingleMatchGroup(matchLines);

			if (TxnHeaderNotification.ErrorsHaveBeenReported)
			{
				return;
			}

			foreach (var matchLine in matchLines)
			{
				var linkedTransactionIDCollection = matchLine.LinkedTransactionIDCollection;
				ZString[] splitedKey = null;
				ValidateMatchLine(linkedTransactionIDCollection, out splitedKey);

				if (TxnHeaderNotification.ErrorsHaveBeenReported)
				{
					break;
				}
				else
				{
					var transactionType = splitedKey[1];

					if (TxnHeaderBuilder.IsSupportedPaidTransactionType(transactionType, TxnHeaderNotification))
					{
						TxnHeader paidTxnHeader;
						var isFirstMatchOnlyLine = !fullyPaidDate.IsEmpty;
						if (isFirstMatchOnlyLine)
						{
							paidTxnHeader = txnHeader;
							paidTxnHeader.FullyPaidDate = fullyPaidDate;
							fullyPaidDate = ZDateTime.Empty;
						}
						else
						{
							paidTxnHeader = txnHeader.PaidTransactions.AddNew();
						}

						TransactionInfo linkedTransaction = null;
						if (transactionCollection != null)
						{
							var linkedTransactionKey = linkedTransactionIDCollection.First().Key.Value;
							linkedTransaction = transactionCollection.FirstOrDefault(x => x.DataContext != null && x.DataContext.DataTargetCollection != null && x.DataContext.DataTargetCollection.First().Key.HasValue && x.DataContext.DataTargetCollection.First().Key.Value == linkedTransactionKey);

							if (linkedTransaction == null && miscTransactionType.Contains(transactionType))
							{
								if (transactionType == ZArchitecture.Core.TransactionTypes.Journal)
								{
									paidTxnHeader.ShouldCreateDuringMatching = false;
								}
								else
								{
									TxnHeaderNotification.AddErrorToNotifications(Res.GetString("0CD2501C-1165-4835-BE0D-D215C1D4F079", "Each Miscellaneous Match Transaction in <MatchLine> must be supported by <Transaction> data with the same <LinkedTransactionID><Key>."));
									break;
								}
							}
							else if (linkedTransaction != null)
							{
								if (miscTransactionType.Contains(transactionType) && linkedTransaction.OSTotal != matchLine.OSPaidAmount)
								{
									if (transactionType == ZArchitecture.Core.TransactionTypes.Journal)
									{
										paidTxnHeader.ShouldCreateDuringMatching = false;
									}
									else
									{
										TxnHeaderNotification.AddErrorToNotifications(Res.GetString("1E919FAC-135A-4A82-8BDC-9FA319DCC434", "Each Miscellaneous Match Transaction in <MatchLine> supported by <Transaction> with the same <LinkedTransactionID><Key>, <Transaction><OSTotal> must be same as <MatchLine><OSPaidAmount>."));
										break;
									}
								}
								paidTxnHeader.Branch = linkedTransaction.Branch?.Code ?? string.Empty;
								paidTxnHeader.Department = linkedTransaction.Department?.Code ?? string.Empty;
							}
						}

						var orgAddress = TxnHeaderBuilder.GetOrgAddress(UniversalObjectFactory, matchLine.OrganizationAddress, Logger);
						var universalMatchLine = new PaymentReceiptUniversalMatchLine(matchLine, linkedTransaction, orgAddress, splitedKey);

						TxnHeaderBuilder.BuidPaidTransaction(paidTxnHeader, universalMatchLine, UniversalObjectFactory.BOFactory, TxnHeaderNotification);
						paidTxnHeader.TxnCategory = universalMatchLine.TransactionCategory;
					}
				}
			}
		}

		void ValidateMatchLine(IEnumerable<LinkedTransactionID> linkedTransactionIDCollection, out ZString[] splitedKey)
		{
			splitedKey = null;
			if (linkedTransactionIDCollection == null || !linkedTransactionIDCollection.Any())
			{
				TxnHeaderNotification.AddErrorToNotifications(Res.GetString("082ac3b7-529a-4689-9977-5b15e3823280", "Match line should have one <LinkedTransactionID> for each <LinkedTransactionIDCollection>."));
				return;
			}

			var firstLinkedTransactionIDKey = linkedTransactionIDCollection.First().Key;
			if (!firstLinkedTransactionIDKey.HasValue)
			{
				TxnHeaderNotification.AddErrorToNotifications(Res.GetString("62284fec-08d2-495c-95d6-fc0fdf4c954f", "The <Key> of <LinkedTransactionID> should have value."));
				return;
			}

			splitedKey = firstLinkedTransactionIDKey.Value.Split(new char[] { ' ' }, 3);
			if (splitedKey.Length != 3)
			{
				TxnHeaderNotification.AddErrorToNotifications(Res.GetString("e79cdb88-2154-4020-b433-10ecec5eade9", "The <Key> of <LinkedTransactionID> should consist of ledger, transaction type and transaction number separated by one space. E.g. AP INV 12345."));
				return;
			}
		}

		void ValidateSingleMatchGroup(IEnumerable<MatchLine> matchLines)
		{
			var matchGroupNumbers = matchLines.Select(x => x.MatchGroupNumber ?? string.Empty).Distinct();

			if (matchGroupNumbers.Count() > 1)
			{
				TxnHeaderNotification.AddErrorToNotifications(Res.GetString("6EE89133-C6B0-4891-B5B4-063C05E86A29", "Only one match group is allowed in <MatchLineCollection>."));
				return;
			}
		}

		List<string> miscTransactionType => new List<string> {
			ZArchitecture.Core.TransactionTypes.Journal,
			ZArchitecture.Core.TransactionTypes.ExchangeDifference,
			ZArchitecture.Core.TransactionTypes.Discount,
			ZArchitecture.Core.TransactionTypes.Overpayment
		};

		#endregion

		#region PerformTransactionBatchImport

		bool PerformTransactionBatchImport(TxnHeaderCollection txnHeaderCollection)
		{
			var result = false;
			var txnHeaderProcessor = new PaymentReceiptUniversalBatchProcessor(UniversalObjectFactory.BOFactory, TxnHeaderNotification);
			txnHeaderProcessor.ProcessWithSaveExceptionHandling += ProcessWithSaveExceptionHandling;
			txnHeaderProcessor.ProcessTxnHeaderCollection(txnHeaderCollection);

			if (!TxnHeaderNotification.ErrorsHaveBeenReported && !((NotificationBuffer)TxnHeaderNotification.NotificationSubscriber).HasErrors)
			{
				result = true;
			}

			return result;
		}

		void ProcessWithSaveExceptionHandling(Action action, INotifications notifications)
		{
			action();
		}

		#endregion
	}
}

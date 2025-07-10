using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class ARLStatementOfAccountDocumentWrapper : NonPersistentBusinessObject, IDocumentWrapper, ISourceIdentifierProvider
	{
		internal ARLStatementOfAccountDocumentWrapper(Enterprise.Messaging.Business.EDIMessage message)
			: this(message.GetEM_MessageTextReader().Parse<TransactionBatch>(), message.Factory)
		{
			this.message = message;
		}

		internal ARLStatementOfAccountDocumentWrapper(TransactionBatch transactionBatch, BusinessObjectFactory factory) : base(factory)
		{
			Argument.NotNull(transactionBatch, "transactionBatch");
			if (transactionBatch.BatchType == null)
			{
				throw new ArgumentNullException(nameof(transactionBatch), "Batch type is null in SOA message.");
			}
			else if (!transactionBatch.BatchType.Code.HasValue || transactionBatch.BatchType.Code.Value != ARLMessageTypes.Codes.StatementOfAccount)
			{
				throw new ArgumentException("ARLStatementOfAccountDocumentWrapper is for SoA message only but was " + transactionBatch.BatchType.Code ?? string.Empty);
			}
			this.transactionBatch = transactionBatch;
		}

		readonly Enterprise.Messaging.Business.EDIMessage message;
		readonly TransactionBatch transactionBatch;

		#region Universal Transaction Batch

		TransactionInfo HeaderTransactionInfo
		{
			get { return fHeaderTransactionInfo ?? (fHeaderTransactionInfo = GetHeaderTransactionInfo()); }
		}
		TransactionInfo fHeaderTransactionInfo;

		TransactionInfo GetHeaderTransactionInfo()
			=> transactionBatch.GetTransactionCollectionByCategory(TransactionBatchConstants.TransactionCategoryCodes.Summary).FirstOrDefault()
				?? throw new InvalidMessageContentException("Cannot find XML node TransactionBatch/TransactionCollection/Transaction[Category='SUM']");

		List<TransactionInfo> ImporterSummaryTransactionCollection
		{
			get
			{
				return fImporterSummaryTransactionCollection ?? (fImporterSummaryTransactionCollection = GetImporterSummaryTransactionCollection());
			}
		}
		List<TransactionInfo> fImporterSummaryTransactionCollection;

		List<TransactionInfo> GetImporterSummaryTransactionCollection()
		{
			var transactions = transactionBatch.GetTransactionCollectionByCategory(TransactionBatchConstants.TransactionCategoryCodes.Summary).ToList();
			if (transactions.Count == 0)
			{
				throw new InvalidMessageContentException("Cannot find XML node TransactionBatch/TransactionCollection/Transaction[Category='SUM']");
			}
			transactions.RemoveAt(0);

			return transactions;
		}

		#endregion

		#region Properties

		public ZInt? NumberOfSupportingDocuments
		{
			get { return HeaderTransactionInfo.NumberOfSupportingDocuments; }
		}

		public ZString NumberOfSupportingDocumentsString
		{
			get { return NumberOfSupportingDocuments.HasValue ? NumberOfSupportingDocuments.Value.ToString() : string.Empty; }
		}

		public ZDate StatementDate
		{
			get { return HeaderTransactionInfo.CreateTime.GetValueOrDefault().Date; }
		}

		public ZDate DueDate
		{
			get { return HeaderTransactionInfo.DueDate.GetValueOrDefault().Date; }
		}

		public ZString ImporterBusinessNumber
		{
			get { return HeaderTransactionInfo.OrganizationAddress.GovRegNum.GetValueOrDefault(); }
		}

		public ZString ImporterLegalName
		{
			get { return HeaderTransactionInfo.OrganizationAddress.CompanyName.GetValueOrDefault(); }
		}

		public ZString AccountSecurityCode
		{
			get { return fAccountSecurityCode ?? (fAccountSecurityCode = GetAccountSecurityCode()); }
			set { fAccountSecurityCode = value; }
		}
		string fAccountSecurityCode;

		public ZString GetAccountSecurityCode(IXmlSessionTracker logger = null)
		{
			fAccountSecurityCode = HeaderTransactionInfo.GetAccountSecurityCode(ImporterBusinessNumber, RMAccountNumber, Factory, logger);
			return fAccountSecurityCode;
		}

		public ZDate BillingDateFrom
		{
			get { return transactionBatch.DateFrom.GetValueOrDefault().Date; }
		}

		public ZDate BillingDateTo
		{
			get { return transactionBatch.DateTo.GetValueOrDefault().Date; }
		}

		public ZDecimal GrandTotal
		{
			get { return HeaderTransactionInfo.LocalTotal.GetValueOrDefault(); }
		}

		public ZString StatementType
		{
			get { return HeaderTransactionInfo.ComplianceSubType.GetValueOrDefault(); }
		}

		public ZString StatementTypeDescription
		{
			get
			{
				ZString result = new CusStatementHeaderTypes().GetDescriptionFromCode(StatementType);
				return result.IsEmpty ? StatementType : result;
			}
		}

		public ZString RMAccountNumber
		{
			get { return HeaderTransactionInfo.Number.GetValueOrDefault(); }
		}

		public ZString DataTargetKey
		{
			get
			{
				var result = ZString.Empty;
				if (transactionBatch.DataContext?.DataTargetCollection != null)
				{
					var dataTarget = transactionBatch.DataContext.DataTargetCollection.FirstOrDefault();
					result = dataTarget != null ? dataTarget.Key.GetValueOrDefault() : ZString.Empty;
				}

				return result;
			}
		}

		public ZString MessageEN
		{
			get { return HeaderTransactionInfo.GetAddInfo(TransactionBatchConstants.TransactionAddInfoKeys.MessageEN); }
		}

		public ZString MessageFR
		{
			get { return HeaderTransactionInfo.GetAddInfo(TransactionBatchConstants.TransactionAddInfoKeys.MessageFR); }
		}

		#endregion

		#region Total Properties

		public ZDecimal TotalCustomsDuties
		{
			get { return DailySummaryTotals.Sum(x => x.CustomsDuties); }
		}

		public ZDecimal TotalSIMA
		{
			get { return DailySummaryTotals.Sum(x => x.SIMA); }
		}

		public ZDecimal TotalExciseTax
		{
			get { return DailySummaryTotals.Sum(x => x.ExciseTax); }
		}

		public ZDecimal TotalGST
		{
			get { return DailySummaryTotals.Sum(x => x.GST); }
		}

		public ZDecimal TotalOthers
		{
			get { return DailySummaryTotals.Sum(x => x.Others); }
		}

		public ZDecimal TotalTotal
		{
			get { return DailySummaryTotals.Sum(x => x.Total); }
		}

		#endregion

		#region ImporterGrandTotal

		public ITableInterpretation GetImporterGrandTotal()
		{
			return new ImportersGrandTotal(this);
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		internal class ImportersGrandTotal : NonPersistentBusinessObject, ITableInterpretation
		{
			internal ImportersGrandTotal(ARLStatementOfAccountDocumentWrapper parent) : base(parent.Factory)
			{
				Argument.NotNull(parent, "parent");
				this.parent = parent;
			}

			readonly ARLStatementOfAccountDocumentWrapper parent;

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return Res.GetString("251ff556-4033-452f-836a-a838538de790", "Report Grand Total"); }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get { return PropertyNameProvider.GetColumnTitles<DailySummaryTotal.TransactionDetails>().Skip(1).Take(6); }
			}

			IEnumerable<object> ITableValues.Values
			{
				get
				{
					yield return InterpretationHelper.FormatAmount(parent.TotalCustomsDuties);
					yield return InterpretationHelper.FormatAmount(parent.TotalSIMA);
					yield return InterpretationHelper.FormatAmount(parent.TotalExciseTax);
					yield return InterpretationHelper.FormatAmount(parent.TotalGST);
					yield return InterpretationHelper.FormatAmount(parent.TotalOthers);
					yield return InterpretationHelper.FormatAmount(parent.TotalTotal);
				}
			}

			#endregion
		}
		#endregion

		#region DailySummaryTotals

		public IEnumerable<DailySummaryTotal> DailySummaryTotals
		{
			get
			{
				return dailySummaryTotals ?? (dailySummaryTotals = ImporterSummaryTransactionCollection.Select(x => new DailySummaryTotal(this, x)).ToArray());
			}
		}
		IEnumerable<DailySummaryTotal> dailySummaryTotals;

		[TestExcludeBusinessObjectsAllHaveTestCases]
		internal class DailySummaryTotal : NonPersistentBusinessObject, ITableInterpretation
		{
			internal DailySummaryTotal(ARLStatementOfAccountDocumentWrapper parent, TransactionInfo importerDetails) : base(parent.Factory)
			{
				Argument.NotNull(parent, nameof(parent));
				Argument.NotNull(importerDetails, nameof(importerDetails));

				this.parent = parent;
				this.importerDetails = importerDetails;
			}

			readonly ARLStatementOfAccountDocumentWrapper parent;
			readonly TransactionInfo importerDetails;

			#region Properties

			public OrgHeader[] Importers => importers ?? (importers = TransactionBatchExtension.GetOrgsFromBN(ImporterBusinessNumber, ZString.Empty, parent.Factory));
			OrgHeader[] importers;

			bool HasMultipleImporters => Importers.Length > 1;

			public ZDate CheckIssueDate
			{
				get { return new ZDate(importerDetails.GetAddInfo(TransactionBatchConstants.TransactionAddInfoKeys.CheckIssueDate)); }
			}

			[ColumnName(0)]
			public ZString ImporterCode
			{
				get { return HasMultipleImporters ? MultipleData : (string)(Importers.FirstOrDefault()?.OH_Code ?? ZString.Empty); }
			}

			string MultipleData => Res.GetString("{222FE7EB-9B91-4006-B63A-98C641CF0524}", "MULTIPLE");

			[ColumnName(1, "Importer BN")]
			public ZString ImporterBusinessNumber
			{
				get { return importerDetails.OrganizationAddress.GovRegNum.GetValueOrDefault(); }
			}

			[ColumnName(2, "Importer Name")]
			public ZString ImporterLegalName
			{
				get
				{
					var importer = HasMultipleImporters ? null : Importers.FirstOrDefault();
					return ImporterLegalNameFromMessage + (importer != null ? string.Format(CultureInfo.CurrentCulture, "\r\n({0})", importer.OH_FullNameTruncated) : string.Empty);
				}
			}

			internal ZString ImporterLegalNameFromMessage => importerDetails.OrganizationAddress.CompanyName.GetValueOrDefault();

			public ZDecimal CustomsDuties
			{
				get { return importerDetails.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.Duty); }
			}

			public ZDecimal SIMA
			{
				get { return importerDetails.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.SIMA); }
			}

			public ZDecimal ExciseTax
			{
				get { return importerDetails.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.ExciseTax); }
			}

			public ZDecimal GST
			{
				get { return importerDetails.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.GST); }
			}

			public ZDecimal Others
			{
				get { return importerDetails.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.Others); }
			}

			[ColumnName(3, "Previous SOA Total")]
			public ZDecimal PreviousMonthlyStatementTotal
			{
				get { return importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.PreviousMonthlyStatementTotal); }
			}

			[ColumnName(4, "Payments Received since last SOA")]
			public ZDecimal PaymentReceivedSinceLastMonthlyStatement
			{
				get { return importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.PaymentReceivedSinceLastMonthlyStatement); }
			}

			[ColumnName(5)]
			public ZDecimal Refund
			{
				get { return importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.Refund); }
			}

			[ColumnName(6)]
			public ZDecimal UnpaidBalanceForward
			{
				get { return importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.UnpaidBalanceForward); }
			}

			[ColumnName(7)]
			public ZDecimal ArrearsInterest
			{
				get { return importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.ArrearsInterest); }
			}

			[ColumnName(8, "Transaction Section Total")]
			public ZDecimal Total
			{
				get { return importerDetails.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.DSB); }
			}

			[ColumnName(9, "Other Charges (new)")]
			public ZDecimal OtherCharges
			{
				get { return importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.OtherCharges); }
			}

			[ColumnName(10)]
			public ZDecimal TotalPayableForImporter
			{
				get { return importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.TotalPayableForImporter); }
			}

			[ColumnName(11)]
			public ZDecimal TotalPayableForBroker
			{
				get
				{
					var result = importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.TotalPayableForBroker);
					return result > 0m ? result : importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.TotalPayableForBrokerOld);
				}
			}

			[ColumnName(12)]
			public ZDecimal TotalCredits
			{
				get { return importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.TotalCredits); }
			}

			[ColumnName(13)]
			public ZDecimal InterestAmount
			{
				get { return importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.InterestAmount); }
			}

			[ColumnName(14)]
			public ZDecimal InstalmentLastAmount
			{
				get { return importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.InstalmentLastAmount); }
			}

			[ColumnName(15)]
			public ZDecimal InstalmentCurrentAmount
			{
				get { return importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.InstalmentCurrentAmount); }
			}

			#endregion

			#region ITableInterpretation Members

			public string Caption
			{
				get { return Res.GetString("048aa97a-a5e6-4df6-a9c7-4dae15491bf7", "Importer Summary"); }
			}

			public IEnumerable<string> Titles => PropertyNameProvider.GetColumnTitles<DailySummaryTotal>().ToList();

			#endregion

			#region ITableValues Members

			public IEnumerable<object> Values
			{
				get
				{
					yield return ImporterCode;
					yield return ImporterBusinessNumber;
					yield return ImporterLegalName;
					yield return InterpretationHelper.FormatAmount(PreviousMonthlyStatementTotal);
					yield return InterpretationHelper.FormatAmount(PaymentReceivedSinceLastMonthlyStatement);
					yield return InterpretationHelper.FormatAmount(Refund);
					yield return InterpretationHelper.FormatAmount(UnpaidBalanceForward);
					yield return InterpretationHelper.FormatAmount(ArrearsInterest);
					yield return InterpretationHelper.FormatAmount(Total);
					yield return InterpretationHelper.FormatAmount(OtherCharges);
					yield return InterpretationHelper.FormatAmount(TotalPayableForImporter);
					yield return InterpretationHelper.FormatAmount(TotalPayableForBroker);
					yield return InterpretationHelper.FormatAmount(TotalCredits);
					yield return InterpretationHelper.FormatAmount(InterestAmount);
					yield return InterpretationHelper.FormatAmount(InstalmentLastAmount);
					yield return InterpretationHelper.FormatAmount(InstalmentCurrentAmount);
				}
			}

			#endregion

			#region ImporterTotal

			public ITableValues GetImporterTotal()
			{
				return new ImporterTotal(this);
			}

			internal class ImporterTotal : ITableValues
			{
				internal ImporterTotal(DailySummaryTotal dailySummaryTotal)
				{
					this.dailySummaryTotal = dailySummaryTotal;
				}

				public IEnumerable<object> Values
				{
					get
					{
						var caption = Res.GetString("65c7a108-f852-4003-80ee-0097b643cb16", "Importer Total");
						yield return new CellWithFormatting(caption, TableInterpretation.Attributes.GetColspanAttribute(1), true);
						yield return InterpretationHelper.FormatAmount(dailySummaryTotal.CustomsDuties);
						yield return InterpretationHelper.FormatAmount(dailySummaryTotal.SIMA);
						yield return InterpretationHelper.FormatAmount(dailySummaryTotal.ExciseTax);
						yield return InterpretationHelper.FormatAmount(dailySummaryTotal.GST);
						yield return InterpretationHelper.FormatAmount(dailySummaryTotal.Others);
						yield return InterpretationHelper.FormatAmount(dailySummaryTotal.Total);
					}
				}

				readonly DailySummaryTotal dailySummaryTotal;
			}

			#endregion

			#region Transactions

			public IEnumerable<TransactionDetails> Transactions
			{
				get { return transactionDetails ?? (transactionDetails = parent.transactionBatch.GetTransactionCollectionByCategory(TransactionBatchConstants.TransactionCategoryCodes.Normal, ImporterBusinessNumber).Select(x => new TransactionDetails(parent, x))); }
			}
			IEnumerable<TransactionDetails> transactionDetails;

			public IEnumerable<OtherTransactionDetails> OtherTransactions
			{
				get { return otherTransactionDetails ?? (otherTransactionDetails = parent.transactionBatch.GetTransactionCollectionByCategory(TransactionBatchConstants.TransactionCategoryCodes.Other, ImporterBusinessNumber).Select(x => new OtherTransactionDetails(parent.Factory, x))); }
			}
			IEnumerable<OtherTransactionDetails> otherTransactionDetails;

			public IEnumerable<OtherTransactionDetails> UnderReviewTransactions
			{
				get { return underReviewTransactionDetails ?? (underReviewTransactionDetails = parent.transactionBatch.GetTransactionCollectionByCategory(TransactionBatchConstants.TransactionCategoryCodes.UnderReview, ImporterBusinessNumber).Select(x => new OtherTransactionDetails(parent.Factory, x))); }
			}
			IEnumerable<OtherTransactionDetails> underReviewTransactionDetails;

			#region TransactionDetails

			[TestExcludeBusinessObjectsAllHaveTestCases]
			internal class TransactionDetails : ITableInterpretation
			{
				internal TransactionDetails(ARLStatementOfAccountDocumentWrapper parent, TransactionInfo transaction)
				{
					Argument.NotNull(parent, "parent");
					Argument.NotNull(transaction, "transaction");
					this.parent = parent;
					this.transaction = transaction;
				}
				protected readonly TransactionInfo transaction;
				protected readonly ARLStatementOfAccountDocumentWrapper parent;

				#region Properties

				[ColumnName(1)]
				public ZDate BusinessDay
				{
					get { return transaction.TransactionDate.GetValueOrDefault().Date; }
				}

				[ColumnName(2)]
				public ZDecimal CustomsDuties
				{
					get { return transaction.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.Duty); }
				}

				[ColumnName(3)]
				public ZDecimal SIMA
				{
					get { return transaction.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.SIMA); }
				}

				[ColumnName(4)]
				public ZDecimal ExciseTax
				{
					get { return transaction.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.ExciseTax); }
				}

				[ColumnName(5, "GST/PST/HST")]
				public ZDecimal GST
				{
					get { return transaction.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.GST); }
				}

				[ColumnName(6)]
				public ZDecimal Others
				{
					get { return transaction.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.Others); }
				}

				[ColumnName(7)]
				public ZDecimal Total
				{
					get { return transaction.LocalTotal.GetValueOrDefault(); }
				}

				#endregion

				#region Implementation of ITableInterpretation

				string ITableInterpretation.Caption
				{
					get { return Res.GetString("271dfb3e-f548-4d2c-9504-92513b85e8a5", "Transactions {0} to {1}", InterpretationHelper.FormatDate(parent.BillingDateFrom), InterpretationHelper.FormatDate(parent.BillingDateTo)); }
				}

				IEnumerable<string> ITableInterpretation.Titles
				{
					get { return PropertyNameProvider.GetColumnTitles<TransactionDetails>(); }
				}

				IEnumerable<object> ITableValues.Values
				{
					get
					{
						return new object[]
						{
							BusinessDay,
							InterpretationHelper.FormatAmount(CustomsDuties, true),
							InterpretationHelper.FormatAmount(SIMA, true),
							InterpretationHelper.FormatAmount(ExciseTax, true),
							InterpretationHelper.FormatAmount(GST, true),
							InterpretationHelper.FormatAmount(Others, true),
							InterpretationHelper.FormatAmount(Total, true)
						};
					}
				}

				#endregion
			}

			#endregion

			#region OtherTransactionDetails

			[TestExcludeBusinessObjectsAllHaveTestCases]
			internal class OtherTransactionDetails : TransactionDetailsWithDocumentNumber, ITableInterpretation
			{
				internal OtherTransactionDetails(BusinessObjectFactory factory, TransactionInfo transaction) : base(factory)
				{
					Argument.NotNull(transaction, "transaction");
					this.transaction = transaction;
				}
				protected readonly TransactionInfo transaction;

				#region Properties

				[ColumnName(1, "Document #")]
				public override ZString DocumentNumber
				{
					get { return transaction.TransactionReference.GetValueOrDefault(); }
				}

				public ZString DocumentType
				{
					get { return transaction.ComplianceSubType.GetValueOrDefault(); }
				}

				[ColumnName(2, "Document Type")]
				public ZString DocumentTypeDescription
				{
					get
					{
						ZString result = new ARLDocumentTypeList().GetDescriptionFromCode(DocumentType);
						return result.IsEmpty ? DocumentType : result;
					}
				}

				[ColumnName(3)]
				public ZDecimal Total
				{
					get { return transaction.LocalTotal.GetValueOrDefault(); }
				}

				public ZString Status
				{
					get { return transaction.RequisitionStatus.GetValueOrDefault(); }
				}

				[ColumnName(4, "Status")]
				public ZString StatusDescription
				{
					get
					{
						return Status.Length == 1
							? new ARLLegacyTransactionStatusList().GetDescriptionFromCode(Status)
							: new ARLTransactionStatusList().GetDescriptionFromCode(Status);
					}
				}

				[ColumnName(5)]
				public ZString BrokerIndicator
				{
					get { return transaction.GetAddInfo(TransactionBatchConstants.TransactionAddInfoKeys.BrokerIndicator); }
				}

				[ColumnName(6)]
				public ZString PaymentCategory
				{
					get { return transaction.GetAddInfo(TransactionBatchConstants.TransactionAddInfoKeys.PaymentCategory); }
				}

				[ColumnName(7)]
				public ZDate PaymentDueDate
				{
					get { return transaction.DueDate.GetValueOrDefault().Date; }
				}

				[ColumnName(8)]
				public ZDate DocumentIssueDate
				{
					get { return transaction.TransactionDate.GetValueOrDefault().Date; }
				}

				[ColumnName(9, "Check Number or Payment Ref.")]
				public ZString CheckNumberOrPaymentRef
				{
					get { return transaction.CheckNumberOrPaymentRef.GetValueOrDefault(); }
				}

				[ColumnName(10, "Related B3 Document")]
				public override ZString B3RelatedDocumentNumber
				{
					get { return transaction.OriginalReference == null ? ZString.Empty : transaction.OriginalReference.OriginalTransactionNumber.GetValueOrDefault(); }
				}

				[ColumnName(11)]
				public override ZString JobNumber
				{
					get { return base.JobNumber; }
				}

				public ZDate AccountingDate
				{
					get
					{
						var dateStr = transaction.GetAddInfo(TransactionBatchConstants.TransactionAddInfoKeys.AccountingDate);
						ZDateTime date;
						if (!dateStr.IsEmpty && ZDateTime.TryParseISO8601Date(dateStr, out date))
						{
							return date.Date;
						}

						return ZDate.Empty;
					}
				}

				#endregion

				#region Implementation of ITableInterpretation

				string ITableInterpretation.Caption
				{
					get { return transaction.Category.GetValueOrDefault() == TransactionBatchConstants.TransactionCategoryCodes.UnderReview ? Res.GetString("a81dfe91-4e8f-4439-b1b0-2bccb31971ed", "Under Review") : Res.GetString("135274e8-2248-41ca-8374-9dc276224231", "Other Transactions"); }
				}

				IEnumerable<string> ITableInterpretation.Titles
				{
					get { return PropertyNameProvider.GetColumnTitles<OtherTransactionDetails>(); }
				}

				IEnumerable<object> ITableValues.Values
				{
					get
					{
						return new object[]
						{
							DocumentNumber,
							DocumentTypeDescription,
							InterpretationHelper.FormatAmount(Total, true),
							StatusDescription,
							BrokerIndicator,
							PaymentCategory,
							InterpretationHelper.FormatDate(PaymentDueDate),
							InterpretationHelper.FormatDate(DocumentIssueDate),
							CheckNumberOrPaymentRef,
							B3RelatedDocumentNumber,
							JobNumber
						};
					}
				}

				#endregion
			}

			#endregion

			#region OtherTransactionTotal

			public ITableValues GetOtherTransactionTotal()
			{
				return new OtherTransactionTotal(OtherTransactions);
			}

			public ITableValues GetUnderReviewTransactionTotal()
			{
				return new OtherTransactionTotal(UnderReviewTransactions);
			}

			internal class OtherTransactionTotal : ITableValues
			{
				internal OtherTransactionTotal(IEnumerable<OtherTransactionDetails> transactions)
				{
					this.transactions = transactions;
				}

				public IEnumerable<object> Values
				{
					get
					{
						var caption = Res.GetString("c7e6623d-3582-4f27-8548-d6b62f2383ad", "TOTAL");
						yield return new CellWithFormatting(caption, TableInterpretation.Attributes.GetColspanAttribute(2), true);
						yield return InterpretationHelper.FormatAmount(transactions.Sum(p => p.Total));
					}
				}

				readonly IEnumerable<OtherTransactionDetails> transactions;
			}

			#endregion

			#endregion
		}

		#region ImporterSummaryTotal

		public ITableValues GetImporterSummaryTotal()
		{
			return new ImporterSummaryTotal(DailySummaryTotals);
		}

		internal class ImporterSummaryTotal : ITableValues
		{
			internal ImporterSummaryTotal(IEnumerable<DailySummaryTotal> dailySummartTotals)
			{
				this.dailySummartTotals = dailySummartTotals;
			}

			public IEnumerable<object> Values
			{
				get
				{
					var caption = Res.GetString("3922fd95-1c95-49e7-9da3-a1d21cb69b62", "TOTAL");
					yield return new CellWithFormatting(caption, TableInterpretation.Attributes.GetColspanAttribute(3), true);
					yield return InterpretationHelper.FormatAmount(dailySummartTotals.Sum(p => p.PreviousMonthlyStatementTotal));
					yield return InterpretationHelper.FormatAmount(dailySummartTotals.Sum(p => p.PaymentReceivedSinceLastMonthlyStatement));
					yield return InterpretationHelper.FormatAmount(dailySummartTotals.Sum(p => p.Refund));
					yield return InterpretationHelper.FormatAmount(dailySummartTotals.Sum(p => p.UnpaidBalanceForward));
					yield return InterpretationHelper.FormatAmount(dailySummartTotals.Sum(p => p.ArrearsInterest));
					yield return InterpretationHelper.FormatAmount(dailySummartTotals.Sum(p => p.Total));
					yield return InterpretationHelper.FormatAmount(dailySummartTotals.Sum(p => p.OtherCharges));
					yield return InterpretationHelper.FormatAmount(dailySummartTotals.Sum(p => p.TotalPayableForImporter));
					yield return InterpretationHelper.FormatAmount(dailySummartTotals.Sum(p => p.TotalPayableForBroker));
					yield return InterpretationHelper.FormatAmount(dailySummartTotals.Sum(p => p.TotalCredits));
					yield return InterpretationHelper.FormatAmount(dailySummartTotals.Sum(p => p.InterestAmount));
					yield return InterpretationHelper.FormatAmount(dailySummartTotals.Sum(p => p.InstalmentLastAmount));
					yield return InterpretationHelper.FormatAmount(dailySummartTotals.Sum(p => p.InstalmentCurrentAmount));
				}
			}

			readonly IEnumerable<DailySummaryTotal> dailySummartTotals;
		}

		#endregion

		#endregion

		#region For Document

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class TransactionsDetailsWrapper : NonPersistentBusinessObject
		{
			public TransactionsDetailsWrapper(ITableInterpretation transactionDetails, DailySummaryTotal dailySummaryTotal, ZString transactionDetailsType) : base(dailySummaryTotal.Factory)
			{
				Argument.NotNull(dailySummaryTotal, "dailySummaryTotal");
				this.dailySummaryTotal = dailySummaryTotal;
				this.transactionDetailsType = transactionDetailsType;

				var details = transactionDetails as DailySummaryTotal.TransactionDetails;
				if (details != null)
				{
					this.BusinessDay = details.BusinessDay;
					this.CustomsDuties = details.CustomsDuties;
					this.SIMA = details.SIMA;
					this.ExciseTax = details.ExciseTax;
					this.GST = details.GST;
					this.Others = details.Others;
					this.Total = details.Total;
				}

				var otherDetails = transactionDetails as DailySummaryTotal.OtherTransactionDetails;
				if (otherDetails != null)
				{
					this.DocumentNumber = otherDetails.DocumentNumber;
					this.DocumentTypeDescription = otherDetails.DocumentTypeDescription;
					this.Total = otherDetails.Total;
					this.StatusDescription = otherDetails.StatusDescription;
					this.PaymentDueDate = otherDetails.PaymentDueDate;
					this.DocumentIssueDate = otherDetails.DocumentIssueDate;
					this.B3RelatedDocumentNumber = otherDetails.B3RelatedDocumentNumber;
					this.JobNumber = Regex.Replace(otherDetails.JobNumber, @"\<.+?\>", ZString.Empty);
				}
			}

			readonly DailySummaryTotal dailySummaryTotal;
			readonly ZString transactionDetailsType;

			#region Header Properties

			public ZString ImporterCode
			{
				get { return dailySummaryTotal.ImporterCode; }
			}

			public ZString ImporterLegalName
			{
				get { return dailySummaryTotal.ImporterLegalName; }
			}

			public ZString ImporterBusinessNumber
			{
				get { return dailySummaryTotal.ImporterBusinessNumber; }
			}

			public ZString TransactionDetailsType
			{
				get { return transactionDetailsType; }
			}

			public ZBool IsFirstItemOfCurrentTransactions { get; set; }
			public ZBool IsLastItemOfCurrentTransactions { get; set; }

			#endregion

			#region TransactionDetails

			public ZDate BusinessDay { get; set; }
			public ZDecimal CustomsDuties { get; set; }
			public ZDecimal SIMA { get; set; }
			public ZDecimal ExciseTax { get; set; }
			public ZDecimal GST { get; set; }
			public ZDecimal Others { get; set; }
			public ZDecimal Total { get; set; }

			#endregion

			#region OtherTransactionDetails

			public ZString DocumentNumber { get; set; }
			public ZString DocumentTypeDescription { get; set; }
			public ZString StatusDescription { get; set; }
			public ZDate PaymentDueDate { get; set; }
			public ZDate DocumentIssueDate { get; set; }
			public ZString B3RelatedDocumentNumber { get; set; }
			public ZString JobNumber { get; set; }

			#endregion

			#region Total Properties

			public ZDecimal TotalCustomsDuties
			{
				get { return dailySummaryTotal.CustomsDuties; }
			}

			public ZDecimal TotalSIMA
			{
				get { return dailySummaryTotal.SIMA; }
			}

			public ZDecimal TotalExciseTax
			{
				get { return dailySummaryTotal.ExciseTax; }
			}

			public ZDecimal TotalGST
			{
				get { return dailySummaryTotal.GST; }
			}

			public ZDecimal TotalOthers
			{
				get { return dailySummaryTotal.Others; }
			}

			public ZDecimal TotalTransactionsTotal
			{
				get { return dailySummaryTotal.Total; }
			}

			public ZDecimal TotalOtherTransactionsTotal { get; set; }

			#endregion

		}

		public BusinessObjectCollectionWrapper<TransactionsDetailsWrapper> AllTransactionsForDocument
		{
			get
			{
				if (allTransactionsForDocument == null)
				{
					var list = new List<TransactionsDetailsWrapper>();
					var countOfTrans = 0;
					foreach (DailySummaryTotal dailySummaryTotal in DailySummaryTotals)
					{
						var countOfList = list.Count;
						if (dailySummaryTotal.Transactions.Any())
						{
							list.AddRange(dailySummaryTotal.Transactions.Select(x => new TransactionsDetailsWrapper(x, dailySummaryTotal, Res.GetString("4d8b2e0e-d8af-4c25-a08f-0491337b65c0", "Transactions"))));
							list[countOfTrans].IsFirstItemOfCurrentTransactions = true;
							list[list.Count - 1].IsLastItemOfCurrentTransactions = true;
							countOfTrans = list.Count;
						}
						if (dailySummaryTotal.OtherTransactions.Any())
						{
							list.AddRange(dailySummaryTotal.OtherTransactions.Select(x => new TransactionsDetailsWrapper(x, dailySummaryTotal, Res.GetString("6a122ee7-3243-482f-9b34-799f417c6b65", "Other Transactions"))));
							list[countOfTrans].IsFirstItemOfCurrentTransactions = true;
							list[list.Count - 1].IsLastItemOfCurrentTransactions = true;
							list[list.Count - 1].TotalOtherTransactionsTotal = dailySummaryTotal.OtherTransactions.Sum(x => x.Total);
							countOfTrans = list.Count;
						}
						if (dailySummaryTotal.UnderReviewTransactions.Any())
						{
							list.AddRange(dailySummaryTotal.UnderReviewTransactions.Select(x => new TransactionsDetailsWrapper(x, dailySummaryTotal, Res.GetString("c2e8d7a4-3677-448a-97bc-3dfd67e8eace", "Under Review"))));
							list[countOfTrans].IsFirstItemOfCurrentTransactions = true;
							list[list.Count - 1].IsLastItemOfCurrentTransactions = true;
							list[list.Count - 1].TotalOtherTransactionsTotal = dailySummaryTotal.UnderReviewTransactions.Sum(x => x.Total);
							countOfTrans = list.Count;
						}
						if (countOfList == list.Count)
						{
							list.Add(new TransactionsDetailsWrapper(null, dailySummaryTotal, ZString.Empty));
							countOfTrans = list.Count;
						}
					}
					allTransactionsForDocument = new BusinessObjectCollectionWrapper<TransactionsDetailsWrapper>(list);
				}
				return allTransactionsForDocument;
			}
		}
		BusinessObjectCollectionWrapper<TransactionsDetailsWrapper> allTransactionsForDocument;

		public BusinessObjectCollectionWrapper<DailySummaryTotal> AllDailySummaryTotalsForDocument
		{
			get
			{
				if (allDailySummaryTotalsForDocument == null)
				{
					allDailySummaryTotalsForDocument = new BusinessObjectCollectionWrapper<DailySummaryTotal>(DailySummaryTotals);
				}
				return allDailySummaryTotalsForDocument;
			}
		}
		BusinessObjectCollectionWrapper<DailySummaryTotal> allDailySummaryTotalsForDocument;

		ZGuid ISourceIdentifierProvider.SourceIdentifier => this.message.PK;

		#endregion
	}
}

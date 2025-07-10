using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public abstract class TxnHeaderProcessorBase
	{
		public TxnHeaderProcessorBase(BusinessObjectFactory factory, INotificationManager notification)
		{
			FactoryToCollectChildFactories = factory;
			NotificationManager = notification;
			NotificationBuffer = new NotificationBuffer(notification.NotificationSubscriber);
		}

		public TxnHeaderProcessorBase(BusinessObjectFactory factory, INotifications notifications)
		{
			Notifications = notifications;
			NotificationBuffer = new NotificationBuffer(Notifications);
			NotificationManager = new NotificationManager(NotificationBuffer);
			FactoryToCollectChildFactories = factory;
		}

		protected readonly INotifications Notifications;
		protected readonly BusinessObjectFactory FactoryToCollectChildFactories;
		protected Xsd.TxnHeader TxnHeader;
		protected INotificationManager NotificationManager;
		protected NotificationBuffer NotificationBuffer;
		protected TransactionHeader TransactionHeader;
		protected BusinessObjectFactory CurrentFactory;

		#region ProcessTxnHeaderCollection

		public void ProcessTxnHeaderCollection(Xsd.TxnHeaderCollection txnHeaderCollection)
		{
			foreach (Xsd.TxnHeader txnHeader in txnHeaderCollection)
			{
				if (!CanContinueProcess)
				{
					return;
				}

				TxnHeader = txnHeader;

				SetupContext();

#if DEBUG
				if (TestOnlyFirstFactoryUsed == null && Globals.IsTest)
				{
					TestOnlyFirstFactoryUsed = CurrentFactory;

					if (SimulateSavingFailure)
					{
						SimulateSavingFailure = false;
						TestOnlyFirstFactoryUsed.Saving += x => throw new ZCannotSaveException("Test saving error", "Error Heading");
					}
				}
#endif

				ProcessTxnHeader();
			}

			PrintReceiptAndPayment();
		}

		protected abstract void SetupContext();

		protected virtual bool CanContinueProcess => true;

		protected virtual void PrintReceiptAndPayment() { }

		#endregion

		#region ProcessTxnHeader

		void ProcessTxnHeader()
		{
			NotificationManager.AddInfoNotification(Res.GetString("e3ac091b-e549-4046-b3cf-08c3b2321c74", "Begin processing {0}", GetContextMessage(TxnHeader)));

			var txnType = TxnHeaderMapper.GetBizObjTypeFromIValueObject(TxnHeader);
			TransactionHeader = CreateOrLoadTransactionFromTxnHeader(txnType);

			if (TransactionHeader == null)
			{
				return;
			}

			Xsd.TxnHeaderCollection paidHeaderCollection = null;
			if (HasPaidTransactions)
			{
				paidHeaderCollection = GetPaidHeaderCollection();
				ValidatePaidTransaction(paidHeaderCollection);
			}

			ProcessTxnHeaderCore();

			ProcessPaidTransactions(paidHeaderCollection);

			if (HasErrors)
			{
				NotificationManager.AddInfoNotification("  " + Res.GetString("bb37fa98-d1da-4690-b457-ac5ce3ae9a82", "This transaction has errors and was not imported."));
				if (FactoryToCollectChildFactories != null)
				{
					RemoveCurrentFactory();
				}
			}
		}

		bool HasPaidTransactions => TxnHeader.PaidTransactions.Count > 0;

		bool HasErrors => NotificationManager.ErrorsHaveBeenReported || NotificationBuffer.HasErrors;

		bool IsMatchOnly => !TxnHeader.FullyPaidDate.IsEmpty;

		void ProcessTxnHeaderCore()
		{
			if (!HasErrors && !IsMatchOnly)
			{
				SetUpTransactionAndValidate();
			}

			if (!HasErrors && !HasPaidTransactions)
			{
				NotificationManager.AddInfoNotification("  " + Res.GetString("5af1f130-2daa-454d-84e5-73b0e200e5ca", "Completed Processing Transaction."));
				if (IsNeedSaveWithCurrentFactory)
				{
					CurrentFactory.Save();
				}

				return;
			}
		}

		string GetContextMessage(Xsd.TxnHeader txnHeader)
		{
			return Res.GetString("ab04e98c-7509-4915-aaa7-614b506be7d9", "Transaction {0} {1} {2} {3} {4}:", txnHeader.Ledger.ToString(), txnHeader.TxnType.ToString(), txnHeader.DebtorOrCreditor.EDICode, txnHeader.BankCode, TxnHeader.ChequeOrReference) + " ";
		}

		string GetContextMessage(IMatching matching)
		{
			return Res.GetString("ac3ccd01-862d-4083-bac1-e1e906487dbf", "Transaction {0} {1} {2}:", matching.Ledger.ToString(), matching.TransactionType.ToString(), matching.TransactionNumber) + " ";
		}

		TransactionHeader CreateOrLoadTransactionFromTxnHeader(Type txnType)
		{
			TransactionHeader result = null;
			if (IsMatchOnly)
			{
				if (HasPaidTransactions)
				{
					var orgPK = FindOrgPK(TxnHeader);
					var transactions = FindTransactions(CurrentFactory, new Xsd.TxnHeader[] { TxnHeader }, orgPK, new Dictionary<Xsd.TxnHeader, ZGuid>());

					if (transactions != null && transactions.Length > 0)
					{
						result = transactions.First();
					}
					else
					{
						AddErrorForTransactionNotExist(TxnHeader);
					}
				}
				else
				{
					NotificationManager.AddErrorToNotifications(RemittanceFileImportHelper.AtLeastTwoPTRLinesForMHRError);
				}
			}
			else
			{
				if (txnType != null && typeof(ReceiptPaymentBase).IsAssignableFrom(txnType))
				{
					result = (ReceiptPaymentBase)CurrentFactory.New(txnType);
				}
				else if (txnType != null && typeof(Journal).IsAssignableFrom(txnType))
				{
					result = (Journal)CurrentFactory.New(txnType);
				}
				else
				{
					NotificationManager.AddErrorToNotifications("  " + Res.GetString("c0f36e7e-5192-4d12-aee6-4613a0c3cbaa", "Unable to create the transaction."));
				}
			}

			return result;
		}

		protected virtual void SetUpTransactionAndValidate()
		{
			var builder = new TransactionHeaderBuilder(NotificationManager, new TransactionBuilderConfig());
			if (TransactionHeader is Receipt r)
			{
				builder.SetValuesOnReceiptBusinessObject(r, TxnHeader, new ValueObjectImportContext(new BusinessObjectFactoryProvider(CurrentFactory), NotificationManager.NotificationSubscriber), "");
			}
			else if (TransactionHeader is Payment p)
			{
				builder.SetValuesOnPaymentBusinessObject(p, TxnHeader, new ValueObjectImportContext(new BusinessObjectFactoryProvider(CurrentFactory), NotificationManager.NotificationSubscriber), "");
			}
			else if (TransactionHeader is Journal j)
			{
				builder.SetValuesOnClearingJournalBusinessObject(j, TxnHeader, new ValueObjectImportContext(new BusinessObjectFactoryProvider(CurrentFactory), NotificationManager.NotificationSubscriber), "");

				TransactionHeader.AH_AG = (Guid)AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}

			if (TransactionHeader.Header != null)
			{
				if (TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable &&
					!TransactionHeader.Header.CompanyData.OB_IsDebtor)
				{
					NotificationManager.AddErrorToNotifications(Res.GetString("f8e1261c-e885-44be-856d-0ad220a59bd0", "The '{0}' organization must be flagged as receivables.", TransactionHeader.Header.OH_Code));
				}
				if (TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable &&
					!TransactionHeader.Header.CompanyData.OB_IsCreditor)
				{
					NotificationManager.AddErrorToNotifications(Res.GetString("49079d11-c321-4a2c-abb0-3e3df366d56c", "The '{0}' organization must be flagged as payables.", TransactionHeader.Header.OH_Code));
				}
			}

			builder.RunValidationAndReportErrors(TransactionHeader);

			ValidateAndSetUpTransactionHeaderReference();

			if (!(TxnHeader.TxnType == Xsd.TxnType.JNL && TxnHeader.LocalInvoiceAmtInclTax.Value == 0))
			{
				if (TransactionHeader.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency && TxnHeader.LocalInvoiceAmtInclTax.Value != TxnHeader.OsInvoiceAmtInclTax.Value)
				{
					NotificationManager.AddErrorToNotifications(Res.GetString("47502653-D391-44CA-B219-067AD7E22802", "Local currency {0} OS Amount does not equal Local Amount.", TransactionHeader.DefaultDescription));
				}
			}
		}

		protected abstract void RemoveCurrentFactory();

		Xsd.TxnHeaderCollection GetPaidHeaderCollection()
		{
			var paidHeaderCollection = new Xsd.TxnHeaderCollection();

			if (IsMatchOnly)
			{
				paidHeaderCollection.Add(TxnHeader);
			}

			foreach (Xsd.TxnHeader item in TxnHeader.PaidTransactions)
			{
				paidHeaderCollection.Add(item);
			}

			return paidHeaderCollection;
		}

		void ValidatePaidTransaction(Xsd.TxnHeaderCollection paidHeaderCollection)
		{
			var duplicatedTransactions = GetDuplicatedPaidTransactions(paidHeaderCollection);
			if (duplicatedTransactions.Length > 0)
			{
				NotificationManager.AddErrorToNotifications(Res.GetString("3fcce117-4999-419d-a11f-ea3c101a0d9f", "There are duplicate Paid Transactions:"));

				foreach (var duplicatedHeader in duplicatedTransactions)
				{
					NotificationManager.AddInfoNotification(string.Format("    {0},{1},{2},{3},{4}", duplicatedHeader.Ledger, duplicatedHeader.TxnType,
						duplicatedHeader.TxnNumber, duplicatedHeader.AmountPaidThisPayment.Value, duplicatedHeader.DebtorOrCreditor.EDICode));
				}
			}

			var paidTransactionsWithoutTxnNumber = paidHeaderCollection.Cast<Xsd.TxnHeader>().Where(x => x.TxnNumber.IsEmpty);
			if (paidTransactionsWithoutTxnNumber.Any())
			{
				NotificationManager.AddErrorToNotifications(Res.GetString("C8649414-B7EE-4c26-B9E3-6E2E85AB7E9E", "There are Paid Transaction without Transaction Number specified:"));

				foreach (var transactionWithoutTxnNumber in paidTransactionsWithoutTxnNumber)
				{
					if (transactionWithoutTxnNumber.TxnNumber.IsEmpty)
					{
						NotificationManager.AddInfoNotification(string.Format("    {0},{1},{2},{3},{4},{5}", transactionWithoutTxnNumber.Ledger, transactionWithoutTxnNumber.TxnType,
							transactionWithoutTxnNumber.TxnNumber, transactionWithoutTxnNumber.AmountPaidThisPayment.Value, transactionWithoutTxnNumber.DebtorOrCreditor.EDICode, transactionWithoutTxnNumber.PaymentReference));
					}
				}
			}
		}

		Xsd.TxnHeader[] GetDuplicatedPaidTransactions(Xsd.TxnHeaderCollection paidHeaderCollection)
		{
			var uniqueSet = new HashSet<string>();
			var duplicatedTransactions = new List<Xsd.TxnHeader>();
			foreach (Xsd.TxnHeader paidTxnHeader in paidHeaderCollection)
			{
				var key = paidTxnHeader.Ledger.ToString() + paidTxnHeader.TxnType.ToString() + paidTxnHeader.TxnNumber +
					(paidTxnHeader.DebtorOrCreditor.IsSpecified ? paidTxnHeader.DebtorOrCreditor.EDICode : TxnHeader.DebtorOrCreditor.EDICode);
				if (!uniqueSet.Contains(key))
				{
					uniqueSet.Add(key);
				}
				else
				{
					duplicatedTransactions.Add(paidTxnHeader);
				}
			}
			return duplicatedTransactions.ToArray();
		}

		protected virtual void ValidateAndSetUpTransactionHeaderReference() { }

		#endregion

		#region ProcessPaidTransactions

		void ProcessPaidTransactions(Xsd.TxnHeaderCollection paidHeaderCollection)
		{
			if (!HasErrors && HasPaidTransactions)
			{
				var (matching, paymentApproval) = GetMatching();

				var foundOrgs = GetOrgsFromPaidTransactions();

				var unmatchedTransatcions = GetUnmatchedTransatcions(foundOrgs);

				if (unmatchedTransatcions == null)
				{
					return;
				}

				matching.UnmatchedTransactions.AddRange(unmatchedTransatcions);

				var transactionsToMatch = ValidateAndGetTransactionsToMatch(foundOrgs, matching, paidHeaderCollection);

				matching.CreateAndAddJournalsForMatching(transactionsToMatch);

				ProcessMatch(matching, transactionsToMatch, paymentApproval);
			}
		}

		TransactionHeader[] GetUnmatchedTransatcions(Dictionary<Xsd.TxnHeader, ZGuid> foundOrgs)
		{
			var paidTransactionsSorted = from Xsd.TxnHeader paid in TxnHeader.PaidTransactions orderby paid.Ledger, paid.TxnType select paid;
			return FindTransactions(CurrentFactory, paidTransactionsSorted, TransactionHeader.AH_OH, foundOrgs);
		}

		Dictionary<Xsd.TxnHeader, ZGuid> GetOrgsFromPaidTransactions()
		{
			var foundOrgs = new Dictionary<Xsd.TxnHeader, ZGuid>();
			foreach (Xsd.TxnHeader paidTxnHeader in TxnHeader.PaidTransactions)
			{
				var orgPK = FindOrgPK(paidTxnHeader);

				if (!orgPK.IsEmpty)
				{
					foundOrgs.Add(paidTxnHeader, orgPK);
				}
			}

			return foundOrgs;
		}

		Dictionary<BusinessObject, ZDecimal> ValidateAndGetTransactionsToMatch(Dictionary<Xsd.TxnHeader, ZGuid> foundOrgs, MatchingBase matching, Xsd.TxnHeaderCollection paidHeaderCollection)
		{
			var transactionsToMatch = new Dictionary<BusinessObject, ZDecimal>();

			AddChildOrganizations();

			foreach (Xsd.TxnHeader paidTxnHeader in paidHeaderCollection)
			{
				if (paidTxnHeader.TxnCategory == TransactionCategory.Codes.Clearing)
				{
					continue;
				}

				var invoiceType = TxnHeaderMapper.GetBizObjTypeFromIValueObject(paidTxnHeader);

				var ledger = paidTxnHeader.Ledger.ToString();
				var transactionType = paidTxnHeader.TxnType.ToString();
				var transactionNumber = paidTxnHeader.TxnNumber;

				var pTRorgPK = foundOrgs.ContainsKey(paidTxnHeader) ? foundOrgs[paidTxnHeader] : ZGuid.Empty;

				var matchingTransactionQuery = GetMatchingTransactionQuery(ledger, transactionType, transactionNumber, TransactionHeader.AH_OH, pTRorgPK);
				var transaction = GetMatchedTransaction(matching.Factory, transactionsToMatch, matchingTransactionQuery);

				var collection = ChildOrgCollection[TransactionHeader.AH_OH];
				if (ledger == LedgerTypes.AccountsPayable && transaction == null && collection != null && collection.Count > 0)
				{
					matchingTransactionQuery = GetMatchingTransactionQuery(ledger, transactionType, transactionNumber, TransactionHeader.AH_OH, pTRorgPK, collection);
					transaction = GetMatchedTransaction(matching.Factory, transactionsToMatch, matchingTransactionQuery);
				}

				if (transaction == null && MiscellaneousTransactionType.Contains(transactionType) && paidTxnHeader.ShouldCreateDuringMatching)
				{
					transaction = CreateMiscellaneousTransaction(paidTxnHeader, matching);
					if (transaction == null)
					{
						break;
					}
				}

				if (transaction != null)
				{
					if (!paidTxnHeader.MatchStatus.IsEmpty)
					{
						transaction.MatchStatus = paidTxnHeader.MatchStatus;
						transaction.MatchStatusReasonCode = paidTxnHeader.MatchStatusReasonCode;
					}

					if (matching.CheckIfTransactionIsPaidViaWebService(transaction as BusinessObject))
					{
						NotificationManager.AddErrorToNotifications(Res.GetString("ee835e0c-b23b-4e38-938b-fe2eb87ba98c", @"Invoice {0} is part paid via Invoice Payment Web Service and cannot be matched against.", transaction.TransactionNumber));
						continue;
					}

					if (transaction.OutstandingAmount == transaction.OriginalOutstandingAmount)
					{
						var paymentAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(CurrentFactory, paidTxnHeader.AmountPaidThisPayment, invoiceType, false);

						if (Math.Abs(paymentAmount) > Math.Abs(transaction.OSOutstandingAmount) && ShouldCreateBalancingJournals(transactionType))
						{
							matching.AddToBalancingJournals(CreateJournal(paidTxnHeader, transaction.OSOutstandingAmount - paymentAmount, transaction, PaymentStatus.OverPaid));
							paymentAmount = transaction.OSOutstandingAmount;
						}
						else
						{
							paymentAmount = Utilities.Round(paymentAmount, transaction.CurrencyDecimals);
						}

						if (paidTxnHeader.FullyPaidDate.IsEmpty)
						{
							transactionsToMatch.Add((BusinessObject)transaction, paymentAmount);
						}
						else
						{
							matching.MoveFromUnmatchToMatch(new Dictionary<BusinessObject, ZDecimal>() { { TransactionHeader, paymentAmount } });
						}
					}
					else
					{
						NotificationManager.AddErrorToNotifications(Res.GetString("EC028EA6-19A5-4c48-BC7E-688C1EFF2D7B", @"Invoice {0} is being paid by an unapproved payment.
Revise the file to exclude this invoice, or remove the invoice from the unapproved payment.", transaction.TransactionNumber));
					}
				}
				else
				{
					if (ShouldCreateBalancingJournals(transactionType))
					{
						var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
						if (ledger == LedgerTypes.AccountsReceivable)
						{
							query.AddSubQuery(GetTransactionFilter(LedgerTypes.AccountsReceivable, new HashSet<string>() { transactionType }, new HashSet<string>() { transactionNumber }, filterOutstandingTransactionsOnly: false), JoinCondition.And);
						}
						else
						{
							query.AddSubQuery(GetTransactionFilter(LedgerTypes.AccountsPayable, new HashSet<string>() { transactionType }, new HashSet<string>() { transactionNumber }, filterOutstandingTransactionsOnly: false, transactionHeaderOrg: TransactionHeader.AH_OH, foundOrgs: foundOrgs), JoinCondition.And);
						}

						transaction = (IMatching)CurrentFactory.LoadTop1<TransactionHeader>(query);
						matching.AddToBalancingJournals(CreateJournal(paidTxnHeader,
							-1 * TxnHeaderMapper.GetDecimalFromXmlFinancialValue(CurrentFactory, paidTxnHeader.OsInvoiceAmtInclTax, invoiceType, false), transaction, transaction == null ? PaymentStatus.InvoiceNotFound : PaymentStatus.FullyPaid));
					}
					else
					{
						AddErrorForTransactionNotExist(paidTxnHeader);
					}
				}
			}

			return transactionsToMatch;
		}

		void AddChildOrganizations()
		{
			if (!ChildOrgCollection.ContainsKey(TransactionHeader.AH_OH))
			{
				var childOrgFilter = new ZDBOnlyQuery(typeof(OrgHeader));
				childOrgFilter.AddSubQuery(GetChildOrganizationFilter(TransactionHeader.AH_OH), JoinCondition.And);
				var childOrgs = CurrentFactory.Load<OrgHeader>(childOrgFilter);

				if (childOrgs.Any())
				{
					var childOrgPks = new List<ZGuid>();
					childOrgs.ForEach(x => childOrgPks.Add(x.PK));

					ChildOrgCollection.Add(TransactionHeader.AH_OH, childOrgPks);
				}
				else
				{
					ChildOrgCollection.Add(TransactionHeader.AH_OH, new List<ZGuid>());
				}
			}
		}

		Dictionary<ZGuid, List<ZGuid>> ChildOrgCollection => childOrgCollection ?? (childOrgCollection = new Dictionary<ZGuid, List<ZGuid>>());
		Dictionary<ZGuid, List<ZGuid>> childOrgCollection;

		ZDBOnlySubQuery GetTransactionFilter(string ledger, HashSet<string> transactionTypes, HashSet<string> transactionNumbers, ZBool filterOutstandingTransactionsOnly)
		{
			return GetTransactionFilter(ledger, transactionTypes, transactionNumbers, filterOutstandingTransactionsOnly, ZGuid.Empty, null);
		}

		ZDBOnlySubQuery GetTransactionFilter(string ledger, HashSet<string> transactionTypes, HashSet<string> transactionNumbers, ZBool filterOutstandingTransactionsOnly, ZGuid transactionHeaderOrg, Dictionary<Xsd.TxnHeader, ZGuid> foundOrgs)
		{
			var transactionQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			transactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			transactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledger);
			transactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionTypes);
			if (filterOutstandingTransactionsOnly)
			{
				transactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_OutstandingAmount, SQLComparisonOperator.NotEqual, 0);
			}

			var numberFilter = GetNumberFilter(transactionNumbers);

			transactionQuery.AddToFilter(numberFilter);

			if (ledger == LedgerTypes.AccountsPayable)
			{
				if (transactionHeaderOrg.IsEmpty)
				{
					NotificationManager.AddErrorToNotifications(Res.GetString("1C448D29-D4CD-4ADD-ACD6-D1B7DF03E777", "Matching failed. Creditor is not found."));
				}

				var orgPks = new List<ZGuid>();
				if (foundOrgs.Any())
				{
					orgPks.AddRange(foundOrgs.Values);
				}

				transactionQuery.AddToFilter(GetOrgFilter(transactionHeaderOrg, orgPks));
			}

			return transactionQuery;
		}

		ZQuery GetNumberFilter(IEnumerable<string> transactionNumbers)
		{
			var numberFilter = new ZQuery();
			numberFilter.DefaultJoinCondition = JoinCondition.Or;
			numberFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transactionNumbers);
			numberFilter.AddToFilter(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, transactionNumbers);
			numberFilter.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, transactionNumbers);
			return numberFilter;
		}

		ZQuery GetMatchingTransactionQuery(string ledger, string transactionType, ZString transactionNumber, ZGuid payRecOrgPK, ZGuid pTRorgPK, List<ZGuid> childOrgPKs = null)
		{
			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledger);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionType);
			query.AddToFilter(AccTransactionHeaderSchema.AH_OutstandingAmount, SQLComparisonOperator.NotEqual, 0);

			ZQuery numberFilter = GetNumberFilter(new string[] { transactionNumber });
			query.AddToFilter(numberFilter);

			if (ledger == LedgerTypes.AccountsReceivable && !pTRorgPK.IsEmpty)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_OH, pTRorgPK);
			}

			if (ledger == LedgerTypes.AccountsPayable)
			{
				var debtorOrCreditorFilter = GetDebtorOrCreditorFilter(payRecOrgPK, pTRorgPK, childOrgPKs);
				query.AddToFilter(debtorOrCreditorFilter);
			}

			query.OrderBy = AccTransactionHeaderSchema.AH_TransactionNum.Name;

			return query;
		}

		ZDBOnlyQuery GetOrgFilter(ZGuid transactionHeaderOrg, List<ZGuid> orgPks)
		{
			orgPks.Add(transactionHeaderOrg);

			var orgFilter = new ZDBOnlyQuery(typeof(OrgHeader));
			orgFilter.AddToFilter(AccTransactionHeaderSchema.AH_OH, orgPks);

			var childOrgSubQuery = GetChildOrganizationFilter(transactionHeaderOrg);
			orgFilter.AddSubQuery(AccTransactionHeaderSchema.AH_OH, childOrgSubQuery, JoinCondition.Or);

			return orgFilter;
		}

		ZBool ShouldCreateBalancingJournals(string transactionType)
		{
			return (transactionType == ZArchitecture.Core.TransactionTypes.Invoice ||
					transactionType == ZArchitecture.Core.TransactionTypes.CreditNote ||
					transactionType == ZArchitecture.Core.TransactionTypes.AdjustmentNote) &&
			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.Value &&
			!IsMatchOnly;
		}

		Journal CreateJournal(Xsd.TxnHeader paidTxnHeader, ZDecimal oSAmount, IMatching transaction, PaymentStatus paymentStatus)
		{
			Journal journal;
			if (paidTxnHeader.Ledger.ToString() == LedgerTypes.AccountsPayable)
			{
				journal = CurrentFactory.New<APJournal>();
			}
			else
			{
				journal = CurrentFactory.New<ARJournal>();
			}

			journal.IsAutoGenerated = true;

			ZString orgCode = paidTxnHeader.DebtorOrCreditor.EDICode.IsEmpty ? TxnHeader.DebtorOrCreditor.EDICode : paidTxnHeader.DebtorOrCreditor.EDICode;
			OrgHeader organisation = CurrentFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, orgCode));
			if (organisation != null)
			{
				journal.AH_OH = organisation.PK;
			}

			if (transaction != null)
			{
				if (!paidTxnHeader.OsCurrencyEmptyFlag)
				{
					journal.AH_RX_NKTransactionCurrency = paidTxnHeader.OsInvoiceAmtInclTax.CurrencyCode;
					if (journal.AH_RX_NKTransactionCurrency != transaction.CurrencyCode)
					{
						NotificationManager.AddErrorToNotifications(Res.GetString("b0868bf7-afa4-4b44-bfca-a9e80d271cc5", "The created journal related to the PTR record with transaction number {0} has a wrong Currency ({1}). It must be equal to the matched transaction currency ({2}).",
							paidTxnHeader.TxnNumber, journal.AH_RX_NKTransactionCurrency, transaction.CurrencyCode));
					}

					var roundedOSAmount = Math.Abs(Utilities.Round(oSAmount, journal.OSCurrencyDecimals));
					if ((paidTxnHeader.LocalInvoiceAmtInclTax.CurrencyCode.IsEmpty && paidTxnHeader.LocalInvoiceAmtInclTax.Value.IsEmpty && paidTxnHeader.OsInvoiceAmtInclTax.CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency) ||
					(!paidTxnHeader.LocalInvoiceAmtInclTax.CurrencyCode.IsEmpty && paidTxnHeader.LocalInvoiceAmtInclTax.CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency && !paidTxnHeader.LocalInvoiceAmtInclTax.Value.IsEmpty))
					{
						if (paymentStatus == PaymentStatus.OverPaid)
						{
							journal.AH_ExchangeRate = transaction.ExchangeRateAmount;
							journal.AH_OSExTaxAmount = roundedOSAmount;
						}
						else if (paymentStatus == PaymentStatus.FullyPaid)
						{
							journal.AH_OSExTaxAmount = roundedOSAmount;
							if (paidTxnHeader.LocalInvoiceAmtInclTax.Value.IsEmpty)
							{
								journal.AH_LocalExTaxAmount = Utilities.Round(paidTxnHeader.OsInvoiceAmtInclTax.Value, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
							}
							else
							{
								journal.AH_LocalExTaxAmount = paidTxnHeader.LocalInvoiceAmtInclTax.Value;
							}
							journal.IsRecalculateExchangeRate = true;
						}
					}
					else
					{
						journal.AH_ExchangeRate = Env.CurrentCompany.ExchangeRate.TodaysRate(journal.AH_RX_NKTransactionCurrency, ExchangeRateType.Buy);
						journal.AH_OSExTaxAmount = roundedOSAmount;
					}
				}
				else
				{
					journal.AH_RX_NKTransactionCurrency = transaction.CurrencyCode;
					journal.AH_ExchangeRate = transaction.ExchangeRateAmount;
					journal.AH_OSExTaxAmount = Math.Abs(Utilities.Round(oSAmount, transaction.CurrencyDecimals));
				}
				journal.AH_TransactionCategory = TransactionCategory.Codes.TransactionAlreadyPaid;
			}
			else
			{
				SetJournalAmountsForMissingTransaction(journal, paidTxnHeader);
				journal.AH_TransactionCategory = TransactionCategory.Codes.TransactionNotFound;
			}

			if (journal.AH_OSExTaxAmount == 0)
			{
				NotificationManager.AddErrorToNotifications(Res.GetString("57bb8914-63ce-4c4c-9173-6ad9645221c9", "Importing a line for matching to transaction \"{0}\" generated a journal with a zero amount.", paidTxnHeader.TxnNumber));
			}

			journal.AH_InvoiceDate = paidTxnHeader.InvoiceDate.IsEmpty ? ZDateTime.Now : paidTxnHeader.InvoiceDate;
			journal.AH_PostDate = paidTxnHeader.PostDate.IsEmpty ? ZDateTime.Now : paidTxnHeader.PostDate;
			journal.AH_DueDate = paidTxnHeader.DueDate.IsEmpty ? ZDateTime.Now : paidTxnHeader.DueDate;
			if (!paidTxnHeader.Description.IsEmpty)
			{
				SetJournalDescription(journal, paidTxnHeader);
			}
			else
			{
				journal.AH_Desc = journal.JournalDefaultDescription;
			}

			SetChequeOrReference(journal, paidTxnHeader);

			journal.DebitCreditSign = oSAmount > 0 ? DebitCreditDataEntry.CR : DebitCreditDataEntry.DR;

			return journal;
		}

		void SetChequeOrReference(Journal journal, Xsd.TxnHeader paidTxnHeader)
		{
			var chequeValue = (paidTxnHeader.PaymentReference.IsEmpty) ? paidTxnHeader.TxnNumber : paidTxnHeader.PaymentReference;
			StringToBusinessObjectFieldConverter.InstanceForCurrentCompany.SetPropertyInfoValue(journal.AH_ChequeOrReferenceInfo, chequeValue, ForeignKeyType.None, NotificationManager.NotificationSubscriber, Res.GetString("01e40b8d-8541-413e-8505-1320533b08fc", "Cheque or Reference"));
		}

		void SetJournalDescription(Journal journal, Xsd.TxnHeader paidTxnHeader)
		{
			StringToBusinessObjectFieldConverter.InstanceForCurrentCompany.SetPropertyInfoValue(journal.AH_DescInfo, paidTxnHeader.Description, ForeignKeyType.None, NotificationManager.NotificationSubscriber, Res.GetString("f8d2b2c0-fbd7-4656-bb20-0e0e211cc443", "Transaction Description"));
		}

		void SetJournalAmountsForMissingTransaction(TransactionHeader journal, Xsd.TxnHeader paidTxnHeader)
		{
			var journalExchangeRate = journal.ExchangeRate.Rate;
			var ptrCurrency = paidTxnHeader.OsInvoiceAmtInclTax.CurrencyCode;
			var payCurrency = TxnHeader.OsInvoiceAmtInclTax.CurrencyCode;
			var currency = RefCurrency.LoadFromCurrencyCode(CurrentFactory, ptrCurrency.IsEmpty ? payCurrency : ptrCurrency);
			if (currency != null)
			{
				var osAMount = Utilities.Round(paidTxnHeader.OsInvoiceAmtInclTax.Value, currency.Decimals);
				var exchangeRateType = (TxnHeader.Ledger.ToString() == LedgerTypes.AccountsReceivable) ? ExchangeRateType.Sell : ExchangeRateType.Buy;

				if (currency.Code == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					journal.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

					journalExchangeRate = new ZDecimal(1m);

					if (!paidTxnHeader.LocalInvoiceAmtInclTax.Value.IsEmpty)
					{
						journal.AH_OSExTaxAmount = paidTxnHeader.LocalInvoiceAmtInclTax.Value;
					}
					else
					{
						journal.AH_OSExTaxAmount = osAMount;
					}
				}
				else if (paidTxnHeader.LocalInvoiceAmtInclTax.CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					journal.AH_RX_NKTransactionCurrency = currency.Code;
					journalExchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(paidTxnHeader.LocalInvoiceAmtInclTax.Value, paidTxnHeader.OsInvoiceAmtInclTax.Value);
					journal.AH_OSExTaxAmount = osAMount;
				}
				else if (paidTxnHeader.LocalInvoiceAmtInclTax.Value.IsEmpty)
				{
					journal.AH_RX_NKTransactionCurrency = currency.Code;
					journalExchangeRate = Env.CurrentCompany.ExchangeRate.TodaysRate(journal.AH_RX_NKTransactionCurrency, exchangeRateType);
					journal.AH_OSExTaxAmount = osAMount;
				}
			}

			if (journalExchangeRate.IsEmpty)
			{
				NotificationManager.AddErrorToNotifications(Res.GetString("61424A8A-1774-415c-A979-2D6CD073A158", "Exchange Rate For Currency '{0}' Is Not Set", journal.AH_RX_NKTransactionCurrency));
			}

			journal.AH_ExchangeRate = !journalExchangeRate.IsEmpty ? journalExchangeRate : new ZDecimal(1m);
		}

		IMatching CreateMiscellaneousTransaction(Xsd.TxnHeader txnHeader, MatchingBase matching)
		{
			var txnType = TxnHeaderMapper.GetBizObjTypeFromIValueObject(txnHeader);
			var result = (IMatching)null;
			var canCreateMiscTransaction = CanCreateMiscellaneousTransaction(txnType, matching);
			if (canCreateMiscTransaction)
			{
				var oSAmount = TxnHeaderMapper.GetDecimalFromXmlFinancialValue(CurrentFactory, txnHeader.OsInvoiceAmtExclTax, txnType);
				result = matching.GetMiscellaneousTransaction(txnHeader.TxnType.ToString(), oSAmount) as IMatching;
				var valueObjectImportContext = new ValueObjectImportContext(new BusinessObjectFactoryProvider(CurrentFactory), NotificationManager.NotificationSubscriber);
				var builder = new TransactionHeaderBuilder(NotificationManager, new TransactionBuilderConfig());

				if (result is Overpayment overpayment)
				{
					builder.SetValuesOnMiscTransactionBusinessObject(overpayment, txnHeader, valueObjectImportContext);
				}
				else if (result is Discount discount)
				{
					builder.SetValuesOnMiscTransactionBusinessObject(discount, txnHeader, valueObjectImportContext);
				}
				else if (result is ExchangeDifference exchangeDifference)
				{
					builder.SetValuesOnMiscTransactionBusinessObject(exchangeDifference, txnHeader, valueObjectImportContext);
				}
				else if (result is Journal journal)
				{
					builder.SetValuesOnBankFeeJournalBusinessObject(journal, txnHeader, valueObjectImportContext);
				}
			}

			return result;
		}

		bool CanCreateMiscellaneousTransaction(Type txnType, MatchingBase matching)
		{
			var result = true;

			if (txnType != null)
			{
				if (typeof(Journal).IsAssignableFrom(txnType) && matching.BankFeeTmp != null)
				{
					NotificationManager.AddErrorToNotifications(Res.GetString("BBC78119-4A5A-46CD-A51C-5B14364E6CB2", @"Only one Bank Fee Journal can be created per match group."));
					result = false;
				}
				else if (typeof(Overpayment).IsAssignableFrom(txnType))
				{
					if (TransactionHeader.AH_TransactionType == TransactionTypes.Payment)
					{
						NotificationManager.AddErrorToNotifications(Res.GetString("6B6C1759-CCBE-4DD2-A1C4-120B1EBABF38", @"Overpayment transaction cannot be created for payment match group."));
						result = false;
					}
					else if (matching.OverpaymentTmp != null)
					{
						NotificationManager.AddErrorToNotifications(Res.GetString("4E924063-0F73-4699-B655-5FF7197D7D79", @"Only one Overpayment transaction can be created per match group."));
						result = false;
					}
				}
				else if (typeof(ExchangeDifference).IsAssignableFrom(txnType) && matching.ExchangeDiffTmp != null)
				{
					NotificationManager.AddErrorToNotifications(Res.GetString("A6D811B1-168B-4D30-B64E-CEEDB4D0B02F", @"Only one Exchange Difference transaction can be created per match group."));
					result = false;
				}
				else if (typeof(Discount).IsAssignableFrom(txnType) && matching.DiscountTmp != null)
				{
					NotificationManager.AddErrorToNotifications(Res.GetString("DF62C970-D18F-4639-B7F9-F02E6C864E2F", @"Only one Discount transaction can be created per match group."));
					result = false;
				}
			}

			return result;
		}

		enum PaymentStatus
		{
			OverPaid,
			FullyPaid,
			InvoiceNotFound
		}

		List<string> MiscellaneousTransactionType => new List<string> { TransactionTypes.Overpayment, TransactionTypes.Discount, TransactionTypes.ExchangeDifference, TransactionTypes.Journal };

		#endregion

		#region Match

		void ProcessMatch(MatchingBase matching, Dictionary<BusinessObject, ZDecimal> transactionsToMatch, PaymentApprovalBase paymentApproval)
		{
			if (transactionsToMatch.Count > 0)
			{
				decimal amountBeingPaid = 0m;
				var matchingCurrency = TransactionHeader.AH_RX_NKTransactionCurrency;
				bool isMatchingCurrenciesMixed = false;
				transactionsToMatch.ForEach(x =>
					{
						if (x.Key is TransactionHeader transactionHeader && transactionHeader.AH_TransactionCreatedByMatching)
						{
							matching.AddMiscellaneousTransaction(transactionHeader);
						}
					}
				);

				var withoutMiscTransactionDictionary = transactionsToMatch.Where(x => x.Key is TransactionHeader transactionHeader && !transactionHeader.AH_TransactionCreatedByMatching).ToDictionary(x => x.Key, y => y.Value);
				matching.MoveFromUnmatchToMatch(withoutMiscTransactionDictionary);

				foreach (IMatching transaction in transactionsToMatch.Keys)
				{
					amountBeingPaid -= transaction.OSPartialPaymentAmount;
					if (matchingCurrency != transaction.CurrencyCode)
					{
						isMatchingCurrenciesMixed = true;
						break;
					}
				}

				var sumOSPartialPaymentAmount = 0M;
				var transactionNotifications = new List<Notification>();
				foreach (IMatching matchTransaction in matching.MatchedTransactions)
				{
					string matchTransactionErrorContext = GetContextMessage(matchTransaction);
					if (matchTransaction == ((IMatching)paymentApproval ?? (IMatching)TransactionHeader) && !IsMatchOnly)
					{
						if (!isMatchingCurrenciesMixed)
						{
							matchTransaction.OSPartialPaymentAmount = amountBeingPaid;
						}
						matchTransactionErrorContext = GetContextMessage(TxnHeader);
					}
					if (matchTransaction.HasErrors())
					{
						NotificationManager.AddErrorToNotifications(matchTransactionErrorContext);
						string notificationString = matchTransaction.Notifications.ToUniqueMessageListString().Replace("Error - ", "");
						NotificationManager.AddInfoNotification(notificationString);
						transactionNotifications.AddRange(matchTransaction.Notifications.Select(x => x as Notification));
					}

					sumOSPartialPaymentAmount += matchTransaction.OSPartialPaymentAmount;
				}

				if (matching.Balance != 0 && (isMatchingCurrenciesMixed || sumOSPartialPaymentAmount == 0M) && matching.ExchangeDifferenceAmount == 0)
				{
					matching.AddMiscellaneousTransaction(matching.GetMiscellaneousTransaction(TransactionTypes.ExchangeDifference));
				}

				matching.RunPreSaveValidation();

				//add matching errors
				if (matching.HasErrors || matching.Balance != 0m)
				{
					//get rid of duplicate errors made by matching validation
					var matchingNotifications = matching.NotificationsIncludingChildren.Select(x => x as Notification);
					string matchingError = matchingNotifications.Except(transactionNotifications).ToUniqueMessageListString();

					if (!string.IsNullOrEmpty(matchingError))
					{
						NotificationManager.AddErrorToNotifications(Res.GetString("2310f550-3a5a-45ff-bf58-02ed06f97583", "Matching session:"));
						NotificationManager.AddInfoNotification(matchingError.Replace("Error - ", ""));
					}
					else if (matching.Balance != 0m)
					{
						NotificationManager.AddErrorToNotifications(Res.GetString("3fda2adc-1224-41a5-bc7b-dcab221c7463", "Matching balance is not 0."));
					}
				}
				else if (!NotificationManager.ErrorsHaveBeenReported && !NotificationBuffer.HasErrors)
				{
					Action action = () =>
					{
						if (paymentApproval != null)
						{
							TransactionHeader.Delete();
							MatchAndClearTransactions(matching);

							var isAutoAllocationEnabled = ((IChequeNumberAutoAllocation)paymentApproval).IsAutoAllocationEnabled;
							PaymentApprovalAllocateCheckNumberAndSave(paymentApproval, isAutoAllocationEnabled);

							SetNewPaymentToTransactionHeaderAndUpdateHeaderReference(paymentApproval);
						}
						else
						{
							MatchAndClearTransactions(matching);
						}
						if (paymentApproval != null && paymentApproval.PaymentCreationErrorMessages.HasErrors())
						{
							NotificationManager.AddErrorToNotifications(Res.GetString("2310f550-3a5a-45ff-bf58-02ed06f97583", "Matching session:"));
							NotificationManager.AddInfoNotification(paymentApproval.PaymentCreationErrorMessages.ToMessageListString());
						}
						else if (TransactionHeader.IsInDatabase && !IsMatchOnly)
						{
							using (matching.MatchedTransactions.SuspendHeaderAmountsRecalculation())
							using (matching.DynamicTransactions.SuspendHeaderAmountsRecalculation())
							using (matching.UnmatchedTransactions.SuspendHeaderAmountsRecalculation())
							{
								TransactionHeader.Logs.AddNew(Events.DataImport);
								if (IsNeedSaveWithCurrentFactory)
								{
									TransactionHeader.Logs.Factory.Save();
								}
								AddReceiptAndPaymentToPrintList();
							}
						}
						NotificationManager.AddInfoNotification("  " + Res.GetString("5af1f130-2daa-454d-84e5-73b0e200e5ca", "Completed Processing Transaction."));
					};

					ProcessWithSaveExceptionHandling(action, NotificationManager.NotificationSubscriber);
				}
			}
			else
			{
				NotificationManager.AddErrorToNotifications(Res.GetString("eb094fa2-9df6-4654-ab9e-2b05d93b9cc3", "Matching failed. There are no matched transaction."));
			}
		}

		(MatchingBase matching, PaymentApprovalBase paymentApproval) GetMatching()
		{
			MatchingBase matching = null;
			PaymentApprovalBase paymentApproval = null;
			if (IsMatchOnly)
			{
				matching = TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable ?
					new APMatchingBase(CurrentFactory, false) :
					new ARMatchingBase(CurrentFactory, false);
				matching.PrimaryOrganisationForGUINotification = TransactionHeader.AH_OH;
				matching.PrimaryOrganization = TransactionHeader.AH_OH;
				matching.MatchDate = TxnHeader.FullyPaidDate;
			}
			else
			{
				if (TransactionHeader is Payment)
				{
					paymentApproval = TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable ?
						CurrentFactory.New<APPaymentApprovalWithoutAuthorisation>() :
						CurrentFactory.New<ARPaymentApprovalWithoutAuthorisation>();
					paymentApproval.SetFieldsFromPayment((Payment)TransactionHeader);
					paymentApproval.IsLoadedFromGUI = false;
					matching = paymentApproval.MatchingBaseObject;
				}
				else if (TransactionHeader is Receipt)
				{
					matching = TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable ?
						new APMatchingBase(CurrentFactory, (ReceiptPaymentBase)TransactionHeader, false) :
						new ARMatchingBase(CurrentFactory, (ReceiptPaymentBase)TransactionHeader, false);
				}
				else if (TransactionHeader is Journal)
				{
					var journal = TransactionHeader as Journal;
					matching = TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable ?
						new APMatchingBase(CurrentFactory, journal, false) :
						new ARMatchingBase(CurrentFactory, journal, false);

					journal.IsRecalculateExchangeRate = true;
				}
			}

			return (matching, paymentApproval);
		}

		protected virtual bool IsNeedSaveWithCurrentFactory => true;

		protected virtual void MatchAndClearTransactions(MatchingBase matching)
		{
			matching.MatchAndClearTransactions();
		}

		protected abstract void PaymentApprovalAllocateCheckNumberAndSave(PaymentApprovalBase paymentApproval, bool isAutoAllocationEnabled);

		protected virtual void SetNewPaymentToTransactionHeaderAndUpdateHeaderReference(PaymentApprovalBase paymentApproval)
		{
			TransactionHeader = paymentApproval.NewPayment;
		}

		protected virtual void AddReceiptAndPaymentToPrintList() { }

		public Action<Action, INotifications> ProcessWithSaveExceptionHandling { get; set; }

		#endregion

		#region Helper Methods

		ReadOnlyBusinessObjectFactory CurrentReadonlyFactory => currentReadonlyFactory ?? (currentReadonlyFactory = new ReadOnlyBusinessObjectFactory());
		ReadOnlyBusinessObjectFactory currentReadonlyFactory;

		ZGuid FindOrgPK(Xsd.TxnHeader txnHeader)
		{
			var orgType = txnHeader.Ledger == Xsd.TxnLedgerType.AP ? OrganisationTypes.Creditor : OrganisationTypes.Debtor;
			var organisation = new ValueObjectImportContext(new BusinessObjectFactoryProvider(CurrentReadonlyFactory), NotificationBuffer).FindOrganisation(txnHeader.DebtorOrCreditor, null, orgType);
			return (organisation != null && (orgType == OrganisationTypes.Creditor && organisation.OH_IsCreditor ||
				orgType == OrganisationTypes.Debtor && organisation.OH_IsDebtor)) ? organisation.PK : ZGuid.Empty;
		}

		void AddErrorForTransactionNotExist(Xsd.TxnHeader paidTxnHeader)
		{
			var invoiceErrorContext = Res.GetString("e10ce735-db57-46de-a67a-cbeb687ee63c", "Paid Transaction {0} {1} {2}:", paidTxnHeader.Ledger.ToString(), paidTxnHeader.TxnType.ToString(), paidTxnHeader.TxnNumber) + " ";
			NotificationManager.AddErrorToNotifications(invoiceErrorContext + Res.GetString("513490ae-2e91-49a5-aa31-0bc6e1bc79a2", "Matching failed. Transaction was not found."));
		}

		TransactionHeader[] FindTransactions(BusinessObjectFactory factory, IEnumerable<Xsd.TxnHeader> paidTransactionsSorted, ZGuid transactionHeaderOrg, Dictionary<Xsd.TxnHeader, ZGuid> foundOrgs)
		{
			var aPtransactionNumbers = new HashSet<string>();
			var aRtransactionNumbers = new HashSet<string>();
			var aPtransactionTypes = new HashSet<string>();
			var aRtransactionTypes = new HashSet<string>();

			foreach (var item in paidTransactionsSorted)
			{
				if (item.Ledger == Xsd.TxnLedgerType.AR)
				{
					aRtransactionNumbers.Add(item.TxnNumber);
					aRtransactionTypes.Add(item.TxnType.ToString());
				}
				else if (item.Ledger == Xsd.TxnLedgerType.AP)
				{
					aPtransactionNumbers.Add(item.TxnNumber);
					aPtransactionTypes.Add(item.TxnType.ToString());
				}
				else
				{
					NotificationManager.AddErrorToNotifications(Res.GetString("65F790B9-3504-4F83-93BE-AD9E2E2FA678", "Invalid ledger: '{0}'", item.Ledger));
					return null;
				}
			}

			var transactionQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery arTransactionFilter = null;
			ZDBOnlySubQuery apTransactionFilter = null;

			if (aRtransactionNumbers.Any())
			{
				arTransactionFilter = GetTransactionFilter(LedgerTypes.AccountsReceivable, aRtransactionTypes, aRtransactionNumbers, filterOutstandingTransactionsOnly: true);
			}
			if (aPtransactionNumbers.Any())
			{
				apTransactionFilter = GetTransactionFilter(LedgerTypes.AccountsPayable, aPtransactionTypes, aPtransactionNumbers, filterOutstandingTransactionsOnly: true, transactionHeaderOrg: transactionHeaderOrg, foundOrgs: foundOrgs);

				arTransactionFilter?.AddAsUnionQuery(apTransactionFilter, addAsUnionAll: true);
			}
			transactionQuery.AddSubQuery(arTransactionFilter ?? apTransactionFilter, JoinCondition.And);

			return factory.Load<TransactionHeader>(transactionQuery);
		}

		ZDBOnlySubQuery GetChildOrganizationFilter(ZGuid transactionHeaderOrg)
		{
			var childOrgSubQuery = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);
			childOrgSubQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, transactionHeaderOrg);
			childOrgSubQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.APSettlementGroup);
			childOrgSubQuery.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);

			return childOrgSubQuery;
		}

		static IMatching GetMatchedTransaction(BusinessObjectFactory factory, Dictionary<BusinessObject, ZDecimal> transactionsToMatch, ZQuery query)
		{
			IMatching transaction = null;
			var transactionPKToExclude = transactionsToMatch.Keys.Select(x => x.PK);

			var target = factory.Load<TransactionHeader>(query).FirstOrDefault(x => !transactionPKToExclude.Contains(x.PK));
			if (target != null)
			{
				transaction = target as IMatching;
			}

			return transaction;
		}

		static ZQuery GetDebtorOrCreditorFilter(ZGuid payRecOrgPK, ZGuid pTRorgPK, List<ZGuid> childOrgPKs)
		{
			var debtorOrCreditorFilter = new ZQuery();
			var orgPks = new List<ZGuid>();
			if (!pTRorgPK.IsEmpty)
			{
				orgPks.Add(pTRorgPK);
			}
			else
			{
				orgPks.Add(payRecOrgPK);
			}
			if (childOrgPKs != null)
			{
				orgPks.AddRange(childOrgPKs);
			}
			debtorOrCreditorFilter.AddToFilter(AccTransactionHeaderSchema.AH_OH, orgPks);
			return debtorOrCreditorFilter;
		}

		#endregion

#if DEBUG
		public BusinessObjectFactory TestOnlyFirstFactoryUsed;

		public static IMatching GetMatchedTransaction_ForTest(BusinessObjectFactory factory, Dictionary<BusinessObject, ZDecimal> transactionsToMatch, ZQuery query)
		{
			return GetMatchedTransaction(factory, transactionsToMatch, query);
		}

		public static ZQuery GetDebtorOrCreditorFilter_ForTest(ZGuid payRecOrgPK, ZGuid pTRorgPK, List<ZGuid> childOrgPKs)
		{
			return GetDebtorOrCreditorFilter(payRecOrgPK, pTRorgPK, childOrgPKs);
		}

		public ZDBOnlySubQuery GetTransactionFilter_ForTest(string ledger, HashSet<string> transactionTypes, HashSet<string> transactionNumbers, ZBool filterOutstandingTransactionsOnly, ZGuid transactionHeaderOrg, Dictionary<Xsd.TxnHeader, ZGuid> foundOrgs)
		{
			return GetTransactionFilter(ledger, transactionTypes, transactionNumbers, filterOutstandingTransactionsOnly, transactionHeaderOrg, foundOrgs);
		}

		public INotificationManager NotificationManager_ForTest
		{
			get { return NotificationManager; }
			set { NotificationManager = value; }
		} 

		[ThreadStatic]
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		public static bool SimulateSavingFailure;
#endif

	}
}

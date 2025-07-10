using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class AccountingJournalTest : NonPersistentBusinessObjectTestCase
	{
		public virtual void TestProperties()
		{
			SetupData();
			AssertTransaction();
			AssertLedger();
			AssertTransactionType();
			AssertTransactionDescription();
			AssertCurrency();
			AssertLocalCurrencyDecimals();
			AssertJob();
			AssertJournalName();
			AssertCreatedDate();
			AssertCreatedBy();
			AssertPostToPeriod();
			AssertRelatedGLJournals();
			AssertRelatedGLJournalEntriesNumber();
		}

		public abstract void TestAJOptionalFields();
		public abstract void TestAccountingJournalLines();

		protected ReadOnlyBusinessObjectFactory ReadonlyFactory
		{
			get { return readonlyFactory ?? (readonlyFactory = Factory.GetCachedReadOnlyFactory()); }
		}
		ReadOnlyBusinessObjectFactory readonlyFactory;

		protected virtual void AssertTransaction()
		{
			AssertNotNull("Underlying Transaction", transaction);
		}

		protected virtual void AssertLedger()
		{
			AssertEquals("Ledger", transaction.AH_Ledger, accountingJournal.Ledger);
		}

		protected virtual void AssertTransactionType()
		{
			AssertEquals("TransactionType", transaction.AH_TransactionType, accountingJournal.TransactionType);
		}

		protected virtual void AssertTransactionNumber()
		{
			AssertEquals("TransactionNumber", transaction.AH_TransactionNum, accountingJournal.TransactionNumber);
		}

		protected virtual void AssertTransactionDescription()
		{
			AssertEquals("TransactionDescription", transaction.AH_Desc, accountingJournal.TransactionDescription);
		}

		protected virtual void AssertCurrency()
		{
			AssertEquals("Currency", transaction.TransactionCurrency.RX_Code, accountingJournal.Currency);
		}

		protected virtual void AssertLocalCurrencyDecimals()
		{
			using (MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("LocalCurrencyDecimals", 2, accountingJournal.LocalCurrencyDecimals);
			}
		}

		protected virtual void AssertJob()
		{
			AssertEquals("Job", transaction.Job, accountingJournal.Job);
		}

		protected virtual void AssertJournalName()
		{
			AssertEquals("JournalName", string.Format("{0} {1} {2}", transaction.AH_Ledger, transaction.AH_TransactionType, transaction.AH_TransactionNum), accountingJournal.JournalName);
		}

		protected virtual void AssertRelatedGLJournals()
		{
			AssertEquals("RelatedGLJournals", transaction.RelatedGLJournals, accountingJournal.RelatedGLJournals);
		}

		protected void AssertRelatedGLJournalEntriesNumber()
		{
			AssertEquals("RelatedGLJournalEntriesNumber should be empty.", string.Empty, accountingJournal.JournalEntriesNumber);
		}

		protected virtual void AssertPostToPeriod()
		{
			AssertEquals("PostToPeriod", transaction.PostPeriod.ToString(), accountingJournal.PostToPeriod);
		}

		protected virtual void AssertCreatedDate()
		{
			AssertEquals("CreatedDate", transaction.CreatedDate, accountingJournal.CreatedDate);
		}

		protected virtual void AssertCreatedBy()
		{
			AssertEquals("CreatedBy", transaction.CreatingUser, accountingJournal.CreatedBy);
		}

		protected virtual void AssertSourceIdentifier()
		{
			AssertEquals("SourceIdentifier", transaction.PK, accountingJournal.SourceIdentifier);
		}

		protected void AssertAJLine(AccountingJournalLine ajline, ZGuid chargeCodePK, ZGuid gLAccountPK, ZString accountDescription, ZString multiSubAccountTypeCode, ZDateTime postDate, ZInt postPeriod, ZString recognitionType, ZDecimal amount, string cashBasis, ZGuid branchPK, ZGuid deptPK)
		{
			AssertNotNull(ajline);
			AssertEquals("Charge Code", chargeCodePK, ajline.AL_AC);
			AssertEquals("GL Account", gLAccountPK, ajline.AL_AG);
			AssertEquals("Account Description", accountDescription, ajline.AL_Desc);
			AssertEquals("Multi Sub Account Type & Code", multiSubAccountTypeCode, ajline.MultiSubAccountTypeCode);
			AssertEquals("Post Date", postDate, ajline.AL_PostDate);
			AssertEquals("Post Period", postPeriod, ajline.AL_PostPeriod);
			AssertEquals("Rev. Recognition Type", recognitionType, ajline.AL_RevRecognitionType);
			AssertEquals("Amount", amount, ajline.AL_LineAmount);
		}

		public virtual void TestTransactionReference()
		{
			accountingJournal = CreateTestAccountingJournal();
			AssertNotNull("transaction", transaction);
			transaction.AH_TransactionReference = "";
			AssertEquals("TransactionReference", ZString.Empty, accountingJournal.TransactionReference);
			transaction.AH_TransactionReference = "11222334455";
			AssertEquals("TransactionReference", "11222334455", accountingJournal.TransactionReference);
		}

		public virtual void TestConsolidatedInvoiceRef()
		{
			accountingJournal = CreateTestAccountingJournal();
			AssertNotNull("transaction", transaction);
			transaction.AH_ConsolidatedInvoiceRef = "";
			AssertEquals("ConsolidatedInvoiceRef", ZString.Empty, accountingJournal.ConsolidatedInvoiceRef);
			transaction.AH_ConsolidatedInvoiceRef = "S00011111/A";
			AssertEquals("ConsolidatedInvoiceRef", "S00011111/A", accountingJournal.ConsolidatedInvoiceRef);
		}

		public virtual void TestComplianceSubType()
		{
			accountingJournal = CreateTestAccountingJournal();
			AssertNotNull("transaction", transaction);
			transaction.AH_ComplianceSubType = "";
			AssertEquals("ComplianceSubType", ZString.Empty, accountingJournal.ComplianceSubType);
			transaction.AH_ComplianceSubType = "TXI";
			AssertEquals("ComplianceSubType", "TXI", accountingJournal.ComplianceSubType);
		}

		public void TestReportingBook()
		{
			var accountingJournal = CreateTestAccountingJournal();
			accountingJournal.reportingBook = CreateReportingBook();
			AssertEquals("TRR", accountingJournal.ReportingBookCode);
			AssertEquals("book", accountingJournal.ReportingBookDescription);
			AssertEquals("MGT", accountingJournal.ChartCode);
			AssertEquals("chart", accountingJournal.ChartDescription);
			Assert(!accountingJournal.DisplayParentAccount);

			var reportingBookAccountingJournalPrintOptionCollection = new ReportingBookAccountingJournalPrintOptionCollection();
			var reportingBookAccountingJournalPrintOption = reportingBookAccountingJournalPrintOptionCollection.AddNew();
			reportingBookAccountingJournalPrintOption.ReportingBook = accountingJournal.ReportingBook.PK;
			reportingBookAccountingJournalPrintOption.Default = true;
			reportingBookAccountingJournalPrintOption.DisplayParentAccount = true;
			using (AccountingMasterFilesRegistry.Instance.ReportingBookAccountingJournalPrintOption.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, reportingBookAccountingJournalPrintOptionCollection))
			{
				Assert(accountingJournal.DisplayParentAccount);
			}
		}

		AccReportingBook CreateReportingBook()
		{
			var chart = Creator.CreateAlternateChart("MGT", "chart");
			Factory.Save();
			var reportingBook = Creator.CreateReportingBook("TRR", "book", chart.PK, "");
			Factory.Save();
			return reportingBook;
		}

		protected AccountingJournal GetJournalForMultiSubAccountTypeCode(BusinessObject bizo, ZString ledgerType, ZString transactionType, bool shouldSupportMultiSub = true)
		{
			AssertEquals(ledgerType, ((TransactionHeader)bizo).AH_Ledger);
			AssertEquals(transactionType, ((TransactionHeader)bizo).AH_TransactionType);

			ISupportMultiSubAccounts supportMultiSub = null;
			AccountingJournal journal = null;

			var headerWithLines = bizo as TransactionHeaderWithLines;
			if (headerWithLines != null && headerWithLines.Lines.Count > 0)
			{
				supportMultiSub = headerWithLines.Lines[0];
			}
			else if (headerWithLines == null)
			{
				supportMultiSub = bizo as ISupportMultiSubAccounts;
			}

			if (shouldSupportMultiSub)
			{
				AssertNotNull("Per-conditon", supportMultiSub);
				AssertNotNull("Per-conditon", supportMultiSub.GLHeader);
				AssertNotNull("Per-conditon", Creator.AR1);
				AssertNotNull("Per-conditon", Creator.ABIGAS);
				AssertNotNull("Per-conditon", Creator.GS1);
				AssertNotNull("Per-conditon", Creator.GG1);

				supportMultiSub.GLHeader.SubAccountTypes.RemoveAndDeleteAll();
				Creator.CreateGLHeaderSubAccount(supportMultiSub.GLHeader, OrgHeaderSchema.Constants.Prefix, false);
				Creator.CreateGLHeaderSubAccount(supportMultiSub.GLHeader, AccGroupsSchema.Constants.Prefix, false);
				Creator.CreateGLHeaderSubAccount(supportMultiSub.GLHeader, GlbStaffSchema.Constants.Prefix, false);
				Creator.CreateGLHeaderSubAccount(supportMultiSub.GLHeader, GlbGroupSchema.Constants.Prefix, false);
				Factory.Save();

				SetupSubAccount(supportMultiSub.SubAccounts, AccGroupsSchema.Constants.Prefix, Creator.AR1.PK);
				SetupSubAccount(supportMultiSub.SubAccounts, OrgHeaderSchema.Constants.Prefix, Creator.ABIGAS.PK);
				SetupSubAccount(supportMultiSub.SubAccounts, GlbStaffSchema.Constants.Prefix, Creator.GS1.PK);
				SetupSubAccount(supportMultiSub.SubAccounts, GlbGroupSchema.Constants.Prefix, Creator.GG1.PK);

				journal = (AccountingJournal)Activator.CreateInstance(GetExpectedBusinessObjectType(), new object[] { bizo as TransactionHeader, ReadonlyFactory, null, null });
				AssertNotNull(journal.Lines.FirstOrDefault(x => x.AL_AG.Equals(supportMultiSub.GLHeader.PK) && x.MultiSubAccountTypeCode.Equals("ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1")));
			}
			else
			{
				Assert(supportMultiSub == null || !supportMultiSub.IsMultiSubAccountsSupported);
			}

			return journal;
		}

		protected void SetupSubAccount(ISupportSubAccountCollection subAccounts, ZString subAccountTypeParentTableCode, ZGuid subAccountParentId)
		{
			var subAccount = subAccounts.AddNew();
			subAccount.SubAccountTypeParentTableCode = subAccountTypeParentTableCode;
			subAccount.SubAccountParentId = subAccountParentId;
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.GSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
		}

		void SetupData()
		{
			accountingJournal = CreateTestAccountingJournal();
			Factory.Save();
		}

		protected abstract AccountingJournal CreateTestAccountingJournal();

		protected AccountingJournal accountingJournal;
		protected TransactionHeader transaction;

		protected TestObjectCreator Creator
		{
			get
			{
				if (creator == null)
				{
					creator = new TestObjectCreator(Factory);
				}
				return creator;
			}
		}
		TestObjectCreator creator;
	}

	[TestedType(typeof(AccountingJournalDocumentSupporter))]
	public class AccountingJournalDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestDocumentSupporter()
		{
			var daj = new DummyAccountingJournal(DummyAccountingJournal.GetSampleTransaction(), Factory.GetCachedReadOnlyFactory());
			AssertEquals("Document Supporter should be of type", typeof(AccountingJournalDocumentSupporter), daj.DocumentSupporter.GetType());
		}

		public void TestSupportedDataContext()
		{
			AssertEquals("DataContext AccountingJournal is supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(Core.Constants.DataContext.AccountingJournal))));
			AssertEquals("DataContext GenericFreightJob is supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(Core.Constants.DataContext.GenericFreightJob))));
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.ARTransaction, DocumentSupporter.BusinessContext);
		}

		public new void TestRunningDocumentsShouldNotCauseException()
		{
			Assert(true);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return new DummyAccountingJournal(DummyAccountingJournal.GetSampleTransaction(), Factory.GetCachedReadOnlyFactory());
		}

		AccountingJournalDocumentSupporter DocumentSupporter
		{
			get
			{
				return new AccountingJournalDocumentSupporter(new DummyAccountingJournal(DummyAccountingJournal.GetSampleTransaction(), Factory.GetCachedReadOnlyFactory()));
			}
		}
	}

	#region Helper Class

	public class DummyAccountingJournal : AccountingJournal
	{
		public DummyAccountingJournal(TransactionHeader transaction, ReadOnlyBusinessObjectFactory factory)
			: base(transaction, factory)
		{
		}

		public override Dictionary<ZString, ZString> ApplicableOptionalFields
		{
			get { return new Dictionary<ZString, ZString>(); }
		}

		protected override IEnumerable<ZString> ValidLedgerTypes
		{
			get
			{
				return Array.Empty<ZString>();
			}
		}

		protected override IEnumerable<ZString> ValidTransactionTypes
		{
			get
			{
				return Array.Empty<ZString>();
			}
		}

		public static TransactionHeader GetSampleTransaction()
		{
			var objectCreator = new TestObjectCreator(new BusinessObjectFactory());
			var glJournal = objectCreator.CreateGLJournal("GJL", ZDateTime.Today, ZDateTime.Today);
			objectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, objectCreator.GLHeader1.PK);
			objectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, objectCreator.GLHeader2.PK);
			objectCreator.Factory.Save();
			return glJournal;
		}
	}

	#endregion
}

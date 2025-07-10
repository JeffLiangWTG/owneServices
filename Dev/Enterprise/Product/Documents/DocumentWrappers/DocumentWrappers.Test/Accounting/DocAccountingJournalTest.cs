using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class DocAccountingJournalTest : DocBaseWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return DocAccountingJournal.New(new DummyAccountingJournal(DummyAccountingJournal.GetSampleTransaction(), Factory.GetCachedReadOnlyFactory()), Factory);
		}

		public void TestPostToPeriod()
		{
			var accountingJournal = new DummyAccountingJournal(DummyAccountingJournal.GetSampleTransaction(), Factory.GetCachedReadOnlyFactory());
			var wrapper = DocAccountingJournal.New(accountingJournal, Factory);
			AssertEquals(accountingJournal.PostToPeriod, wrapper.PostToPeriod);
		}

		public void TestGetRelatedGLJournalEntriesNumber()
		{
			var accountingJournal = new DummyAccountingJournal(DummyAccountingJournal.GetSampleTransaction(), Factory.GetCachedReadOnlyFactory());
			var wrapper = DocAccountingJournal.New(accountingJournal, Factory);
			AssertEquals(accountingJournal.JournalEntriesNumber, wrapper.JournalEntriesNumber);
		}

		public void TestCurrentCompanyCurrencyDecimalPlaces()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var accountingJournal = new DummyAccountingJournal(DummyAccountingJournal.GetSampleTransaction(), Factory.GetCachedReadOnlyFactory());
				var wrapper = DocAccountingJournal.New(accountingJournal, Factory);
				AssertEquals(2, wrapper.CurrentCompanyCurrencyDecimalPlaces);
			}
		}

		public void TestRelatedJournals()
		{
			var table = CreateGeneralLedgerTransactionData();
			var creator = new TestObjectCreator(Factory);
			var invoice = creator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, creator.AALSHI, creator.GLHeader2.PK, "FIN");
			var journal1 = Factory.NewWithValidTestData<GLJournal>();
			journal1.AH_TransactionType = TransactionTypes.GLStandardJournal;
			journal1.AH_TransactionBelongsToGroup = invoice.PK;
			var line = Factory.NewWithValidTestData<GLJournalLine>();
			line.AL_AH = journal1.PK;
			PopulateDataRow(table, line.PK, journal1.PK, line.GLHeader.AG_AccountNum, "111", "A");
			var journal2 = Factory.NewWithValidTestData<GLJournal>();
			journal2.AH_TransactionType = TransactionTypes.GLStandardJournal;
			journal2.AH_TransactionBelongsToGroup = invoice.PK;
			line = Factory.NewWithValidTestData<GLJournalLine>();
			line.AL_AH = journal2.PK;
			PopulateDataRow(table, line.PK, journal2.PK, line.GLHeader.AG_AccountNum, "222", "B");
			Factory.Save();

			var accountingJournal = new DummyAccountingJournal(invoice, Factory.GetCachedReadOnlyFactory(), true);
			accountingJournal.reportingBook = CreateReportingBook();
			accountingJournal.generalLedgerTransactionData = new DataTable();
			var wrapper = DocAccountingJournal.New(accountingJournal, Factory);
			var supporter = ((IGenericTransactionHeaderPlugIn)wrapper).HeaderSupporter;
			var relatedJournals = supporter.GetRelatedJournals();
			AssertEquals(2, relatedJournals.Count);
			AssertEquals(journal1.AH_TransactionNum, relatedJournals[0].TransactionNumber);
			AssertEquals(journal2.AH_TransactionNum, relatedJournals[1].TransactionNumber);
			AssertEquals(accountingJournal.ReportingBookCode, relatedJournals[0].ReportingBookCode);
			AssertEquals(accountingJournal.ReportingBookCode, relatedJournals[1].ReportingBookCode);
			AssertEquals(accountingJournal.ReportingBookDescription, relatedJournals[0].ReportingBookDescription);
			AssertEquals(accountingJournal.ReportingBookDescription, relatedJournals[1].ReportingBookDescription);
			AssertEquals(accountingJournal.ChartCode, relatedJournals[0].ChartCode);
			AssertEquals(accountingJournal.ChartCode, relatedJournals[1].ChartCode);
			AssertEquals(accountingJournal.ChartDescription, relatedJournals[0].ChartDescription);
			AssertEquals(accountingJournal.ChartDescription, relatedJournals[1].ChartDescription);
			AssertEquals(accountingJournal.DisplayParentAccount, relatedJournals[0].DisplayParentAccount);
			AssertEquals(accountingJournal.DisplayParentAccount, relatedJournals[1].DisplayParentAccount);
		}

		AccReportingBook CreateReportingBook()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			Factory.Save();
			var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
			reportingBook.ARB_Code = "TRR";
			reportingBook.ARB_Description = "Description";
			reportingBook.ARB_AAC_AlternateChart = chart.PK;
			Factory.Save();
			return reportingBook;
		}

		DataTable CreateGeneralLedgerTransactionData()
		{
			var dataTable = new DataTable("GeneralLedgerTransactionData");
			dataTable.Locale = CultureInfo.InvariantCulture;
			dataTable.Columns.Add("AlternateGLAccountDescription", typeof(string));
			dataTable.Columns.Add("AlternateGLAccount", typeof(string));
			dataTable.Columns.Add("GLAccountNum", typeof(string));
			dataTable.Columns.Add("TransactionHeaderID", typeof(Guid));
			dataTable.Columns.Add("TransactionLineID", typeof(Guid));
			return dataTable;
		}

		void PopulateDataRow(DataTable table, ZGuid lineID, ZGuid headerID, ZString glAccountNum, ZString alternateGLAccount, ZString desc)
		{
			var row = table.NewRow();
			row["TransactionLineID"] = lineID.ToGuid();
			row["TransactionHeaderID"] = headerID.ToGuid();
			row["GLAccountNum"] = glAccountNum;
			row["AlternateGLAccount"] = alternateGLAccount;
			row["AlternateGLAccountDescription"] = desc;
		}

		public void TestTaxDetails()
		{
			var journalTaxDetails1Mock = new Mock<IAccountingJournalTaxDetail>();
			journalTaxDetails1Mock.Setup(x => x.Amount).Returns(120M);
			var journalTaxDetails2Mock = new Mock<IAccountingJournalTaxDetail>();
			journalTaxDetails2Mock.Setup(x => x.Amount).Returns(0M);
			var journalTaxDetails3Mock = new Mock<IAccountingJournalTaxDetail>();
			journalTaxDetails3Mock.Setup(x => x.Amount).Returns(-100M);

			var accountingJournalTaxDetailCollection = new List<IAccountingJournalTaxDetail>();
			accountingJournalTaxDetailCollection.Add(journalTaxDetails1Mock.Object);
			accountingJournalTaxDetailCollection.Add(journalTaxDetails2Mock.Object);
			accountingJournalTaxDetailCollection.Add(journalTaxDetails3Mock.Object);

			var accountingJournalMock = new Mock<IAccountingJournal>();
			accountingJournalMock.Setup(x => x.Factory).Returns(Factory);
			accountingJournalMock.Setup(x => x.TaxDetails).Returns(accountingJournalTaxDetailCollection);

			var wrapper = DocAccountingJournal.New(accountingJournalMock.Object, Factory);
			var supporter = ((IGenericTransactionHeaderPlugIn)wrapper).HeaderSupporter;
			var docTaxDetailsCollection = supporter.GetTaxDetails();

			AssertEquals("2 items in Tax Details Collection", 2, docTaxDetailsCollection.Count);
			AssertEquals(120M, docTaxDetailsCollection[0].DebitAmountDecimal);
			AssertEquals(0M, docTaxDetailsCollection[0].CreditAmountDecimal);

			AssertEquals(0M, docTaxDetailsCollection[1].DebitAmountDecimal);
			AssertEquals(100M, docTaxDetailsCollection[1].CreditAmountDecimal);
		}

		public void TestGetTransactionReference()
		{
			var mockAndSupporter = SetupAndGetMockAndSupporter();
			mockAndSupporter.mock.Setup(x => x.TransactionReference).Returns("11222334455");

			var transactionReference = mockAndSupporter.supporter.GetTransactionReference();
			AssertEquals("TransactionReference", "11222334455", transactionReference);
		}

		public void TestGetConsolidatedInvoiceRef()
		{
			var mockAndSupporter = SetupAndGetMockAndSupporter();
			mockAndSupporter.mock.Setup(x => x.ConsolidatedInvoiceRef).Returns("S00011111/A");

			var consolidatedInvoiceRef = mockAndSupporter.supporter.GetConsolidatedInvoiceRef();
			AssertEquals("ConsolidatedInvoiceRef", "S00011111/A", consolidatedInvoiceRef);
		}

		public void TestGetComplianceSubType()
		{
			var mockAndSupporter = SetupAndGetMockAndSupporter();
			mockAndSupporter.mock.Setup(x => x.ComplianceSubType).Returns("TXI");

			var complianceSubType = mockAndSupporter.supporter.GetComplianceSubType();
			AssertEquals("ComplianceSubType", "TXI", complianceSubType);
		}

		(Mock<IAccountingJournal> mock, GenericTransactionHeaderSupporter supporter) SetupAndGetMockAndSupporter()
		{
			var mock = new Mock<IAccountingJournal>();
			mock.Setup(x => x.Factory).Returns(Factory);

			var wrapper = DocAccountingJournal.New(mock.Object, Factory);
			var supporter = ((IGenericTransactionHeaderPlugIn)wrapper).HeaderSupporter;
			return (mock, supporter);
		}
	}

	public class DummyAccountingJournal : AccountingJournal
	{
		public DummyAccountingJournal(TransactionHeader transaction, ReadOnlyBusinessObjectFactory factory, bool skipPopulateJournalLines = false)
			: base(transaction, factory)
		{
			if (!skipPopulateJournalLines)
			{
				PopulateJournalLines();
			}
		}

		public override Dictionary<ZString, ZString> ApplicableOptionalFields
		{
			get { return new Dictionary<ZString, ZString>(); }
		}

		protected override IEnumerable<ZString> ValidLedgerTypes
		{
			get { return new ZString[] { LedgerTypes.General }; }
		}

		protected override IEnumerable<ZString> ValidTransactionTypes
		{
			get { return new ZString[] { TransactionTypes.GLStandardJournal }; }
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
}

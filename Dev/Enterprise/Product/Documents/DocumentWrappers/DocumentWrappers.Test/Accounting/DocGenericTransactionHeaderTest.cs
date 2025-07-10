using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class DocGenericTransactionHeaderTest : DocBaseWrapperTest
	{
		#region Construction

		public void TestNew()
		{
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(Factory.NewWithValidTestData<APPayment>(), Factory).GetType());
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(Factory.NewWithValidTestData<DirectPayment>(), Factory).GetType());
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(Factory.NewWithValidTestData<BankTransferFromRow>(), Factory).GetType());
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(Factory.NewWithValidTestData<BankTransferToRow>(), Factory).GetType());
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>(), Factory).GetType());
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(Factory.NewWithValidTestData<GLJournal>(), Factory).GetType());

			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(Factory.NewWithValidTestData<APPayment>(), Factory, GetFreightWrapper()).GetType());
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(Factory.NewWithValidTestData<DirectPayment>(), Factory, GetFreightWrapper()).GetType());
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(Factory.NewWithValidTestData<BankTransferFromRow>(), Factory, GetFreightWrapper()).GetType());
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(Factory.NewWithValidTestData<BankTransferToRow>(), Factory, GetFreightWrapper()).GetType());
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>(), Factory, GetFreightWrapper()).GetType());
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(Factory.NewWithValidTestData<GLJournal>(), Factory, GetFreightWrapper()).GetType());
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(Factory.NewWithValidTestData<JobRevenueJournal>(), Factory, GetFreightWrapper()).GetType());
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(Factory.NewWithValidTestData<AccCashAdvanceRequestHeader>(), Factory).GetType());
			AssertEquals("The new method should return should be of type", typeof(DocGenericTransactionHeader), DocGenericTransactionHeader.New(new DummyAccountingJournal(DummyAccountingJournal.GetSampleTransaction(), Factory.GetCachedReadOnlyFactory()), Factory, GetFreightWrapper()).GetType());
		}

		public void TestGetTransactionReference()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Debtor);
			invoice.AH_TransactionReference = "111111";

			var header = DocGenericTransactionHeader.New(invoice, Factory, GetFreightWrapper());
			AssertEquals("111111", header.TransactionReference);
		}

		public void TestGetConsolidatedInvoiceRef()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Debtor);
			invoice.AH_ConsolidatedInvoiceRef = "S00111111";

			var header = DocGenericTransactionHeader.New(invoice, Factory, GetFreightWrapper());
			AssertEquals("ConsolidatedInvoiceRef", "S00111111", header.ConsolidatedInvoiceRef);
		}

		public void TestGetComplianceSubType()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR0001000", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Debtor);
			invoice.AH_ComplianceSubType = "TXI";

			var header = DocGenericTransactionHeader.New(invoice, Factory, GetFreightWrapper());
			AssertEquals("ComplianceSubType not implemented for Invoice", ZString.Empty, header.ComplianceSubType);
		}

		public void TestCashAdvanceRequestRelatedProperties()
		{
			var cashAdvanceRequest = Factory.New<AccCashAdvanceRequestHeader>();
			cashAdvanceRequest.CAH_RequestReferenceNumber = "00001001";
			cashAdvanceRequest.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
			cashAdvanceRequest.CAH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			cashAdvanceRequest.CAH_OSAmount = 100m;
			cashAdvanceRequest.CAH_LocalAmount = 75m;
			cashAdvanceRequest.CAH_OSPaidAmount = 60m;
			cashAdvanceRequest.CAH_LocalPaidAmount = 45m;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			org.OH_FullName = "ABC PTY LTD";
			cashAdvanceRequest.CAH_OH_Organization = org.PK;
			var docHeader = DocGenericTransactionHeader.New(cashAdvanceRequest, Factory);
			AssertEquals("00001001", docHeader.TransactionNumber);
			AssertEquals("PAI", docHeader.Status);
			AssertEquals("USD", docHeader.CurrencyCode);
			AssertEquals("OSAmount", 100m, docHeader.TotalOSAmount);
			AssertEquals("OSPaidAmount", 60m, docHeader.TotalOSPaidAmount);
			AssertEquals("LocalAmount", 75m, docHeader.TotalLocalAmount);
			AssertEquals("LocalPaidAmount", 45m, docHeader.TotalLocalPaidAmount);
			AssertEquals("Organisation Code", "ABC", docHeader.Organisation.Code);
			AssertEquals("Organisation Name", "ABC PTY LTD", docHeader.Organisation.Name);
		}

		public void TestCurrentCompanyReciprocal()
		{
			var creator = new TestObjectCreator(Factory);
			creator.SetCurrentCompanyReciprocal(true);
			DirectPayment directPayment = Factory.New<DirectPayment>();
			DocGenericTransactionHeader header = DocGenericTransactionHeader.New(directPayment, Factory, GetFreightWrapper());
			AssertEquals(header.CurrentCompanyReciprocal, 6M);
			creator.SetCurrentCompanyReciprocal(false);
			AssertEquals(header.CurrentCompanyReciprocal, 6M);
		}

		public void TestCurrentCompanyCurrencyDecimalPlaces()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var glJournal = Factory.New<GLJournal>();
				var header = DocGenericTransactionHeader.New(glJournal, Factory, GetFreightWrapper());
				AssertEquals(2, header.CurrentCompanyCurrencyDecimalPlaces);
			}
		}

		public void TestDefaultCurrentCompanyReciprocal()
		{
			var dummyTransactionHeaderSupporter = new DummyTransactionHeaderSupporter();
			AssertEquals("default exchange rate decimal should always be 6M", 6M, dummyTransactionHeaderSupporter.GetCurrentCompanyReciprocal());
		}

		public void TestOperationalJob()
		{
			APPayment apPayment = Factory.New<APPayment>();
			DocGenericTransactionHeader wrapper = DocGenericTransactionHeader.New(apPayment, Factory, GetFreightWrapper());
			AssertNotNull(wrapper.OperationalJob);
			AssertType(typeof(FreightWrapperFromInvoice), wrapper.OperationalJob);
		}

		public void TestPropertiesForJobRevenueJournal()
		{
			var journal = Factory.NewWithValidTestData<JobRevenueJournal>();
			journal.JournalLines.AddNew();
			journal.JournalLines.AddNew();
			journal.JournalLines.AddNew();
			var genericDocWrapper = DocGenericTransactionHeader.New(journal, Factory, GetFreightWrapper());

			AssertEquals("The new method should return should be of type", typeof(DocJobRevenueJournal), genericDocWrapper.HeaderPlugIn.GetType());
			AssertEquals("Job Revenue Journal", genericDocWrapper.DocumentTitle);
			AssertEquals("A collection should contain Job Revenue Journal lines to show on a document", 3, genericDocWrapper.LinesForInvoice.Count);
		}

		#endregion

		public void TestInhericDocStatement_ReceiptBankAccountIBAN()
		{
			const string testIBAN = "ES2637011181545485279943";
			var printStatement = new PrintStatement(Factory, GlbBranch.CurrentBranch)
			{
				CurrencyNK = "USD"
			};
			var headerBisObj = Factory.NewWithValidTestData<AccGLHeader>();
			var accountBisObj = Factory.New<AccBankAccount>();
			accountBisObj.AB_GC = GlbCompany.CurrentCompany.PK;
			accountBisObj.AB_RX_NKAccountCurrency = printStatement.CurrencyNK;
			accountBisObj.AB_IsDefaultReceiptBankAccount = ZBool.True;
			accountBisObj.AB_AG = headerBisObj.PK;
			accountBisObj.IBAN = testIBAN;
			accountBisObj.AB_Code = "ABCBANK";
			Factory.Save();

			var doc = DocGenericTransactionHeader.New(printStatement, Factory);
			AssertEquals("IBAN value should as GetReceiptBankAccountIBAN result", testIBAN, doc.ReceiptBankAccountIBAN);
		}

		public void TestDocDataProviderReflectorMember()
		{
			var docDataProviderReflector = new DocumentEngine.ReflectiveFieldMap.DocDataProviderReflector(typeof(DocGenericTransactionHeader));
			var expectedProps = new List<string> {
				"ReceiptBankAccountIBAN"
			};
			var errors = new List<string>();
			foreach (string propName in expectedProps)
			{
				try
				{
					Assert(FormattableString.Invariant($"should contain prop:{propName}"), docDataProviderReflector.Members.Any(member => member.GetFullPath() == propName));
				}
				catch (AssertionFailedError assertError)
				{
					errors.Add(assertError.Message);
				}
			}
			if (errors.Any())
			{
				throw new AssertionFailedError(string.Join(",", errors));
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			APPayment apPayment = Factory.New<APPayment>();
			return DocGenericTransactionHeader.New(apPayment, Factory);
		}

		FreightWrapper GetFreightWrapper()
		{
			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			Job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			apInvoice.AH_JH = Job.PK;
			return FreightWrapper.New(apInvoice, Factory)[0];
		}

		JobHeader Job;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		class DummyTransactionHeaderSupporter : GenericTransactionHeaderSupporter
		{
			protected internal override ZString GetTransactionType()
			{
				throw new NotImplementedException();
			}
		}
	}
}

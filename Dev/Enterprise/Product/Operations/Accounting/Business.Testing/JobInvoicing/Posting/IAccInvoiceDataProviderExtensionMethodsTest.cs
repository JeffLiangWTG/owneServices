using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class IAccInvoiceDataProviderExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetAPInvoiceNumberToMatch()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();

			var charge1 = new CustomsCharge(null, "TEST", 110.0m, 0m, true, creditor.PK);
			var customsCharge1 = CreateMockCustomCharge(charge1);
			var customsJob1 = CreateMockCustomsJobProvider("AAA111");
			var chargesProvider1 = CreateMockCustomsChargesProvider(customsJob1, "ABCD1234", customsCharge1);

			var reversedInvoice = CreateAPInvoice(chargesProvider1, 110m, creditor, true);
			Factory.Save();

			AssertEquals("ABCD1234/1", chargesProvider1.GetAPInvoiceNumberToMatch(Factory, creditor.PK));
		}

		public void TestGetAPInvoiceNumberToMatchWhenNothingWasReversed()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();

			var charge1 = new CustomsCharge(null, "TEST", 110.0m, 0m, true, creditor.PK);
			var customsCharge1 = CreateMockCustomCharge(charge1);
			var customsJob1 = CreateMockCustomsJobProvider("AAA111");
			var chargesProvider1 = CreateMockCustomsChargesProvider(customsJob1, "ABCD1234", customsCharge1);

			var activeInvoice = CreateAPInvoice(chargesProvider1, 110m, creditor, false);

			Factory.Save();

			AssertEquals("ABCD1234", chargesProvider1.GetAPInvoiceNumberToMatch(Factory, creditor.PK));
		}

		public void TestGetAPInvoiceNumberToMatchWhenUniqueNumberEmpty()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();

			var charge = new CustomsCharge(null, "TEST", 110.0m, 0m, true, creditor.PK);
			var customsCharge = CreateMockCustomCharge(charge);
			var customsJob = CreateMockCustomsJobProvider("AAA111");
			var chargesProvider = CreateMockCustomsChargesProvider(customsJob, ZString.Empty, customsCharge);
			AssertEquals(ZString.Empty, chargesProvider.GetAPInvoiceNumberToMatch(Factory, creditor.PK));
		}

		APInvoice CreateAPInvoice(IAccInvoiceDataProvider dataProvider, ZDecimal invoiceAmount, OrgHeader creditor, bool isCancelled)
		{
			var jobHeader = new Job.Loader(dataProvider.CustomsJob.TopLevelObjectForJobToReference).TryLoadOrCreateWithoutMutexForTestOnly();
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = dataProvider.UniqueNumber;
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_OH = creditor.PK;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_IsCancelled = isCancelled;

			if (isCancelled)
			{
				var matchLink = ((IMatching)invoice).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
				matchLink.AP_AH = invoice.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
			}

			var line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_JH = jobHeader.PK;
			line.AL_OSExTaxAmount = invoiceAmount;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var creator = new TestObjectCreator(Factory);
			if (!isCancelled)
			{
				creator.CreateJobCharge(line, jobHeader, creator.CC1, creator.AUD);
			}
			line.AL_AG = creator.GLHeader1.PK;

			return invoice;
		}

		MockCustomCharge CreateMockCustomCharge(params CustomsCharge[] charges)
		{
			var mockCustomCharge = new MockCustomCharge();
			mockCustomCharge.fIsActive = true;
			mockCustomCharge.fCustomsCharges = charges;
			return mockCustomCharge;
		}

		MockCustomsJobProvider CreateMockCustomsJobProvider(ZString jobNumber)
		{
			var mockCustomsJobProvider = Factory.New<MockCustomsJobProvider>();
			mockCustomsJobProvider.JobNumber = jobNumber;
			return mockCustomsJobProvider;
		}

		MockCustomsChargesProvider CreateMockCustomsChargesProvider(MockCustomsJobProvider customsJobProvider,
			ZString invoiceNumber, params ICustomsCharges[] customsCharges)
		{
			var mockCustomsChargesProvider = new MockCustomsChargesProvider();
			mockCustomsChargesProvider.CustomsJob = customsJobProvider;
			mockCustomsChargesProvider.Factory = Factory;
			mockCustomsChargesProvider.CustomsCharges = customsCharges;
			mockCustomsChargesProvider.InvoiceNumber = invoiceNumber;
			return mockCustomsChargesProvider;
		}
	}
}

using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MY.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
	{
		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(false, dec.AreMultipleEntryInstructionsAllowed);
		}

		public void TestReciprocalRates()
		{
			AssertEquals(true, Factory.New<JobDeclaration>().IsReciprocalRates);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			AssertEquals(Core.Constants.CurrencyCodes.Malaysia, Factory.New<JobDeclaration>().LocalCurrencyCode);
		}

		public void TestHouseBillsCollectionIsOfRightType()
		{
			JobDeclaration declaration = (JobDeclaration)GetNewBusinessObject();
			AssertEquals(typeof(BillCollection<Bill, JobDeclaration>), declaration.Bills.GetType());
		}

		public void TestLookupObjectIsCached()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			AssertSame(declaration.Lookups, declaration.Lookups);
		}

		public void TestFilteredInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return declaration;
		}
	}
}

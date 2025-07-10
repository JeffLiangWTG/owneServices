using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class CreationStrategyTest : EU.Business.Declaration.Testing.EntryCreationStrategyTest
	{
		public void TestLineIsValidForMerge()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			CombineAssertions(() =>
			{
				AssertEquals(false, entryCreationStrategy.LineIsValidForMerge(invoiceLine));

				invoiceLine.JI_CEI = entryInstruction.PK;
				AssertEquals(true, entryCreationStrategy.LineIsValidForMerge(invoiceLine));
			});
		}

		public void TestMergeKeyContainsBuyer()
		{
			var jobDeclarationForTest = GetJobDeclarationForTest();
			var jobComInvoiceHeader = jobDeclarationForTest.Invoices.AddNew();
			jobComInvoiceHeader.JZ_OA_BuyerAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			jobComInvoiceHeader.JZ_OA_SellerAddress = Factory.NewWithValidTestData<OrgAddress>().PK;

			var invLine = jobComInvoiceHeader.InvoiceLines.AddNew();

			var mergeStrategy = jobDeclarationForTest.CreateEntryCreationStrategy();

			Assert("Precondition: Address is set", !jobComInvoiceHeader.JZ_OA_BuyerAddress.IsEmpty);
			Assert(mergeStrategy.GetKeyForHeader(invLine).Contains(jobComInvoiceHeader.JZ_OA_BuyerAddress));
		}

		public void TestMergeKeyContainsSeller()
		{
			var jobDeclarationForTest = GetJobDeclarationForTest();
			var jobComInvoiceHeader = jobDeclarationForTest.Invoices.AddNew();
			jobComInvoiceHeader.JZ_OA_BuyerAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			jobComInvoiceHeader.JZ_OA_SellerAddress = Factory.NewWithValidTestData<OrgAddress>().PK;

			var invLine = jobComInvoiceHeader.InvoiceLines.AddNew();

			var mergeStrategy = jobDeclarationForTest.CreateEntryCreationStrategy();

			Assert("Precondition: Address is set", !jobComInvoiceHeader.JZ_OA_SellerAddress.IsEmpty);
			Assert(mergeStrategy.GetKeyForHeader(invLine).Contains(jobComInvoiceHeader.JZ_OA_SellerAddress));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			entryCreationStrategy = new EntryCreationStrategy(declaration);
		}
		EntryCreationStrategy entryCreationStrategy;
	}
}

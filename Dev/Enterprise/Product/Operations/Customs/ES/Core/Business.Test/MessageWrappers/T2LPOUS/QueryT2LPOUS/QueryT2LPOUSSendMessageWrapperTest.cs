using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class QueryT2LPOUSSendMessageWrapperTest : WrapperHelperTest<QueryT2LPOUSSendMessageWrapper>
	{
		public void TestMRN()
		{
			entryHeader.MovementReferenceNumber = "TestMRN";
			AssertEquals("Expected filled MRN", "TestMRN", wrapper.MRN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = new QueryT2LPOUSSendMessageWrapper(entryHeader, Certificate);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		QueryT2LPOUSSendMessageWrapper wrapper;

		protected override QueryT2LPOUSSendMessageWrapper GetProvider() => wrapper;
	}
}

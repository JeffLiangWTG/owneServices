using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DepartureCertReqAESSendMessageWrapperTest : WrapperHelperTest<DepartureCertReqAESSendMessageWrapper>
	{
		public void TestExportOperation()
		{
			var exportOperation = wrapper.ExportOperation;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled ExportOperation", exportOperation);
				AssertSame("Cached ExportOperation", wrapper.ExportOperation, exportOperation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			var entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = new DepartureCertReqAESSendMessageWrapper(entryHeader, Certificate);
		}

		DepartureCertReqAESSendMessageWrapper wrapper;

		protected override DepartureCertReqAESSendMessageWrapper GetProvider() => wrapper;
	}
}

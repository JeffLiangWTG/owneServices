using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CancelAESExportOperationWrapperTest : WrapperHelperTest<CancelAESExportOperationWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","reasonForCancellation"), () => new CancelAESExportOperationWrapper(entryHeader, null));
		}

		public void TestInvalidationReason()
		{
			AssertEquals("Expected filled InvalidationReason", "01-Cancel Reason", wrapper.InvalidationReason);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			declaration.CustomsEntryInstructions.AddNew();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			var reasonForCancellation = new ReasonForCancellation(Factory);
			reasonForCancellation.Code = "01";
			reasonForCancellation.Reason = "Cancel Reason";

			wrapper = new CancelAESExportOperationWrapper(entryHeader, reasonForCancellation);
		}
		CusEntryHeader entryHeader;
		CancelAESExportOperationWrapper wrapper;

		protected override CancelAESExportOperationWrapper GetProvider() => wrapper;
	}
}

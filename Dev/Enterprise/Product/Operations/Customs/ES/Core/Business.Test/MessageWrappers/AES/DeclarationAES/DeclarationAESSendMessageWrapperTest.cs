using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationAESSendMessageWrapperTest : WrapperHelperTest<DeclarationAESSendMessageWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if messageType is empty", typeof(ArgumentException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be empty string (\"\").","messageType"), () => new DeclarationAESSendMessageWrapper(entryHeader, Certificate, ZString.Empty, ZString.Empty, DeclarationMessageSubTypeList.Codes.OriginalDeclaration));
		}

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
			declaration.CustomsEntryInstructions.AddNew();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = new DeclarationAESSendMessageWrapper(entryHeader, Certificate, "2", DeclarationMessageTypeList.Codes.ExportUcc6, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		}
		CusEntryHeader entryHeader;
		DeclarationAESSendMessageWrapper wrapper;

		protected override DeclarationAESSendMessageWrapper GetProvider() => wrapper;
	}
}

using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AESCommonExportOperationMRNWrapperTest : WrapperHelperTest<AESCommonExportOperationMRNWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryHeader"), () => new AESCommonExportOperationMRNWrapper(null));
		}

		public void TestMRN()
		{
			entryHeader.MovementReferenceNumber = "TestMRN";
			AssertEquals("Expected filled MRN", "TestMRN", wrapper.MRN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();

			wrapper = new AESCommonExportOperationMRNWrapper(entryHeader);
		}

		CusEntryHeader entryHeader;
		AESCommonExportOperationMRNWrapper wrapper;

		protected override AESCommonExportOperationMRNWrapper GetProvider() => wrapper;
	}
}

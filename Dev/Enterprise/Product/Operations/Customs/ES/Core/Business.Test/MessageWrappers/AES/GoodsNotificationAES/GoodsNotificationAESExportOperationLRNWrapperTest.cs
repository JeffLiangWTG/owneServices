using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class GoodsNotificationAESExportOperationLRNWrapperTest : WrapperHelperTest<GoodsNotificationAESExportOperationLRNWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryHeader"), () => new GoodsNotificationAESExportOperationLRNWrapper(null));

				AssertExceptionThrown("Constructor Throws Exception if jobDeclaration is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","Declaration"), () => new GoodsNotificationAESExportOperationLRNWrapper(Factory.New<CusEntryHeader>()));
			});
		}
		public void TestLRN()
		{
			AssertEquals("Expected filled LRN with only entry ref num when no declarant with id is declared", "ES00001", wrapper.LRN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "ES00001";

			wrapper = new GoodsNotificationAESExportOperationLRNWrapper(entryHeader);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		GoodsNotificationAESExportOperationLRNWrapper wrapper;

		protected override GoodsNotificationAESExportOperationLRNWrapper GetProvider() => wrapper;
	}
}

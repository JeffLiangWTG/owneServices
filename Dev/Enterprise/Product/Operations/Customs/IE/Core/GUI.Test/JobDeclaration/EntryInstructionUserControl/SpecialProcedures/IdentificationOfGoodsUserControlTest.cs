using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class IdentificationOfGoodsUserControlTest : TestCaseWithFactory
	{
		public void TestGetIdentificationofGoodsGroupSubBoxCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;

			using (var control = new IdentificationOfGoodsUserControlForTest())
			{
				control.SetJobDeclarationForTest(declaration);

				var groupBox = control.FindSingleOrDefault<ZGroupBox>("IdentificationofGoodsSubGroupBox");
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "[Art. 163 5/8] Identification of Goods", groupBox.CaptionResourceString.Caption);
					AssertEquals("Full Description", "[Article 163 5/8] Identification of Goods", groupBox.CaptionResourceString.FullDescription);
				});
			}
		}

		public void TestGetProcessedProductsGroupBoxCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;

			using (var control = new IdentificationOfGoodsUserControlForTest())
			{
				control.SetJobDeclarationForTest(declaration);

				var groupBox = control.FindSingleOrDefault<ZGroupBox>("ProcessedProductsGroupBox");
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "[Art. 163 5/7] Processed Products", groupBox.CaptionResourceString.Caption);
					AssertEquals("Full Description", "[Article 163 5/7] Processed Products", groupBox.CaptionResourceString.FullDescription);
				});
			}
		}
	}

	class IdentificationOfGoodsUserControlForTest : IdentificationOfGoodsUserControl
	{
		public void SetJobDeclarationForTest(JobDeclaration jobDeclaration)
		{
			this.BindingSource.SetDataBinding(jobDeclaration, ".");
			SetCaption();
		}
	}
}

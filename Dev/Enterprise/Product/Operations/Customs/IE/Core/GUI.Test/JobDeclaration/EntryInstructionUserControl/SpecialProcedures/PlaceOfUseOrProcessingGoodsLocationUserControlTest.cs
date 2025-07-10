using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class PlaceOfUseOrProcessingGoodsLocationUserControlTest : TestCaseWithFactory
	{
		public void TestGroupBoxCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;

			using (var control = new PlaceOfUseOrProcessingGoodsLocationUserControlForTest())
			{
				control.SetJobDeclarationForTest(declaration);

				var groupBox = control.FindSingleOrDefault<ZGroupBox>("PlacesOfUseOrProcessingGroupBox");
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "[Article 163 4/9] Place(s) of Use or Processing", groupBox.CaptionResourceString.Caption);
				});
			}
		}
	}

	class PlaceOfUseOrProcessingGoodsLocationUserControlForTest : PlaceOfUseOrProcessingGoodsLocationUserControl
	{
		public void SetJobDeclarationForTest(JobDeclaration jobDeclaration)
		{
			this.BindingSource.SetDataBinding(jobDeclaration, ".");
			SetCaption();
		}
	}
}

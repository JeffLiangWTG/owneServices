using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class FirstPlaceOfUseOrProcessingUserControlTest : TestCaseWithFactory
	{
		public void TestGroupBoxCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;

			using (var control = new FirstPlaceOfUseOrProcessingUserControlForTest())
			{
				control.SetJobDeclarationForTest(declaration);

				var groupBox = control.FindSingleOrDefault<ZGroupBox>("FirstPlaceOfUseOrProcessingGroupBox");

				CombineAssertions(() =>
				{
					AssertEquals("Caption", "[Art. 163 4/5] First Place of Processing", groupBox.CaptionResourceString.Caption);
					AssertEquals("Full Description", "[Article 163 4/5] First Place of Processing", groupBox.CaptionResourceString.FullDescription);
				});
			}
		}
	}

	class FirstPlaceOfUseOrProcessingUserControlForTest : FirstPlaceOfUseOrProcessingUserControl
	{
		public void SetJobDeclarationForTest(JobDeclaration jobDeclaration)
		{
			this.BindingSource.SetDataBinding(jobDeclaration, ".");
			SetCaption();
		}
	}
}

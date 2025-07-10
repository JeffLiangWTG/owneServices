using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class ConditionsAndTermsUserControlTest : TestCaseWithFactory
	{
		public void TestGroupBoxCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;

			using (var control = new ConditionsAndTermsUserControlForTest())
			{
				control.SetJobDeclarationForTest(declaration);

				var groupBox = control.FindSingleOrDefault<ZGroupBox>("ConditionsAndTermsGroupBox");

				CombineAssertions(() =>
				{
					AssertEquals("Caption", "[Art. 163 6/2] Conditions and Terms", groupBox.CaptionResourceString.Caption);
					AssertEquals("Full Description", "[Article 163 6/2] Conditions and Terms", groupBox.CaptionResourceString.FullDescription);
				});
			}
		}
	}
	class ConditionsAndTermsUserControlForTest : ConditionsAndTermsUserControl
	{
		public void SetJobDeclarationForTest(JobDeclaration jobDeclaration)
		{
			this.BindingSource.SetDataBinding(jobDeclaration, ".");
			SetCaption();
		}
	}
}

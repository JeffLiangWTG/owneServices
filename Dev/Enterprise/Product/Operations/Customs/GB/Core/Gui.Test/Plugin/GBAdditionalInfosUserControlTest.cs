using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Plugin.Testing
{
	class GBAdditionalInfosUserControlTest : TestCaseWithFactory
	{
		public void TestCaptionWithApplicationCodeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_ApplicationCode = "CHF";
			using (var form = new ZForm(declaration))
			{
				using (var control = new GBAdditionalInfosUserControl())
				{
					control.JobDeclaration = declaration;

					form.Controls.Add(control);
					form.Show();

					AssertEquals("[44] Additional Info", control.FindSingle<ZGroupBox>(x => x.Name == "AdditionalInfosGroupBox").Text);
				}
			}

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			using (var form = new ZForm(declaration))
			{
				using (var control = new GBAdditionalInfosUserControl())
				{
					control.JobDeclaration = declaration;

					form.Controls.Add(control);
					form.Show();

					AssertEquals("[UCC 2/2] Additional Info", control.FindSingle<ZGroupBox>(x => x.Name == "AdditionalInfosGroupBox").Text);
				}
			}
		}
	}
}

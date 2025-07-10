using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Plugin.Testing
{
	class GBPreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestCaptionWithApplicationCodeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_ApplicationCode = "CHF";
			using (var form = new ZForm(declaration))
			{
				using (var control = new GBPreviousDocumentsUserControl())
				{
					control.JobDeclaration = declaration;

					form.Controls.Add(control);
					form.Show();

					AssertEquals("[40] Previous Documents", control.FindSingle<ZGroupBox>(x => x.Name == "PrevDocsGroupBox").Text);
				}
			}

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			using (var form = new ZForm(declaration))
			{
				using (var control = new GBPreviousDocumentsUserControl())
				{
					control.JobDeclaration = declaration;

					form.Controls.Add(control);
					form.Show();

					AssertEquals("[UCC 2/1] Previous Documents", control.FindSingle<ZGroupBox>(x => x.Name == "PrevDocsGroupBox").Text);
				}
			}
		}
	}
}

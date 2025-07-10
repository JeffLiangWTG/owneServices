using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(CompanyCredentialsUserControl))]
sealed class CompanyCredentialsUserControlTest : BasherTest
{
	public override Form GetFormToBash()
	{
		ZForm form = new ZForm();
		form.CaptionRenderingEnabled = true;
		var userControl = new CompanyCredentialsUserControl();
		userControl.Dock = DockStyle.Fill;
		form.Controls.Add(userControl);
		var provider = GlbCompanyWrapperProvider.GetProvider(Core.Constants.CountryCodes.Belgium);
		var glbCompany = Factory.New<GlbCompany>();
		glbCompany.GC_Code = "BEC";
		form.SetDataBinding(provider.GetWrapper(glbCompany), "");
		return form;
	}

	public void TestCurrentPasswordTextBox()
	{
		using (var userControl = new CompanyCredentialsUserControl())
		{
			AssertEquals("Password char", '*', userControl.CurrentPasswordTextBox.PasswordChar);
			AssertEquals("Character Casing", CharacterCasing.Normal, userControl.CurrentPasswordTextBox.CharacterCasing);
		}
	}
}

using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(CompanyCredentialsDetailsUserControl))]
sealed class CompanyCredentialsDetailsUserControlTest : BasherTest
{
	string CountryToTestAgainst => Core.Constants.CountryCodes.Switzerland;

	public override Form GetFormToBash()
	{
		var form = new ZForm();
		form.CaptionRenderingEnabled = true;
		var userControl = new CompanyCredentialsDetailsUserControl();
		userControl.Dock = DockStyle.Fill;
		form.Controls.Add(userControl);
		var glbCompany = Factory.New<GlbCompany>();
		glbCompany.GC_Code = "ZAC";
		var provider = (GlbCompanyWrapperProvider)GlbCompanyWrapperProvider.GetProvider(CountryToTestAgainst);
		form.SetDataBinding(provider.GetWrapper(glbCompany), "");
		return form;
	}
}

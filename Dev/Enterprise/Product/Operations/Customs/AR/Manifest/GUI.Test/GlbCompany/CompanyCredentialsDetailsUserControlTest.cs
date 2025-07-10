using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.GUI.Testing
{
	[TestedType(typeof(CompanyCredentialsDetailsUserControl))]
	sealed class CompanyCredentialsDetailsUserControlTest : BasherTest
	{
		public void TestToggleCertificateLoaderUserControlStatus()
		{
			using (var form = GetFormToBash())
			{
				form.Show();

				var companyWrapper = (Business.GlbCompanyWrapper)((ZForm)form).CurrentDataItem;
				var certificateLoaderUserControl = (DigitalCertificateControl_p12)form.Controls.Find("CertificateLoaderUserControl", true).SingleOrDefault();
				Assert(certificateLoaderUserControl.Enabled);
			}
		}

		public override Form GetFormToBash()
		{
			ZForm form = new ZForm();
			form.CaptionRenderingEnabled = true;
			var userControl = new CompanyCredentialsDetailsUserControl();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			var glbCompany = Factory.New<GlbCompany>();
			glbCompany.GC_Code = "ZAC";
			var provider = (GlbCompanyWrapperProvider)GlbCompanyWrapperProvider.GetProvider(Core.Constants.CountryCodes.Argentina);
			form.SetDataBinding(provider.GetWrapper(glbCompany), "");
			return form;
		}
	}
}

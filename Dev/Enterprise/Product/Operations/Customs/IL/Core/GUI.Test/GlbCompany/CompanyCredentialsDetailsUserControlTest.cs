using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing.GlbCompany
{
	[TestedType(typeof(CompanyCredentialsDetailsUserControl))]
	sealed class CompanyCredentialsDetailsUserControlTest : BasherTest
	{
		public void TestGroupBoxCaptions()
		{
			using (var form = GetZChildFormToBash())
			{
				var customsCredentialsGroupBox = form.FindSingle<ZGroupBox>("CustomsCredentialsGroupBox");
				var defaultuserGroupBox = form.FindSingle<ZGroupBox>("DefaultUserGroupBox");
				AssertEquals("CustomsCredentialsGroupBox Caption", "Israel Customs Certificate", customsCredentialsGroupBox.CaptionResourceString.Caption);
				AssertEquals("DefaultUserGroupBox Caption", "Company's default user for message signing", defaultuserGroupBox.CaptionResourceString.Caption);
			}
		}

		public override Form GetFormToBash()
		{
			return GetZChildFormToBash();
		}

		ZChildForm GetZChildFormToBash()
		{
			var form = new ZChildForm();
			form.CaptionRenderingEnabled = true;

			var userControl = new CompanyCredentialsDetailsUserControl();
			userControl.Dock = DockStyle.Fill;

			form.Controls.Add(userControl);
			form.SetDataBinding(Provider.GetWrapper(Company), "");

			return form;
		}

		MasterFiles.Business.GlbCompany Company
		{
			get
			{
				if (glbCompany == null)
				{
					glbCompany = Factory.New<MasterFiles.Business.GlbCompany>();
					glbCompany.GC_Code = "ZAC";
				}
				return glbCompany;
			}
		}
		MasterFiles.Business.GlbCompany glbCompany;

		GlbCompanyWrapperProvider Provider => provider ?? (provider = (GlbCompanyWrapperProvider)GlbCompanyWrapperProvider.GetProvider(Core.Constants.CountryCodes.Israel));
		GlbCompanyWrapperProvider provider;
	}
}

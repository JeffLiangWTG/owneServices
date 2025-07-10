using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(CompanyCredentialsDetailsUserControl))]
sealed class CompanyCredentialsDetailsUserControlTest : BasherTest
{
	public void TestAccountUCCListGroupBoxCaption()
	{
		using (var form = GetZChildFormToBash())
		{
			var accountUCCListGroupBox = form.FindSingle<ZGroupBox>("AccountUCCListGroupBox");
			AssertEquals("AccountUCCListGroupBox Caption", "Account UCC List", accountUCCListGroupBox.CaptionResourceString.Caption);
		}
	}

	public void TestAccountGridColumns()
	{
		using (var form = GetZChildFormToBash())
		{
			var accountsGrid = form.FindSingle<ZGrid>("ITAccUserGrid");
			AssertEquals("Columns Count", 3, accountsGrid.ColumnStyles.Count);
			CombineAssertions("Column Styles", () =>
			{
				AssertColumn(accountsGrid, AutoGlbExternalPassword.Schema.GP_UserID);
				AssertColumn(accountsGrid, AutoGlbExternalPassword.Schema.GP_Name);
				AssertColumn(accountsGrid, AutoGlbExternalPassword.Schema.GP_MailBoxID);
			});
		}
	}

	public void TestToggleCertificateLoaderUserControlStatus()
	{
		using (var form = GetZChildFormToBash())
		{
			form.Show();

			var companyWrapper = (Business.GlbCompanyWrapper)form.CurrentDataItem;
			var certificateLoaderUserControl = (DigitalCertificateControl_p12)form.Controls.Find("CertificateLoaderUserControl", true).SingleOrDefault();
			companyWrapper.PasswordCollection.RemoveAndDeleteAll();
			Assert(!certificateLoaderUserControl.Enabled);

			companyWrapper.PasswordCollection.AddNew();
			Assert(certificateLoaderUserControl.Enabled);
		}
	}

	public void TestCertificatePasswordTextBox()
	{
		using (var form = GetZChildFormToBash())
		{
			form.Show();

			var certificatePasswordTextBox = form.FindSingle<ZTextBox>("CertificatePasswordTextBox");
			AssertEquals("CharacterCasing", CharacterCasing.Normal, certificatePasswordTextBox.CharacterCasing);

			var companyWrapper = (Business.GlbCompanyWrapper)form.CurrentDataItem;
			var glbPassword = companyWrapper.PasswordCollection.AddNew();
			glbPassword.CurrentDecryptedCertificatePassphrase = "UPPERCASE PWD";
			AssertEquals("When Password is entered uppercase, Text", "UPPERCASE PWD", certificatePasswordTextBox.Text);

			glbPassword.CurrentDecryptedCertificatePassphrase = "lowercase pwd";
			AssertEquals("When Password is entered lowercase, Text", "lowercase pwd", certificatePasswordTextBox.Text);

			glbPassword.CurrentDecryptedCertificatePassphrase = "mIxEdCaSe PwD";
			AssertEquals("When Password is entered mixed case, Text", "mIxEdCaSe PwD", certificatePasswordTextBox.Text);
		}
	}

	void AssertColumn(ZGrid parentGrid, ZString columnName)
	{
		var columnInfo = parentGrid.GetColumnStyle(columnName);
		AssertNotNull($"'{columnName}' not null", columnInfo);
		AssertEquals($"'{columnName}' IsVisible", true, columnInfo.IsVisible);
		AssertEquals($"'{columnName}' IsReadOnly", false, columnInfo.IsReadOnly);
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
		var glbCompany = Factory.New<GlbCompany>();
		glbCompany.GC_Code = "ZAC";
		var provider = (GlbCompanyWrapperProvider)MasterFiles.GUI.GlbCompanyWrapperProvider.GetProvider(Core.Constants.CountryCodes.Italy);
		form.SetDataBinding(provider.GetWrapper(glbCompany), "");

		return form;
	}
}

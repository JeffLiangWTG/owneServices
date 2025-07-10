using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using GlbCompanyWrapper = Enterprise.Customs.CH.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.CH.GUI.Testing;

class TokenCredentialsUserControlTest : TestCaseWithFactory
{
	GlbCompanyWrapper company;

	protected override void SetUp()
	{
		base.SetUp();

		company = GlbCompanyWrapper.GetWrapper<Enterprise.Customs.CH.Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany);
	}

	public void TestSetTokenCredentialsShouldBeBoundToTokenCredentialsEnabled()
	{
		WithInitializedTokenCredentialsUserControl(tokenCredentialsUserControl =>
		{
			var setTokenCredentialsCheckBox = tokenCredentialsUserControl.SetTokenCredentialsCheckBox;

			company.TokenCredentialsEnabled = true;
			Assert($"Set Credentials should be checked when {nameof(company.TokenCredentialsEnabled)} = true", setTokenCredentialsCheckBox.Checked);

			company.TokenCredentialsEnabled = false;
			Assert($"Set Credentials should be unchecked when {nameof(company.TokenCredentialsEnabled)} = false", !setTokenCredentialsCheckBox.Checked);
		});
	}

	public void TestSetTokenCredentialsShouldUpdateOtherControlsTextAndReadonly()
	{
		WithInitializedTokenCredentialsUserControl(tokenCredentialsUserControl =>
		{
			var setTokenCredentialsCheckBox = tokenCredentialsUserControl.SetTokenCredentialsCheckBox;
			var accessTokenTextBox = tokenCredentialsUserControl.AccessTokenTextBox;
			var certificatePassPhraseTextBox = tokenCredentialsUserControl.CertificatePassPhraseTextBox;
			var userIdTextBox = tokenCredentialsUserControl.UserIdTextBox;
			var clientSecretTextBox = tokenCredentialsUserControl.ClientSecretTextBox;

			setTokenCredentialsCheckBox.Checked = true;
			CombineAssertions("Set Token Credentials is checked", () =>
			{
				Assert(nameof(accessTokenTextBox), !accessTokenTextBox.ReadOnly);
				Assert(nameof(certificatePassPhraseTextBox), !certificatePassPhraseTextBox.ReadOnly);
				Assert(nameof(userIdTextBox), !userIdTextBox.ReadOnly);
				Assert(nameof(clientSecretTextBox), !clientSecretTextBox.ReadOnly);
			});

			accessTokenTextBox.Text = "123";
			certificatePassPhraseTextBox.Text = "123";
			userIdTextBox.Text = "123";
			clientSecretTextBox.Text = "123";
			setTokenCredentialsCheckBox.Checked = false;
			CombineAssertions("Set Token Credentials is unchecked", () =>
			{
				Assert(nameof(accessTokenTextBox), accessTokenTextBox.ReadOnly);
				AssertNullOrEmpty(nameof(accessTokenTextBox), accessTokenTextBox.Text);

				Assert(nameof(certificatePassPhraseTextBox), certificatePassPhraseTextBox.ReadOnly);
				AssertNullOrEmpty(nameof(certificatePassPhraseTextBox), certificatePassPhraseTextBox.Text);

				Assert(nameof(userIdTextBox), userIdTextBox.ReadOnly);
				AssertNullOrEmpty(nameof(userIdTextBox), userIdTextBox.Text);

				Assert(nameof(clientSecretTextBox), clientSecretTextBox.ReadOnly);
				AssertNullOrEmpty(nameof(clientSecretTextBox), clientSecretTextBox.Text);
			});
		});
	}

	public void TestCaptionsAreNotTranslated()
	{
		GlbStaff.CurrentUser.GS_WorkingLanguage = "FR";
		using var control = new TokenCredentialsUserControl();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(control.AccessTokenTextBox), "Access Token", control.AccessTokenTextBox.CaptionResourceString.Caption);
			AssertEquals(nameof(control.CertificatePassPhraseTextBox), "Refresh Token", control.CertificatePassPhraseTextBox.CaptionResourceString.Caption);
			AssertEquals(nameof(control.UserIdTextBox), "Customer Key", control.UserIdTextBox.CaptionResourceString.Caption);
			AssertEquals(nameof(control.ClientSecretTextBox), "Customer Secret", control.ClientSecretTextBox.CaptionResourceString.Caption);
		});
	}

	void WithInitializedTokenCredentialsUserControl(Action<TokenCredentialsUserControl> action)
	{
		using (var form = new ZForm(company))
		using (var tokenCredentialsUserControl = new TokenCredentialsUserControl())
		{
			form.Controls.Add(tokenCredentialsUserControl);
			form.SetDataBinding(company, ".");
			form.Show();

			action.Invoke(tokenCredentialsUserControl);
		}
	}
}

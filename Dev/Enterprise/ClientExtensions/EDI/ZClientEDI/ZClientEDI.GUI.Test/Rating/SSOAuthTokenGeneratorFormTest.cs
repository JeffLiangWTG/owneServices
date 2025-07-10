using System;
using System.Threading;
using System.Windows.Forms;
using AuthenticationService.Client.Models;
using Enterprise.Integration.Rating;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;
using ZClientEDI.GUI.Rating;

namespace ZClientEDI.GUI.Test.Rating
{
	[TestedType(typeof(SSOAuthTokenGeneratorForm))]
	public class SSOAuthTokenGeneratorFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var mappedLoginInfo = new LoginInfo();
			var authTokenProviderMock = new Mock<IAuthTokenProvider>();
			authTokenProviderMock.Setup(m => m.GetToken(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
				.Returns(() => ("This is the token", null));

			return new SSOAuthTokenGeneratorForm(DummyToken, NoClipboard, authTokenProviderMock.Object);
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "TokenResultTextBox" || base.ShouldIgnoreMissingBindingMember(control);
		}

		public void TestAuthValidationFailure()
		{
			var authTokenProviderMock = new Mock<IAuthTokenProvider>();
			authTokenProviderMock.Setup(m => m.GetToken(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
				.Returns(() => (null, "This is a validation failure"));

			string clipboard = null;
			Action<string> doClipboard = (token) => clipboard = token;

			using (var form = new SSOAuthTokenGeneratorFormForTest(DummyToken, doClipboard, authTokenProviderMock.Object))
			{
				form.Show();
				Application.DoEvents();

				form.GenerateButton.PerformClick();
				Application.DoEvents();

				AssertEquals("This is a validation failure", form.TokenResultTextBox.Text);
				AssertNull(clipboard);
			}
		}

		public void TestAuthSuccess()
		{
			var mappedLoginInfo = new LoginInfo();
			var authTokenProviderMock = new Mock<IAuthTokenProvider>();
			authTokenProviderMock
				.Setup(m => m.GetToken(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Action<LoginInfo>>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
				.Callback((string s, int i, Action<LoginInfo> overrideLoginInfo, CancellationToken ct, bool _) =>
				{
					overrideLoginInfo(mappedLoginInfo);
				})
				.Returns(() => ("This is the token", null));

			string clipboard = null;
			Action<string> doClipboard = (token) => clipboard = token;

			using (var form = new SSOAuthTokenGeneratorFormForTest(DummyToken, doClipboard, authTokenProviderMock.Object))
			{
				form.Show();
				Application.DoEvents();

				form.CompanyCodeLabel.Text = "CompanyCodeLabel";
				form.EnterpriseCodeLabel.Text = "EnterpriseCodeLabel";
				form.UserCodeLabel.Text = "UserCodeLabel";
				form.ServerCodeLabel.Text = "ServerCodeLabel";
				form.UserFullNameLabel.Text = "UserFullNameLabel";
				form.CompanyNameLabel.Text = "CompanyNameLabel";
				form.UserEmailLabel.Text = "test@wisetechglobal.com";
				form.RolesCheckedListBox.SetItemChecked(0, value: true);
				form.TokenResultLabel.Text = "TokenResultLabel";
				form.DatabaseNumberUpDown.Text = "5";
				Application.DoEvents();

				form.GenerateButton.PerformClick();
				Application.DoEvents();

				AssertEquals("This is the token", form.TokenResultTextBox.Text);
				AssertEquals("This is the token", clipboard);

				CombineAssertions("GUI parameter mapping to Authentication Service API", () =>
				{
					AssertEquals(form.CompanyCodeLabel.Text, mappedLoginInfo.CompanyCode);
					AssertEquals(form.EnterpriseCodeLabel.Text, mappedLoginInfo.EnterpriseCode);
					AssertEquals(form.UserCodeLabel.Text, mappedLoginInfo.UserCode);
					AssertEquals(form.ServerCodeLabel.Text, mappedLoginInfo.ServerCode);
					AssertEquals(form.UserFullNameLabel.Text, mappedLoginInfo.UserFullName);
					AssertEquals(form.CompanyNameLabel.Text, mappedLoginInfo.CompanyName);
					AssertEquals(form.UserEmailLabel.Text, mappedLoginInfo.UserEmail);
					AssertArrayEqualsByElements(new[] { "support", "CargoSphereRateAdmin" }, mappedLoginInfo.Roles);
					AssertEquals(form.DatabaseNumberUpDown.Text, mappedLoginInfo.DatabaseNumber.ToString());
					AssertNotNull(mappedLoginInfo.Password);
				});
			}
		}

		string DummyToken() => "Dummy token";

		void NoClipboard(string token) { }
	}

	class SSOAuthTokenGeneratorFormForTest : SSOAuthTokenGeneratorForm
	{
		public SSOAuthTokenGeneratorFormForTest(Func<string> generateSupportLoginToken, Action<string> doClipboard, IAuthTokenProvider authTokenProvider) : base(generateSupportLoginToken, doClipboard, authTokenProvider) { }

		public ZTextBox CompanyCodeLabel => (ZTextBox)Controls.Find("CompanyCodeLabel", searchAllChildren: true)[0];
		public ZTextBox EnterpriseCodeLabel => (ZTextBox)Controls.Find("EnterpriseCodeLabel", searchAllChildren: true)[0];
		public ZTextBox UserCodeLabel => (ZTextBox)Controls.Find("UserCodeLabel", searchAllChildren: true)[0];
		public ZTextBox ServerCodeLabel => (ZTextBox)Controls.Find("ServerCodeLabel", searchAllChildren: true)[0];
		public ZTextBox UserFullNameLabel => (ZTextBox)Controls.Find("UserFullNameLabel", searchAllChildren: true)[0];
		public ZTextBox CompanyNameLabel => (ZTextBox)Controls.Find("CompanyNameLabel", searchAllChildren: true)[0];
		public ZTextBox UserEmailLabel => (ZTextBox)Controls.Find("UserEmailLabel", searchAllChildren: true)[0];
		public ZTextBox TokenResultTextBox => (ZTextBox)Controls.Find("TokenResultTextBox", searchAllChildren: true)[0];
		public ZCheckedListBox RolesCheckedListBox => (ZCheckedListBox)Controls.Find("RolesCheckedListBox", searchAllChildren: true)[0];
		public ZLabel RolesLabel => (ZLabel)Controls.Find("RolesLabel", searchAllChildren: true)[0];
		public ZLabel TokenResultLabel => (ZLabel)Controls.Find("TokenResultLabel", searchAllChildren: true)[0];
		public ZNumericUpDown DatabaseNumberUpDown => (ZNumericUpDown)Controls.Find("DatabaseNumberUpDown", searchAllChildren: true)[0];
		public ZButton GenerateButton => (ZButton)Controls.Find("GenerateButton", searchAllChildren: true)[0];
	}
}

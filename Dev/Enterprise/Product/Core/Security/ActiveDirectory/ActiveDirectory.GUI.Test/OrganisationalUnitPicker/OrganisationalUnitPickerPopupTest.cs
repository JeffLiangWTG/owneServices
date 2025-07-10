using System.Windows.Forms;
using CargoWise.ActiveDirectory.TestFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	class OrganisationalUnitPickerPopupTest : TestCase
	{
		public void TestShowModal_WithInvalidDomainName()
		{
			var domainCredentials = new DomainCredentials
			{
				DomainName = "fake.domain",
				DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
				DomainUserPassword = TestConstants.ADTestUserAccount.Password
			};
			var expectedMessage = @"Your domain credentials are not valid:
The domain cannot be reached. Please check its availability or if its name is valid.";

			AssertShowModal_WithInvalidDomainCredentials(domainCredentials, expectedMessage);
		}

		public void TestShowModal_WithInvalidUserName()
		{
			var domainCredentials = new DomainCredentials
			{
				DomainName = TestConstants.Domain,
				DomainUserName = "fake.user@domain",
				DomainUserPassword = TestConstants.ADTestUserAccount.Password
			};
			var expectedMessage = @"Your domain credentials are not valid:
The domain user name or password is incorrect.";

			AssertShowModal_WithInvalidDomainCredentials(domainCredentials, expectedMessage);
		}

		public void TestShowModal_WithInvalidUserPassword()
		{
			var domainCredentials = new DomainCredentials
			{
				DomainName = TestConstants.Domain,
				DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
				DomainUserPassword = "ThisIsNotTheRightPassword"
			};
			var expectedMessage = @"Your domain credentials are not valid:
The domain user name or password is incorrect.";

			AssertShowModal_WithInvalidDomainCredentials(domainCredentials, expectedMessage);
		}

		void AssertShowModal_WithInvalidDomainCredentials(DomainCredentials invalidDomainCredentials, string expectedMessage)
		{
			using (var form = new ZChildForm())
			using (var findBox = new OrganisationalUnitPickerFindBox())
			{
				AssertEquals("Pre-condition: CodeBox is empty", string.Empty, findBox.CodeBox.Text);

				using (var popup = new OrganisationalUnitPickerPopup(() => invalidDomainCredentials))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					popup.ShowModal(findBox, form);
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(string.Empty, findBox.CodeBox.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		public void TestShowModal_WithValidDomainCredentials()
		{
			var validDomainCredentials = new DomainCredentials
			{
				DomainName = TestConstants.Domain,
				DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
				DomainUserPassword = TestConstants.ADTestUserAccount.Password,
				UserOrganisationalUnit = TestConstants.ValidOU
			};

			using (var form = new ZChildForm())
			using (var findBox = new OrganisationalUnitPickerFindBox())
			{
				findBox.CodeBox.Text = validDomainCredentials.UserOrganisationalUnit;
				Assert("Pre-condition: Valid OU should not be empty", !string.IsNullOrEmpty(TestConstants.ValidOU));

				using (var popup = new OrganisationalUnitPickerPopup(() => validDomainCredentials))
				{
					var selectedOU = string.Empty;
					ZFormModaliser.SetDelegateToCallOnFormClosing((ouForm) =>
					{
						var formAsDocumentForm = ((OrganisationalUnitPickerForm)ouForm);
						selectedOU = formAsDocumentForm.OUPicker.DirectoryTreeView.SelectedNode.Tag.ToString();
					});
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					popup.ShowModal(findBox, form);
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("The credentials' OU should be preselected in when the OUPicker shows up", TestConstants.ValidOU, selectedOU);
				}
			}
		}

		public void TestShowModal_WithValidDomainCredentials_WhenCancelOnForm()
		{
			AssertShowModal_WithValidDomainCredentials(
				string.Empty,
				DialogResult.Cancel,
				(ouForm) =>
				{
					var formAsDocumentForm = ((OrganisationalUnitPickerForm)ouForm);
					formAsDocumentForm.OUPicker.SelectedOU = TestConstants.ValidOU;
				},
				string.Empty,
				string.Empty
			);
		}

		public void TestShowModal_WithValidDomainCredentials_WhenOKOnForm()
		{
			AssertShowModal_WithValidDomainCredentials(
				string.Empty,
				DialogResult.OK,
				(ouForm) =>
				{
					var formAsDocumentForm = ((OrganisationalUnitPickerForm)ouForm);
					formAsDocumentForm.OUPicker.SelectedOU = TestConstants.ValidOU;
				},
				string.Empty,
				string.Empty
			);
		}

		public void TestShowModal_WithValidDomainCredentials_WithPreselectedOU()
		{
			AssertShowModal_WithValidDomainCredentials(
				TestConstants.ValidOU,
				DialogResult.OK,
				null,
				TestConstants.ValidOU,
				TestConstants.ValidOU
			);
		}

		void AssertShowModal_WithValidDomainCredentials(string preselectedOU, DialogResult dialogResult, ZFormModaliser.PreShowInvoker delegateToCallBeforeShowingFormsOrDialogs, string expectedCodeBoxText, string expectedSelectedOU)
		{
			var validDomainCredentials = new DomainCredentials
			{
				DomainName = TestConstants.Domain,
				DomainUserName = TestConstants.ADTestUserAccount.NameWithDomain,
				DomainUserPassword = TestConstants.ADTestUserAccount.Password,
				UserOrganisationalUnit = preselectedOU
			};

			using (var form = new ZChildForm())
			using (var findBox = new OrganisationalUnitPickerFindBox())
			{
				findBox.CodeBox.Text = validDomainCredentials.UserOrganisationalUnit;
				Assert("Pre-condition: Valid OU should not be empty", !string.IsNullOrEmpty(TestConstants.ValidOU));

				using (var popup = new OrganisationalUnitPickerPopup(() => validDomainCredentials))
				{
					var selectedOU = string.Empty;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegateToCallBeforeShowingFormsOrDialogs);
					ZFormModaliser.SetDelegateToCallOnFormClosing((ouForm) =>
					{
						var formAsDocumentForm = ((OrganisationalUnitPickerForm)ouForm);
						selectedOU = formAsDocumentForm.OUPicker.DirectoryTreeView.SelectedNode.Tag.ToString();
					});
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = dialogResult;
					popup.ShowModal(findBox, form);
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(expectedCodeBoxText, findBox.CodeBox.Text);
					AssertEquals("The credentials' OU should be preselected in when the OUPicker shows up", expectedSelectedOU, selectedOU);
				}
			}
		}
	}
}

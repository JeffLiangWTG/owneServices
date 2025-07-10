using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	class EMCSMenuExtensionTest : TestCaseWithFactory
	{
		public void TestCanSend_Inactive()
		{
			using (var form = new ZForm(declaration))
			using (var menu = new EMCSMenu(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				declaration.CanSend((ZForm)menu.GetMainMenu()?.GetForm(), InactiveNotificationText, () => false);
				AssertEquals(InactiveNotificationText, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCanSend_IsActive()
		{
			using (var form = new ZForm(declaration))
			using (var menu = new EMCSMenu(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				declaration.CanSend((ZForm)menu.GetMainMenu()?.GetForm(), InactiveNotificationText, () => true);
				AssertNotEquals(InactiveNotificationText, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCanSend_RunValidationWhileSaving()
		{
			using (var form = new ZForm(declaration))
			using (var menu = new EMCSMenu(declaration))
			{
				form.Menu.MenuItems.Add(menu);
				CombineAssertions(() =>
				{
					declaration.HasChanges = true;
					AssertNoMessageErrorContaining("Miscellaneous messageErrors doesn't exists", declaration.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);

					ZFormModaliser.ShowDialogsInTest = true;
					UnitTestUserNotification.Instance.ClearMessages();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					declaration.CanSend((ZForm)menu.GetMainMenu()?.GetForm(), InactiveNotificationText, () => true);
					AssertEquals("Safe confirmation appears", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(SafeConfirmationText));
					AssertHasMessageErrorContaining("Miscellaneous messageErrors exists", declaration.JE_TransportModeInfo, MandatoryValidation.YouHaveNotEntered);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
		const string InactiveNotificationText = "This declaration is inactive.";
		const string SafeConfirmationText = "You need to save first. Would you like to save now and proceed?";
	}
}

using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(CompanyCredentialsPlugIn))]
	sealed class CompanyCredentialsPlugInTest : TestCaseWithFactory
	{
		public void TestGetNewTopLevelMenu()
		{
			Assert("Precondition", Env.CurrentUser.IsSupportUser);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertNoExceptionThrown("When not set registry EnableICS2Functions to true, plugin cannot get a Form to insert the menu", () =>
				{
					using (var form = new ZForm(GlbCompany.CurrentCompany))
					{
						form.PlugIns.Add(ControllerIDs.CompanyCredentialsPlugIn);
						form.Show();
					}
				});

				using (var form = new ZForm(GlbCompany.CurrentCompany))
				{
					form.PlugIns.Add(ControllerIDs.CompanyCredentialsPlugIn);
					form.Show();

					var menu = form.Menu;
					var actionMenu = menu.MenuItems.FindByName("ActionsMenuItem", false);
					var increaseInterchangeNumberMenu = actionMenu.MenuItems.FindByName("IncreaseInterchangeNumberMenuItem", false);
					AssertNotNull(increaseInterchangeNumberMenu);
				}

				GlbStaff.CurrentUser.GS_LoginName = "Dummy";
				Assert("Precondition", !Env.CurrentUser.IsSupportUser);
				using (var form = new ZForm(GlbCompany.CurrentCompany))
				{
					form.PlugIns.Add(ControllerIDs.CompanyCredentialsPlugIn);
					form.Show();

					var menu = form.Menu;
					var actionMenu = menu.MenuItems.FindByName("ActionsMenuItem", false);
					var increaseInterchangeNumberMenu = actionMenu.MenuItems.FindByName("IncreaseInterchangeNumberMenuItem", false);
					AssertNull(increaseInterchangeNumberMenu);
				}
			}
		}

		public void TestIncreaseInterchangeNumberMenuItem_Click()
		{
			const string expectedErrorMessage = "There are no existing messages for the EDI Site listed against this Company, please send a message before trying again";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				using (var form = new ZForm(GlbCompany.CurrentCompany))
				{
					form.PlugIns.Add(ControllerIDs.CompanyCredentialsPlugIn);
					form.Show();

					var menu = form.Menu;
					var increaseInterchangeNumberMenu = menu.MenuItems.FindByName("IncreaseInterchangeNumberMenuItem", true);
					UnitTestUserNotification.Instance.ClearMessages();
					increaseInterchangeNumberMenu.PerformClick();
					AssertContains(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					InsertTestRecord($"I{GlbCompany.CurrentCompany.GC_CustomsRegistrationNo}AAA336C", 123456);
					UnitTestUserNotification.Instance.ClearMessages();
					AssertNotContains(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		internal static void InsertTestRecord(string snName, long snValue)
		{
			Db.Connection.ExecuteNonQuery($"INSERT INTO dbo.StmNums (SN_Name, SN_Value, SN_SystemCreateTimeUtc) VALUES ('{snName}', {snValue}, GETUTCDATE());");
		}
	}
}

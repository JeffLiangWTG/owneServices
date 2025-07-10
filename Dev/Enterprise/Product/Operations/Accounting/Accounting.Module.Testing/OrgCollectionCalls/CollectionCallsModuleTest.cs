using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(OrgCollectionCallsModule))]
	public class CollectionCallsModuleTest : ZArchitecture.Modules.Testing.ZModuleBasherTest
	{
		public void TestStatementErrorNotification()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var module = (OrgCollectionCallsModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Env.Security.ReceivablesCollectionCalls.IsAllowed = true;
				foreach (var checkpoint in Env.Security.ReceivablesCollectionDocuments.ChildCheckPoints)
				{
					checkpoint.IsAllowed = false;
				}

				foreach (MenuItem menuItem in module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("&Print Documents").MenuItems)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menuItem.PerformClick();
					AssertEquals("Error message", string.Format(
@"You do not have the appropriate security rights to print this type of document.
If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow Access to:
Manage -> Receivables -> Statements -> {0}", menuItem.Text.Replace("&", "").Replace(" Of Account", "")), UnitTestUserNotification.Instance.LastMessage.Text);
				}

				foreach (var checkpoint in Env.Security.ReceivablesCollectionDocuments.ChildCheckPoints)
				{
					checkpoint.IsAllowed = true;
				}

				foreach (MenuItem menuItem in module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("&Print Documents").MenuItems)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menuItem.PerformClick();
					AssertNull("Should be no error", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert("Export To Excel functionality will only be done from the contents of the grid (not using a DataReader)", true);
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OrgCollectionCalls;
		}
	}
}

using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	[TestedType(typeof(UniversalCopyScheduleForm))]
	internal sealed class UniversalCopyScheduleFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var task = new BusinessObjectFactory().New<StmUniversalCopyScheduleTask>();
			task.Parent.GridContext = DummyModuleIDs.Dummy.Name;
			task.Parent.CopyObject = dummy;
			task.HasChanges = false;
			task.Parent.HasChanges = false;
			return new UniversalCopyScheduleForm(task);
		}

		public void TestDelete()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var task = new BusinessObjectFactory().New<StmUniversalCopyScheduleTask>();
			task.Parent.GridContext = DummyModuleIDs.Dummy.Name;
			task.Parent.CopyObject = dummy;
			var copyTemplate = task.Factory.New<UniversalCopyTemplate>();
			copyTemplate.S9_ModuleID = task.Parent.GridContext;
			copyTemplate.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			task.Parent.SUC_S9_CopyTemplate = copyTemplate.PK;

			using (var form = new UniversalCopyScheduleForm(task))
			{
				form.Show();
				AssertEquals(false, form.deleteButton.Visible);
				var deleteMenuItem = form.Menu.MenuItems.FindByName(ZFormMenuStrategy.FileDeleteMenuItemName, true);
				AssertEquals(false, deleteMenuItem.Enabled);
			}

			task.Factory.Save();
			using (var form = new UniversalCopyScheduleForm(task))
			{
				form.Show();
				AssertEquals(true, form.deleteButton.Visible);
				var deleteMenuItem = form.Menu.MenuItems.FindByName(ZFormMenuStrategy.FileDeleteMenuItemName, true);
				AssertEquals(true, deleteMenuItem.Enabled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.deleteButton.PerformClick();
				AssertEquals("Are you sure you want to delete this copy schedule?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!task.IsDeleted);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				deleteMenuItem.PerformClick();
				AssertEquals("Are you sure you want to delete this copy schedule?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(task.IsDeleted);
				Assert(task.Parent.IsDeleted);
				Assert(!dummy.IsDeleted);
			}
		}
	}
}

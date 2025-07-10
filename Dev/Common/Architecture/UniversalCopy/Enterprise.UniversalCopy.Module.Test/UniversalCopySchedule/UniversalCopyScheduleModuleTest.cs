using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.UniversalCopy.Business;
using Enterprise.UniversalCopy.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.Module.Testing
{
	[TestedType(typeof(UniversalCopyScheduleModule))]
	internal class UniversalCopyScheduleModuleBasherTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.UniversalCopySchedule;
		}

		public void TestControllerInstance()
		{
			var security = new UniversalCopySecurity(DummyModuleIDs.Dummy);
			bool wasAllowed = security.UniversalCopyCEDPrivateCheckpoint.IsAllowed;
			security.UniversalCopyCEDPrivateCheckpoint.IsAllowed = false;
			try
			{
				var copyTemplate = Factory.New<UniversalCopyTemplate>();
				copyTemplate.S9_ModuleID = DummyModuleIDs.Dummy.Name + ZArchitecture.Business.StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;
				var copy = Factory.New<StmUniversalCopy>();
				copy.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
				copy.SUC_S9_CopyTemplate = copyTemplate.PK;
				copy.SUC_CopyObjectId = Factory.New<DummyBusinessObject>().PK;
				var sched = Factory.New<StmUniversalCopyScheduleTask>();
				sched.S5_ParentID = copy.PK;
				sched.S5_ParentTableCode = StmUniversalCopySchema.Constants.Prefix;
				Factory.Save();

				using (var form = (EmbeddedModulePopup)new UniversalCopyScheduleModule().ShowPopup())
				{
					var filterControl = (UniversalCopyScheduleFilterControl)form.Controls.Find("UniversalCopyScheduleFilterControl", true)[0];
					filterControl.Find();
					filterControl.Grid.Select(0);

					const string expectedSecurityError =
@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

TESTING 1 2 3 -> Universal Copy -> Create/Edit/Delete Private Copy Template";

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.FindToolBarButtonByText("Edit").PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals(expectedSecurityError, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.FindToolBarButtonByText("Delete").PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals(expectedSecurityError, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.FindToolBarButtonByText("View").PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals(expectedSecurityError, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				security.UniversalCopyCEDPrivateCheckpoint.IsAllowed = true;
				using (var form = (EmbeddedModulePopup)new UniversalCopyScheduleModule().ShowPopup())
				{
					object formCreated = null;
					var formCreatedHandler = new EventHandler(delegate(object sender, EventArgs args)
					{ formCreated = sender; });
					CargoWise.Windows.UI.KForm.FormCreated += formCreatedHandler;
					try
					{
						var filterControl = (UniversalCopyScheduleFilterControl)form.Controls.Find("UniversalCopyScheduleFilterControl", true)[0];
						filterControl.Find();
						filterControl.Grid.Select(0);
						form.FindToolBarButtonByText("Edit").PerformClick();
						Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
						AssertType(typeof(UniversalCopyScheduleForm), formCreated);
						((IDisposable)formCreated).Dispose();
					}
					finally
					{
						CargoWise.Windows.UI.KForm.FormCreated -= formCreatedHandler;
					}
				}
			}
			finally
			{
				security.UniversalCopyCEDPrivateCheckpoint.IsAllowed = wasAllowed;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestSecurityCheckpointShouldNotBeNone()
		{
			using (UniversalCopyScheduleModule universalCopyScheduleModule = new UniversalCopyScheduleModule())
			{
				AssertEquals(universalCopyScheduleModule.SecurityCheckpoint, Env.Security.UniversalCopySchedules);
			}
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var copyTemplate = Factory.New<UniversalCopyTemplate>();
			copyTemplate.S9_ModuleID = DummyModuleIDs.Dummy.Name + ZArchitecture.Business.StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;
			var copy = Factory.New<StmUniversalCopy>();
			copy.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
			copy.SUC_S9_CopyTemplate = copyTemplate.PK;
			copy.SUC_CopyObjectId = Factory.New<DummyBusinessObject>().PK;
			var sched = Factory.New<StmUniversalCopyScheduleTask>();
			sched.S5_ParentID = copy.PK;
			sched.S5_ParentTableCode = StmUniversalCopySchema.Constants.Prefix;
			Factory.Save();
			collection.Add(copy);
		}
	}
}

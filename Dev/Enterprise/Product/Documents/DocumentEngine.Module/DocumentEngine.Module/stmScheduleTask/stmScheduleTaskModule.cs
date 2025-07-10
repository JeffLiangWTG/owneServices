using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Scheduler.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using ResString = Enterprise.DocumentEngine.Module.ResString;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public class ScheduledReportsModule : ZFilterGridModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ScheduledReports; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ScheduledReports);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new StmScheduleTaskFilterControl(GridCollection, (StmScheduleTaskFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ReportScheduleTaskCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new StmScheduleTaskFilterBusinessObject();
		}

		public override ToolBarButton[] ToolBarButtons
		{
			get
			{
				var buttons = base.ToolBarButtons;
				var runNowButton = Array.Find(buttons, (button) => button.Text == ScheduleNowMenuText);
				runNowButton.ImageIndex = Icons.GetImageIndex(IconTypes.Events);
				return buttons;
			}
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			var result = base.GetNewAdditionalMenuItems().ToList();
			result.Add(new ZMenuItem(ScheduleNowMenuText, ScheduleNowMenu_Click));
			return result.ToArray();
		}

		public override bool AllowDefaultActivateDeactivate => true;

		#region ScheduleNow

		static MultilingualString ScheduleNowMenuText
		{
			get { return ResString.GetMultilingualString("186c20c1-0603-4a8a-b1e3-6d5cdbf3e13d", "Schedule Now"); }
		}

		bool CanCurrentUserScheduleTasks(IEnumerable<StmScheduleTask> tasks)
		{
			return Env.Security.ScheduledTaskEditOtherReport.IsAllowed || tasks.All(x => x.S5_SystemCreateUser == Env.CurrentUser.Initials);
		}

		void ScheduleNowMenu_Click(object sender, EventArgs e)
		{
			var tasksToSchedule = new List<StmScheduleTask>();
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				tasksToSchedule.AddRange(SelectedBusinessObjects.Cast<StmScheduleTask>());
			}
			else if (CurrentBusinessObjectInGrid != null)
			{
				tasksToSchedule.Add(CurrentBusinessObjectInGrid as StmScheduleTask);
			}

			if (!CanCurrentUserScheduleTasks(tasksToSchedule))
			{
				Env.Security.ScheduledTaskEditOtherReport.ShowError();
				return;
			}

			var message = ResString.GetMultilingualString("F70CE733-2CBC-485A-8FCC-BACE9770BB6D", "Do you really want to schedule the selected schedule report(s) to run now?");
			var caption = ResString.GetMultilingualString("7B55F155-69D8-45D5-B711-227C1C935B46", "Schedule Now Confirmation");
			if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}

			if (tasksToSchedule.Count == 0)
			{
				ShowNoSelectedMessage();
				return;
			}

			foreach (var task in tasksToSchedule)
			{
				ScheduleNowCore(task);
			}
		}

		void ScheduleNowCore(StmScheduleTask stmScheduleTask)
		{
			var scheduleTaskClone = (StmScheduleTask)stmScheduleTask.Clone();
			scheduleTaskClone.S5_IsPrivate = true;
			scheduleTaskClone.CalcNextRunTimeLocal = ZDateTime.Now;
			scheduleTaskClone.Factory.Save();
		}

		#endregion

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ScheduledTask; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		public override bool AllowUniversalCopy => false;
	}
}

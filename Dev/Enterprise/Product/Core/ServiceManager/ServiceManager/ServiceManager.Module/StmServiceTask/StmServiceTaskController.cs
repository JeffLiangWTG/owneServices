using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.GUI;
using Enterprise.ServiceManager.Module.ScheduleNow;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;

namespace Enterprise.ServiceManager.Module
{
	public class StmServiceTaskController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Standard Controller Overrides

		public override ControllerID ID => ControllerIDs.StmServiceTask;

		public override Type TypeOfTopLevelBusinessObject => typeof(StmServiceTask);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new StmServiceTaskForm((StmServiceTask)businessEntity);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.StmServiceTask;

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ServiceTaskView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ServiceTaskNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ServiceTaskEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ServiceTaskDelete;

		#endregion

		#region Activate / Deactivate

		public void ActivateDeactivate(BusinessObject[] selectedElements, bool isActive)
		{
			var checkpoint = CheckPointForEdit;
			if (!checkpoint.IsAllowed)
			{
				checkpoint.ShowError();
				return;
			}

			if (selectedElements.Length == 0)
			{
				Globals.Message.Show(NoTasksSelectedMessage);
				return;
			}

			var message = Res.GetString("6C830E8B-A966-499D-84B1-FD9CB38FDB8E", "Do you really want to {0} the selected {1} task(s)?", isActive ? Res.GetString("B5BDC9B5-FA97-4BCD-A3A2-68C144688327", "activate") : Res.GetString("522F826A-D3D4-4F15-96CC-BE829B0D1759", "deactivate"), selectedElements.Length);
			var caption = isActive ? Res.GetString("068B24B7-72CD-45B4-BAFE-9E5CAABEE3EE", "Activate") : Res.GetString("BDAEF1CA-8B68-40FE-94B5-03811512D90E", "Deactivate");

			if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}

			var factory = new BusinessObjectFactory();
			foreach (StmServiceTask bo in selectedElements)
			{
				var taskSchedule = factory.Load<StmServiceTask>(bo.PK);
				if (taskSchedule != null && taskSchedule.SST_Active != isActive)
				{
					taskSchedule.SST_Active = isActive;
					if (taskSchedule.SST_ActiveInfo.HasErrors())
					{
						Globals.Message.ShowError(taskSchedule.SST_ActiveInfo.GetErrors().ToMessageListString(), taskSchedule.SST_ServiceTaskCode);
						taskSchedule.SST_Active = !isActive;
					}
				}
			}
			factory.Save();
		}

		#endregion

		#region ScheduleNow

		public void ScheduleNow(IEnumerable<BusinessObject> selectedElements, IScheduleNowController scheduleNowController, bool forceRestart)
		{
			_ = selectedElements ?? throw new ArgumentNullException(nameof(selectedElements));
			_ = scheduleNowController ?? throw new ArgumentNullException(nameof(scheduleNowController));

			var checkpoint = CheckPointForView; // re-using view security checkpoint. if you can view it, you can action on it. but you cannot edit it.
			if (!checkpoint.IsAllowed)
			{
				checkpoint.ShowError();
				return;
			}

			var tasks = selectedElements
				.OfType<StmServiceTask>()
				.Select(bo => new TaskCodeDTO(bo.SST_ServiceTaskCode))
				.ToList();

			if (tasks.Count <= 0)
			{
				Globals.Message.ShowError(string.Concat(Res.GetString("00FA98F8-CB51-44C0-B4F5-B8A4B13BFED0", "Failed to Schedule Tasks"), ":\r\n\r\n", NoTasksSelectedMessage));
				return;
			}

			var upgradeServiceTaskInList = tasks.Any(a => a.Code == "UPG");
			var upgradeMessage = Res.GetString("BD616F4E-A1CE-4351-B316-CFF102591398", "One of the selected tasks is System Upgrade service task. Triggering this task may cause the system to be upgraded to a new version. Do you really want to continue? Type 'yes' to proceed.");
			var message = Res.GetString("6C830E8B-A966-499D-84B1-FD9CB38FDB8E", "Do you really want to {0} the selected {1} task(s)?", Res.GetString("29F7A433-E0A3-4120-B90F-C34ED175B703", "schedule"), tasks.Count);
			var caption = Res.GetString("88D9E91F-22C9-4CF2-B6E1-E35518114ABC", "Run");

			var condition = upgradeServiceTaskInList
				? Globals.Message.ShowConfirmation(upgradeMessage, caption, "yes", MessageBoxIcon.Warning) == DialogResult.OK
				: Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

			if (!condition)
			{
				return;
			}

			scheduleNowController.ScheduleNow(tasks.Select(dto => dto.Code), s => Globals.Message.ShowInformation(s), forceRestart);
		}

		#endregion

		static string NoTasksSelectedMessage => Res.GetString("8954574F-AA77-4009-A965-5466898D8E21", "There are no Tasks Selected.");
	}
}

using System;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI
{
	public partial class ReportUserControl : ZEmbeddedModuleControl
	{
		public ReportUserControl()
		{
			InitializeComponent();
		}

		public ReportUserControl(ModuleIdentifier moduleId, ReportCommandCollection reports, ISecurityCheckpoint securityCheckpoint)
			: this()
		{
			this.moduleId = moduleId;
			this.Reports = reports;
			reports.SetAllowNew(false);
			this.securityCheckpoint = securityCheckpoint;
			SetDataBinding(reports, "");

			ReportGrid.ContextMenu.MenuItems.Add(0, new ZMenuItem(ResString.GetMultilingualString("bee0ffaa-44a0-4a76-9451-41fc646bb11c", "Run"), new EventHandler(HandleRunReport)));
			ReportGrid.ContextMenu.MenuItems.Add(1, new ZMenuItem(ResString.GetMultilingualString("7b0b6515-fb2e-467e-9b23-87e8c1caf774", "Schedule"), new EventHandler(HandleScheduleReport)));
			ReportGrid.DoubleClick += new EventHandler(HandleRunReport);
		}
		readonly ISecurityCheckpoint securityCheckpoint;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (Reports == null)
			{
				ErrorReporter.ReportOnce("ReportUserControl_NoReport", new InvalidOperationException("A ReportUserControl was created without a Report Collection"));
			}
			else if (!DesignModeFinder.IsDesigning)
			{
				var customizeReportCheckpoint = securityCheckpoint == Env.Security.None ?
					Env.Security.None :
					Env.Security.FindOrCreateReportCustomizeCheckpoint(moduleId, securityCheckpoint);

				var maker = Maker(customizeReportCheckpoint);
				maker.Make(ReportGrid.ContextMenu.MenuItems);
			}
		}

#if DEBUG
		public virtual
#endif
		ReportCustomisationMenusMaker Maker(ISecurityCheckpoint customizeReportCheckpoint)
		{
			return new ReportCustomisationMenusMaker(ParentForm, customizeReportCheckpoint, Reports.BusinessContext);
		}

		readonly ModuleIdentifier moduleId;
		protected ReportCommandCollection Reports;

		protected void HandleRunReport(object sender, EventArgs e)
		{
			if (ReportGrid.ListManager.Count > 0)
			{
				ReportCommand command = (ReportCommand)ReportGrid.ListManager.GetCurrent();
				if (CheckShouldRun(command))
				{
					command = (ReportCommand)command.Factory.CreateNewFactory().ImportFromAnotherFactory((ReportCommand)ReportGrid.ListManager.GetCurrent());
					ReportPrintSet set = GetNewPrintSet(command);
					PrintReportSet(set);
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("d7bd2fc7-cace-4447-b9a9-8c295eb53b3f", "There are no reports to print."), Res.GetString("a74410a5-e4a1-40ce-a2cc-65f622144f42", "Error"));
			}
		}

		protected virtual void PrintReportSet(ReportPrintSet set)
		{
			set.Run(Env.Security.None);
		}

		protected virtual ReportPrintSet GetNewPrintSet(ReportCommand command)
		{
			return new ReportPrintSet(command);
		}

		void HandleScheduleReport(object sender, EventArgs e)
		{
			if (ReportGrid.ListManager.Count > 0)
			{
				if (Env.Security.ScheduledTaskNew.IsAllowed)
				{
					ReportCommand command = (ReportCommand)ReportGrid.ListManager.GetCurrent();
					if (CheckShouldRun(command))
					{
						command = (ReportCommand)command.Factory.CreateNewFactory().ImportFromAnotherFactory((ReportCommand)ReportGrid.ListManager.GetCurrent());
						ReportPrintSet set = GetNewPrintSet(command);
						set.Schedule();
					}
				}
				else
				{
					Env.Security.ScheduledTaskNew.ShowError();
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("44691ed3-452b-4f6e-b7cd-c504b1bfbfd0", "There are no reports to schedule."), Res.GetString("a74410a5-e4a1-40ce-a2cc-65f622144f42", "Error"));
			}
		}

		bool CheckShouldRun(ReportCommand command)
		{
			bool result = true;
			if (securityCheckpoint != Env.Security.None)
			{
				var checkpoint = Env.Security.FindOrCreateReportCheckpoint(command.PK.ToGuid(), command.SU_MenuNameMultilingual, this.moduleId, securityCheckpoint);
				result = checkpoint.IsAllowed;
				if (!result)
				{
					checkpoint.ShowError();
				}
			}
			return result;
		}
	}
}

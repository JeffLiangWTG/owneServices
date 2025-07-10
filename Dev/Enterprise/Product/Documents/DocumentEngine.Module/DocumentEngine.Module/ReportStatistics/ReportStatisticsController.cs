using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.GUI.Scheduler;
using Enterprise.Environment;
using Enterprise.Scheduler.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public class ReportStatisticsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ReportStatistics; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ReportStatistics; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StmReportRun); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ReportStatisticsForm((StmReportRun)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ReportStatisticsView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ReportStatistics; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ReportStatistics; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReportStatistics; }
		}
	}
}

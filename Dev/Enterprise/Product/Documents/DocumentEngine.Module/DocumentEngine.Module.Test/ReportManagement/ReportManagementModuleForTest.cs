using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Scheduler.Module.Testing
{
	sealed class ReportManagementModuleForTest : ReportManagementModule
	{
		public new BusinessObjectFactory Factory => base.Factory;

		public IFilterControl NewFilterControl => GetNewFilterControl();

		public IBusinessObjectCollection NewGridCollection => GetNewGridCollection();

		public FilterBusinessObject NewFilterBusinessObject => GetNewFilterBusinessObject();

		public new MenuItem[] GetNewAdditionalContextMenuItems()
		{
			return base.GetNewAdditionalContextMenuItems();
		}

		public new MenuItem[] ContextMenu => base.ContextMenu;

		public new void CancelScheduleReportCore(BusinessObject[] scheduleReports, bool markAsInactive)
		{
			base.CancelScheduleReportCore(scheduleReports, markAsInactive);
		}
	}
}

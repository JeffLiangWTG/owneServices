using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.GUI
{
	public static class ComplianceLogTabHelper
	{
		public static void AddLogTabIfNeeded(BusinessObject parentJob, ZLogsTabPage zLogsTabPage, ZUserControl dpsLogsUserControl)
		{
			if (parentJob is IComplianceItemRiskStatusProvider provider && provider.IsEnabledComplianceWise)
			{
				var showRemovedCommoditiesTabPage = (parentJob is not IForwardingConsol) && parentJob.CommodityScreeningEnabledForLogTab();
				var complianceLogUserControl = new ComplianceLogUserControl(dpsLogsUserControl, showRemovedCommoditiesTabPage);
				complianceLogUserControl.SetBindingMember(".");
				zLogsTabPage.AddAdditionalTab(Res.GetString("8FDF273F-5EB2-4B88-826C-950C680B13F2", "Compliance Logs"), complianceLogUserControl, excludeFromBindingOnSave: true);
			}
			else
			{
				zLogsTabPage.AddAdditionalTab(Res.GetString("53E05CA2-E7FE-4BB5-A460-C800DB6E94A5", "Denied Party Screening Logs"), dpsLogsUserControl);
			}
		}
	}
}

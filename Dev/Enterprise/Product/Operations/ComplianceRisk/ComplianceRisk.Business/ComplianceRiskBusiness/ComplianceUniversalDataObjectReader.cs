using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceUniversalDataObjectReader
	{
		public ComplianceUniversalDataObjectReader(IBusiness businessObject)
		{
			if (businessObject is IComplianceItemRiskStatusProvider complianceRiskProvider
				&& complianceRiskProvider.IsEnabledComplianceWise)
			{
				ComplianceRiskBusinessObject = new(businessObject);
				ComplianceRiskBusinessObject.RefreshData();
			}
		}

		/// <summary>
		/// Synchronize the compliance risk status to latest if current status is out of date.
		/// Please see "<see cref="https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/4042/ComplianceWise-Plug-in"/>" for more details.
		/// </summary>
		public void SynchronizeComplianceRiskStatusIfNeeded()
		{
			if (ComplianceRiskBusinessObject != null)
			{
				ComplianceRiskStatusSynchronizer.Synchronize(ComplianceRiskBusinessObject);
			}
		}

		public void InitializeComplianceMaterialChangesSnapshotIfNeeded()
		{
			ComplianceRiskBusinessObject?.InitializeComplianceMaterialChangesSnapshotForUniversalDataTransfer();
		}

		internal ComplianceRiskBusinessObject ComplianceRiskBusinessObject { get; }
	}
}

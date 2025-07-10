using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceJobEntityCache : AutoComplianceJobEntityCache
	{
		public ComplianceJobEntityCache(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("ComplianceRiskStatus")]
		public override ZGuid CJE_COR_ComplianceRisk
		{
			get => base.CJE_COR_ComplianceRisk;
			set => base.CJE_COR_ComplianceRisk = value;
		}

		public ComplianceRiskStatus ComplianceRiskStatus => Factory.Load<ComplianceRiskStatus>(CJE_COR_ComplianceRisk.IsValid
			? CJE_COR_ComplianceRisk : (ZGuid)CJE_COR_ComplianceRiskInfo.OriginalValue);
	}
}

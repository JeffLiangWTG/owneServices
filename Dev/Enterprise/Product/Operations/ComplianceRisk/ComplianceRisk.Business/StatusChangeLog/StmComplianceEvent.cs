using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business
{
	public class StmComplianceEvent : AutoStmComplianceEvent
	{
		public StmComplianceEvent(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			SCE_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			SCE_EventType = AutoEvents.StatusUpdated.Code;
			SCE_EventSubType = ComplianceRiskStatusCodeList.Codes.OverrideClear;
		}
#endif
	}
}

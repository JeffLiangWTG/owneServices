using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ComplianceRisk.Integration.ComplianceEventList;

namespace Enterprise.ComplianceRisk.Business
{
	public class RemovedCommoditiesLogCollection : NonPersistentBusinessObjectCollection<RemovedCommoditiesLog>
	{
		readonly IComplianceItemRiskStatusProvider complianceRiskStatusProvider;

		public RemovedCommoditiesLogCollection(IComplianceItemRiskStatusProvider complianceRiskStatusProvider)
			: base(complianceRiskStatusProvider.Factory)
		{
			this.complianceRiskStatusProvider = complianceRiskStatusProvider;
		}

		public void LoadCollection()
		{
			using (SuspendListChanged())
			{
				RemoveAndDeleteAll();

				var query = new ZQuery(StmComplianceEventSchema.SCE_ParentID, complianceRiskStatusProvider.ParentID)
					.AddToFilter(StmComplianceEventSchema.SCE_ParentTableCode, complianceRiskStatusProvider.ParentTableCode)
					.AddToFilter(StmComplianceEventSchema.SCE_EventType, EventType.ComplianceCommodityInteraction)
					.AddToFilter(StmComplianceEventSchema.SCE_EventSubType, Codes.CommodityLineDeleted);
				Factory.Load<StmComplianceEvent>(query).ForEach(stmComplianceEvent => Add(new RemovedCommoditiesLog(stmComplianceEvent)));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => RemovedCommoditiesLog.EmptyLog;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}

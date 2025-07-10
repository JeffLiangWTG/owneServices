using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRiskStatusChangeLogCollection : NonPersistentBusinessObjectCollection<ComplianceRiskStatusChangeLog>
	{
		public ComplianceRiskStatusChangeLogCollection(IComplianceItemRiskStatusProvider complianceRiskStatusProvider)
			: base(complianceRiskStatusProvider.Factory)
		{
			ComplianceRiskStatusProvider = complianceRiskStatusProvider;
		}

		public void LoadCollection()
		{
			using (SuspendListChanged())
			{
				RemoveAndDeleteAll();

				var query = new ZQuery(StmComplianceEventSchema.SCE_ParentID, ComplianceRiskStatusProvider.ParentID);
				query.AddToFilter(StmComplianceEventSchema.SCE_EventType, AutoEvents.StatusUpdatedCode);
				query.AddToFilter(StmComplianceEventSchema.SCE_EventSubType, new[] { ComplianceEventList.Codes.OverallRisk, ComplianceEventList.Codes.ComplianceDecisionChanged });
				var sortedEvents = Factory.Load<StmComplianceEvent>(query).OrderBy(u => u.SCE_SystemCreateTimeUtc).ToArray();

				for (var i = 0; i < sortedEvents.Length; i++)
				{
					var eventCurrent = sortedEvents[i];

					if (i < sortedEvents.Length - 1)
					{
						var eventNext = sortedEvents[i + 1];
						var interval = eventNext.SCE_SystemCreateTimeUtc - eventCurrent.SCE_SystemCreateTimeUtc;

						if (interval.TotalSeconds < 1.0
							&& eventCurrent.SCE_NewValue == eventNext.SCE_NewValue
							&& (eventCurrent.SCE_EventSubType == ComplianceEventList.Codes.OverallRisk && eventNext.SCE_EventSubType == ComplianceEventList.Codes.ComplianceDecisionChanged
								|| eventCurrent.SCE_EventSubType == ComplianceEventList.Codes.ComplianceDecisionChanged && eventNext.SCE_EventSubType == ComplianceEventList.Codes.OverallRisk))
						{
							if (eventNext.SCE_EventSubType == ComplianceEventList.Codes.ComplianceDecisionChanged)
							{
								Add(new ComplianceRiskStatusChangeLog(eventNext));
							}
							else
							{
								Add(new ComplianceRiskStatusChangeLog(eventCurrent));
							}

							i++;
							continue;
						}
					}

					Add(new ComplianceRiskStatusChangeLog(eventCurrent));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => ComplianceRiskStatusChangeLog.EmptyLog;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		IComplianceItemRiskStatusProvider ComplianceRiskStatusProvider { get; }
	}
}

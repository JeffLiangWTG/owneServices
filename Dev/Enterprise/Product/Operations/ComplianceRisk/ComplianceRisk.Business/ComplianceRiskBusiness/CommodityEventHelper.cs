using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Newtonsoft.Json;
using static Enterprise.ComplianceRisk.Integration.ComplianceEventList;

namespace Enterprise.ComplianceRisk.Business
{
	class CommodityEventHelper
	{
		internal CommodityEventHelper(ComplianceRiskStatus complianceRiskStatus)
		{
			ComplianceRiskStatus = complianceRiskStatus;
		}

		internal void AddCommodityAdditionLogForBillingWhenAssessmentInitialized(bool initializedByUser)
		{
			var allCommodities = GetCommodities(isInitialized: true);

			if (allCommodities.Length > 0)
			{
				AddAdditionEvent(allCommodities, initializedByUser);
			}
		}

		internal static void AddCommodityAdditionAndDeletionLogIfNeeded(ComplianceRiskStatus complianceRiskStatus)
		{
			complianceRiskStatus.CommodityEventHelper.AddCommodityAdditionAndDeletionLogIfNeeded();
		}

		void AddCommodityAdditionAndDeletionLogIfNeeded()
		{
			if (ComplianceRiskStatus.IsAssessmentInitialized)
			{
				var newCommodities = GetCommodities(isInitialized: false);

				if (newCommodities.Length > 0)
				{
					AddAdditionEvent(newCommodities);
				}
			}

			if (RemovedCommodities.Commodities.Count > 0)
			{
				AddEvent(EventType.ComplianceCommodityInteraction, Codes.CommodityLineDeleted, JsonConvert.SerializeObject(RemovedCommodities), RemovedCommodities.Commodities.Count);
				RemovedCommodities.Commodities.Clear();
			}
		}

		ZGuid[] GetCommodities(bool isInitialized)
		{
			return ComplianceCheckRequestModelBuilder.GetApplicableCommodityDetails(ComplianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>())
				.Where(c => isInitialized || !c.IsInDatabase)
				.Select(u => u.PK)
				.Where(v => !additionCommodities.Contains(v))
				.ToArray();
		}

		internal void AddRemovedCommodity(Commodity commodity)
		{
			RemovedCommodities.Commodities.Add(commodity);
		}

		void AddAdditionEvent(ZGuid[] commodities, bool initializedByUser = true)
		{
			AddEvent(Codes.ComplianceWiseValueObtained, Codes.ComplianceWiseCommoditiesValueObtained, string.Empty, commodities.Length, initializedByUser);

			// Prevent duplicate billing
			foreach (var commodity in commodities)
			{
				additionCommodities.Add(commodity);
			}
		}

		void AddEvent(string eventType, string eventSubType, string snapshot, int itemsCount, bool initializedByUser = true)
		{
			var eventLog = ComplianceRiskStatus.Factory.New<StmComplianceEvent>();
			eventLog.SCE_ParentID = ComplianceRiskStatus.COR_ParentID;
			eventLog.SCE_ParentTableCode = ComplianceRiskStatus.COR_ParentTableCode;
			eventLog.SCE_EventTimeOffset = ZDateTimeOffset.Now;
			eventLog.SCE_EventType = eventType;
			eventLog.SCE_EventSubType = eventSubType;
			eventLog.SCE_ItemsCount = itemsCount;
			eventLog.SCE_Snapshot = snapshot;

			if (!initializedByUser)
			{
				var userCode = ZArchitecture.Environment.User.ServiceUserCode;
				eventLog.SCE_SystemCreateUser = userCode;
				eventLog.SCE_SystemLastEditUser = userCode;
			}
		}

		RemovedCommoditiesSnapshot RemovedCommodities { get; } = new();

		ComplianceRiskStatus ComplianceRiskStatus { get; }

		readonly HashSet<ZGuid> additionCommodities = [];
	}
}

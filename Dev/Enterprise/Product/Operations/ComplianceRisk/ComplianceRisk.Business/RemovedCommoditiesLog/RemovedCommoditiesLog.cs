using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using static Enterprise.ComplianceRisk.Integration.ComplianceEventList;

namespace Enterprise.ComplianceRisk.Business
{
	public class RemovedCommoditiesLog : NonPersistentBusinessObject
	{
		readonly Lazy<ComplianceCommodityRiskLogCollection> lazyComplianceCommodityRiskLogCollection;

		RemovedCommoditiesLog()
		{
			lazyComplianceCommodityRiskLogCollection = new(() => []);
		}

		public RemovedCommoditiesLog(StmComplianceEvent stmComplianceEvent) : base(stmComplianceEvent.Factory)
		{
			EventDateTime = stmComplianceEvent.SCE_EventTimeOffset.IsValid ? stmComplianceEvent.SCE_EventTimeOffset.ToDateTime() : ZDateTime.Empty;
			PostedDateTimeLocal = stmComplianceEvent.SCE_SystemCreateTimeUtc.ToLocalBranchTime();
			User = ((IGlbStaff)Factory.LoadFromNaturalKey(ObjectFactory.GetType<IGlbStaff>(), GlbStaffSchema.GS_Code, stmComplianceEvent.SCE_SystemCreateUser))?.GS_FullName ?? stmComplianceEvent.SCE_SystemCreateUser;
			lazyComplianceCommodityRiskLogCollection = new(() => InitCommodityRiskLogCollection(stmComplianceEvent));
		}

		public static RemovedCommoditiesLog EmptyLog => new();

		public ComplianceCommodityRiskLogCollection CommodityRiskLogCollection => lazyComplianceCommodityRiskLogCollection.Value;

		[ResourceStringData("4bddbfea-968b-414d-a942-0d5dc620d45e", Caption = "Event Date & Time")]
		public ZDateTime EventDateTime { get; }

		[ResourceStringData("27e531dd-c3ca-4415-8347-56d3a7eac50a", Caption = "Posted Date & Time(Local)")]
		public ZDateTime PostedDateTimeLocal { get; }

		[ResourceStringData("1bce0fe8-53b6-450a-8fc7-7f06f8e34c32", Caption = "By Whom")]
		public ZString User { get; }

		ComplianceCommodityRiskLogCollection InitCommodityRiskLogCollection(StmComplianceEvent stmComplianceEvent)
		{
			var collection = new ComplianceCommodityRiskLogCollection();
			if (stmComplianceEvent.SCE_EventType == EventType.ComplianceCommodityInteraction &&
				stmComplianceEvent.SCE_EventSubType == Codes.CommodityLineDeleted &&
				!stmComplianceEvent.SCE_Snapshot.IsEmpty)
			{
				try
				{
					var snapshot = JsonConvert.DeserializeObject<RemovedCommoditiesSnapshot>(stmComplianceEvent.SCE_Snapshot);
					snapshot?.Commodities?.ForEach(u => collection.Add(new ComplianceCommodityRiskLog(u)));
				}
				catch (JsonException ex)
				{
					ExceptionReporter.Instance.ReportDeveloperException("DeserializeCommoditiesHistorySnapshot", $"Failed to deserialize: {stmComplianceEvent.SCE_Snapshot}", ex);
				}
			}

			return collection;
		}
	}
}

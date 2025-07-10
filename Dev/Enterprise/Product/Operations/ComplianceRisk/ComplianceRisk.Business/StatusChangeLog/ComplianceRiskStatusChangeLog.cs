using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRiskStatusChangeLog : NonPersistentBusinessObject
	{
		ComplianceRiskStatusChangeLog()
		{
		}

		public ComplianceRiskStatusChangeLog(StmComplianceEvent stmComplianceEvent) : base(stmComplianceEvent.Factory)
		{
			OverallRisk = stmComplianceEvent.SCE_NewValue;
			EventDateTime = stmComplianceEvent.SCE_EventTimeOffset.IsValid ? stmComplianceEvent.SCE_EventTimeOffset.ToDateTime() : ZDateTime.Empty;
			PostedDateTimeLocal = stmComplianceEvent.SCE_SystemCreateTimeUtc.ToLocalBranchTime();
			User = ((IGlbStaff)Factory.LoadFromNaturalKey(ObjectFactory.GetType<IGlbStaff>(), GlbStaffSchema.GS_Code, stmComplianceEvent.SCE_SystemCreateUser))?.GS_FullName ?? stmComplianceEvent.SCE_SystemCreateUser;
			Init(stmComplianceEvent);
		}

		public static ComplianceRiskStatusChangeLog EmptyLog => new();

		public ZString OverallRisk { get; }

		[ResourceStringData("968BC74B-52C7-436D-AAE6-02F0C3AD83B4", Caption = "Job Compliance status")]
		public ZString OverallRiskDescription => GetRiskStatusDescription(OverallRisk);

		[ResourceStringData("F7243682-958B-47ED-A2F5-FC54A0DBA8F8", Caption = "Event Date & Time")]
		public ZDateTime EventDateTime { get; }

		[ResourceStringData("C08CA4D1-63D8-4AF4-8A1E-385EB727352D", Caption = "Posted Date & Time(Local)")]
		public ZDateTime PostedDateTimeLocal { get; }

		[ResourceStringData("B9DC757C-0AAC-43EC-AAEC-4D4C610B1D64", Caption = "Cleared Reason")]
		public ZString ClearedReason { get; private set; }

		[ResourceStringData("A5834C68-C437-423C-98C6-6922BBBB9218", Caption = "By Whom")]
		public ZString User { get; }

		public ZString PartyRisk { get; private set; }

		public ZString PartyRiskDescription => GetRiskStatusDescription(PartyRisk);

		public ZString LocationRisk { get; private set; }

		public ZString LocationRiskDescription => GetRiskStatusDescription(LocationRisk);

		public ZString CommodityRisk { get; private set; }

		public ZString CommodityRiskDescription => GetRiskStatusDescription(CommodityRisk);

		public ZString AssessmentRisk { get; private set; }

		public ZString AssessmentRiskDescription => GetRiskStatusDescription(AssessmentRisk);

		public CompliancePartyRiskLogCollection PartyRiskLogCollection { get; } = [];

		public ComplianceLocationRiskLogCollection LocationRiskLogCollection { get; } = [];

		public ComplianceCommodityRiskLogCollection CommodityRiskLogCollection { get; } = [];

		public ComplianceJobDirection ComplianceJobDirection { get; } = new();

		public ZBool IsComplianceCommodityRiskProvider { get; private set; }

		public ZBool IsCompliancePartyRiskProvider { get; private set; }

		public ZBool IsComplianceLocationRiskProvider { get; private set; }

		void Init(StmComplianceEvent stmComplianceEvent)
		{
			if ((stmComplianceEvent.SCE_EventSubType == ComplianceEventList.Codes.ComplianceDecisionChanged || OverallRisk == Codes.OverrideClear)
				&& !stmComplianceEvent.SCE_Snapshot.IsEmpty)
			{
				SnapshotExists = true;
				ComplianceAuditSnapshot snapshot = null;
				try
				{
					snapshot = JsonConvert.DeserializeObject<ComplianceAuditSnapshot>(stmComplianceEvent.SCE_Snapshot);
				}
				catch (JsonException ex)
				{
					ExceptionReporter.Instance.ReportDeveloperException("DeserializeComplianceAuditSnapshot", $"Failed to deserialize: {stmComplianceEvent.SCE_Snapshot}", ex);
				}

				if (snapshot != null)
				{
					var stmLogPKs = (snapshot.Parties?.Select(u => u.LogPK) ?? Enumerable.Empty<Guid>()).Where(u => u != Guid.Empty).Distinct();
					var stmEntityScreeningLogs = Factory.Load<IStmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PK, stmLogPKs));

					snapshot.Parties?.ForEach(u => PartyRiskLogCollection.Add(new CompliancePartyRiskLog(u, ScreeningStatusesList, stmEntityScreeningLogs.FirstOrDefault(v => v.PK == u.LogPK))));
					snapshot.Countries?.ForEach(u => LocationRiskLogCollection.Add(new ComplianceLocationRiskLog(u, snapshot.LocationRisk)));
					snapshot.Commodities?.ForEach(u => CommodityRiskLogCollection.Add(new ComplianceCommodityRiskLog(u)));
					PartyRisk = snapshot.PartyRisk ?? (snapshot.Parties?.Any(u => OrganisationsDataRegistry.ScreeningStatusNotClear(u.Status)) ?? false ? Codes.PotentialRisk : Codes.Clear);
					LocationRisk = snapshot.LocationRisk ?? (snapshot.Countries?.Any(u => u.IsSanctioned) ?? false ? Codes.PotentialRisk : Codes.Clear);
					CommodityRisk = snapshot.CommodityRisk ?? GetCommodityRisk(snapshot);
					ClearedReason = string.Join(", ", new[] { snapshot.OverrideDecision?.Code, snapshot.OverrideDecision?.Description, snapshot.OverrideDecision?.Reason }.Where(r => !string.IsNullOrWhiteSpace(r)));
					ComplianceJobDirection.IsInternational = snapshot.ComplianceJobDirection?.IsInternational ?? false;
					ComplianceJobDirection.Direction = snapshot.ComplianceJobDirection?.Direction;
					IsCompliancePartyRiskProvider = snapshot.IsCompliancePartyRiskProvider;
					IsComplianceLocationRiskProvider = snapshot.IsComplianceLocationRiskProvider;
					IsComplianceCommodityRiskProvider = snapshot.IsComplianceCommodityRiskProvider;
				}
			}
		}

		public bool SnapshotExists { get; private set; }

		string GetCommodityRisk(ComplianceAuditSnapshot snapshot)
		{
			if (snapshot.IsComplianceCommodityRiskProvider && (snapshot.ComplianceJobDirection?.IsInternational ?? false))
			{
				if (snapshot.Commodities?.Count > 0)
				{
					return snapshot.Commodities
						.Any(u => u.RiskStatus.HasCommodityRiskFactor())
						? Codes.PotentialRisk : Codes.Clear;
				}

				return Codes.Incomplete;
			}

			return Codes.NotApplicable;
		}

		string GetRiskStatusDescription(string riskCode) => StatusCodeList.ContainsCode(riskCode) ?
			StatusCodeList[riskCode].Description :
			Res.GetString("b3cdd712-2fd1-41c8-88f6-3f92cb2d1d9f", "Not Available");

		ComplianceRiskStatusCodeList StatusCodeList
		{
			get
			{
				return statusCodeList ??= Factory.GetCachedValue("ComplianceRiskStatusChangeLog|StatusCodeList", () => new ComplianceRiskStatusCodeList());
			}
		}
		ComplianceRiskStatusCodeList statusCodeList;

		public ScreeningStatusesList ScreeningStatusesList
		{
			get { return screeningStatusesList ??= Factory.GetCachedValue("ComplianceRiskStatusChangeLog|ScreeningStatuses", () => new ScreeningStatusesList()); }
		}
		ScreeningStatusesList screeningStatusesList;
	}
}

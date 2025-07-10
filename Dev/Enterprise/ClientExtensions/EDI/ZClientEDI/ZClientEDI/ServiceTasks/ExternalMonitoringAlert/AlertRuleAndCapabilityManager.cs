using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert
{
	class AlertRuleAndCapabilityManager
	{
		readonly DbConnection wiseGridReportingConnection;

		public AlertRuleAndCapabilityManager(DbConnection wiseGridReportingConnection)
		{
			this.wiseGridReportingConnection = wiseGridReportingConnection;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public IEnumerable<AlertDefault> LoadAlertDefaults()
		{
			var alertDefaults = new List<AlertDefault>();
			using (var wiseGridReportingGetCapabilityCommand = wiseGridReportingConnection.Command("SELECT AD_Name, AD_Value FROM dbo.AlertDefault"))
			using (var reader = wiseGridReportingGetCapabilityCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					alertDefaults.Add(new AlertDefault
					{
						Name = (string)reader["AD_Name"],
						Value = (string)reader["AD_Value"]
					});
				}
			}
			return alertDefaults;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public IEnumerable<AlertCapability> LoadAlertCapabilities(out Dictionary<string, int> capabilityCounts)
		{
			var alertCapabilities = new List<AlertCapability>();
			using (var wiseGridReportingGetCapabilityCommand = wiseGridReportingConnection.Command("SELECT AC_PK, AC_Capability, AC_Product, AC_Module, AC_CapabilityId, AC_ProgramArea, AC_Priority, AC_ChangeType, AC_MaxTargets FROM dbo.AlertCapability"))
			using (var reader = wiseGridReportingGetCapabilityCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					alertCapabilities.Add(new AlertCapability
					{
						PK = (Guid)reader["AC_PK"],
						Capability = (string)reader["AC_Capability"],
						Product = (string)reader["AC_Product"],
						Module = (string)reader["AC_Module"],
						ProgramArea = (string)reader["AC_ProgramArea"],
						Priority = (string)reader["AC_Priority"],
						ChangeType = (string)reader["AC_ChangeType"],
						CapabilityId = (Guid)reader["AC_CapabilityId"],
						MaxTargets = reader["AC_MaxTargets"] != DBNull.Value ? Convert.ToInt32(reader["AC_MaxTargets"], CultureInfo.InvariantCulture) : 0
					});
				}
			}

			var alertCapabilitiesWithoutMaxTargets = alertCapabilities.Where(x => x.MaxTargets == 0).ToArray();
			capabilityCounts = GetCapabilityUserCountFromCW1(alertCapabilities.Select(x => x.Capability));
			foreach (var alertCapability in alertCapabilitiesWithoutMaxTargets)
			{
				if (capabilityCounts.TryGetValue(alertCapability.Capability, out var count))
				{
					alertCapability.MaxTargets = count;
				}
			}
			return alertCapabilities;
		}

		static Dictionary<string, int> GetCapabilityUserCountFromCW1(IEnumerable<string> capabilityCodes)
		{
			var boFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var capabilityCollection = new DynamicBusinessObjectCollection(boFactory);
			const string ColumnCount = "Count";
			var capabilityCodeList = string.Join(",", capabilityCodes.Select(x => FormattableString.Invariant($"'{x}'")));
			var capabilityQuery = FormattableString.Invariant($@"
SELECT {GlbCapability.Schema.G4_Code}, COUNT(*) AS {ColumnCount} FROM {GlbResourceCapabilityPivotSchema.Constants.SqlSchemaName}.{GlbResourceCapabilityPivotSchema.Constants.TableName} INNER JOIN {GlbCapabilitySchema.Constants.SqlSchemaName}.{GlbCapabilitySchema.Constants.TableName} ON {GlbResourceCapabilityPivot.Schema.G5_G4_Capability} = {GlbCapability.Schema.PK}
	INNER JOIN {GlbStaffSchema.Constants.SqlSchemaName}.{GlbStaffSchema.Constants.TableName} ON {GlbResourceCapabilityPivot.Schema.G5_GS_Resource} = {GlbStaff.Schema.PK}
	WHERE {GlbStaff.Schema.GS_IsActive} = 1 AND {GlbStaff.Schema.GS_IsResource} = 0 AND {GlbStaff.Schema.GS_IsSystemAccount} = 0 AND {GlbStaff.Schema.GS_CanLogin} = 1
		AND {GlbCapability.Schema.G4_Code} IN ({capabilityCodeList})
	GROUP BY {GlbCapability.Schema.G4_Code}");
			capabilityCollection.Load(capabilityQuery);
			return capabilityCollection.Select(x => new KeyValuePair<string, int>(Convert.ToString(x[GlbCapability.Schema.G4_Code], CultureInfo.InvariantCulture), (ZInt)(x[ColumnCount]))).ToDictionary(x => x.Key, x => x.Value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public IList<AlertRule> LoadAlertRules(AlertRuleType type)
		{
			var alertRules = new List<AlertRule>();
			using (var wiseGridReportingGetCapabilityCommand = wiseGridReportingConnection.Command("EXEC dbo.usp_GetAlertRules @Type"))
			{
				wiseGridReportingGetCapabilityCommand.AddParameter("@Type", SqlDbType.Int, (int)type);
				using (var reader = wiseGridReportingGetCapabilityCommand.ExecuteReader())
				{
					while (reader.Read())
					{
						alertRules.Add(new AlertRule
						{
							PK = (Guid)reader["AR_PK"],
							AlertName = Convert.ToString(reader["AR_AlertName"], CultureInfo.InvariantCulture),
							Type = (int)reader["AR_Type"],
							Path = Convert.ToString(reader["AR_Path"], CultureInfo.InvariantCulture),
							Owner = (string)reader["AR_Owner"],
							IsDefault = (bool)reader["AR_IsDefault"],
							Capability = (string)reader["AC_Capability"],
							Module = (string)reader["AC_Module"],
							Product = (string)reader["AC_Product"],
							ProgramArea = (string)reader["AC_ProgramArea"],
							Priority = (string)reader["AC_Priority"],
							ChangeType = (string)reader["AC_ChangeType"],
							Criticality = Convert.ToString(reader["AR_Criticality"], CultureInfo.InvariantCulture),
							MaxTargets = (int)reader["AC_MaxTargets"]
						});
					}
				}
			}
			return alertRules;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public void AddAlertIncident(Guid sourceRuleId, Guid sourceAlertId, Guid alertRulePk, string incidentNumber)
		{
			using (var wiseGridReportingInsertIncidentCommand = wiseGridReportingConnection.Command("EXEC dbo.usp_AddAlertIncident @SourceRuleId, @SourceAlertId, @AlertRuleId, @IncidentNumber"))
			{
				wiseGridReportingInsertIncidentCommand.AddParameter("@SourceRuleId", SqlDbType.UniqueIdentifier, sourceRuleId);
				wiseGridReportingInsertIncidentCommand.AddParameter("@SourceAlertId", SqlDbType.UniqueIdentifier, sourceAlertId);
				wiseGridReportingInsertIncidentCommand.AddParameter("@AlertRuleId", SqlDbType.UniqueIdentifier, alertRulePk);
				wiseGridReportingInsertIncidentCommand.AddParameter("@IncidentNumber", SqlDbType.VarChar, incidentNumber);
				wiseGridReportingInsertIncidentCommand.ExecuteNonQuery();
			}
		}
	}

	public enum AlertRuleType
	{
		None = 0,
		SCOM = 1,
		SqlCpuUsage = 2
	}
}

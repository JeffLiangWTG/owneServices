using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.AuditDataServices.HVLV.Subscribers
{
	public class HVLVItemGlowUsageSubscriber : HVLVUsageSubscriber, IBacklogCountOverridable
	{
		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => false;

		public override bool NotifyDelete => false;

		public override string Code => "HIG";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description")]
		public override string Description => "HVLV Items Glow Usage Subscriber";

		public override ITableSchema Table => StmALogSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => null;

		public override bool IsRequired() => true;

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[StmALogSchema.Constants.SL_Table] == DBNull.Value
				|| Convert.ToString(row[StmALogSchema.Constants.SL_Table]) != HVLVItemSchema.Constants.TableName
				|| row[StmALogSchema.Constants.SL_SE_NKEvent] == DBNull.Value
				|| (Convert.ToString(row[StmALogSchema.Constants.SL_SE_NKEvent]) != "EDT" && Convert.ToString(row[StmALogSchema.Constants.SL_SE_NKEvent]) != "SSC")
				|| row[SL_DataSource] == DBNull.Value
				|| Convert.ToString(row[SL_DataSource]) != "G")
			{
				row.Delete();
			}
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message logging")]
		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			logger.Log(LogType.Debug, $"{changeTable.Rows.Count} Glow usage(s) of HVLV Items have been collected, populating usage table if required...");

			var countByUsageCode = new Dictionary<string, int>();

			foreach (DataRow row in changeTable.Rows)
			{
				var userCode = row[StmALogSchema.Constants.SL_GS_NKUser].ToString().Trim();
				var itemUsageData = new ItemUsageData(new ZGuid(row[StmALogSchema.Constants.SL_Parent]), userCode)
				{
					UsageCategory = UsageCategories.WebUsage,
					BranchCode = row[StmALogSchema.Constants.SL_GB_NKBranch].ToString(),
				};

				var reference = row[StmALogSchema.Constants.SL_Reference].ToString().Trim();
				itemUsageData.UsageCode = EcommerceGLOWModuleCodeCodeInSLReference(reference) ??
								(itemUsageData.UserCode == User.WebUserCode ? UsageCodes.GlowContactEditUsage : UsageCodes.GlowStaffEditUsage);

				var logBranch = DataFactory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, itemUsageData.BranchCode);

				if (logBranch != null)
				{
					itemUsageData.CompanyCode = logBranch.Company.GC_Code;
					var recordsInserted = Populate(itemUsageData);
					if (recordsInserted > 0)
					{
						if (countByUsageCode.TryGetValue(itemUsageData.UsageCode, out var currentCount))
						{
							countByUsageCode[itemUsageData.UsageCode] = currentCount + recordsInserted;
						}
						else
						{
							countByUsageCode[itemUsageData.UsageCode] = recordsInserted;
						}
					}
				}
				else
				{
					var errorMessageBuilder = new StringBuilder();
					errorMessageBuilder.AppendLine("A Glow usage has been skipped as the branch for event log was unknown.");
					errorMessageBuilder.AppendLine("-------DEBUG INFORMATION-------");
					errorMessageBuilder.AppendLine("Values from the StmALog record being processed when error occured:");
					errorMessageBuilder.AppendLine($"  Log PK: {row[StmALogSchema.Constants.PK]}");
					errorMessageBuilder.AppendLine($"  Item PK: {itemUsageData.ItemPK}");
					errorMessageBuilder.AppendLine($"  Event: {row[StmALogSchema.Constants.SL_SE_NKEvent]}");
					errorMessageBuilder.AppendLine($"  Data Source: {row[SL_DataSource]}");
					errorMessageBuilder.AppendLine($"  Reference: {row[StmALogSchema.Constants.SL_Reference]}");
					errorMessageBuilder.AppendLine($"  User: {itemUsageData.UserCode}");
					errorMessageBuilder.AppendLine($"  Branch: {itemUsageData.BranchCode}");
					errorMessageBuilder.AppendLine($"  Department: {row[StmALogSchema.Constants.SL_GE_NKDepartment]}");
					errorMessageBuilder.AppendLine($"  Posted Time: {row[StmALogSchema.Constants.SL_PostedTimeUtc]}");
					errorMessageBuilder.AppendLine($"  Event Time: {row[StmALogSchema.Constants.SL_EventTimeUtc]}");
					ErrorReporter.ReportOnce("HVLVItemGlowUsageSubscriber|InvalidData", errorMessageBuilder.ToString());
				}
			}

			foreach (var keyValue in countByUsageCode)
			{
				logger.Log(LogType.Information, $"{keyValue.Value} Glow usage(s) of {keyValue.Key} have been recorded.");
			}
		}

		string EcommerceGLOWModuleCodeCodeInSLReference(string reference)
		{
			const string EcommerceDestination = "EDP";
			const string EcommerceOrigin = "EOD";
			const string EcommerceShipper = "ESH";
			string[] portalCodes = { "ESH", "EDP", "EOD", "EOS", "ETL", "ES2", "NEO" };

			foreach (var code in portalCodes)
			{
				if (reference.Contains($"|MID={code}"))
				{
					return code switch
					{
						"EDP" or "ETL" => EcommerceDestination,
						"EOD" or "EOS" => EcommerceOrigin,
						"ESH" or "ES2" or "NEO" => EcommerceShipper,
						_ => null,
					};
				}
			}
			return null;
		}

		const string SL_DataSource = "SL_DataSource"; // hidden schema column name

		BusinessObjectFactory DataFactory => dataFactory ?? (dataFactory = new BusinessObjectFactory { RefreshEnabled = false });
		BusinessObjectFactory dataFactory;

		#region IBacklogCountOverridable

		string IBacklogCountOverridable.EffectiveTableName => HVLVItemSchema.Constants.TableName;
		bool IBacklogCountOverridable.CountInsert => false;
		bool IBacklogCountOverridable.CountUpdate => true;
		bool IBacklogCountOverridable.CountDelete => false;

		#endregion IBacklogCountOverridable
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.AuditDataServices.HVLV.Subscribers
{
	public class HVLVItemCreationSubscriber : HVLVUsageSubscriber
	{
		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => false;

		public override bool NotifyDelete => false;

		public override string Code => "HAI";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description")]
		public override string Description => "HVLV Items Usage Subscriber";

		public override ITableSchema Table => HVLVItemSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => null;

		public override bool IsRequired() => true;

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			var sysCreateUser = row[HVLVItemSchema.Constants.HVI_SystemCreateUser] != DBNull.Value
					? row[HVLVItemSchema.Constants.HVI_SystemCreateUser]
					: null;
			if (Convert.ToString(sysCreateUser) == User.ServiceUserCode
				|| Convert.ToString(sysCreateUser) == User.SupportUserCode
				|| Convert.ToString(sysCreateUser) == User.UnKnownUserCode)
			{
				row.Delete();
			}
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message logging")]
		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			logger.Log(LogType.Debug, $"{changeTable.Rows.Count} creation usage(s) of HVLV Items have been collected, populating usage table if required...");

			var countByUsageCode = new Dictionary<string, int>();

			foreach (DataRow row in changeTable.Rows)
			{
				var errorMessageBuilder = new StringBuilder();
				var itemUsageData = GetItemUsageData(row);

				if (!itemUsageData.BranchCode.IsEmpty)
				{
					var recordsInserted = PopulateCreationUsage(itemUsageData);

					if (recordsInserted > 0)
					{
						countByUsageCode.TryGetValue(itemUsageData.UsageCode, out var currentCount);
						countByUsageCode[itemUsageData.UsageCode] = currentCount + recordsInserted;
					}
				}
				else if (!CreationUsageExists(new ZGuid(row[HVLVItemSchema.Constants.PK])))
				{
					errorMessageBuilder.AppendLine($"An item creation usage has been skipped as the branch for HVI_SystemCreateUser was unknown.");
					errorMessageBuilder.AppendLine("-------DEBUG INFORMATION-------");
					errorMessageBuilder.AppendLine("Values from the HVLVItem record being processed when error occured:");
					errorMessageBuilder.AppendLine($"  Item PK: {itemUsageData.ItemPK}");
					errorMessageBuilder.AppendLine($"  User: {itemUsageData.UserCode}");
				}

				if (errorMessageBuilder.Length != 0)
				{
					ErrorReporter.ReportOnce("HVLVItemCreationSubscriber|InvalidData", errorMessageBuilder.ToString());
				}
			}

			foreach (var keyValue in countByUsageCode)
			{
				logger.Log(LogType.Information, $"{keyValue.Value} usage(s) of {keyValue.Key} have been recorded.");
			}
		}

		ItemUsageData GetItemUsageData(DataRow row)
		{
			var userCode = row[HVLVItemSchema.Constants.HVI_SystemCreateUser].ToString().Trim();
			var itemUsageData = new ItemUsageData(row, userCode);

			var branchCode = GetBranchCodeFromHVI_LastUsageCodeForNewItemBecauseWeCannotFindABetterPlaceForIt(row);
			var itemCreatedViaGlowWhereADDLogStillExists = string.IsNullOrEmpty(branchCode);

			if (itemCreatedViaGlowWhereADDLogStillExists)
			{
				var addLogQuery = new ZQuery(StmALogSchema.SL_Parent, itemUsageData.ItemPK);
				addLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystemCode);

				var addLog = DataFactory.LoadTop1<StmALog>(addLogQuery);
				if (addLog != null)
				{
					branchCode = addLog.SL_GB_NKBranch;
				}

				itemUsageData.UsageCategory = UsageCategories.WebUsage;
				itemUsageData.UsageCode = userCode == User.WebUserCode ? UsageCodes.GlowContactAddUsage : UsageCodes.GlowStaffAddUsage;
			}
			else
			{
				itemUsageData.UsageCategory = UsageCategories.CargoWiseOneUsage;
				itemUsageData.UsageCode = UsageCodes.CargoWiseUsage;
			}

			var companyCode = branchCode.IsNullOrEmpty() ? string.Empty : GetCompanyCode(branchCode);

			if (!companyCode.IsNullOrEmpty())
			{
				itemUsageData.CompanyCode = companyCode;
				itemUsageData.BranchCode = branchCode;
			}

			return itemUsageData;
		}

		string GetBranchCodeFromHVI_LastUsageCodeForNewItemBecauseWeCannotFindABetterPlaceForIt(DataRow row)
		{
			return row[HVLVItemSchema.Constants.HVI_LastUsageCode].ToString();
		}

		string GetCompanyCode(string branchCode)
		{
			branchAndCompanyCodes ??= new Dictionary<string, string>();
			if (!branchAndCompanyCodes.TryGetValue(branchCode, out var companyCode))
			{
				var query = new ZDBOnlyQuery(typeof(GlbCompany));
				var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_GC);
				branchQuery.AddToFilter(GlbBranchSchema.GB_Code, branchCode);
				query.AddSubQuery(branchQuery, JoinCondition.And);
				var company = DataFactory.LoadTop1<GlbCompany>(query);
				if (company != null)
				{
					companyCode = company.GC_Code;
				}

				branchAndCompanyCodes[branchCode] = companyCode;
			}

			return companyCode;
		}

		int PopulateCreationUsage(ItemUsageData itemUsageData)
		{
			var sql = $@"
IF NOT EXISTS (SELECT NULL FROM dbo.HVLVUsage WHERE HXU_HVI_ParentItem = '{itemUsageData.ItemPK}' AND HXU_Code IN ('{UsageCodes.CargoWiseUsage}', '{UsageCodes.GlowContactAddUsage}', '{UsageCodes.GlowStaffAddUsage}'))
BEGIN
	INSERT INTO dbo.HVLVUsage (HXU_PK, HXU_HVI_ParentItem, HXU_Category, HXU_GS_NKUser, HXU_Code, HXU_GC_NKCompany, HXU_BranchCode, HXU_UsageTimeUtc)
	VALUES (NEWID(), '{itemUsageData.ItemPK}', '{itemUsageData.UsageCategory}', '{itemUsageData.UserCode}', '{itemUsageData.UsageCode}', '{itemUsageData.CompanyCode}', '{itemUsageData.BranchCode}', GETUTCDATE())
END";

			return Db.Connection.ExecuteNonQuery(sql);
		}

		bool CreationUsageExists(ZGuid itemPK)
		{
			var sql = $@"FROM dbo.HVLVUsage WHERE HXU_HVI_ParentItem = '{itemPK}' AND HXU_Code IN ('{UsageCodes.CargoWiseUsage}', '{UsageCodes.GlowContactAddUsage}', '{UsageCodes.GlowStaffAddUsage}')";
			return Db.Connection.Exists(sql);
		}

		BusinessObjectFactory DataFactory => dataFactory ?? (dataFactory = new BusinessObjectFactory { RefreshEnabled = false });
		BusinessObjectFactory dataFactory;

		Dictionary<string, string> branchAndCompanyCodes;
	}
}

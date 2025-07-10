using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.AuditDataServices.HVLV.Subscribers
{
	public class HVLVItemLastUsageCodeSubscriber : HVLVUsageSubscriber
	{
		#region Usage Subscriber Defaults

		public override bool NotifyInsert => false;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		public override bool IsRequired() => true;

		#endregion

		#region Usage Subscriber Overrides

		public override string Code => "HIU";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description")]
		public override string Description => "HVLV Items LastUsageCode Subscriber";

		public override ITableSchema Table => HVLVItemSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => new[] { HVLVItemSchema.HVI_LastUsageCode };

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL Value Comparison")]
		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			var invalidUsers = new List<string> { "~AD", "~BP", "E", "ZZ" };
			if (row[HVLVItemSchema.Constants.HVI_IsActive] == DBNull.Value
				|| !Convert.ToBoolean(row[HVLVItemSchema.Constants.HVI_IsActive])
				|| row[HVLVItemSchema.Constants.HVI_LastUsageCode] == DBNull.Value
				|| Convert.ToString(row[HVLVItemSchema.Constants.HVI_LastUsageCode]).IsNullOrEmpty()
				|| (row[HVLVItemSchema.Constants.HVI_SystemLastEditUser] != DBNull.Value && invalidUsers.Contains(Convert.ToString(row[HVLVItemSchema.Constants.HVI_SystemLastEditUser])))
				|| (row[HVLVItemSchema.Constants.HVI_SystemCreateUser] != DBNull.Value && invalidUsers.Contains(Convert.ToString(row[HVLVItemSchema.Constants.HVI_SystemCreateUser]))))
			{
				row.Delete();
			}
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message logging")]
		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			logger.Log(LogType.Debug, $"A usage of {changeTable.Rows.Count} HVLV Items LastUsageCode has been collected, populating usage table if required...");

			var countByUsageCode = new Dictionary<string, int>();
			var usageCategoryByCode = UsageCategories.LookupByUsageCode;

			foreach (DataRow row in changeTable.Rows)
			{
				var userCode = row[HVLVItemSchema.Constants.HVI_SystemLastEditUser].ToString().Trim();
				var itemUsageData = new ItemUsageData(row, userCode)
				{
					UsageCode = row[HVLVItemSchema.Constants.HVI_LastUsageCode].ToString(),
				};

				itemUsageData.UsageCategory = usageCategoryByCode.GetValueSafe(itemUsageData.UsageCode);

				var lastEditBranch = GetBranchFromStaffCode(itemUsageData.UserCode, DataFactory);

				if (lastEditBranch != null && !string.IsNullOrEmpty(itemUsageData.UsageCategory))
				{
					itemUsageData.BranchCode = lastEditBranch.GB_Code;
					itemUsageData.CompanyCode = lastEditBranch.Company.GC_Code;

					var recordsInserted = Populate(itemUsageData);
					if (recordsInserted >= 0)
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
					errorMessageBuilder.AppendLine("A usage has been skipped for the following reason(s):");
					if (string.IsNullOrEmpty(itemUsageData.UsageCategory))
					{
						errorMessageBuilder.AppendLine("  --The UsageCategory was unknown.");
					}
					if (lastEditBranch == null)
					{
						errorMessageBuilder.AppendLine("  --The lastEditBranch was unknown.");
					}

					errorMessageBuilder.AppendLine("-------DEBUG INFORMATION-------");
					errorMessageBuilder.AppendLine("Values from the HVLVItem record being processed when error occured:");
					errorMessageBuilder.AppendLine($"  HVLVItem.PK: {itemUsageData.ItemPK}");
					errorMessageBuilder.AppendLine($"  UsageCode: {itemUsageData.UsageCode}");
					errorMessageBuilder.AppendLine($"  LastEdit Staff Code: {itemUsageData.UserCode}");
					errorMessageBuilder.AppendLine("Computed values that should not be empty or null:");
					errorMessageBuilder.AppendLine($"  LastEdit Branch Code: {lastEditBranch?.GB_Code}");
					errorMessageBuilder.AppendLine($"  UsageCategory: {itemUsageData.UsageCategory}");
					errorMessageBuilder.AppendLine("Current contents of HVLVConstants.UsageCategories.LookupByUsageCode:");
					foreach (var pair in usageCategoryByCode)
					{
						errorMessageBuilder.AppendLine($"  {pair.Key}: {pair.Value}");
					}

					ErrorReporter.ReportOnce("HVLVItemLastUsageCodeSubscriber|InvalidData", errorMessageBuilder.ToString());
				}
			}

			foreach (var keyValue in countByUsageCode)
			{
				logger.Log(LogType.Information, $"{keyValue.Value} usages of {keyValue.Key} have been recorded.");
			}
		}

		#endregion

		BusinessObjectFactory DataFactory => dataFactory ?? (dataFactory = new BusinessObjectFactory { RefreshEnabled = false });
		BusinessObjectFactory dataFactory;
	}
}

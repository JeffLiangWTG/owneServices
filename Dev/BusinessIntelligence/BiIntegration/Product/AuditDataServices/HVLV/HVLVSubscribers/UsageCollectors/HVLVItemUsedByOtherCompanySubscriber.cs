using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.AuditDataServices.HVLV.Subscribers
{
	public class HVLVItemUsedByOtherCompanySubscriber : HVLVUsageSubscriber
	{
		#region Usage Subscriber Defaults

		public override bool NotifyInsert => false;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		public override bool IsRequired() => true;

		#endregion

		#region Usage Subscriber Overrides

		public override string Code => "HXI";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description")]
		public override string Description => "HVLV Items Extra Usage Subscriber";

		public override ITableSchema Table => HVLVItemSchema.Instance;

		IEnumerable<SchemaColumn> ColumnsToExclude => new SchemaColumn[]
		{
			HVLVItemSchema.PK,
			HVLVItemSchema.HVI_ClusterKey,
			HVLVItemSchema.HVI_ItemId,
			HVLVItemSchema.HVI_IsValidatedForUniqueness,
			HVLVItemSchema.HVI_SystemCreateTimeUtc,
			HVLVItemSchema.HVI_SystemCreateUser,
		};

		public override IEnumerable<SchemaColumn> SpecificColumns => HVLVItemSchema.All.Except(ColumnsToExclude);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL Value Comparison")]
		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			var invalidUsers = new List<string> { "~AD", "~BP", "E", "ZZ" };
			if (row[HVLVItemSchema.Constants.HVI_IsActive] == DBNull.Value
				|| Convert.ToInt32(row[HVLVItemSchema.Constants.HVI_IsActive]) != 1
				|| (row[HVLVItemSchema.Constants.HVI_SystemLastEditUser] != DBNull.Value && invalidUsers.Contains(Convert.ToString(row[HVLVItemSchema.Constants.HVI_SystemLastEditUser])))
				|| (row[HVLVItemSchema.Constants.HVI_SystemCreateUser] != DBNull.Value && invalidUsers.Contains(Convert.ToString(row[HVLVItemSchema.Constants.HVI_SystemCreateUser]))))
			{
				row.Delete();
			}
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message logging")]
		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			foreach (DataRow row in changeTable.Rows)
			{
				logger.Log(LogType.Debug, $"A usage of HVLV Item {row[HVLVItemSchema.Constants.HVI_ItemId]} has been collected, populating usage table if required...");

				var userCode = row[HVLVItemSchema.Constants.HVI_SystemLastEditUser].ToString().Trim();
				var itemUsageData = new ItemUsageData(row, userCode);

				var lastEditBranch = GetBranchFromStaffCode(itemUsageData.UserCode, DataFactory);
				var creatingStaffCode = row[HVLVItemSchema.Constants.HVI_SystemCreateUser].ToString();
				var creatingBranch = GetBranchFromStaffCode(creatingStaffCode, DataFactory);

				if (lastEditBranch != null && creatingBranch != null)
				{
					var lastEditCompany = lastEditBranch.Company;
					var creatingCompany = creatingBranch.Company;

					if (lastEditCompany != null && creatingCompany != null && lastEditCompany != creatingCompany)
					{
						itemUsageData.BranchCode = lastEditBranch.GB_Code;
						itemUsageData.CompanyCode = lastEditCompany.GC_Code;
						itemUsageData.UsageCategory = UsageCategories.CargoWiseOneUsage;
						itemUsageData.UsageCode = UsageCodes.CargoWiseUsageByOtherCompany;

						var recordInserted = Populate(itemUsageData);

						if (recordInserted <= 0)
						{
							logger.Log(LogType.Debug, $"No new usage records created.");
						}
						else
						{
							logger.Log(LogType.Information, $"{recordInserted} usage has been recorded.");
						}
					}
				}
				else
				{
					var errorMessageBuilder = new StringBuilder();
					errorMessageBuilder.AppendLine($"A usage was not collected as an error has occurred.");
					errorMessageBuilder.AppendLine("-------DEBUG INFORMATION-------");
					errorMessageBuilder.AppendLine("Values from the HVLVItem record being processed when error occured:");
					errorMessageBuilder.AppendLine($"  HVLVItem.PK: {itemUsageData.ItemPK}");
					errorMessageBuilder.AppendLine($"  LastEdit Staff Code: {itemUsageData.UserCode}");
					errorMessageBuilder.AppendLine($"  LastEdit Branch Code: {lastEditBranch?.GB_Code}");
					errorMessageBuilder.AppendLine($"  Creating Staff Code: {creatingStaffCode}");
					errorMessageBuilder.AppendLine($"  Creating Branch Code: {creatingBranch?.GB_Code}");

					ErrorReporter.ReportOnce("HVLVItemUsedByOtherCompanySubscriber|InvalidData", errorMessageBuilder.ToString());
				}
			}
		}

		#endregion

		protected BusinessObjectFactory DataFactory => dataFactory ?? (dataFactory = new BusinessObjectFactory { RefreshEnabled = false });
		BusinessObjectFactory dataFactory;
	}
}

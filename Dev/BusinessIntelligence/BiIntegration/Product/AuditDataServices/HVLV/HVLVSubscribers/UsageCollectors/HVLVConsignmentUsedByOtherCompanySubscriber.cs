using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.eTail.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.AuditDataServices.HVLV.Subscribers
{
	public class HVLVConsignmentUsedByOtherCompanySubscriber : HVLVUsageSubscriber
	{
		#region Usage Subscriber Defaults

		public override bool NotifyInsert => false;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		public override bool IsRequired() => true;

		#endregion

		#region Usage Subscriber Overrides

		public override string Code => "HXC";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description")]
		public override string Description => "HVLV Consignments Extra Usage Subscriber";

		public override ITableSchema Table => HVLVConsignmentSchema.Instance;

		IEnumerable<SchemaColumn> ColumnsToExclude => new SchemaColumn[]
		{
			HVLVConsignmentSchema.PK,
			HVLVConsignmentSchema.HVC_ClusterKey,
			HVLVConsignmentSchema.HVC_ConsignmentId,
			HVLVConsignmentSchema.HVC_GoodsDescription,
			HVLVConsignmentSchema.HVC_IsValidatedForUniqueness,
			HVLVConsignmentSchema.HVC_SystemCreateTimeUtc,
			HVLVConsignmentSchema.HVC_SystemCreateUser,
		};

		public override IEnumerable<SchemaColumn> SpecificColumns => HVLVConsignmentSchema.All.Except(ColumnsToExclude);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL Value Comparison")]
		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			var invalidUsers = new List<string> { "~AD", "~BP", "E", "ZZ" };
			if (row[HVLVConsignmentSchema.Constants.HVC_IsActive] == DBNull.Value
				|| Convert.ToInt32(row[HVLVConsignmentSchema.Constants.HVC_IsActive]) != 1
				|| (row[HVLVConsignmentSchema.Constants.HVC_SystemLastEditUser] != DBNull.Value && invalidUsers.Contains(Convert.ToString(row[HVLVConsignmentSchema.Constants.HVC_SystemLastEditUser])))
				|| (row[HVLVConsignmentSchema.Constants.HVC_SystemCreateUser] != DBNull.Value && invalidUsers.Contains(Convert.ToString(row[HVLVConsignmentSchema.Constants.HVC_SystemCreateUser]))))
			{
				row.Delete();
			}
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message logging")]
		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			foreach (DataRow row in changeTable.Rows)
			{
				logger.Log(LogType.Debug, $"A usage of HVLV Consignment {row[HVLVConsignmentSchema.Constants.HVC_ConsignmentId]} has been collected, populating usage table if required...");

				var bizoPK = row[HVLVConsignmentSchema.Constants.PK].ToString();
				var lastEditStaffCode = row[HVLVConsignmentSchema.Constants.HVC_SystemLastEditUser].ToString();
				var lastEditBranch = GetBranchFromStaffCode(lastEditStaffCode, DataFactory);
				var creatingStaffCode = row[HVLVConsignmentSchema.Constants.HVC_SystemCreateUser].ToString();
				var creatingBranch = GetBranchFromStaffCode(creatingStaffCode, DataFactory);

				if (lastEditBranch != null && creatingBranch != null)
				{
					var lastEditCompany = lastEditBranch.Company;
					var creatingCompany = creatingBranch.Company;

					if (lastEditCompany != null && creatingCompany != null && lastEditCompany != creatingCompany)
					{
						var branchCode = lastEditBranch.GB_Code;
						var companyCode = lastEditCompany.GC_Code;
						var consignment = DataFactory.Load<IHVLVConsignment>(Guid.Parse(bizoPK));

						if (consignment != null)
						{
							foreach (IHVLVItem item in consignment.Items)
							{
								var recordInserted = Populate(
									new ItemUsageData(item.PK, lastEditStaffCode)
									{
										UsageCategory = UsageCategories.CargoWiseOneUsage,
										UsageCode = UsageCodes.CargoWiseUsageByOtherCompany,
										CompanyCode = companyCode,
										BranchCode = branchCode
									});

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
					}
				}
				else
				{
					var errorMessageBuilder = new StringBuilder();
					errorMessageBuilder.AppendLine($"A usage was not collected as an error has occurred.");
					errorMessageBuilder.AppendLine("-------DEBUG INFORMATION-------");
					errorMessageBuilder.AppendLine("Values from the HVLVConsignment record being processed when error occured:");
					errorMessageBuilder.AppendLine($"  HVLVConsignment.PK: {bizoPK}");
					errorMessageBuilder.AppendLine($"  LastEdit Staff Code: {lastEditStaffCode}");
					errorMessageBuilder.AppendLine($"  LastEdit Branch Code: {lastEditBranch?.GB_Code}");
					errorMessageBuilder.AppendLine($"  Creating Staff Code: {creatingStaffCode}");
					errorMessageBuilder.AppendLine($"  Creating Branch Code: {creatingBranch?.GB_Code}");

					ErrorReporter.ReportOnce("HVLVConsignmentUsedByOtherCompanySubscriber|InvalidData", errorMessageBuilder.ToString());
				}
			}
		}

		#endregion

		protected BusinessObjectFactory DataFactory => dataFactory ?? (dataFactory = new BusinessObjectFactory { RefreshEnabled = false });
		BusinessObjectFactory dataFactory;
	}
}

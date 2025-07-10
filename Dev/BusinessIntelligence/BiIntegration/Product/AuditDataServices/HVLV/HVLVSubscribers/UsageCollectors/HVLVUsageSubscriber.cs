using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.HVLV.Subscribers
{
	public abstract class HVLVUsageSubscriber : ActualDataChangesAuditSubscriber
	{
		public int Populate(ItemUsageData itemUsageData)
		{
			var sql = $@"
IF NOT EXISTS (SELECT * FROM dbo.HVLVUsage WHERE HXU_HVI_ParentItem = '{itemUsageData.ItemPK}' AND HXU_Code = '{itemUsageData.UsageCode}' AND HXU_GC_NKCompany = '{itemUsageData.CompanyCode}')
BEGIN
	INSERT INTO dbo.HVLVUsage (HXU_PK, HXU_HVI_ParentItem, HXU_Category, HXU_GS_NKUser, HXU_Code, HXU_GC_NKCompany, HXU_BranchCode, HXU_UsageTimeUtc)
	VALUES (NEWID(), '{itemUsageData.ItemPK}', '{itemUsageData.UsageCategory}', '{itemUsageData.UserCode}', '{itemUsageData.UsageCode}', '{itemUsageData.CompanyCode}', '{itemUsageData.BranchCode}', GETUTCDATE())
END";

			return Db.Connection.ExecuteNonQuery(sql);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message logging")]
		public GlbBranch GetBranchFromStaffCode(string staffCode, BusinessObjectFactory factory)
		{
			var query = GetBranchQuery(staffCode, GlbStaffSchema.GS_GB_LastLogonBranch);
			var branch = factory.LoadTop1<GlbBranch>(query);

			if (branch == null)
			{
				var fallbackQuery = GetBranchQuery(staffCode, GlbStaffSchema.GS_GB_HomeBranch);
				branch = factory.LoadTop1<GlbBranch>(fallbackQuery);

				var errorMessageBuilder = new StringBuilder();
				errorMessageBuilder.AppendLine("Cannot find Last Logon Branch, using Home Branch instead.");
				ErrorReporter.ReportOnce("HVLVUsageSubscriber|NullGS_GB_LastLogonBranch", errorMessageBuilder.ToString());
			}

			return branch;
		}

		public ZDateTime UsageSubscriptionCutOffTime => ZDateTime.UtcToday.AddDays(-7);

		ZDBOnlyQuery GetBranchQuery(string staffCode, SchemaGuidColumn subQueryColumn)
		{
			var query = new ZDBOnlyQuery(typeof(GlbBranch));
			var staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), subQueryColumn);
			staffSubQuery.AddToFilter(GlbStaffSchema.GS_Code, staffCode);
			query.AddSubQuery(GlbBranchSchema.PK, staffSubQuery, JoinCondition.And);
			return query;
		}
	}
}

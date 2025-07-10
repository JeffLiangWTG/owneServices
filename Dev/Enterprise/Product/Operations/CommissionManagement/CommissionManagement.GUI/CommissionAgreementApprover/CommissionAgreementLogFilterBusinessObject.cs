using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.GUI
{
	public class CommissionAgreementLogFilterBusinessObject : ZStmALogFilterBusinessObject
	{
		public CommissionAgreementLogFilterBusinessObject(OrgCommissionAgreement draft)
			: base(draft.MainVersion)
		{
			this.draft = draft;
		}

		public CommissionAgreementLogFilterBusinessObject()
		{
		}

		readonly OrgCommissionAgreement draft;

		#region Filter

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				result.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode);

				return result;
			}
		}

		#endregion

		#region Module Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			var sinceLastApprovedOrDisapprovedFilter = result.AddFlagsFilter(FilterDescription.SinceLastApprovedOrDisapproved, new string[] { Res.GetString("0a1eb83f-3a24-4f33-96ba-125d15152643", "Yes") }, new GetFlagsQuery[] { GetSinceLastApprovedQuery });
			sinceLastApprovedOrDisapprovedFilter.MultilingualDescription = ResString.GetMultilingualString("445d59d3-76d8-4024-99c7-17051088ec26", "Only since last approved/disapproved");
			sinceLastApprovedOrDisapprovedFilter.Property0 = ZBool.True;
			sinceLastApprovedOrDisapprovedFilter.Visibility = FilterVisibility.AlwaysVisible;

			return result;
		}

		ZQuery GetSinceLastApprovedQuery(ZBool value)
		{
			var query = new ZQuery();
			if (value == ZBool.True)
			{
				query.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, draft.CA0_SystemCreateTimeUtc);
			}
			return query;
		}

		#endregion

		#region Module Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string SinceLastApprovedOrDisapproved = "SinceLastApprovedOrDisapproved";

			#endregion
		}

		#endregion
	}
}

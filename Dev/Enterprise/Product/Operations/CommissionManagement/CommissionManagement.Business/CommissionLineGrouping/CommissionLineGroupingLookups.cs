using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionLineGroupingLookups<TGrouping, TLine> : ZLookups
		where TGrouping : CommissionLineGrouping<TGrouping, TLine>
		where TLine : BusinessObject, IViewCommissionLineProvider
	{
		public CommissionLineGroupingLookups(CommissionLineGrouping<TGrouping, TLine> grouping)
			: base(grouping)
		{
		}

		#region Companies

		public GlbCompanyCollection Companies
		{
			get { return new GlbCompanyCollection(Factory); }
		}

		#endregion

		#region Organisations

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion

		#region Staff

		public GlbStaffCollection Staff
		{
			get { return new GlbStaffCollection(Factory); }
		}

		#endregion

		#region CommissionStatuses

		public ReadOnlyCodeDescriptionPairList CommissionStatuses
		{
			get { return new AccCommissionLineCommissionStatusList(); }
		}

		#endregion

		#region CommissionApprovalRequests

		public AccCommissionApprovalRequestCollection CommissionApprovalRequests
		{
			get { return new AccCommissionApprovalRequestCollection(Factory); }
		}

		#endregion
	}
}

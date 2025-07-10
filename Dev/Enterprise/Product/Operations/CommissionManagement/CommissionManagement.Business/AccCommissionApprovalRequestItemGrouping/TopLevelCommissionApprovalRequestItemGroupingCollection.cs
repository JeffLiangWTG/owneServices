using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.CommissionManagement.Business
{
	public class TopLevelCommissionApprovalRequestItemGroupingCollection : TopLevelCommissionLineGroupingCollection<CommissionApprovalRequestItemGrouping, AccCommissionApprovalRequestItem>
	{
		#region Constructor

		public TopLevelCommissionApprovalRequestItemGroupingCollection(AccCommissionApprovalRequestItemCollection innerCollection)
			: base(innerCollection, GetGroupers().ToArray())
		{
		}

		static IEnumerable<ViewCommissionLineGrouper<AccCommissionApprovalRequestItem>> GetGroupers()
		{
			yield return new ViewCommissionLineRecipientAndPreferredCompanyGrouper<AccCommissionApprovalRequestItem>();

			if (!OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value)
			{
				yield return new ViewCommissionLineLocalCompanyGrouper<AccCommissionApprovalRequestItem>();
			}

			yield return new ViewCommissionLineSourceAndStatusAndApprovalRequestGrouper<AccCommissionApprovalRequestItem>();
		}

		#endregion

		#region AddNew

		protected override CommissionApprovalRequestItemGrouping CreateNew(ViewCommissionLineGrouper<AccCommissionApprovalRequestItem>[] subGroupers)
		{
			return new CommissionApprovalRequestItemGrouping(Factory, subGroupers);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CommissionApprovalRequestItemGrouping(Factory);
		}

		#endregion
	}
}

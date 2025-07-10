using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class TopLevelCommissionFinalizerLineItemGroupingCollection : TopLevelCommissionLineGroupingCollection<CommissionFinalizerLineItemGrouping, CommissionFinalizerLineItem>
	{
		#region Constructor

		public TopLevelCommissionFinalizerLineItemGroupingCollection(CommissionFinalizerLineItemCollection innerCollection)
			: base(innerCollection, GetGroupers().ToArray())
		{
		}

		#endregion

		#region AddNew

		protected override CommissionFinalizerLineItemGrouping CreateNew(ViewCommissionLineGrouper<CommissionFinalizerLineItem>[] subGroupers)
		{
			return new CommissionFinalizerLineItemGrouping(Factory, subGroupers);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CommissionFinalizerLineItemGrouping(Factory);
		}

		#endregion

		#region Groupers

		static IEnumerable<ViewCommissionLineGrouper<CommissionFinalizerLineItem>> GetGroupers()
		{
			yield return new ViewCommissionLineRecipientAndPreferredCompanyGrouper<CommissionFinalizerLineItem>();

			if (!OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Value)
			{
				yield return new ViewCommissionLineLocalCompanyGrouper<CommissionFinalizerLineItem>();
			}

			yield return new ViewCommissionLineSourceAndStatusGrouper<CommissionFinalizerLineItem>();
		}

		#endregion

		#region Fetch Hints

		protected override void AddRefreshFetchHints()
		{
			base.AddRefreshFetchHints();

			foreach (var element in InnerCollectionElements)
			{
				Factory.AddFetchHint(AccCommissionApprovalRequestItemSchema.CRI_CL0, element.ViewCommissionLine.PK);
			}
		}

		#endregion
	}
}

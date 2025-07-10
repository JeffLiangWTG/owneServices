using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class ABLEntryNumRelatedPacksGenPivotCollection : CustomsGenPivotCollection<ABLEntryNumRelatedPacksGenPivot, ABLEntryNum, AsycudaPack>
	{
		public ABLEntryNumRelatedPacksGenPivotCollection(ABLEntryNum master)
			: base(master)
		{
		}

		protected override string RelationType => GenPivotTypeDecider.Types.ABCEntryNumRelatedPacksGenPivot;

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			ValidateAndRefresh();
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			ValidateAndRefresh();
		}

		void ValidateAndRefresh()
		{
			if (!Master.IsDeleted)
			{
				Master.Validation.ValidateAssociatedPacks();
				Master.Bill?.RefreshBinding();
			}
		}
	}
}

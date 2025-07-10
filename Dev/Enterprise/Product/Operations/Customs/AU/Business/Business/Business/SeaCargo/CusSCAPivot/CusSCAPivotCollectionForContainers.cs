using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAPivotCollectionForContainers : DependentBusinessObjectCollection<CusSCAPivot, CusSCAContainer>
	{
		public CusSCAPivotCollectionForContainers(CusSCAContainer container)
			: base(container)
		{
			Container = Argument.NotNull(container, nameof(container));
		}

		public readonly CusSCAContainer Container;

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			var pivot = (CusSCAPivot)dependent;
			var oceanBillPk = Container.CN_CB;
			if (pivot.CV_CB != oceanBillPk)
			{
				pivot.CV_CB = oceanBillPk;
			}
			base.SetCollectionRelationships(dependent);
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = new ZQuery(CusSCAPivotSchema.CV_CB, Container.CN_CB);
			result.AddToFilter(base.CreateRelationshipFilter());
			return result;
		}

		protected override void AddItemsToCollectionForLoad(ZQuery filter)
		{
			var byContainer = Container.OceanBill?.GetPivotsIfAlreadyLoaded()?.ByContainer;

			var pk = Container.PK;
			if (AdditionalFilter.IsEmpty && (byContainer?.Contains(pk) ?? false))
			{
				AddRange(byContainer[pk]);
			}
			else
			{
				base.AddItemsToCollectionForLoad(filter);
			}
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject dependent, bool forDelete)
		{
			using (IsDeletingForDataRefresh ? dependent.SuspendSettingHasChanges() : null)
			{
				base.RemoveCollectionRelationshipsCore(dependent, forDelete);
			}
		}
	}
}

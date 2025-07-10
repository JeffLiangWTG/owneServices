using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAPivotCollectionForHouseBill : DependentBusinessObjectCollection<CusSCAPivot, CusSCAHouse>
	{
		public CusSCAPivotCollectionForHouseBill(CusSCAHouse houseBill)
			: base(houseBill)
		{
			HouseBill = Argument.NotNull(houseBill, nameof(houseBill));
		}

		public readonly CusSCAHouse HouseBill;

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			var pivot = (CusSCAPivot)dependent;
			var oceanBillPk = HouseBill.CA_CB;
			if (pivot.CV_CB != oceanBillPk)
			{
				pivot.CV_CB = oceanBillPk;
			}
			base.SetCollectionRelationships(dependent);
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = new ZQuery(CusSCAPivotSchema.CV_CB, HouseBill.CA_CB);
			result.AddToFilter(base.CreateRelationshipFilter());
			return result;
		}

		protected override void AddItemsToCollectionForLoad(ZQuery filter)
		{
			var byHouseBill = HouseBill.OceanBill?.GetPivotsIfAlreadyLoaded()?.ByHouseBill;

			var pk = HouseBill.PK;
			if (AdditionalFilter.IsEmpty && (byHouseBill?.Contains(pk) ?? false))
			{
				AddRange(byHouseBill[pk]);
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

		public bool ContainsContainerNumber(ZString containerNumber)
			=> this.Cast<CusSCAPivot>().Any(pivot => pivot.CV_AssociatedContainer == containerNumber);

		public bool HasAnElementSelfAssessed => this.Cast<CusSCAPivot>().Any(pivot => pivot.CV_IsSAC);

		public CusSCAPivot FromContainerNumber(ZString containerNumber)
			=> this.Cast<CusSCAPivot>().FirstOrDefault(pivot => pivot.CV_AssociatedContainer == containerNumber);

		public CusSCAPivot FromContainer(CusSCAContainer container)
			=> this.Cast<CusSCAPivot>().FirstOrDefault(pivot => pivot.CV_CN == container.PK);

		protected bool ContainerBreakBulkOrBulk
		{
			get
			{
				return Count == 1 && this[0].CV_CN.IsValid &&
					(this[0].CV_AssociatedContainer == CusSCAPivot.BreakBulk || this[0].CV_AssociatedContainer == CusSCAPivot.Bulk || this[0].CV_AssociatedContainer == CusSCAPivot.Liquid);
			}
		}

		protected override bool AllowNewCore => !ReadOnly;
	}
}

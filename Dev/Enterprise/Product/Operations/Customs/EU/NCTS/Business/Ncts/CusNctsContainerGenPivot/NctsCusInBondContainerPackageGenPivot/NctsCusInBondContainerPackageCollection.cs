using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsCusInBondContainerPackageCollection<out TPivot, TMaster, TParent> : ICustomsGenPivotCollection<TPivot, TMaster, TParent>
		where TPivot : NctsCusInBondContainerPackageGenPivot
		where TMaster : NctsPackage
		where TParent : NctsCusInBondContainer
	{
		IReadOnlyList<TParent> Containers { get; }
	}

	public class NctsCusInBondContainerPackageCollection<TPivot, TMaster, TParent> : CustomsGenPivotCollection<TPivot, TMaster, TParent>, INctsCusInBondContainerPackageCollection<TPivot, TMaster, TParent>
		where TPivot : NctsCusInBondContainerPackageGenPivot
		where TMaster : NctsPackage
		where TParent : NctsCusInBondContainer
	{
		public NctsCusInBondContainerPackageCollection(TMaster associatedPackage)
			: base(associatedPackage)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(GenPivotSchema.XX_Relation1TableCode, CusInvPackSchema.Constants.Prefix);
			result.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusInBondContainerSchema.Constants.Prefix);
			return result;
		}

		public IReadOnlyList<TParent> Containers => Master.Factory.GetValue(ref containersCached, () =>
		{
			return this.Select(x => x.Relation2Object as TParent).WhereNotNull().ToArray();
		});
		CachedProperty<TParent[]> containersCached;

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			var pivot = ((TPivot)dependent);
			pivot.XX_Relation2TableCode = CusInBondContainerSchema.Constants.Prefix;
		}

		protected override string RelationType => GenPivotTypeDecider.Types.CusNctsContainer;
		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}

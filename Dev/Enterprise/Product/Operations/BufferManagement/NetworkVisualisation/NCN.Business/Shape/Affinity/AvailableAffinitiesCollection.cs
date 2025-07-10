using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class AvailableAffinitiesCollection : ImpObservableCollection<IAffinity>
	{
		public AvailableAffinitiesCollection(ShapeNetworkEntity shape)
		{
			this.shape = shape;
			Build();
		}

		readonly ShapeNetworkEntity shape;

		protected override void OnReloading(AllSelectedEntities reloadEntitiesOperation)
		{
			base.OnReloading(reloadEntitiesOperation);
			Build();
		}

		void Build()
		{
			var linksForThisShape = shape.Root.Shape.ShapeAffinityLinks.Cast<ShapeAffinityLink>().Where(sal => sal.IsApplied && sal.ShapePK == shape.PK).Select(sal => sal.ShapeAffinityPK);
			var affinitiesAvailable = shape.Root.Shape.ShapeAffinities.Cast<ShapeAffinity>().Where(sa => !linksForThisShape.Contains(sa.AffinityPK));

			AddRange(affinitiesAvailable);
		}
	}
}

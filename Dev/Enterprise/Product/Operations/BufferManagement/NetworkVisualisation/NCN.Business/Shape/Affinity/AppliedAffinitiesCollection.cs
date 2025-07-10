using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class AppliedAffinitiesCollection : ImpObservableCollection<IAffinity>
	{
		public AppliedAffinitiesCollection(ShapeNetworkEntity entity)
		{
			this.entity = entity;
			Build();
		}

		readonly ShapeNetworkEntity entity;

		protected override void OnReloading(AllSelectedEntities reloader)
		{
			base.OnReloading(reloader);
			Build();
		}

		void Build()
		{
			var linksForThisShape = entity.Root.Shape.ShapeAffinityLinks.Cast<ShapeAffinityLink>().Where(sal => sal.IsApplied && sal.ShapePK == entity.PK).Select(sal => sal.ShapeAffinityPK);
			var affinitiesApplied = entity.Root.Shape.ShapeAffinities.Cast<ShapeAffinity>().Where(sa => linksForThisShape.Contains(sa.AffinityPK));

			AddRange(affinitiesApplied);
		}
	}
}

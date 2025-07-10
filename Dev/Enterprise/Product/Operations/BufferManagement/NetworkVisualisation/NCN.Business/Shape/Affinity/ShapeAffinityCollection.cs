using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ShapeAffinityCollection : NonPersistentBusinessObjectCollection<ShapeAffinity>
	{
		public ShapeAffinityCollection(BusinessObjectFactory factory, BMNCNShape diagramEntity)
			: base(factory)
		{
			this.diagramEntity = diagramEntity;
		}

		readonly BMNCNShape diagramEntity;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ShapeAffinity(Factory);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var shapeAffinity = (ShapeAffinity)child;
			shapeAffinity.AffinityPK = ZGuid.NewZGuid();
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			var shapeAffinity = (ShapeAffinity)bizO;

			var linksToRemove = diagramEntity.ShapeAffinityLinks.Cast<ShapeAffinityLink>().Where(sal => sal.ShapeAffinityPK == shapeAffinity.AffinityPK).ToArray();

			diagramEntity.ShapeAffinityLinks.RemoveRange(linksToRemove);
		}

		public bool HasErrors
		{
			get { return this.Any(sa => sa.HasErrors); }
		}
	}
}

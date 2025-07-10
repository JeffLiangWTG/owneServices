using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ShapeAffinityLinkCollection : NonPersistentBusinessObjectCollection<ShapeAffinityLink>
	{
		public ShapeAffinityLinkCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ShapeAffinityLink(Factory);
		}
	}
}

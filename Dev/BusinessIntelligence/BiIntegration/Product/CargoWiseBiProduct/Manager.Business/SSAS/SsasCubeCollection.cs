using CargoWise.EntityFramework;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class SsasCubeCollection : NonPersistentBusinessObjectCollection<SsasCube>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SsasCube("");
		}
	}
}

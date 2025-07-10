using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	public class GridLayoutElementCollection : NonPersistentBusinessObjectCollection<GridLayoutElement>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GridLayoutElement();
		}
	}
}

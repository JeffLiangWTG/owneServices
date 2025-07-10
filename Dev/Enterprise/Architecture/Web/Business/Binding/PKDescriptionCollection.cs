using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class PKDescriptionCollection : NonPersistentBusinessObjectCollection<PKDescription>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PKDescription();
		}
	}
}

using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IPortReferenceCollectionProvider
		{
			IPortReferenceCollection GetCollection(BusinessObject parent);
		}
	}
}

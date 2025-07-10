using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICustomsReferenceCollectionProvider
		{
			ICustomsReferenceCollection GetCollection(BusinessObject parent);
		}
	}
}

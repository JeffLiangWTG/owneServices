using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusEntryNumReferenceCollectionProvider
		{
			ICusEntryNumReferenceCollection GetCollection(BusinessObject parent);
		}
	}
}

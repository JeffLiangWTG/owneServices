using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusEntryNumAdditionalReferenceCollectionProvider
		{
			ICusEntryNumAdditionalReferenceCollection GetCollection(BusinessObject parent);
		}
	}
}

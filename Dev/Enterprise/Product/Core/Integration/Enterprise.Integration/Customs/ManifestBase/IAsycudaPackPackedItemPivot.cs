using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ManifestBase
		{
			public interface IAsycudaPackPackedItemPivot
			{
				bool IsInDatabase { get; }
				ZGuid PK { get; }
				ZGuid APP_APA_Pack { get; set; }
				ZGuid APP_API_Item { get; set; }
			}
		}
	}
}

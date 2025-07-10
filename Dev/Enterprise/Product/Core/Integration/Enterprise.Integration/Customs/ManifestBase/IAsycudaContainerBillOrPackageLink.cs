using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ManifestBase
		{
			public interface IAsycudaContainerBillOrPackageLink
			{
				bool IsInDatabase { get; }
				ZGuid PK { get; }
				ZGuid APC_ACN_Container { get; set; }
				ZGuid APC_APA_Pack { get; set; }
				ZGuid APC_ABL_Bill { get; set; }
			}
		}
	}
}
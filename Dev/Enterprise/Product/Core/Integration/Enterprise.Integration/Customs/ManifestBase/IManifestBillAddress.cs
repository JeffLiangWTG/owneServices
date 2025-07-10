using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ManifestBase
		{
			public interface IManifestBillAddress
			{
				ZPropertyInfo OA_AddressInfo { get; }
			}
		}
	}
}

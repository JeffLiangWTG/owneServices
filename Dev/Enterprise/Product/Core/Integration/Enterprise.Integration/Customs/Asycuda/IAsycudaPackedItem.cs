using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public interface IAsycudaPackedItem : ManifestBase.IAsycudaPackedItem
			{
				ZString API_FormattedTariff { get; set; }
				ZString RegistrationNumber { get; }
			}
		}
	}
}
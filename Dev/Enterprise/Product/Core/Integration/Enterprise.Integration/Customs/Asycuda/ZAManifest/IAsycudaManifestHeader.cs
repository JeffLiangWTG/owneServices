using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public static partial class ZAManifest
			{
				public partial interface IAsycudaManifestHeader
				{
					ZString CARN { get; set; }
					ZDateTime EstimatedTimeOfLoading { get; set; }
					ZString PlaceOfEntry { get; set; }
					ZString PlaceOfExit { get; set; }
				}
			}
		}
	}
}
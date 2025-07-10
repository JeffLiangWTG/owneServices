using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public interface IAsycudaPack : ManifestBase.IAsycudaPack
			{
				ZInt ConsignmentReference { get; set; }
				ZString MatchingReference { get; set; }
				ZDecimal LinePrice { get; set; }
				ZString LinePriceCurrency { get; set; }
			}
		}
	}
}
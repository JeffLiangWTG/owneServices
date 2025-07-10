using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ManifestBase
		{
			public interface IAsycudaPack
			{
				bool IsInDatabase { get; }
				ZGuid PK { get; }
				ZGuid APA_ABL_Bill { get; set; }
				ZString APA_CommodityCode { get; set; }
				ZString APA_GoodsDescription { get; set; }
				ZString APA_MarksAndNumbers { get; set; }
				ZInt APA_PackQty { get; set; }
				ZString APA_PackUQ { get; set; }
				ZString APA_VINNumber { get; set; }
				ZDecimal APA_Volume { get; set; }
				ZString APA_VolumeUQ { get; set; }
				ZDecimal APA_Weight { get; set; }
				ZString APA_WeightUQ { get; set; }

				ZGuid ContainerPK { get; set; }
			}
		}
	}
}

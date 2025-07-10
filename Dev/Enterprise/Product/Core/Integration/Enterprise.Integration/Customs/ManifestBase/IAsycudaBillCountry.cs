using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ManifestBase
		{
			public partial interface IAsycudaBill
			{
				ZString ABL_BillIssuer { get; set; }
				ZString ABL_BillStatus { get; set; }
				ZString ABL_GoodsLocation { get; set; }
				ZString ABL_LocationInformation { get; set; }
				ZString ABL_MessageStatus { get; set; }
				ZString ABL_SenderReference { get; set; }
				ZString ABL_ShipmentType { get; set; }
			}
		}
	}
}

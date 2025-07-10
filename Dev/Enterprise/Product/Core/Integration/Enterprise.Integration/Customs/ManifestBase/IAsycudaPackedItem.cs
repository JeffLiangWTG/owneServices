using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ManifestBase
		{
			public interface IAsycudaPackedItem
			{
				bool IsInDatabase { get; }
				ZGuid PK { get; }
				ZGuid API_ABL_Bill { get; set; }
				ZDecimal API_CustomsQty { get; set; }
				ZString API_CustomsUQ { get; set; }
				ZDecimal API_CustomsValue { get; set; }
				ZDecimal API_DutyAmount { get; set; }
				ZString API_GoodsDescription { get; set; }
				ZString API_MessageStatus { get; set; }
				ZString API_PackStatus { get; set; }
				ZString API_RN_NKGoodsOrigin { get; set; }
				ZString API_Tariff { get; set; }
				ZDecimal API_TaxAmount { get; set; }
			}
		}
	}
}

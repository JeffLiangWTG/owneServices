using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class CommodityCode02Provider : ICommodityCode02
	{
		public static CommodityCode02Provider New(string headingCode, string nomenclatureCode) => new CommodityCode02Provider(headingCode, nomenclatureCode);

		CommodityCode02Provider(string headingCode, string nomenclatureCode)
		{
			HarmonizedSystemSubHeadingCode = headingCode;
			CombinedNomenclatureCode = nomenclatureCode;
		}

		public string HarmonizedSystemSubHeadingCode { get; set; }

		public string CombinedNomenclatureCode { get; set; }
	}
}

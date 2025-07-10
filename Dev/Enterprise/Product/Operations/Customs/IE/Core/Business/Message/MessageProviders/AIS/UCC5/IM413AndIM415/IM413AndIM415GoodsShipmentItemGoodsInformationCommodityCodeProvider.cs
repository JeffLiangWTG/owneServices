using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415GoodsShipmentItemGoodsInformationCommodityCodeProvider : IGoodsShipmentItemTypeGoodsInformationCommodityCode
	{
		public IM413AndIM415GoodsShipmentItemGoodsInformationCommodityCodeProvider(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}
		readonly JobComInvoiceLine invoiceLine;

		public string CombinedNomenclatureCode => invoiceLine.JI_Tariff.Left(8);

		public string TaricCode => invoiceLine.JI_Tariff.SubstringSafe(8, 2);

		public IReadOnlyCollection<string> TaricAdditionalCodes => taricAdditionalCodesCached ??= GetTaricAdditionalCodes();
		IReadOnlyCollection<string> taricAdditionalCodesCached;

		public IReadOnlyCollection<string> NationalAdditionalCodes => nationalAdditionalCodesCached ??= GetNationalAdditionalCodes();
		IReadOnlyCollection<string> nationalAdditionalCodesCached;

		string[] GetTaricAdditionalCodes()
		{
			var codes = new List<string>();
			var code1 = invoiceLine.JI_SupplementaryCode1;
			var code2 = invoiceLine.JI_SupplementaryCode2;
			if (!code1.IsEmpty)
			{
				codes.Add(code1);
			}
			if (!code2.IsEmpty)
			{
				codes.Add(code2);
			}

			var additionalSupplements = invoiceLine.AdditionalSupplementaryCodes.Cast<SupplementaryCode>().Where(p => !p.CY_Code.IsEmpty).OrderBy(p => p.CY_Order).Select(p => p.CY_Code.ToString());
			codes.AddRange(additionalSupplements);

			return codes.ToArray();
		}

		string[] GetNationalAdditionalCodes()
		{
			var codes = new List<string>();
			var taxType = invoiceLine.JI_ZZF_NKTaxType;
			if (!taxType.IsEmpty)
			{
				codes.Add(taxType);
			}

			var tariffs = invoiceLine.CusLineTariffDetails.Where(p => !p.BZ_Tariff.IsEmpty).Select(p => p.BZ_Tariff.ToString());
			codes.AddRange(tariffs);

			return codes.ToArray();
		}
	}
}

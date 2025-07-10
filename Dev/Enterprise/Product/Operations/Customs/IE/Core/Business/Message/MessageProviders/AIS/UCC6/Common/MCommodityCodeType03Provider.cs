using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	class MCommodityCodeType03Provider : IMCommodityCode
	{
		public MCommodityCodeType03Provider(EntryLineWrapper entryLineWrapper)
		{
			randomInvoiceLine = entryLineWrapper.RandomInvoiceLine;
			instruction = entryLineWrapper.Instruction;
		}

		readonly JobComInvoiceLine randomInvoiceLine;
		readonly CusEntryInstruction instruction;

		public string HarmonizedSystemSubheadingCode => randomInvoiceLine.JI_Tariff.Length < 6 ? null : randomInvoiceLine.JI_Tariff.Left(6).ToString();

		public string CombinedNomenclatureCode => randomInvoiceLine.JI_Tariff.Length < 8 || NotPopulateTariffCode ? null : randomInvoiceLine.JI_Tariff.SubstringSafe(6, 2).ToString();

		public string TaricCode => randomInvoiceLine.JI_Tariff.Length < 10 || NotPopulateTariffCode ? null : randomInvoiceLine.JI_Tariff.SubstringSafe(8, 2).ToString();

		public IReadOnlyCollection<ITaricAdditionalCode> TaricAdditionalCode => taricAdditionalCode ?? (taricAdditionalCode = GetTaricAdditionalCode().ToArray());
		IReadOnlyCollection<ITaricAdditionalCode> taricAdditionalCode;

		public IReadOnlyCollection<ICcQualifierNationalAdditionalCode> NationalAdditionalCode => nationalAdditionalCode ?? (nationalAdditionalCode = randomInvoiceLine.CusLineTariffDetails.Cast<CusLineTariffDetail>().Select((x, i) => new NationalAdditionalCodeProvider(i + 1, x)).ToArray());
		IReadOnlyCollection<ICcQualifierNationalAdditionalCode> nationalAdditionalCode;

		public string TypeGoods => null;

		IEnumerable<ITaricAdditionalCode> GetTaricAdditionalCode()
		{
			yield return new TaricAdditionalCodeProvider(1, randomInvoiceLine);
		}

		bool NotPopulateTariffCode => instruction.IsI1EntryAndTotalPriceLessThanOrEqualTo22EUR;
	}
}

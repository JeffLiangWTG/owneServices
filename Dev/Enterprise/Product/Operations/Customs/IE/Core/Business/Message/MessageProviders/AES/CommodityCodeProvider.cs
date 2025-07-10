using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES
{
	internal class CommodityCodeProvider : ICommodityCode
	{
		public CommodityCodeProvider(EntryLineWrapper entryLineWrapper)
		{
			entryLine = entryLineWrapper.EntryLine;
			invoiceLine = entryLineWrapper.RandomInvoiceLine;
		}
		readonly CusEntryLine entryLine;
		readonly JobComInvoiceLine invoiceLine;

		public string CombinedNomenclatureCode => entryLine.CL_AdValoremTariff.SubstringSafe(6, 2);

		public string HarmonizedCode => entryLine.CL_AdValoremTariff.Left(6);

		public IReadOnlyCollection<string> TaricAdditonalCodes => taricAdditonalCodes ?? (taricAdditonalCodes = invoiceLine.SupplementaryCodes.OrderBy(x => x.CY_Order).Select(x => (string)x.CY_Code).ToArray());
		IReadOnlyCollection<string> taricAdditonalCodes;

		public IReadOnlyCollection<string> NationalAdditionalCodes => nationalAdditionalCodes ?? (nationalAdditionalCodes = invoiceLine.NationalCodes.OrderBy(x => x.CY_Order).Select(x => (string)x.CY_Code).ToArray());
		IReadOnlyCollection<string> nationalAdditionalCodes;
	}
}

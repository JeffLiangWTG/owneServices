using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class CommodityCodeProvider : ICommodityCode
{
	public CommodityCodeProvider(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}

	readonly JobComInvoiceLine invoiceLine;

	public string CombinedNomenclatureCode => invoiceLine.JI_Tariff.SubstringSafe(6, 2);

	public string HarmonizedSystemSubHeadingCode => invoiceLine.JI_Tariff.SubstringSafe(0, 6);

	public IReadOnlyCollection<ICode> TARICAdditionalCodes => taricAdditionalCodes ?? (taricAdditionalCodes =
		invoiceLine.SupplementaryCodes
		.Where(x => !IsNationalCode(x.CY_Code))
		.Select((x, i) => new CodeProvider(x, i + 1))
		.ToArray<ICode>());
	IReadOnlyCollection<ICode> taricAdditionalCodes;

	public IReadOnlyCollection<ICode> NationalAdditionalCodes => nationalAdditionalCodes ?? (nationalAdditionalCodes =
		invoiceLine.SupplementaryCodes
		.Where(x => IsNationalCode(x.CY_Code))
		.Select((x, i) => new CodeProvider(x, i + 1))
		.ToArray<ICode>());

	public string TARICCode => invoiceLine.JI_Tariff.SubstringSafe(8, 2);

	IReadOnlyCollection<ICode> nationalAdditionalCodes;

	bool IsNationalCode(ZString supplementaryCode) => !supplementaryCode.IsEmpty && supplementaryCode.SubstringSafe(0, 1).IsNumbersOnlyOrEmpty;
}

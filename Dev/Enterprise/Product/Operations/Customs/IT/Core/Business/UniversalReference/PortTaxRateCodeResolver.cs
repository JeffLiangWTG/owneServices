using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

sealed class PortTaxRateCodeResolver
{
	public PortTaxRateCodeResolver(IPortTaxRateCodesLoader portTaxRateCodesLoader)
	{
		this.portTaxRateCodesLoader = Argument.NotNull(portTaxRateCodesLoader, nameof(portTaxRateCodesLoader));
	}

	public bool IsPortTaxRateCode(ZString rateCode)
	{
		if (rateCode.IsEmpty)
		{
			return false;
		}

		var applicableRateCodes = portTaxRateCodesLoader.GetAllRateCodes();
		return applicableRateCodes.Contains(rateCode);
	}

	readonly IPortTaxRateCodesLoader portTaxRateCodesLoader;
}

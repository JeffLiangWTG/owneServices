using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface IExtraFeeCalculator
	{
		IEnumerable<IDutyCalculationIntermediateResult> CalculateExtraFees();

		ZString RateCode { get; }
	}
}

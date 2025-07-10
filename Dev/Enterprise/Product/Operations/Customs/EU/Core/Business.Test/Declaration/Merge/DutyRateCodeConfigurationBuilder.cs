using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

public class DutyRateCodeConfigurationBuilder
{
	public DutyRateCodeConfigurationBuilder(RateTypeEnum rateType, ZString rateCode, ZString rateFormula, ZString preference, ZString[] additionalCodes)
	{
		RateType = rateType;
		RateCode = rateCode;
		RateFormula = rateFormula;
		Preference = preference;
		AdditionalCodes = additionalCodes ?? Array.Empty<ZString>();
	}
	public RateTypeEnum RateType { get; }
	public ZString RateCode { get; }
	public ZString RateFormula { get; }
	public ZString Preference { get; }
	public IReadOnlyList<ZString> AdditionalCodes { get; }
}

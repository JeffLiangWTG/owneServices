using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

public class DutyTariffConfigurationBuilder
{
	DutyTariffConfigurationBuilder(DutyTariffTypeConfigurationBuilder parentDutyTariffTypeConfigurationBuilder, ZString tariffCode, ZString taxOrFeeCode)
	{
		DutyTariffTypeConfigurationBuilder = Argument.NotNull(parentDutyTariffTypeConfigurationBuilder, nameof(parentDutyTariffTypeConfigurationBuilder));
		TariffCode = Argument.NotNullOrEmpty(tariffCode, nameof(tariffCode));
		TaxOrFeeCode = taxOrFeeCode;
		RateCodes = new List<DutyRateCodeConfigurationBuilder>();
	}
	public DutyTariffTypeConfigurationBuilder DutyTariffTypeConfigurationBuilder { get; }
	public ZString TariffCode { get; }
	public ZString TaxOrFeeCode { get; }
	public List<DutyRateCodeConfigurationBuilder> RateCodes { get; }

	public DutyTariffConfigurationBuilder AddRateCode(RateTypeEnum rateType, ZString rateCode, ZString rateFormula, ZString preference, params ZString[] additionalCodes)
	{
		RateCodes.Add(new DutyRateCodeConfigurationBuilder(rateType, rateCode, rateFormula, preference, additionalCodes));
		return this;
	}
	public static DutyTariffConfigurationBuilder New(DutyTariffTypeConfigurationBuilder parentDutyTariffTypeConfigurationBuilder, ZString tariffCode, ZString taxOrFeeCode) => new DutyTariffConfigurationBuilder(parentDutyTariffTypeConfigurationBuilder, tariffCode, taxOrFeeCode);

	public void Configure() => DutyTariffTypeConfigurationBuilder.DutyReferenceDataConfigurationBuilder.Configure();
}

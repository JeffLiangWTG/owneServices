using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

public class DutyTariffTypeConfigurationBuilder
{
	DutyTariffTypeConfigurationBuilder(DutyReferenceDataConfigurationBuilder parentDutyReferenceDataConfigurationBuilder, ZString typeCode)
	{
		DutyReferenceDataConfigurationBuilder = Argument.NotNull(parentDutyReferenceDataConfigurationBuilder, nameof(parentDutyReferenceDataConfigurationBuilder));
		TypeCode = Argument.NotNullOrEmpty(typeCode, nameof(typeCode));
		Tariffs = new List<DutyTariffConfigurationBuilder>();
	}

	public DutyReferenceDataConfigurationBuilder DutyReferenceDataConfigurationBuilder { get; }
	public ZString TypeCode { get; }
	public List<DutyTariffConfigurationBuilder> Tariffs { get; }

	public DutyTariffConfigurationBuilder AddTariff(ZString tariffCode, string taxOrFeeCode = null)
	{
		var tariff = DutyTariffConfigurationBuilder.New(this, tariffCode, taxOrFeeCode);
		Tariffs.Add(tariff);
		return tariff;
	}

	public static DutyTariffTypeConfigurationBuilder New(DutyReferenceDataConfigurationBuilder parentDutyReferenceDataConfigurationBuilder, ZString typeCode) => new DutyTariffTypeConfigurationBuilder(parentDutyReferenceDataConfigurationBuilder, typeCode);
}

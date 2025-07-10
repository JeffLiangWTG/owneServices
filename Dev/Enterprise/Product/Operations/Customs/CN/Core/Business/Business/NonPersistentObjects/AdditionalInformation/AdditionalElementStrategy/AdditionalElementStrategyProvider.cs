using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public static class AdditionalElementStrategyProvider
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
		static readonly ImmutableDictionary<ZString, IAdditionalElementStrategy> strategies = ImmutableDictionary.CreateRange(new Dictionary<ZString, IAdditionalElementStrategy>
		{
			{ NameOfGoodsElementStrategy.AdditionalElementCode, new NameOfGoodsElementStrategy() },
			{ PackSpecElementStrategy.AdditionalElementCode, new PackSpecElementStrategy() },
			{ SpecModelElementStrategy.AdditionalElementCode, new SpecModelElementStrategy() },
			{ ManufactureDateElementStrategy.AdditionalElementCode, new ManufactureDateElementStrategy() },
			{ BrandTypeElementStrategy.AdditionalElementCode, new BrandTypeElementStrategy() },
			{ PreferentialTariffTreatmentOnDestinationStrategy.AdditionalElementCode, new PreferentialTariffTreatmentOnDestinationStrategy() },
			{ GTINElementStrategy.AdditionalElementCode, new GTINElementStrategy() },
			{ CASElementStrategy.AdditionalElementCode, new CASElementStrategy() },
			{ OthersElementStrategy.AdditionalElementCode, new OthersElementStrategy() },
			{ OriginalManufacturerNameCNStrategy.AdditionalElementCode, new OriginalManufacturerNameCNStrategy() },
			{ OriginalManufacturerNameENStrategy.AdditionalElementCode, new OriginalManufacturerNameENStrategy() },
			{ AntiDumpingDutyRateStrategy.AdditionalElementCode, new AntiDumpingDutyRateStrategy() },
			{ CountervailingDutyRateStrategy.AdditionalElementCode, new CountervailingDutyRateStrategy() },
			{ MeetsPricePromiseStrategy.AdditionalElementCode, new MeetsPricePromiseStrategy() }
		});

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is readonly")]
		static readonly IAdditionalElementStrategy CommonAdditionalElementStrategy = new CommonAdditionalElementStrategy();

		public static IAdditionalElementStrategy GetAdditionalElementStrategy(ZString additionalElmentCode)
		{
			return strategies.ContainsKey(additionalElmentCode) ? strategies[additionalElmentCode] : CommonAdditionalElementStrategy;
		}
	}
}

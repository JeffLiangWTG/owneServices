using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using UniversalReferenceConstants = Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsDepartureCargoDescPhase5Lookups : EU.NCTS.Business.NctsDepartureCargoDescPhase5Lookups
	{
		public NctsDepartureCargoDescPhase5Lookups(NctsDepartureCargoDesc parent) : base(parent)
		{
		}

		public new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

		public CodeDescriptionPairList ExciseCodeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (DepartureCargoDesc.UniversalTariff != null)
				{
					var exciseCode = DepartureCargoDesc.IsCustomOfficeCanaryIsland ? UniversalReferenceConstants.RefCusTariffType.CANEX : UniversalReferenceConstants.RefCusTariffType.ESEXC;
					result = Factory.GetCachedValue(FormattableString.Invariant($"ES.ExciseCodeList_{DepartureCargoDesc.BY_HarmonisedTariff}_{exciseCode}"), () =>
					{
						var list = new CodeDescriptionPairList();
						var childTariffs = GetChildTariffsGivenTariffTypeCode(exciseCode).Select(x => x.RelatedTariffFrom);

						foreach (var tariff in childTariffs)
						{
							if (tariff.ZZ1_TariffCode != UniversalReferenceConstants.RefCusRateCode.NonRecycledPlasticFee
								&& tariff.ZZ1_TariffCode != UniversalReferenceConstants.RefCusRateCode.FluorinatedGases)
							{
								list.AddPair(tariff.ZZ1_TariffCode, tariff.ZZ1_Description);
							}
						}

						return list;
					});
				}

				return result;
			}
		}

		public IEnumerable<TariffRelationshipView> GetChildTariffsGivenTariffTypeCode(ZString tariffType) => DepartureCargoDesc.UniversalTariff.ChildTariffs.Where(x => x.RelatedTariffType.ZZI_TariffType == tariffType);

		NctsDepartureCargoDesc DepartureCargoDesc => Parent;
	}
}

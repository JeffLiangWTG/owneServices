using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class TransportCostCalculator
	{
		public TransportCostCalculator(CusEntryHeader entryHeader, ZString incoTerm, ZString incoTermPlaceCode, ZString transportMode, ZString airRouteType)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			this.incoTerm = Argument.NotNull(incoTerm, nameof(incoTerm));
			this.incoTermPlaceCode = Argument.NotNull(incoTermPlaceCode, nameof(incoTermPlaceCode));
			this.transportMode = Argument.NotNull(transportMode, nameof(transportMode));
			this.airRouteType = Argument.NotNull(airRouteType, nameof(airRouteType));
			this.calculationElements = GetCalculationElements();
		}

		public ICostsAndInsurance CumulTiers => cumulTiers ?? (cumulTiers = GetCostsAndInsurance(x => x.CumulTiers));
		ICostsAndInsurance cumulTiers;

		public ICostsAndInsurance CumulAerienTiers => cumulAerienTiers ?? (cumulAerienTiers = GetCostsAndInsurance(x => x.CumulAerienTiers));
		ICostsAndInsurance cumulAerienTiers;

		public ICostsAndInsurance CumulCEHorsFRInclus => cumulCEHorsFRInclus ?? (cumulCEHorsFRInclus = GetCostsAndInsurance(x => x.CumulCEHorsFRInclus));
		ICostsAndInsurance cumulCEHorsFRInclus;

		public ICostsAndInsurance CumulCEHorsFRExclus => cumulCEHorsFRExclus ?? (cumulCEHorsFRExclus = GetCostsAndInsurance(x => x.CumulCEHorsFRExclus));
		ICostsAndInsurance cumulCEHorsFRExclus;

		public ICostsAndInsurance CumulAerienFR => cumulAerienFR ?? (cumulAerienFR = GetCostsAndInsurance(x => x.CumulAerienFR));
		ICostsAndInsurance cumulAerienFR;

		public ICostsAndInsurance CumulFRInclus => cumulFRInclus ?? (cumulFRInclus = GetCostsAndInsurance(x => x.CumulFRInclus));
		ICostsAndInsurance cumulFRInclus;

		public ICostsAndInsurance CumulFRExclus => cumulFRExclus ?? (cumulFRExclus = GetCostsAndInsurance(x => x.CumulFRExclus));
		ICostsAndInsurance cumulFRExclus;

		ICostsAndInsurance GetCostsAndInsurance(Func<Results, Result> getResult)
		{
			return new CostsAndInsuranceWrapper(entryHeader, GetChargeCode(getResult, true), GetChargeCode(getResult, false), GetChargeFlags(getResult, true), GetChargeFlags(getResult, false));
		}

		ZString GetChargeCode(Func<Results, Result> getResult, bool isFreight)
		{
			var result = getResult(calculationElements);
			var cost = isFreight ? result?.Freight : result?.Insurance;
			return cost?.Charge?.ChargeType ?? ZString.Empty;
		}

		ZString GetChargeFlags(Func<Results, Result> getResult, bool isFreight)
		{
			var result = getResult(calculationElements);
			var charge = isFreight ? result?.Freight?.Charge : result?.Insurance?.Charge;
			return charge == null ? ZString.Empty : GetFlags(charge.GetThirdCountryCharges, charge.GetEUCharges, charge.GetDomesticCharges, charge.NoFilter, charge.IsIncludedInInvoice);
		}

		ZString GetFlags(bool getThirdCountryCharges, bool getEUCharges, bool getDomesticCharges, bool noFilter, bool isIncludedInInvoice)
		{
			var result = "";

			if (!noFilter)
			{
				if (getThirdCountryCharges && getEUCharges && getDomesticCharges)
				{
					result = AmountAndCurrencyWrapper.ShouldBeThirdCountryOrEUOrDomestic;
				}
				else if (getThirdCountryCharges && getEUCharges && !getDomesticCharges)
				{
					result = AmountAndCurrencyWrapper.ShouldBeThirdCountryOrEU;
				}
				else if (!getThirdCountryCharges && getEUCharges && getDomesticCharges)
				{
					result = AmountAndCurrencyWrapper.ShouldBeEUOrDomestic;
				}
				else if (getThirdCountryCharges && !getEUCharges && !getDomesticCharges)
				{
					result = AmountAndCurrencyWrapper.ShouldBeThirdCountry;
				}
				else if (!getThirdCountryCharges && getEUCharges && !getDomesticCharges)
				{
					result = AmountAndCurrencyWrapper.ShouldBeEU;
				}
				else if (!getThirdCountryCharges && !getEUCharges && getDomesticCharges)
				{
					result = AmountAndCurrencyWrapper.ShouldBeDomestic;
				}
			}

			result += isIncludedInInvoice ? AmountAndCurrencyWrapper.ShouldBeIncludedInInvoice : AmountAndCurrencyWrapper.ShouldNotBeIncludedInInvoice;

			return result;
		}

		Results GetCalculationElements()
		{
			var result = new Results();
			foreach (var mapping in XmlCalculationSource.Mapping)
			{
				var filter = mapping.Filter;
				if (filter.IncoTerm == incoTerm && filter.AgreedPlace == incoTermPlaceCode && filter.TransportModes.TransportMode.Any(x => x == transportMode))
				{
					if (airRouteType != ZString.Empty && filter.AirRouteTypes != null && !filter.AirRouteTypes.AirRouteType.Any(x => x == airRouteType))
					{
						continue;
					}
					result = mapping.Results;
					break;
				}
			}
			return result;
		}

		protected readonly Results calculationElements;
		protected readonly CusEntryHeader entryHeader;
		protected readonly ZString incoTerm;
		protected readonly ZString incoTermPlaceCode;
		protected readonly ZString transportMode;
		protected readonly ZString airRouteType;

		protected static FranceFreightCalculation XmlCalculationSource => xmlCalculationSource ?? (xmlCalculationSource = XMLExtractor.GetFreightCalculation(ConfigurationFile));

		[ThreadStatic]
		protected static FranceFreightCalculation xmlCalculationSource;

		const string ConfigurationFile = "Enterprise.Customs.FR.Business.Declaration.Valuation.FranceFreightCalculation.xml";
	}
}

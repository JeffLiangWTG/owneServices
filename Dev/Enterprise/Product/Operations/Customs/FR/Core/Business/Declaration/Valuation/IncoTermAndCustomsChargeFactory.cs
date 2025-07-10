using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class IncoTermAndCustomsChargeFactory : UCCIncoTermAndCustomsChargeFactory
	{
		public IncoTermAndCustomsChargeFactory()
		{
			xmlConfig = XMLExtractor.GetIncoTermsConfiguration(GetConfigurationFile());
			CustomsChargeCodeDictionary = InitCustomsChargeCodeDictionary();
		}

		readonly IncoTermsConfiguration xmlConfig;

		protected override ICustomsChargeCode[] GetCharges()
		{
			return new ICustomsChargeCode[]
			{
				ChargesProvider.AirInsuranceCosts,
				UCCChargesProvider.AirTransportCosts,
				ChargesProvider.InsuranceCosts,
				UCCChargesProvider.TransportCosts,
				ChargesProvider.Adjustment,
				ChargesProvider.BuyingCommissions,
				UCCChargesProvider.CommissionAndBrokerage,
				ChargesProvider.CommissionExceptBuyingCommissions,
				ChargesProvider.ConstructionErectionAssembly,
				ChargesProvider.ContainersAndPacking,
				ChargesProvider.EngineeringDevelopmentArtwork,
				ChargesProvider.ExclusiveFreightInsideEU,
				ChargesProvider.ExclusiveInsuranceInsideEU,
				ChargesProvider.ExclusiveFreightToFrenchDestination,
				ChargesProvider.ExclusiveInsuranceToFrenchDestination,
				ChargesProvider.ImportDutiesOrOther,
				ChargesProvider.InclusiveFreightFromFrenchBorder,
				ChargesProvider.InclusiveInsuranceFromFrenchBorder,
				ChargesProvider.InclusiveFreightInsideEU,
				ChargesProvider.InclusiveInsuranceInsideEU,
				ChargesProvider.Interest,
				ChargesProvider.MaterialsConsumed,
				ChargesProvider.MaterialsComponentsParts,
				ChargesProvider.ProceedsOfAnySubsequentResale,
				ChargesProvider.RoyaltiesLicenseFee,
				UCCChargesProvider.DeductionsNotElsewhereDeclared,
				UCCChargesProvider.OtherNotElsewhereDeclared,
				ChargeCodeProvider.StatisticalValue,
				ChargesProvider.Cut
			};
		}

		public override bool CanThisChargeBeIncludedOnLineButNotOnInvoice(ZString chargeCode)
		{
			return base.CanThisChargeBeIncludedOnLineButNotOnInvoice(chargeCode) || chargeCode == FRCustomsChargeTypeList.Codes.Cut;
		}

		#region Set Up IncoTerm Charge Configurations

		public override void SetupToEUBorderCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			base.SetupToEUBorderCharge(charge, amount, currency);
			charge.J7_IsIncludedInITOT = ShouldCreatedFreightChargeBeIncludedInITOT;
		}

		public override void SetupAfterEUBorderCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			base.SetupAfterEUBorderCharge(charge, amount, currency);
			charge.J7_IsIncludedInITOT = ShouldCreatedFreightChargeBeIncludedInITOT;
		}

		public override void SetupDomesticCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			base.SetupDomesticCharge(charge, amount, currency);
			charge.J7_IsIncludedInITOT = ShouldCreatedFreightChargeBeIncludedInITOT;
		}

		protected override void SetupIncotermChargeConfigurations()
		{
			base.SetupIncotermChargeConfigurations();
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.ExWorks);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.FreeCarrier);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.FreeAlongsideShip);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.FreeOnBoard);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.CostAndFreight);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.CostInsuranceAndFreight);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.CarriagePaidTo);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.DeliveredAtTerminal);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.DeliveredAtPlace);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.DeliveredAtFrontier);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.DeliveredDutyPaid);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.DeliveredDutyUnpaid);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.DeliveredExShip);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.DeliveredExQuay);
			SetupChargeConfigurationForIncoTerm(Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded);
		}

		void SetupChargeConfigurationForIncoTerm(string incoTerm)
		{
			var charges = new Dictionary<string, (bool isIncludedInInvoice, bool isMandatory)>();

			foreach (var mapping in xmlConfig.Mapping)
			{
				if (mapping.Filter.IncoTerm == incoTerm)
				{
					foreach (var xmlCharge in mapping.Charges.Charge)
					{
						if (!charges.TryGetValue(xmlCharge.ChargeType, out var existingCharge))
						{
							charges.Add(xmlCharge.ChargeType, (xmlCharge.IsIncludedInInvoice, xmlCharge.IsMandatory));
						}
						else
						{
							if (existingCharge.isIncludedInInvoice != xmlCharge.IsIncludedInInvoice)
							{
								existingCharge = (false, existingCharge.isMandatory);
								charges[xmlCharge.ChargeType] = existingCharge;
							}
							if (existingCharge.isMandatory != xmlCharge.IsMandatory)
							{
								existingCharge = (existingCharge.isIncludedInInvoice, true);
								charges[xmlCharge.ChargeType] = existingCharge;
							}
						}
					}
				}
			}

			foreach (var charge in charges)
			{
				AddChargeConfiguration(incoTerm, charge.Key, new ChargeConfiguration() { IsIncludedInInvoice = charge.Value.isIncludedInInvoice, IsMandatory = charge.Value.isMandatory, IsRecommended = true }, true);
			}
		}

		#endregion

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<IncoTermChargesConfigurationKey, List<FlagManagedCharge>> CustomsChargeCodeDictionary { get; }

		protected Dictionary<IncoTermChargesConfigurationKey, List<FlagManagedCharge>> InitCustomsChargeCodeDictionary()
		{
			var customsChargeCodeDictionary = new Dictionary<IncoTermChargesConfigurationKey, List<FlagManagedCharge>>();

			foreach (var mapping in xmlConfig.Mapping)
			{
				var keys = BuildKeysFromXMLFilter(mapping.Filter);

				var chargeList = new List<FlagManagedCharge>();
				foreach (var xmlCharge in mapping.Charges.Charge)
				{
					var charge = CreateChargeFromXMLCharge(xmlCharge);
					chargeList.Add(charge);
				}

				foreach (var key in keys)
				{
					customsChargeCodeDictionary.Add(key, chargeList);
				}
			}

			return customsChargeCodeDictionary;
		}

		FlagManagedCharge CreateChargeFromXMLCharge(Charge charge) => new FlagManagedCharge(charge.ChargeType, ChargeTypeList.GetMultilingualDescriptionFromCode(charge.ChargeType))
		{
			IsDutiable = charge.IsDutiable,
			IsStatisticalValueApplicable = charge.IsStatisticalValueApplicable,
			IsVATible = charge.IsGSTApplicable,
			IsIncludedInITOT = charge.IsIncludedInInvoice
		};

		List<IncoTermChargesConfigurationKey> BuildKeysFromXMLFilter(Filter filter)
		{
			var result = new List<IncoTermChargesConfigurationKey>();
			foreach (var transportMode in filter.TransportModes.TransportMode)
			{
				if (filter.AirRouteTypes?.AirRouteType.Any() ?? false)
				{
					foreach (var airRouteType in filter.AirRouteTypes.AirRouteType)
					{
						result.Add(new IncoTermChargesConfigurationKey(filter.IncoTerm, filter.AgreedPlace, transportMode, airRouteType));
					}
				}
				else
				{
					result.Add(new IncoTermChargesConfigurationKey(filter.IncoTerm, filter.AgreedPlace, transportMode, ""));
				}
			}
			return result;
		}

		protected virtual string GetConfigurationFile()
		{
			return ImportConfigurationFile;
		}

		const string ImportConfigurationFile = "Enterprise.Customs.FR.Business.Declaration.Valuation.ImportIncoTermsConfiguration.xml";

		protected override string FreightToEUBorderCodeCore => ChargeCodeList.Codes.FRFreightToEUBorderCode;
		protected override MultilingualString FreightToEUBorderDesc => ChargeCodeList.Descriptions.FRFreightToEUBorderCode;
		protected override string FreightAfterEUBorderCodeCore => ChargeCodeList.Codes.FRFreightAfterEUBorder;
		protected override MultilingualString FreightAfterEUBorderDesc => ChargeCodeList.Descriptions.FRFreightAfterEUBorder;
		public override string FreightDomesticCode => ChargeCodeList.Codes.FRFreightAfterEUBorder;

		FRCustomsChargeTypeList ChargeTypeList => chargeTypeList ?? (chargeTypeList = new FRCustomsChargeTypeList());
		FRCustomsChargeTypeList chargeTypeList;
	}
}

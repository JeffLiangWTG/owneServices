using System;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class IncoTermAndCustomsChargeFactoryTest : EUIncoTermAndCustomsChargeFactoryTest
	{
		public override void TestCanThisChargeBeIncludedOnLineButNotOnInvoice()
		{
			Assert("CUT can be included on line but not on invoice", incoTermAndChargeFactory.CanThisChargeBeIncludedOnLineButNotOnInvoice("CUT"));
			Assert("Any other codes can be included on line but not on invoice", !incoTermAndChargeFactory.CanThisChargeBeIncludedOnLineButNotOnInvoice("XXX"));
		}

		public void TestIsIncludedInInvoiceIsCorrectlySetInDictionary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			declaration.JE_AirRouteType = "";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CIP";
			invoice.ZG_AgreedPlaceCode = "1";

			var incotermFactory = new IncoTermAndCustomsChargeFactoryForTest();
			var chargeConfigKey = new IncoTermChargesConfigurationKey(invoice.JZ_IncoTerm, invoice.ZG_AgreedPlaceCode, declaration.JE_TransportMode, declaration.JE_AirRouteType);
			var dictionary = incotermFactory.CustomsChargeCodeDictionary;

			dictionary.TryGetValue(chargeConfigKey, out var chargeCodeList);

			AssertEquals(6, chargeCodeList.Count);
			AssertEquals("First charge to create should be of type CEI", "CEI", chargeCodeList[0].Code);
			AssertEquals("First charge to create should be included in line", true, chargeCodeList[0].IsIncludedInITOT);
			AssertEquals("Second charge to create should be of type CNI", "CNI", chargeCodeList[1].Code);
			AssertEquals("Second charge to create should be included in line", true, chargeCodeList[1].IsIncludedInITOT);
			AssertEquals("Third charge to create should be of type FRI", "FRI", chargeCodeList[2].Code);
			AssertEquals("Third charge to create should be included in line", true, chargeCodeList[2].IsIncludedInITOT);
			AssertEquals("Fourth charge to create should be of type FNI", "FNI", chargeCodeList[3].Code);
			AssertEquals("Fourth charge to create should be included in line", true, chargeCodeList[3].IsIncludedInITOT);
			AssertEquals("Fifth charge to create should be of type FRE", "FRE", chargeCodeList[4].Code);
			AssertEquals("Fifth charge to create should not be included in line", false, chargeCodeList[4].IsIncludedInITOT);
			AssertEquals("Sixth charge to create should be of type FNE", "FNE", chargeCodeList[5].Code);
			AssertEquals("Sixth charge to create should not be included in line", false, chargeCodeList[5].IsIncludedInITOT);
		}

		public virtual void TestConfigurationFile()
		{
			var incotermFactory = new IncoTermAndCustomsChargeFactoryForTest();
			AssertEquals("Import Incoterm Factory should use import configuration xml", "Enterprise.Customs.FR.Business.Declaration.Valuation.ImportIncoTermsConfiguration.xml", incotermFactory.ConfigurationFile);
		}

		public override void TestFactoryType()
		{
			AssertType<IncoTermAndCustomsChargeFactory>(incoTermAndChargeFactory);
		}

		public override void TestGetAllIncoTerms()
		{
			AssertEquals("Count", 17, incoTermAndChargeFactory.GetAllIncoTerms().Length);
		}

		public override void TestGetAllCharges()
		{
			AssertEquals(29, incoTermAndChargeFactory.GetAllCharges().Length);
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge, ChargesProvider.AirInsuranceCosts);
			AssertGetCharge(FRCustomsChargeTypeList.Codes.ExclusiveFreightInsideEU, ChargesProvider.ExclusiveFreightInsideEU);
			AssertGetCharge(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceInsideEU, ChargesProvider.ExclusiveInsuranceInsideEU);
			AssertGetCharge(FRCustomsChargeTypeList.Codes.InclusiveFreightInsideEU, ChargesProvider.InclusiveFreightInsideEU);
			AssertGetCharge(FRCustomsChargeTypeList.Codes.InclusiveInsuranceInsideEU, ChargesProvider.InclusiveInsuranceInsideEU);
			AssertGetCharge(FRCustomsChargeTypeList.Codes.InclusiveFreightFromFrenchBorder, ChargesProvider.InclusiveFreightFromFrenchBorder);
			AssertGetCharge(FRCustomsChargeTypeList.Codes.InclusiveInsuranceFromFrenchBorder, ChargesProvider.InclusiveInsuranceFromFrenchBorder);
			AssertGetCharge(FRCustomsChargeTypeList.Codes.ExclusiveFreightToFrenchDestination, ChargesProvider.ExclusiveFreightToFrenchDestination);
			AssertGetCharge(FRCustomsChargeTypeList.Codes.ExclusiveInsuranceToFrenchDestination, ChargesProvider.ExclusiveInsuranceToFrenchDestination);

			AssertGetCharge(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, UCCChargesProvider.AirTransportCosts);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, UCCChargesProvider.InsuranceCosts);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, UCCChargesProvider.TransportCosts);

			AssertGetCharge(UCCCustomsChargeTypeList.Codes.AdjustmentCharge, ChargesProvider.Adjustment);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, ChargesProvider.BuyingCommissions);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, UCCChargesProvider.CommissionAndBrokerage);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge, ChargesProvider.CommissionExceptBuyingCommissions);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, ChargesProvider.ConstructionErectionAssembly);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, ChargesProvider.ContainersAndPacking);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge, ChargesProvider.EngineeringDevelopmentArtwork);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, ChargesProvider.ImportDutiesOrOther);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.InterestCharge, ChargesProvider.Interest);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, ChargesProvider.MaterialsConsumed);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, ChargesProvider.MaterialsComponentsParts);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, ChargesProvider.ProceedsOfAnySubsequentResale);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, ChargesProvider.RoyaltiesLicenseFee);

			AssertGetCharge(UCCCustomsChargeTypeList.Codes.DeductionsNotElsewhereDeclaredCharge, UCCChargesProvider.DeductionsNotElsewhereDeclared);
			AssertGetCharge(UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge, UCCChargesProvider.OtherNotElsewhereDeclared);
			AssertGetCharge(ChargeTypeList.Codes.StatisticalValue, ChargeCodeProvider.StatisticalValue);
			AssertGetCharge(FRCustomsChargeTypeList.Codes.Cut, ChargesProvider.Cut);

			AssertNull(incoTermAndChargeFactory.GetCharge(UCCCustomsChargeTypeList.Codes.Additions71Charge));
			AssertNull(incoTermAndChargeFactory.GetCharge(UCCCustomsChargeTypeList.Codes.Deductions71Charge));
			AssertNull(incoTermAndChargeFactory.GetCharge(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge));
			AssertNull(incoTermAndChargeFactory.GetCharge(UCCCustomsChargeTypeList.Codes.RightToReproduceCharge));
			AssertNull(incoTermAndChargeFactory.GetCharge(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge));
		}

		public virtual void TestIncotermsConfigurationXML()
		{
			var config = XMLExtractor.GetIncoTermsConfiguration("Enterprise.Customs.FR.Business.Declaration.Valuation.ImportIncoTermsConfiguration.xml");
			AssertNotNull(config);
			AssertEquals("Mapping count", 73, config.Mapping.Length);
			AssertEquals("EXW", "EXW", config.Mapping[0].Filter.IncoTerm);
			AssertEquals("Agreed Place 3", "3", config.Mapping[0].Filter.AgreedPlace);
			AssertEquals("6 charges", 6, config.Mapping[0].Charges.Charge.Length);
			AssertEquals("ChargeType", "OFT", config.Mapping[0].Charges.Charge[0].ChargeType);
			AssertEquals("IsIncludedInInvoice", false, config.Mapping[0].Charges.Charge[0].IsIncludedInInvoice);
			AssertEquals("IsMandatory", true, config.Mapping[0].Charges.Charge[0].IsMandatory);
			AssertEquals("IsDutiable", true, config.Mapping[0].Charges.Charge[0].IsDutiable);
			AssertEquals("IsStatisticalValueApplicable", true, config.Mapping[0].Charges.Charge[0].IsStatisticalValueApplicable);
			AssertEquals("IsGSTApplicable", true, config.Mapping[0].Charges.Charge[0].IsGSTApplicable);
			AssertEquals("ONS", "ONS", config.Mapping[0].Charges.Charge[1].ChargeType);
			AssertEquals("IsIncludedInInvoice", false, config.Mapping[0].Charges.Charge[1].IsIncludedInInvoice);
			AssertEquals("IsMandatory", true, config.Mapping[0].Charges.Charge[1].IsMandatory);
			AssertEquals("IsDutiable", true, config.Mapping[0].Charges.Charge[1].IsDutiable);
			AssertEquals("IsStatisticalValueApplicable", true, config.Mapping[1].Charges.Charge[1].IsStatisticalValueApplicable);
			AssertEquals("IsGSTApplicable", true, config.Mapping[0].Charges.Charge[1].IsGSTApplicable);

			AssertEquals("AFT", "AFT", config.Mapping[1].Charges.Charge[2].ChargeType);
		}

		public override void TestIncoTermAndCustomsChargeConfiguration()
		{
			var resultData = new ZStringBuilder(DataHeading);
			var incoTermFactory = new IncoTermAndCustomsChargeFactory();
			var customsChargeCodeDictionary = incoTermFactory.CustomsChargeCodeDictionary;

			foreach (var incoTermChargeConfiguration in customsChargeCodeDictionary)
			{
				var key = incoTermChargeConfiguration.Key;
				foreach (var charge in incoTermChargeConfiguration.Value)
				{
					resultData.Append(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
									key.IncoTerm,
									key.EffectiveAgreedPlaceCode,
									key.ModeOfTransport,
									key.AirRouteType,
									charge.Code,
									charge.IsIncludedInITOT,
									charge.IsDutiable,
									charge.IsStatisticalValueApplicable,
									charge.IsVATible));
				}
			}
			AssertMultilineASCIIEquals("Text from file '" + IncoTermAndCustomsChargeConfigurationFilename + "'", resourceRetriever.Value.GetString(IncoTermAndCustomsChargeConfigurationFilename), resultData.ToStringWithNewLineBetweenAppends());
		}
		const string DataHeading = "IncoTerm, AgreedPlace, TransportMode, AirRouteType, ChargeCode, IsIncludedInInvoice, IsDutiable, IsStatisticalValueApplicable, IsGSTApplicable";
		protected override string IncoTermAndCustomsChargeConfigurationFilename => "Enterprise.Customs.FR.Business.Testing.Declaration.Valuation.IncoTermAndCustomsChargeConfiguration.csv";
		protected override string FreightToEUBorderCode => ChargeCodeList.Codes.FRFreightToEUBorderCode;
		protected override string FreightAfterEUBorderCode => ChargeCodeList.Codes.FRFreightAfterEUBorder;
		protected override string FreightDomesticCode => ChargeCodeList.Codes.FRFreightAfterEUBorder;
		protected override string GetCountryContext() => Core.Constants.CountryCodes.France;

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}

	public class IncoTermAndCustomsChargeFactoryForTest : IncoTermAndCustomsChargeFactory
	{
		public IncoTermAndCustomsChargeFactoryForTest() : base()
		{
		}

		public ZString ConfigurationFile => GetConfigurationFile();
	}
}

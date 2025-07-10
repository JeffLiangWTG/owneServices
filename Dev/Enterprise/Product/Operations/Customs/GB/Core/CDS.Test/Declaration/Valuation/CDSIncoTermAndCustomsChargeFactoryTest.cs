using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Testing
{
	sealed class IncoTermAndCustomsChargeFactoryTest : EUIncoTermAndCustomsChargeFactoryTest
	{
		public void TestIsThisChargeRecommendedForThisInvoice()
		{
			var testDec = Factory.New<JobDeclaration>();
			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			Assert(!incoTermAndChargeFactory.IsThisChargeRecommendedForThisInvoice(invoice, Core.Constants.IncoTerms.ExWorks, CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge));
			Assert(!incoTermAndChargeFactory.IsThisChargeRecommendedForThisInvoice(invoice, Core.Constants.IncoTerms.ExWorks, CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge));
			Assert(!incoTermAndChargeFactory.IsThisChargeRecommendedForThisInvoice(invoice, Core.Constants.IncoTerms.FreeAlongsideShip, CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge));
			Assert(!incoTermAndChargeFactory.IsThisChargeRecommendedForThisInvoice(invoice, Core.Constants.IncoTerms.FreeCarrier, CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge));
			Assert(!incoTermAndChargeFactory.IsThisChargeRecommendedForThisInvoice(invoice, Core.Constants.IncoTerms.FreeOnBoard, CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge));

			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			Assert(incoTermAndChargeFactory.IsThisChargeRecommendedForThisInvoice(invoice, Core.Constants.IncoTerms.FreeOnBoard, CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge));
			Assert(incoTermAndChargeFactory.IsThisChargeRecommendedForThisInvoice(invoice, Core.Constants.IncoTerms.ExWorks, CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge));
			Assert(incoTermAndChargeFactory.IsThisChargeRecommendedForThisInvoice(invoice, Core.Constants.IncoTerms.FreeAlongsideShip, CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge));
			Assert(incoTermAndChargeFactory.IsThisChargeRecommendedForThisInvoice(invoice, Core.Constants.IncoTerms.FreeCarrier, CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge));
			Assert(incoTermAndChargeFactory.IsThisChargeRecommendedForThisInvoice(invoice, Core.Constants.IncoTerms.FreeOnBoard, CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge));
		}

		public override void TestFactoryType()
		{
			AssertEquals(typeof(CDSIncoTermAndChargeFactory), incoTermAndChargeFactory.GetType());
		}

		public override void TestGetAllIncoTerms()
		{
			AssertEquals("Count", 12, incoTermAndChargeFactory.GetAllIncoTerms().Length);
		}

		public override void TestGetAllCharges()
		{
			var expectedCharges = new CustomsChargeCode[]
			{
				UCCChargesProvider.CommissionAndBrokerage,
				UCCChargesProvider.ContainersAndPacking,
				UCCChargesProvider.MaterialsComponentsParts,
				UCCChargesProvider.ToolsMiesMoulds,
				UCCChargesProvider.MaterialsConsumed,
				UCCChargesProvider.EngineeringDevelopmentArtwork,
				UCCChargesProvider.RoyaltiesLicenseFee,
				UCCChargesProvider.ProceedsOfAnySubsequentResale,
				UCCChargesProvider.IndirectAndOtherPayments,
				UCCChargesProvider.InsuranceCosts,
				UCCChargesProvider.Additions71,
				UCCChargesProvider.TransportCosts,
				UCCChargesProvider.OtherNotElsewhereDeclared,
				UCCChargesProvider.Adjustment,
				UCCChargesProvider.AirTransportCosts,
				UCCChargesProvider.ConstructionErectionAssembly,
				UCCChargesProvider.ImportDutiesOrOther,
				UCCChargesProvider.Interest,
				UCCChargesProvider.RightToReproduce,
				UCCChargesProvider.BuyingCommissions,
				UCCChargesProvider.Deductions71,
				UCCChargesProvider.DiscountNotElsewhereDeclared,
				UCCChargesProvider.DeductionsNotElsewhereDeclared,
				UCCChargesProvider.CommissionExceptBuyingCommissions
			};

			AssertContainsExactElementsInAnyOrder(expectedCharges, incoTermAndChargeFactory.GetAllCharges());
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, UCCChargesProvider.CommissionAndBrokerage);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.ContainersAndPackingCharge, UCCChargesProvider.ContainersAndPacking);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, UCCChargesProvider.MaterialsComponentsParts);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, UCCChargesProvider.ToolsMiesMoulds);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.MaterialsConsumedCharge, UCCChargesProvider.MaterialsConsumed);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge, UCCChargesProvider.EngineeringDevelopmentArtwork);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, UCCChargesProvider.RoyaltiesLicenseFee);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, UCCChargesProvider.ProceedsOfAnySubsequentResale);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge, UCCChargesProvider.IndirectAndOtherPayments);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.InsuranceCostsCharge, UCCChargesProvider.InsuranceCosts);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.Additions71Charge, UCCChargesProvider.Additions71);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.TransportCostsCharge, UCCChargesProvider.TransportCosts);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge, UCCChargesProvider.OtherNotElsewhereDeclared);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.AdjustmentCharge, UCCChargesProvider.Adjustment);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge, UCCChargesProvider.AirTransportCosts);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, UCCChargesProvider.ConstructionErectionAssembly);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, UCCChargesProvider.ImportDutiesOrOther);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.InterestCharge, UCCChargesProvider.Interest);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.RightToReproduceCharge, UCCChargesProvider.RightToReproduce);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.BuyingCommissionsCharge, UCCChargesProvider.BuyingCommissions);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.Deductions71Charge, UCCChargesProvider.Deductions71);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.DiscountNotElsewhereDeclaredCharge, UCCChargesProvider.DiscountNotElsewhereDeclared);
			AssertGetCharge(CDSCustomsChargeTypeList.Codes.DeductionsNotElsewhereDeclaredCharge, UCCChargesProvider.DeductionsNotElsewhereDeclared);
		}

		[SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\GB\Core\CDS.Test\Declaration\TestFiles\CDSIncoTermAndCustomsChargeConfiguration.csv";

		protected override string FreightToEUBorderCode => CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge;
		protected override string FreightAfterEUBorderCode => CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge;

		protected override void AssertAfterEUBorderCharge(JobComInvCharge charge)
		{
			Assert("J7_IsDutiable", !charge.J7_IsDutiable);
			Assert("J7_IsGSTApplicable", charge.J7_IsGSTApplicable);
			Assert("J7_IsStatisticalValueApplicable", !charge.J7_IsStatisticalValueApplicable);
		}

		public override void TestCanThisChargeBeIncludedOnLineButNotOnInvoice()
		{
			AssertEquals("AFT", true, incoTermAndChargeFactory.CanThisChargeBeIncludedOnLineButNotOnInvoice(CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge));
			AssertEquals("OFT", true, incoTermAndChargeFactory.CanThisChargeBeIncludedOnLineButNotOnInvoice(CDSCustomsChargeTypeList.Codes.TransportCostsCharge));
			AssertEquals("ONS", true, incoTermAndChargeFactory.CanThisChargeBeIncludedOnLineButNotOnInvoice(CDSCustomsChargeTypeList.Codes.InsuranceCostsCharge));
			AssertEquals("ADD", false, incoTermAndChargeFactory.CanThisChargeBeIncludedOnLineButNotOnInvoice(CDSCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge));
		}

		public void TestGroupChargesBeIncludedOnLineButNotOnInvoice()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredAtPlace;

				var invoice = declaration.Invoices.AddNew();
				invoice.Charges.AddNew(CDSCustomsChargeTypeList.Codes.AirTransportCostsCharge);
				invoice.Charges.AddNew(CDSCustomsChargeTypeList.Codes.TransportCostsCharge);
				invoice.Charges.AddNew(CDSCustomsChargeTypeList.Codes.InsuranceCostsCharge);
				foreach (var charge in invoice.Charges)
				{
					charge.J7_IsIncludedInITOT = true;
					charge.J7_IsNotIncludedInInvoice = true;
					charge.J7_IsIncludedInITOT = true;
					AssertEquals($"\"{charge.J7_ChargeType}\" should be included in invoice line", true, charge.J7_IsIncludedInITOT);
					AssertEquals($"\"{charge.J7_ChargeType}\" should not be included in invoice", false, !charge.J7_IsNotIncludedInInvoice);
				}

				var otherCharge = invoice.Charges.AddNew(CDSCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge);
				otherCharge.J7_IsIncludedInITOT = true;
				otherCharge.J7_IsNotIncludedInInvoice = true;
				otherCharge.J7_IsIncludedInITOT = true;
				AssertEquals($"\"{otherCharge.J7_ChargeType}\" should be included in invoice line", true, otherCharge.J7_IsIncludedInITOT);
				AssertEquals($"\"{otherCharge.J7_ChargeType}\" should be included in invoice", true, !otherCharge.J7_IsNotIncludedInInvoice);
			});
		}

		protected override string GetCountryContext() => "GBCDS";

		public void TestIncotermListForDifferentModesOfTransport()
		{
			var defaultIncoTerms = new[]
			{
				Core.Constants.IncoTerms.ExWorks,
				Core.Constants.IncoTerms.FreeCarrier,
				Core.Constants.IncoTerms.CarriagePaidTo,
				Core.Constants.IncoTerms.CarriageAndInsurancePaidTo,
				Core.Constants.IncoTerms.DeliveredAtPlace,
				Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded,
				Core.Constants.IncoTerms.DeliveredDutyPaid,
				Core.Constants.IncoTerms.Other
			};
			var seaInlandWaterway = new[]
			{
				Core.Constants.IncoTerms.FreeOnBoard,
				Core.Constants.IncoTerms.CostAndFreight,
				Core.Constants.IncoTerms.CostInsuranceAndFreight,
				Core.Constants.IncoTerms.FreeAlongsideShip
			};

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var lookup = new Business.Declaration.JobDeclarationLookups(declaration);
			AssertContainsExactElementsInAnyOrder(defaultIncoTerms, lookup.IncoTermList.GetAllCodes());

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			lookup = new Business.Declaration.JobDeclarationLookups(declaration);
			AssertContainsExactElementsInAnyOrder(defaultIncoTerms.Union(seaInlandWaterway), lookup.IncoTermList.GetAllCodes());

			declaration.JE_TransportMode = TransportTypeList.Codes.InlandWaterwayTransport;
			lookup = new Business.Declaration.JobDeclarationLookups(declaration);
			AssertContainsExactElementsInAnyOrder(defaultIncoTerms.Union(seaInlandWaterway), lookup.IncoTermList.GetAllCodes());
		}

		public void TestIncotermDPUInInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded;

			var ons = invoice.Charges.GetCharge(Common.CustomsChargeTypeList.Codes.OverseasInsurance);
			AssertNotNull(ons);
		}
	}
}

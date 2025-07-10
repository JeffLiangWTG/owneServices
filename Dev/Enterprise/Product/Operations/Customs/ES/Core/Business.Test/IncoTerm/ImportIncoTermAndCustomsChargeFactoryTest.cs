using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ImportIncoTermAndCustomsChargeFactoryTest : UCCIncoTermAndCustomsChargeFactoryTest
	{
		JobDeclaration testDec;

		public override void TestFactoryType()
		{
			AssertEquals(typeof(ImportIncoTermAndCustomsChargeFactory), incoTermAndChargeFactory.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = MessageTypeList.Codes.Import;
			testDec.Invoices.AddNew();
		}

		public override void TestGetAllCharges()
		{
			AssertEquals(33, incoTermAndChargeFactory.GetAllCharges().Length);
		}

		public override void TestGetCharge()
		{
			CombineAssertions(() =>
			{
				AssertGetCharge(ESCustomsChargeTypeList.Codes.InternationalFreight, ESChargeProvider.GetNewInternationalFreight());
				AssertGetCharge(ESCustomsChargeTypeList.Codes.TransportCostsAfterEUEntry, ESChargeProvider.GetNewTransportCostsAfterEUEntry());
				AssertGetCharge(ESCustomsChargeTypeList.Codes.UnloadingOfGoods, ESChargeProvider.GetNewUnloadingOfGoods());
				AssertGetCharge(ESCustomsChargeTypeList.Codes.PortTransitFee, ESChargeProvider.GetNewPortTransitFee());
				AssertGetCharge(ESCustomsChargeTypeList.Codes.TerminalHandlingCharge, ESChargeProvider.GetNewTerminalHandling());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge, ESChargeProvider.GetNewIndirectAndOtherPayments());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge, ESChargeProvider.GetNewCommissionAndBrokerage());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge, ESChargeProvider.GetNewContainersAndPacking());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge, ESChargeProvider.GetNewMaterialsComponentsParts());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, ESChargeProvider.GetNewToolsMiesMoulds());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge, ESChargeProvider.GetNewMaterialsConsumed());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge, ESChargeProvider.GetNewEngineeringDevelopmentArtwork());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge, ESChargeProvider.GetNewRoyaltiesLicenseFee());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge, ESChargeProvider.GetNewProceedsOfAnySubsequentResale());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, ESChargeProvider.GetNewTransportCosts());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, ESChargeProvider.GetNewInsuranceCosts());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, ESChargeProvider.GetNewConstructionErectionAssembly());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, ESChargeProvider.GetNewImportDutiesOrOther());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.AdjustmentCharge, ESChargeProvider.GetNewAdjustment());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, ESChargeProvider.GetNewAirTransportCosts());
				AssertGetCharge(ESCustomsChargeTypeList.Codes.ExportedGoodsValueForOutwardProcessing, ESChargeProvider.GetNewExportedGoodsValueForOutwardProcessing());
				AssertGetCharge(ESCustomsChargeTypeList.Codes.InvoicedExportedGoodsValueForOutwardProcessing, ESChargeProvider.GetNewInvoicedExportedGoodsValueForOutwardProcessing());
				AssertGetCharge(ESCustomsChargeTypeList.Codes.ReaAidAmountIgicBaseCalculation, ESChargeProvider.GetNewReaAidAmountIgicBaseCalculation());
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + Constants.ProjectRelativePath + @"IncoTerm\TestFiles\ESIncoTermAndCustomsChargeConfigurationIMP.csv";
		protected override string GetCountryContext() => Core.Constants.CountryCodes.Spain + MessageTypeList.Codes.Import;

		protected override string FreightToEUBorderCode => ESCustomsChargeTypeList.Codes.InternationalFreight;

		protected override string FreightAfterEUBorderCode => ESCustomsChargeTypeList.Codes.TransportCostsAfterEUEntry;
	}
}

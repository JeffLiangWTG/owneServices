using System.Collections.Generic;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class ImportIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		protected override ICustomsChargeCode[] GetCharges()
		{
			var result = new List<ICustomsChargeCode>(base.GetCharges());
			result.Remove(UCCChargesProvider.IndirectAndOtherPayments);
			result.Remove(UCCChargesProvider.CommissionAndBrokerage);
			result.Remove(UCCChargesProvider.ContainersAndPacking);
			result.Remove(UCCChargesProvider.MaterialsComponentsParts);
			result.Remove(UCCChargesProvider.ToolsMiesMoulds);
			result.Remove(UCCChargesProvider.MaterialsConsumed);
			result.Remove(UCCChargesProvider.EngineeringDevelopmentArtwork);
			result.Remove(UCCChargesProvider.RoyaltiesLicenseFee);
			result.Remove(UCCChargesProvider.ProceedsOfAnySubsequentResale);
			result.Remove(UCCChargesProvider.TransportCosts);
			result.Remove(UCCChargesProvider.InsuranceCosts);
			result.Remove(UCCChargesProvider.ConstructionErectionAssembly);
			result.Remove(UCCChargesProvider.ImportDutiesOrOther);
			result.Remove(UCCChargesProvider.Adjustment);
			result.Remove(UCCChargesProvider.AirTransportCosts);
			result.Add(ESChargeProvider.GetNewInternationalFreight());
			result.Add(ESChargeProvider.GetNewTransportCostsAfterEUEntry());
			result.Add(ESChargeProvider.GetNewPortTransitFee());
			result.Add(ESChargeProvider.GetNewUnloadingOfGoods());
			result.Add(ESChargeProvider.GetNewTerminalHandling());
			result.Add(ESChargeProvider.GetNewIndirectAndOtherPayments());
			result.Add(ESChargeProvider.GetNewCommissionAndBrokerage());
			result.Add(ESChargeProvider.GetNewContainersAndPacking());
			result.Add(ESChargeProvider.GetNewMaterialsComponentsParts());
			result.Add(ESChargeProvider.GetNewToolsMiesMoulds());
			result.Add(ESChargeProvider.GetNewMaterialsConsumed());
			result.Add(ESChargeProvider.GetNewEngineeringDevelopmentArtwork());
			result.Add(ESChargeProvider.GetNewRoyaltiesLicenseFee());
			result.Add(ESChargeProvider.GetNewProceedsOfAnySubsequentResale());
			result.Add(ESChargeProvider.GetNewTransportCosts());
			result.Add(ESChargeProvider.GetNewInsuranceCosts());
			result.Add(ESChargeProvider.GetNewConstructionErectionAssembly());
			result.Add(ESChargeProvider.GetNewImportDutiesOrOther());
			result.Add(ESChargeProvider.GetNewAdjustment());
			result.Add(ESChargeProvider.GetNewAirTransportCosts());
			result.Add(ESChargeProvider.GetNewExportedGoodsValueForOutwardProcessing());
			result.Add(ESChargeProvider.GetNewInvoicedExportedGoodsValueForOutwardProcessing());
			result.Add(ESChargeProvider.GetNewReaAidAmountIgicBaseCalculation());
			return result.ToArray();
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ESChargeProvider.GetNewTransportCosts(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ESChargeProvider.GetNewImportDutiesOrOther(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ESChargeProvider.GetNewUnloadingOfGoods(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ESChargeProvider.GetNewPortTransitFee(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ESChargeProvider.GetNewTerminalHandling(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ESChargeProvider.GetNewInsuranceCosts(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, ESChargeProvider.GetNewInternationalFreight(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewInternationalFreight(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewTransportCosts(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewInsuranceCosts(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewTransportCostsAfterEUEntry(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewConstructionErectionAssembly(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewImportDutiesOrOther(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewUnloadingOfGoods(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewPortTransitFee(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, ESChargeProvider.GetNewTerminalHandling(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewInternationalFreight(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewInsuranceCosts(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewTransportCosts(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewTransportCostsAfterEUEntry(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewConstructionErectionAssembly(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewImportDutiesOrOther(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewUnloadingOfGoods(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewPortTransitFee(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, ESChargeProvider.GetNewTerminalHandling(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCarriageAndInsurancePaidToConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ESChargeProvider.GetNewInternationalFreight(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ESChargeProvider.GetNewInsuranceCosts(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ESChargeProvider.GetNewTransportCosts(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ESChargeProvider.GetNewUnloadingOfGoods(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ESChargeProvider.GetNewPortTransitFee(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, ESChargeProvider.GetNewTerminalHandling(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCarriagePaidToConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ESChargeProvider.GetNewInternationalFreight(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ESChargeProvider.GetNewTransportCosts(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ESChargeProvider.GetNewInsuranceCosts(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ESChargeProvider.GetNewImportDutiesOrOther(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ESChargeProvider.GetNewUnloadingOfGoods(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ESChargeProvider.GetNewPortTransitFee(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, ESChargeProvider.GetNewTerminalHandling(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ESChargeProvider.GetNewInternationalFreight(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ESChargeProvider.GetNewTransportCostsAfterEUEntry(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ESChargeProvider.GetNewTransportCosts(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, ESChargeProvider.GetNewInsuranceCosts(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupDeliveredAtTerminalConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ESChargeProvider.GetNewInternationalFreight(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ESChargeProvider.GetNewTransportCosts(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ESChargeProvider.GetNewTransportCostsAfterEUEntry(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ESChargeProvider.GetNewConstructionErectionAssembly(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ESChargeProvider.GetNewImportDutiesOrOther(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, ESChargeProvider.GetNewInsuranceCosts(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupDeliveredDutyPaidConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ESChargeProvider.GetNewInternationalFreight(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ESChargeProvider.GetNewTransportCostsAfterEUEntry(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ESChargeProvider.GetNewImportDutiesOrOther(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ESChargeProvider.GetNewTransportCosts(), new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, ESChargeProvider.GetNewInsuranceCosts(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false, IsMandatory = true, IsRecommended = true });
		}

		protected override void SetupExWorksConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewInternationalFreight(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewInsuranceCosts(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewTransportCosts(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewTransportCostsAfterEUEntry(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewConstructionErectionAssembly(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewImportDutiesOrOther(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewUnloadingOfGoods(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewPortTransitFee(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, ESChargeProvider.GetNewTerminalHandling(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewInternationalFreight(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewInsuranceCosts(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewTransportCosts(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewTransportCostsAfterEUEntry(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewConstructionErectionAssembly(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewImportDutiesOrOther(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewUnloadingOfGoods(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewPortTransitFee(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, ESChargeProvider.GetNewTerminalHandling(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewInternationalFreight(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewInsuranceCosts(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewTransportCosts(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewTransportCostsAfterEUEntry(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewConstructionErectionAssembly(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewImportDutiesOrOther(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewUnloadingOfGoods(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewPortTransitFee(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, ESChargeProvider.GetNewTerminalHandling(), new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true, IsMandatory = false, IsRecommended = false });
		}

		protected override string FreightToEUBorderCodeCore => ESCustomsChargeTypeList.Codes.InternationalFreight;

		protected override string FreightAfterEUBorderCodeCore => ESCustomsChargeTypeList.Codes.TransportCostsAfterEUEntry;

		protected override MultilingualString FreightToEUBorderDesc => ESCustomsChargeTypeList.Descriptions.InternationalFreight;

		protected override MultilingualString FreightAfterEUBorderDesc => ESCustomsChargeTypeList.Descriptions.TransportCostsAfterEUEntry;
	}
}


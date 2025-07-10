using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.ITOTIncoTerm;
using IncoTerms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Customs.AU.Declaration.Business
{
	#region EdificeIncoTermFactory

	public partial class EdificeIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		protected override bool IsIncludedInITOTReadOnlyForGroupChargeCore(ZString chargeCode)
		{
			return chargeCode != CustomsChargeTypeList.Codes.PackingCost && base.IsIncludedInITOTReadOnlyForGroupChargeCore(chargeCode);
		}

		protected override void SetupIncotermChargeConfigurations()
		{
			SetupCIFConfiguration();
			SetupCNFConfiguration();
			SetupCNIConfiguration();
			SetupFOBConfiguration();
			SetupLISConfiguration();
			SetupPAFConfiguration();
			SetupUAFConfiguration();
			SetupUCFConfiguration();
			SetupUCIConfiguration();
			SetupUFBConfiguration();
		}

		void SetupUFBConfiguration()
		{
			AddChargeConfiguration(IncoTerms.UnpackedFreeOnBoard, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedFreeOnBoard, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedFreeOnBoard, EdificeOverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.UnpackedFreeOnBoard, CustomsChargeCodeProvider.OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.UnpackedFreeOnBoard, EdificePackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.UnpackedFreeOnBoard, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedFreeOnBoard, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedFreeOnBoard, EdificeBuyingCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedFreeOnBoard, EdificeOtherCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedFreeOnBoard, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedFreeOnBoard, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedFreeOnBoard, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupUCIConfiguration()
		{
			AddChargeConfiguration(IncoTerms.UnpackedCostInsuranceAndFreight, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostInsuranceAndFreight, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostInsuranceAndFreight, EdificeOverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.UnpackedCostInsuranceAndFreight, CustomsChargeCodeProvider.OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.UnpackedCostInsuranceAndFreight, EdificePackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.UnpackedCostInsuranceAndFreight, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostInsuranceAndFreight, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostInsuranceAndFreight, EdificeBuyingCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostInsuranceAndFreight, EdificeOtherCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostInsuranceAndFreight, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostInsuranceAndFreight, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostInsuranceAndFreight, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupUCFConfiguration()
		{
			AddChargeConfiguration(IncoTerms.UnpackedCostAndFreight, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostAndFreight, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostAndFreight, EdificeOverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.UnpackedCostAndFreight, CustomsChargeCodeProvider.OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.UnpackedCostAndFreight, EdificePackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.UnpackedCostAndFreight, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostAndFreight, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostAndFreight, EdificeBuyingCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostAndFreight, EdificeOtherCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostAndFreight, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostAndFreight, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedCostAndFreight, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupUAFConfiguration()
		{
			AddChargeConfiguration(IncoTerms.UnpackedAtFactory, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedAtFactory, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.UnpackedAtFactory, EdificeOverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.UnpackedAtFactory, CustomsChargeCodeProvider.OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.UnpackedAtFactory, EdificePackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.UnpackedAtFactory, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedAtFactory, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedAtFactory, EdificeBuyingCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedAtFactory, EdificeOtherCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedAtFactory, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedAtFactory, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.UnpackedAtFactory, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupPAFConfiguration()
		{
			AddChargeConfiguration(IncoTerms.PackedAtFactory, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.PackedAtFactory, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.PackedAtFactory, EdificeOverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.PackedAtFactory, CustomsChargeCodeProvider.OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.PackedAtFactory, EdificePackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.PackedAtFactory, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.PackedAtFactory, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.PackedAtFactory, EdificeBuyingCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.PackedAtFactory, EdificeOtherCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.PackedAtFactory, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.PackedAtFactory, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.PackedAtFactory, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupErrorConfiguration()
		{
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, EdificeOverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(ErrorIncoTermCode, EdificePackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, EdificeBuyingCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, EdificeOtherCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(ErrorIncoTermCode, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupCIFConfiguration()
		{
			AddChargeConfiguration(IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostInsuranceAndFreight, EdificeOverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.CostInsuranceAndFreight, EdificePackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostInsuranceAndFreight, EdificeBuyingCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostInsuranceAndFreight, EdificeOtherCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostInsuranceAndFreight, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupCNFConfiguration()
		{
			AddChargeConfiguration(IncoTerms.CostFreightWithAmpersand, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostFreightWithAmpersand, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostFreightWithAmpersand, EdificeOverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.CostFreightWithAmpersand, CustomsChargeCodeProvider.OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.CostFreightWithAmpersand, EdificePackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostFreightWithAmpersand, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostFreightWithAmpersand, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostFreightWithAmpersand, EdificeBuyingCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostFreightWithAmpersand, EdificeOtherCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostFreightWithAmpersand, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostFreightWithAmpersand, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostFreightWithAmpersand, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupCNIConfiguration()
		{
			AddChargeConfiguration(IncoTerms.CostAndInsurance, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostAndInsurance, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostAndInsurance, EdificeOverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.CostAndInsurance, CustomsChargeCodeProvider.OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.CostAndInsurance, EdificePackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostAndInsurance, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostAndInsurance, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostAndInsurance, EdificeBuyingCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostAndInsurance, EdificeOtherCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostAndInsurance, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostAndInsurance, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.CostAndInsurance, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupLISConfiguration()
		{
			AddChargeConfiguration(IncoTerms.LandedIntoStore, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.LandedIntoStore, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.LandedIntoStore, EdificeOverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.LandedIntoStore, CustomsChargeCodeProvider.OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.LandedIntoStore, EdificePackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.LandedIntoStore, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.LandedIntoStore, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.LandedIntoStore, EdificeBuyingCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.LandedIntoStore, EdificeOtherCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.LandedIntoStore, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.LandedIntoStore, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.LandedIntoStore, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupFOBConfiguration()
		{
			AddChargeConfiguration(IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.FreeOnBoard, EdificeOverseasFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.OverseasInsurance, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(IncoTerms.FreeOnBoard, EdificePackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.FreeOnBoard, EdificeBuyingCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.FreeOnBoard, EdificeOtherCommission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.FreeOnBoard, CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override IEnumerable<IITOTIncoTermCalculator> GetIITOTIncoTermCalculators()
		{
			yield return new CIFIncoTerm();
			yield return new CNFIncoTerm();
			yield return new CNIIncoTerm();
			yield return new FOBIncoTerm();
			yield return new LISIncoTerm();
			yield return new PAFIncoTerm();
			yield return new UAFIncoTerm();
			yield return new UCFIncoTerm();
			yield return new UCIIncoTerm();
			yield return new UFBIncoTerm();
		}
	}

	#endregion
}

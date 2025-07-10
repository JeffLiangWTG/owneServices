using Enterprise.Customs.Common;
using static Enterprise.Core.Constants;
using JPIncoTermList = Enterprise.Customs.JP.Business.IncoTermList;

namespace Enterprise.Customs.JP.Business
{
	public abstract class BaseIncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
	{
		protected override void SetupIncotermChargeConfigurations()
		{
			SetupCostAndFreightDomesticConfiguration();
			SetupCostAndInsuranceDomesticConfiguration();

			SetupCarriageAndInsurancePaidToConfiguration();
			SetupDeliveredAtPlaceConfiguration();
			SetupDeliveredAtTerminalConfiguration();
			SetupExWorksConfiguration();
			SetupDeliveredDutyPaidConfiguration();
			SetupCarriagePaidToConfiguration();
			SetupCostInsuranceAndFreightConfiguration();
			SetupFreeOnBoardConfiguration();
			SetupCostAndFreightConfiguration();
			SetupFreeAlongsideShipConfiguration();
			SetupFreeCarrierConfiguration();
			SetupDeliveredAtFrontierConfiguration();
			SetupDeliveredExShipConfiguration();
			SetupDeliveredDutyUnpaidConfiguration();
			SetupDeliveredAtPlaceUnloadedConfiguration();
			SetupDeliveredExQuayConfiguration();
		}

		void SetupCostAndInsuranceDomesticConfiguration()
		{
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndInsurance, GetExWorks(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndInsurance, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndInsurance, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndInsurance, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = true });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndInsurance, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndInsurance, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndInsurance, GetAdditionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndInsurance, GetCommission(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndInsurance, GetDeductionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndInsurance, GetDiscount(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndInsurance, GetOtherCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupCostAndFreightDomesticConfiguration()
		{
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndFreight, GetExWorks(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndFreight, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndFreight, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndFreight, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndFreight, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndFreight, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndFreight, GetAdditionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndFreight, GetCommission(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndFreight, GetDeductionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndFreight, GetDiscount(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(JPIncoTermList.Codes.CostAndFreight, GetOtherCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			AddChargeConfiguration(IncoTerms.DeliveredAtPlace, GetExWorks(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlace, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlace, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlace, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlace, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlace, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlace, GetAdditionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlace, GetCommission(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlace, GetDeductionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlace, GetDiscount(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlace, GetOtherCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupDeliveredAtFrontierConfiguration()
		{
			AddChargeConfiguration(IncoTerms.DeliveredAtFrontier, GetExWorks(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtFrontier, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtFrontier, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtFrontier, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtFrontier, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtFrontier, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtFrontier, GetAdditionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtFrontier, GetCommission(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtFrontier, GetDeductionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtFrontier, GetDiscount(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtFrontier, GetOtherCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupDeliveredExShipConfiguration()
		{
			AddChargeConfiguration(IncoTerms.DeliveredExShip, GetExWorks(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExShip, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExShip, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExShip, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExShip, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExShip, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExShip, GetAdditionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExShip, GetCommission(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExShip, GetDeductionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExShip, GetDiscount(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExShip, GetOtherCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupDeliveredDutyUnpaidConfiguration()
		{
			AddChargeConfiguration(IncoTerms.DeliveredDutyUnpaid, GetExWorks(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredDutyUnpaid, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredDutyUnpaid, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredDutyUnpaid, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredDutyUnpaid, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredDutyUnpaid, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredDutyUnpaid, GetAdditionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredDutyUnpaid, GetCommission(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredDutyUnpaid, GetDeductionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredDutyUnpaid, GetDiscount(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredDutyUnpaid, GetOtherCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupDeliveredAtPlaceUnloadedConfiguration()
		{
			AddChargeConfiguration(IncoTerms.DeliveredAtPlaceUnloaded, GetExWorks(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlaceUnloaded, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlaceUnloaded, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlaceUnloaded, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlaceUnloaded, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlaceUnloaded, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlaceUnloaded, GetAdditionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlaceUnloaded, GetCommission(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlaceUnloaded, GetDeductionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlaceUnloaded, GetDiscount(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredAtPlaceUnloaded, GetOtherCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		void SetupDeliveredExQuayConfiguration()
		{
			AddChargeConfiguration(IncoTerms.DeliveredExQuay, GetExWorks(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExQuay, GetForeignInlandFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExQuay, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExQuay, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExQuay, GetPackingCost(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExQuay, GetLandingCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExQuay, GetAdditionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExQuay, GetCommission(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExQuay, GetDeductionCharge(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExQuay, GetDiscount(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncoTerms.DeliveredExQuay, GetOtherCharges(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupErrorConfiguration()
		{
		}

		public override bool MakeFlagsReadOnlyWhenDeemed => true;

		protected override ICustomsChargeCode[] GetCharges()
		{
			return new ICustomsChargeCode[]
			{
				GetExWorks(),
				GetForeignInlandFreight(),
				GetOverseasFreight(),
				GetOverseasInsurance(),
				GetPackingCost(),
				GetLandingCharges(),
				GetAdditionCharge(),
				GetCommission(),
				GetDeductionCharge(),
				GetDiscount(),
				GetOtherCharges(),
			};
		}

		#region Charge Codes

		protected abstract ICustomsChargeCode GetExWorks();

		protected abstract ICustomsChargeCode GetForeignInlandFreight();

		protected abstract ICustomsChargeCode GetPackingCost();

		protected abstract ICustomsChargeCode GetLandingCharges();

		protected abstract ICustomsChargeCode GetAdditionCharge();

		protected abstract ICustomsChargeCode GetCommission();

		protected abstract ICustomsChargeCode GetDeductionCharge();

		protected abstract ICustomsChargeCode GetDiscount();

		protected abstract ICustomsChargeCode GetOtherCharges();

		#endregion
	}
}

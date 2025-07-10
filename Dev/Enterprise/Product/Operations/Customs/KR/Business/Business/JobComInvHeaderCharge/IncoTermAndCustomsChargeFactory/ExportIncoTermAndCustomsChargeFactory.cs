using System.Collections.Generic;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.KR.Business
{
	public class ExportIncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
	{
		protected override void SetupIncotermChargeConfigurations()
		{
			SetupCostAndFreightConfiguration();
			SetupCostInsuranceAndFreightConfiguration();
			SetupCarriageAndInsurancePaidToConfiguration();
			SetupCarriagePaidToConfiguration();
			SetupDeliveredDutyPaidConfiguration();
			SetupExWorksConfiguration();
			SetupFreeAlongsideShipConfiguration();
			SetupFreeCarrierConfiguration();
			SetupFreeOnBoardConfiguration();

			SetupCostandInsuranceConfiguration();
			SetupDeliveredAtFrontierConfiguration();
			SetupDeliveredAtPlaceConfiguration();
			SetupDeliveredAtTerminalConfiguration();
			SetupDeliveredDutyUnpaidConfiguration();
			SetupDeliveredExQuayConfiguration();
			SetupDeliveredExShipConfiguration();
			SetupDeliveredAtPlaceUnloadedConfiguration();
		}
		void SetupCostandInsuranceConfiguration()
		{
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, Common.CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, Common.CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, Common.CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, Common.CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, Common.CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, Common.CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, Common.CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, Common.CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}
		void SetupDeliveredAtFrontierConfiguration()
		{
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtFrontier, Common.CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtFrontier, Common.CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtFrontier, Common.CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtFrontier, Common.CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtFrontier, Common.CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtFrontier, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtFrontier, Common.CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtFrontier, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtFrontier, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtFrontier, Common.CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtFrontier, Common.CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}
		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlace, Common.CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlace, Common.CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlace, Common.CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlace, Common.CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlace, Common.CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlace, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlace, Common.CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlace, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlace, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlace, Common.CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlace, Common.CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}
		protected override void SetupDeliveredAtTerminalConfiguration()
		{
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtTerminal, Common.CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtTerminal, Common.CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtTerminal, Common.CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtTerminal, Common.CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtTerminal, Common.CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtTerminal, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtTerminal, Common.CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtTerminal, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtTerminal, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtTerminal, Common.CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtTerminal, Common.CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}
		void SetupDeliveredDutyUnpaidConfiguration()
		{
			AddChargeConfiguration(IncotermList.Codes.DeliveredDutyUnpaid, Common.CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredDutyUnpaid, Common.CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredDutyUnpaid, Common.CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredDutyUnpaid, Common.CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredDutyUnpaid, Common.CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredDutyUnpaid, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredDutyUnpaid, Common.CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredDutyUnpaid, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredDutyUnpaid, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredDutyUnpaid, Common.CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredDutyUnpaid, Common.CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}
		void SetupDeliveredExQuayConfiguration()
		{
			AddChargeConfiguration(IncotermList.Codes.DeliveredExQuay, Common.CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExQuay, Common.CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExQuay, Common.CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExQuay, Common.CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExQuay, Common.CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExQuay, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExQuay, Common.CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExQuay, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExQuay, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExQuay, Common.CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExQuay, Common.CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}
		void SetupDeliveredExShipConfiguration()
		{
			AddChargeConfiguration(IncotermList.Codes.DeliveredExShip, Common.CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExShip, Common.CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExShip, Common.CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExShip, Common.CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExShip, Common.CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExShip, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExShip, Common.CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExShip, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExShip, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExShip, Common.CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredExShip, Common.CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}
		void SetupDeliveredAtPlaceUnloadedConfiguration()
		{
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlaceUnloaded, Common.CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlaceUnloaded, Common.CustomsChargeCodeProvider.Commission, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlaceUnloaded, Common.CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlaceUnloaded, Common.CustomsChargeCodeProvider.Discount, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlaceUnloaded, Common.CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlaceUnloaded, CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlaceUnloaded, Common.CustomsChargeCodeProvider.LandingCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlaceUnloaded, GetOverseasFreight(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlaceUnloaded, GetOverseasInsurance(), new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = true, IsRecommended = true });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlaceUnloaded, Common.CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
			AddChargeConfiguration(IncotermList.Codes.DeliveredAtPlaceUnloaded, Common.CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}
		protected override void SetupErrorConfiguration()
		{
		}

		protected override ICustomsChargeCode[] GetCharges()
		{
			var result = new List<ICustomsChargeCode>(base.GetCharges());
			var fifCharge = result.Find(x => x.Code == CustomsChargeTypeList.Codes.ForeignInlandFreight);
			result.Remove(fifCharge);
			result.Add(CustomsChargeCodeProvider.ForeignInlandFreight);
			return result.ToArray();
		}

		public override bool MakeFlagsReadOnlyWhenDeemed => true;
	}
}

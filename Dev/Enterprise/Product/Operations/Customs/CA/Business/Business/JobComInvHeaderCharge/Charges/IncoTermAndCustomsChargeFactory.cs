using System.Collections.Generic;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.CA.Business
{
	public partial class IncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
	{
		protected override void SetupCarriageAndInsurancePaidToConfiguration()
		{
			base.SetupCarriageAndInsurancePaidToConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriageAndInsurancePaidTo, IncoTermAndCustomsChargeFactory.Construction, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCarriagePaidToConfiguration()
		{
			base.SetupCarriagePaidToConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.CarriagePaidTo, IncoTermAndCustomsChargeFactory.Construction, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCostAndFreightConfiguration()
		{
			base.SetupCostAndFreightConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.CostAndFreight, IncoTermAndCustomsChargeFactory.Construction, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupCostInsuranceAndFreightConfiguration()
		{
			base.SetupCostInsuranceAndFreightConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.CostInsuranceAndFreight, IncoTermAndCustomsChargeFactory.Construction, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredAtPlaceConfiguration()
		{
			base.SetupDeliveredAtPlaceConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtPlace, IncoTermAndCustomsChargeFactory.Construction, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredAtTerminalConfiguration()
		{
			base.SetupDeliveredAtTerminalConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredAtTerminal, IncoTermAndCustomsChargeFactory.Construction, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupDeliveredDutyPaidConfiguration()
		{
			base.SetupDeliveredDutyPaidConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.DeliveredDutyPaid, IncoTermAndCustomsChargeFactory.Construction, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupErrorConfiguration()
		{
			base.SetupErrorConfiguration();
			AddChargeConfiguration(ErrorIncoTermCode, IncoTermAndCustomsChargeFactory.Construction, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupExWorksConfiguration()
		{
			base.SetupExWorksConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.ExWorks, IncoTermAndCustomsChargeFactory.Construction, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			base.SetupFreeAlongsideShipConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeAlongsideShip, IncoTermAndCustomsChargeFactory.Construction, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			base.SetupFreeCarrierConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeCarrier, IncoTermAndCustomsChargeFactory.Construction, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			base.SetupFreeOnBoardConfiguration();
			AddChargeConfiguration(Core.Constants.IncoTerms.FreeOnBoard, IncoTermAndCustomsChargeFactory.Construction, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = false, IsIncludedInInvoice = true, IsMandatory = false, IsRecommended = false });
		}

		protected override ICustomsChargeCode[] GetCharges()
		{
			var result = new List<ICustomsChargeCode>(base.GetCharges());
			result.Add(Construction);
			var addCharge = result.Find(x => x.Code == CustomsChargeTypeList.Codes.AdditionCharge);
			result.Remove(addCharge);
			result.Add(AdditionCharge);
			return result.ToArray();
		}
	}
}

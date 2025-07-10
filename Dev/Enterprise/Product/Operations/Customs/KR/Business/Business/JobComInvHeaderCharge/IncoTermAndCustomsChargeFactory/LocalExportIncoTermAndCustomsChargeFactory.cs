using System.Collections.Generic;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportIncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
	{
		protected override ICustomsChargeCode[] GetCharges()
		{
			var result = new List<ICustomsChargeCode>(base.GetCharges());
			var lchCharge = result.Find(x => x.Code == CustomsChargeTypeList.Codes.LandingCharges);
			if (lchCharge != null)
			{
				result.Remove(lchCharge);
			}
			var oftCharge = result.Find(x => x.Code == CustomsChargeTypeList.Codes.OverseasFreight);
			if (oftCharge != null)
			{
				result.Remove(oftCharge);
			}
			var onsCharge = result.Find(x => x.Code == CustomsChargeTypeList.Codes.OverseasInsurance);
			if (onsCharge != null)
			{
				result.Remove(onsCharge);
			}
			return result.ToArray();
		}

		protected override void SetupIncotermChargeConfigurations()
		{
			SetupExWorksConfiguration();
			SetupFreeAlongsideShipConfiguration();
			SetupFreeCarrierConfiguration();
			SetupFreeOnBoardConfiguration();
		}

		protected override void SetupExWorksConfiguration()
		{
			AddChargeConfiguration(IncotermList.Codes.ExWorks, Common.CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.ExWorks, Common.CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = false,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(IncotermList.Codes.ExWorks, Common.CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = true
			});
			AddChargeConfiguration(IncotermList.Codes.ExWorks, Common.CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.ExWorks, Common.CustomsChargeCodeProvider.Commission, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.ExWorks, Common.CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.ExWorks, Common.CustomsChargeCodeProvider.Discount, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.ExWorks, Common.CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
		}

		protected override void SetupFreeOnBoardConfiguration()
		{
			AddChargeConfiguration(IncotermList.Codes.FreeOnBoard, Common.CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeOnBoard, Common.CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeOnBoard, Common.CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeOnBoard, Common.CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeOnBoard, Common.CustomsChargeCodeProvider.Commission, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeOnBoard, Common.CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeOnBoard, Common.CustomsChargeCodeProvider.Discount, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeOnBoard, Common.CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
		}

		protected override void SetupFreeCarrierConfiguration()
		{
			AddChargeConfiguration(IncotermList.Codes.FreeCarrier, Common.CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeCarrier, Common.CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeCarrier, Common.CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeCarrier, Common.CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeCarrier, Common.CustomsChargeCodeProvider.Commission, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeCarrier, Common.CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeCarrier, Common.CustomsChargeCodeProvider.Discount, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeCarrier, Common.CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
		}

		protected override void SetupFreeAlongsideShipConfiguration()
		{
			AddChargeConfiguration(IncotermList.Codes.FreeAlongsideShip, Common.CustomsChargeCodeProvider.ExWorks, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeAlongsideShip, Common.CustomsChargeCodeProvider.ForeignInlandFreight, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeAlongsideShip, Common.CustomsChargeCodeProvider.PackingCost, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = true,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeAlongsideShip, Common.CustomsChargeCodeProvider.AdditionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeAlongsideShip, Common.CustomsChargeCodeProvider.Commission, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeAlongsideShip, Common.CustomsChargeCodeProvider.DeductionCharge, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeAlongsideShip, Common.CustomsChargeCodeProvider.Discount, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
			AddChargeConfiguration(IncotermList.Codes.FreeAlongsideShip, Common.CustomsChargeCodeProvider.OtherCharges, new ChargeConfiguration
			{
				IsIncludedInInvoiceAmountFixed = false,
				IsIncludedInInvoice = true,
				IsMandatory = false,
				IsRecommended = false
			});
		}

		protected override void SetupErrorConfiguration()
		{
		}
	}
}

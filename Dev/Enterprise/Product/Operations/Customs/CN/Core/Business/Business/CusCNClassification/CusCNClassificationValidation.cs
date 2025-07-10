using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class CusCNClassificationValidation : AutoCusCNClassificationValidation
	{
		public CusCNClassificationValidation(AutoCusCNClassification parent)
			: base(parent)
		{
		}

		protected new CusCNClassification Parent => (CusCNClassification)base.Parent;

		CusClassPartPivot CusClassPartPivot => Parent.Pivot;

		protected CusCNClassificationLookups Lookups => Parent.Lookups;

		protected override void CheckCNC_CIQTariff()
		{
			base.CheckCNC_CIQTariff();

			var ciTariff = CusClassPartPivot.CI_TariffNum;
			var ciqTariff = Parent.CNC_CIQTariff;
			var ciqTariffView = CNRefTariffDataLoader.GetCIQTariff(Parent.Factory, ciqTariff, ZDateTime.Today);

			if (ciqTariffView == null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CNC_CIQTariffInfo);
			}
			else if (!ciTariff.IsEmpty && !ciqTariff.IsEmpty && CNRefTariffDataLoader.GetCIQTariff(Parent.Factory, ciqTariff, ZDateTime.Today, ciTariff) == null)
			{
				Parent.CNC_CIQTariffInfo.AddMessageError(Res.GetString("c2440805-fdac-4765-b21c-7adbfc62ea93", "The CIQ Tariff Code '{0}' is not valid for the Customs Tariff Code '{1}'.", ciqTariff, ciTariff));
			}
		}

		protected override void CheckCNC_EndUse()
		{
			base.CheckCNC_EndUse();
			ListValidation.MessageErrorIfInvalidCode(Parent.CNC_EndUseInfo);
		}

		protected override void CheckCNC_DestinationDistrict()
		{
			base.CheckCNC_DestinationDistrict();
			ListValidation.MessageErrorIfInvalidCode(Parent.CNC_DestinationDistrictInfo);
		}

		protected override void CheckCNC_OriginDistrict()
		{
			base.CheckCNC_OriginDistrict();
			ListValidation.MessageErrorIfInvalidCode(Parent.CNC_OriginDistrictInfo);
		}

		protected override void CheckCNC_DestinationRegion()
		{
			base.CheckCNC_DestinationRegion();
			ListValidation.MessageErrorIfInvalidCode(Parent.CNC_DestinationRegionInfo);
		}

		protected override void CheckCNC_OriginState()
		{
			base.CheckCNC_OriginState();
			ListValidation.MessageErrorIfInvalidCode(Parent.CNC_OriginStateInfo);
		}

		protected override void CheckCNC_OriginRegion()
		{
			base.CheckCNC_OriginRegion();
			ListValidation.MessageErrorIfInvalidCode(Parent.CNC_OriginRegionInfo);
		}

		protected override void CheckCNC_TradeUnitQty()
		{
			base.CheckCNC_TradeUnitQty();
			var targetInfo = Parent.CNC_TradeUnitQtyInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
			if (Parent.CNC_TradeUnitPrice > 0 && Parent.CNC_TradeUnitQty.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("2BCEADCF-EE31-4C94-9941-A42221AED351", "Please enter Trade Quantity Unit.\r\nThe system will not default the value to the invoice line if the Trade Quantity Unit is not specified."));
			}
		}

		protected override void CheckCNC_TradeUnitPrice()
		{
			base.CheckCNC_TradeUnitPrice();
			MandatoryValidation.CheckNotNegative(Parent.CNC_TradeUnitPriceInfo);
		}

		protected override void CheckCNC_GoodsSpecModel()
		{
			base.CheckCNC_GoodsSpecModel();

			var tariff = Parent.Pivot.UniversalTariff;
			if (tariff != null)
			{
				Parent.Pivot.AdditionalInformationHelper.ValidateGoodsSpecModel();

				if (Parent.Pivot.AdditionalInformationCodes.Cast<AdditionalInformation>().Any(x => !x.CY_Data.IsEmpty && !tariff.GetSortedAdditionalInfoAttributes(Parent.Pivot.IsEnteringOrExiting).Any(attr => attr.ZZ3_Value == x.CY_Code)))
				{
					Parent.CNC_GoodsSpecModelInfo.AddWarning(Res.GetString("4ac6e517-c089-4d8b-981f-6e08dd897482", "Some Additional Information is not available for the selected Tariff, will be deleted after saving."));
				}
			}
		}

		protected override void CheckCNC_RX_NKTradeUnitPriceCurrency()
		{
			base.CheckCNC_RX_NKTradeUnitPriceCurrency();
			if (Parent.CNC_TradeUnitPrice > 0 && Parent.CNC_RX_NKTradeUnitPriceCurrency.IsEmpty)
			{
				Parent.CNC_RX_NKTradeUnitPriceCurrencyInfo.AddError(Res.GetString("451C504F-8026-4C21-80AE-077FAA43248F", "You should enter a currency when you have entered Trade Unit Price."));
			}
		}
	}
}

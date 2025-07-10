using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected override ZBool NeedsToCheckABL_GrossWeightMatchSumOfPacks => true;

		protected override ZBool NeedsToCheckABL_VolumeMatchSumOfPacks => true;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateCargoDisposition();
			ValidateTravelDocumentType();
		}

		#region CargoDisposition

		public void ValidateCargoDisposition()
		{
			ValidateCalculatedProperty(Parent.CargoDispositionInfo);
		}

		protected void CheckCargoDisposition()
		{
			if (Parent.Header.AMA_TransportMode == Core.Constants.TransportModes.Sea)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CargoDispositionInfo);
			}
		}

		#endregion

		#region TravelDocumentType

		public void ValidateTravelDocumentType()
		{
			ValidateCalculatedProperty(Parent.TravelDocumentTypeInfo);
		}

		protected void CheckTravelDocumentType()
		{
			if (Parent.Header.AMA_TransportMode == Core.Constants.TransportModes.Sea)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.TravelDocumentTypeInfo);
			}
		}

		#endregion

		#region  Shipper

		protected override void CheckABL_OA_Shipper()
		{
			base.CheckABL_OA_Shipper();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_OA_ShipperInfo);

			if (!Parent.ABL_OA_Shipper.IsEmpty)
			{
				if (Parent.ABL_RN_NKShipperCountry == Core.Constants.CountryCodes.Colombia)
				{
					var shipperNIT = Parent.Shipper?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == ColombiaOrgCusCodeInfo.OrgCusCodes.NIT)?.OK_CustomsRegNo ?? ZString.Empty;

					if (shipperNIT.IsEmpty)
					{
						Parent.ABL_OA_ShipperInfo.AddMessageError(ResString.GetMultilingualString("FFAA7347-D290-4214-9C97-16008E34B6FE", "The selected Shipper should have NIT"));
					}
				}
			}
		}

		protected override void CheckABL_ShipperRegNoType()
		{
			base.CheckABL_ShipperRegNoType();
			if (Parent.ABL_RN_NKShipperCountry == Core.Constants.CountryCodes.Colombia)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_ShipperRegNoTypeInfo);
			}
		}

		protected override void CheckABL_ShipperRegNo()
		{
			base.CheckABL_ShipperRegNo();
			if (Parent.ABL_RN_NKShipperCountry == Core.Constants.CountryCodes.Colombia)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperRegNoInfo);
			}
		}

		#endregion

		#region Consignee

		protected override void CheckABL_OA_Consignee()
		{
			base.CheckABL_OA_Consignee();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_OA_ConsigneeInfo);

			if (!Parent.ABL_OA_Consignee.IsEmpty)
			{
				if (Parent.ABL_RN_NKConsigneeCountry == Core.Constants.CountryCodes.Colombia)
				{
					var consigneeNIT = Parent.Consignee?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == ColombiaOrgCusCodeInfo.OrgCusCodes.NIT)?.OK_CustomsRegNo ?? ZString.Empty;

					if (consigneeNIT.IsEmpty)
					{
						Parent.ABL_OA_ConsigneeInfo.AddMessageError(ResString.GetMultilingualString("0692B832-F37D-4E36-A0B5-75ED105F9269", "The selected Consignee should have NIT"));
					}
				}
			}
		}

		protected override void CheckABL_ConsigneeRegNoType()
		{
			base.CheckABL_ConsigneeRegNoType();
			if (Parent.ABL_RN_NKConsigneeCountry == Core.Constants.CountryCodes.Colombia)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_ConsigneeRegNoTypeInfo);
			}
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();
			if (Parent.ABL_RN_NKConsigneeCountry == Core.Constants.CountryCodes.Colombia)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeRegNoInfo);
			}
		}

		#endregion

		protected override void CheckABL_OA_GoodsLocation()
		{
			base.CheckABL_OA_GoodsLocation();
			var header = Parent.Header;
			if (header != null && header.IsSea)
			{
				if (Parent.ABL_OA_GoodsLocation.IsEmpty)
				{
					Parent.ABL_OA_GoodsLocationInfo.AddMessageError(ResString.GetMultilingualString("7ED65EF0-1CB5-4E2B-AD9D-849220C0D20C", "A Warehouse is required"));
				}
				else
				{
					var goodsLocation = Parent.GoodsLocation?.Header;
					if (goodsLocation != null)
					{
						var regNumberCCP = goodsLocation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(header.AMA_RN_NKCountry, new ZString[] { OrgCusCode.CodeTypes.ControlledPremisesID });
						if (regNumberCCP.IsEmpty)
						{
							Parent.ABL_OA_GoodsLocationInfo.AddMessageError(ResString.GetMultilingualString("13352EFE-E14E-4E9C-82A7-A686BBD4317D", "The Warehouse should have a CCP assigned number."));
						}
					}
				}
			}
		}

		protected override void CheckABL_RX_NKGoodsValueCurrency()
		{
			base.CheckABL_RX_NKGoodsValueCurrency();

			if (Parent.ABL_GoodsValue > 0 && Parent.GoodsValuesWithUSD == 0)
			{
				Parent.ABL_RX_NKGoodsValueCurrencyInfo.AddMessageError(NoValidExRatesMessage(Parent.Header.AMA_MasterBillIssueDate.ToString("d", null)));
			}
		}

		protected override void CheckABL_RX_NKFreightValueCurrency()
		{
			base.CheckABL_RX_NKFreightValueCurrency();

			if (Parent.ABL_FreightValue > 0 && Parent.FreightValuesWithUSD == 0)
			{
				Parent.ABL_RX_NKFreightValueCurrencyInfo.AddMessageError(NoValidExRatesMessage(Parent.Header.AMA_MasterBillIssueDate.ToString("d", null)));
			}
		}

		protected override void CheckABL_Volume()
		{
			base.CheckABL_Volume();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_VolumeInfo);
		}

		protected override INotificationType ABL_ManifestQtyMatchSumOfPacksNotificationType => CargoWise.EntityFramework.NotificationType.MessageError;

		string NoValidExRatesMessage(string currencyConverterDateForRate)
		{
			ZStringBuilder noValidExRatesMessage = new ZStringBuilder();
			noValidExRatesMessage.Append(Res.GetString("F14C5BB0-3DBC-4463-83E9-147B7B5C6522", "There is no valid exchange rate for this currency for {0}.", currencyConverterDateForRate));
			noValidExRatesMessage.Append(GetAdviceHowToFixNoValidExchangeRates());
			return noValidExRatesMessage.ToString();
		}

		string GetAdviceHowToFixNoValidExchangeRates()
		{
			return System.Environment.NewLine + Res.GetString("82A1BB3E-C166-43DE-B6BD-923900E7A6DB", "Please check with your system administrator to ensure that your system is downloading customs exchange rate correctly.");
		}

		protected override void CheckABL_BillIssueDate()
		{
			base.CheckABL_BillIssueDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BillIssueDateInfo);
		}

		protected override void CheckABL_GoodsDescription()
		{
			base.CheckABL_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_GoodsDescriptionInfo);
		}

		protected override void CheckABL_VolumeUQ()
		{
			base.CheckABL_VolumeUQ();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_VolumeUQInfo);
		}

		protected override void CheckABL_RL_NKFinalDestination()
		{
			base.CheckABL_RL_NKFinalDestination();

			if (!Parent.ABL_RL_NKFinalDestination.IsEmpty)
			{
				var uNLOCO = Parent.FinalDestination;
				if (uNLOCO != null && uNLOCO.RL_RW.IsEmpty)
				{
					Parent.ABL_RL_NKFinalDestinationInfo.AddMessageError(ResString.GetMultilingualString("65958158-AF0C-4156-A64F-1E9122D4EC5E", "The selected Final Destination should have State entered"));
				}
			}
		}

		protected override void CheckABL_ContainerMode()
		{
			base.CheckABL_ContainerMode();
			if (Parent.IsSea)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_ContainerModeInfo);
			}
		}
	}
}

using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidationForMasterChild
	{
		public AsycudaBillValidationForMasterChild(ASYCUDA.Business.AsycudaBill parent) : base(parent)
		{
		}
		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateTransportDocumentType();
			ValidateShipperPersonType();
		}

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected override bool IsABL_E_DEPRequired
		{
			get
			{
				var parent = Parent;
				var manifestHeader = parent.Header;

				var optionalSpecificCircumstances = new ZString[] {
					EUICS2SpecificCircumstanceList.Codes.F40,
					EUICS2SpecificCircumstanceList.Codes.F41,
					EUICS2SpecificCircumstanceList.Codes.F44,
					EUICS2SpecificCircumstanceList.Codes.F50,
					EUICS2SpecificCircumstanceList.Codes.F51 };

				var result = !manifestHeader.IsSea && !manifestHeader.IsInlandWaterway
					&& !manifestHeader.SpecificCircumstanceIndicator.In(optionalSpecificCircumstances);

				return result;
			}
		}

		protected override void CheckMandatoryABL_E_ARV()
		{
			if (!Header.IsSea && !Header.IsInlandWaterway && Header.SpecificCircumstanceIndicator != EUICS2SpecificCircumstanceList.Codes.F44)
			{
				base.CheckMandatoryABL_E_ARV();
			}
		}

		protected override void CheckABL_BillNumberCore()
		{
			if (Header.SpecificCircumstanceIndicator != EUICS2SpecificCircumstanceList.Codes.F44)
			{
				base.CheckABL_BillNumberCore();
			}
		}

		protected override void CheckABL_RL_NKPortOfLoading()
		{
			var parent = Parent;
			if (parent.Factory.IsMemberOfICS2(parent.ABL_RL_NKPortOfLoading.SubstringSafe(0, 2)))
			{
				parent.ABL_RL_NKPortOfLoadingInfo.AddMessageError(Res.GetString("b3329022-e0c6-48b3-b8da-2a15334529b0", "The Load port must not be an EU port."));
			}

			var manifestHeader = parent.Header;
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(parent.ABL_RL_NKPortOfLoadingInfo, manifestHeader.SpecificCircumstanceIndicatorInfo,
				(ZString)EUICS2SpecificCircumstanceList.Codes.F40, Res.GetString("CFD4D1BB-EA3A-444E-81FC-2F90593AA3FB", "You have not entered a Port Of Loading."));
		}

		protected override void CheckABL_RL_NKPortOfDischarge()
		{
			var parent = Parent;
			var manifestHeader = parent.Header;

			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(parent.ABL_RL_NKPortOfDischargeInfo, manifestHeader.SpecificCircumstanceIndicatorInfo,
				(ZString)EUICS2SpecificCircumstanceList.Codes.F40, Res.GetString("6C90E903-125A-4D6F-9D11-48D573D2953D", "You have not entered a Port of Unloading."));
		}

		protected override void CheckABL_RL_NKOrigin()
		{
			var parent = Parent;
			if (parent.Header is AsycudaManifestHeader header && header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.ABL_RL_NKOriginInfo);
			}
		}

		protected override void CheckABL_RL_NKFinalDestination()
		{
			var parent = Parent;
			if (parent.Header is AsycudaManifestHeader header && header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.ABL_RL_NKFinalDestinationInfo);
			}
		}

		#region Shipper

		public void ValidateShipperPersonType()
		{
			ValidateCalculatedProperty(Parent.ShipperPersonTypeInfo);
		}

		protected void CheckShipperPersonType()
		{
			if (Parent.Header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator)
			{
				ValidationHelper.AddShipperRequiredMessageErrorIfEmpty(Parent.ShipperPersonTypeInfo);
			}
		}

		protected override void CheckABL_ShipperPostcode()
		{
			var shipperCountry = Parent.ABL_RN_NKShipperCountry;
			if (Parent.Header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator && !shipperCountry.IsEmpty && !Parent.Lookups.CodeList733.ContainsCode(shipperCountry))
			{
				ValidationHelper.AddShipperRequiredMessageErrorIfEmpty(Parent.ABL_ShipperPostcodeInfo);
			}
		}

		protected override void CheckABL_ShipperName()
		{
			base.CheckABL_ShipperName();
			if (Parent.Header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator)
			{
				ValidationHelper.AddShipperRequiredMessageErrorIfEmpty(Parent.ABL_ShipperNameInfo);
			}
		}

		protected override void CheckABL_ShipperCity()
		{
			base.CheckABL_ShipperCity();
			if (Parent.Header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator)
			{
				ValidationHelper.AddShipperRequiredMessageErrorIfEmpty(Parent.ABL_ShipperCityInfo);
			}
		}

		protected override void CheckABL_OA_Shipper()
		{
			base.CheckABL_OA_Shipper();
			if (Parent.ABL_ShipperName.IsEmpty && Parent.Header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator)
			{
				ValidationHelper.AddShipperRequiredMessageErrorIfEmpty(Parent.ABL_OA_ShipperInfo);
			}
		}

		protected override void CheckABL_RN_NKShipperCountry()
		{
			base.CheckABL_RN_NKShipperCountry();
			if (Parent.Header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator)
			{
				ValidationHelper.AddShipperRequiredMessageErrorIfEmpty(Parent.ABL_RN_NKShipperCountryInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.ABL_RN_NKShipperCountryInfo);
			}
		}
		#endregion

		public void ValidateTransportDocumentType()
		{
			ValidateCalculatedProperty(Parent.TransportDocumentTypeInfo);
		}

		protected void CheckTransportDocumentType()
		{
			var parent = Parent;
			var manifestHeader = parent.Header;
			if (manifestHeader.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F40)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.TransportDocumentTypeInfo);
			}

			ValidationHelper.CheckTransportDocumentType(parent);
		}
	}
}

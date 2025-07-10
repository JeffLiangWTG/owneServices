using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.GB.Business.GBUniversalReferenceConstants;

namespace Enterprise.Customs.GB.H7.Business
{
	public class AsycudaBillValidationForRegularBill : EU.H7.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
		{
		}

		new AsycudaBill Parent => base.Parent as AsycudaBill;

		AsycudaManifestHeader Header => Parent.Header;

		bool IsForBIRDS => Header?.IsForBIRDS ?? false;

		bool IOSSNumberProvided => !Parent.ABL_SellerRegNo.IsEmpty
			&& Parent.ABL_SellerRegNoType == OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAddititionalProcedureCodeAsString();
			ValidateGoodsLocationDescription();
			ValidateAdditionalInfos();
			ValidatePostponedVatAccountingCheck();
		}

		protected void CheckAdditionalProcedureCodesAsString()
		{
			if (!Parent.AdditionalProcedureCodes.ContainsCode("4000C07") && !Parent.AdditionalProcedureCodes.ContainsCode("4000C08"))
			{
				Parent.AdditionalProcedureCodesAsStringInfo.AddMessageError(Res.GetString("f2bda048-79a4-404a-b783-60730fc29358", "Either Additional Procedure Code 4000C07 or 4000C08 need to be entered."));
			}
		}

		void ValidateAdditionalInfos()
		{
			var nidomRequiredMessage = Res.GetString("a0ec2266-239f-432d-9ebd-105b9bb475ff", "Additional Info Code ‘NIDOM’ is required when Load Port is in mainland Great Britain and Discharge Port is in Northern Ireland.");
			var niimpRequiredMessage = Res.GetString("632a6289-e534-4c8a-beca-aff794019b94", "Additional Info Code ‘NIIMP’ is required when Load Port is not in Great Britain and Discharge Port is in Northern Ireland.");
			var mutuallyExclusiveMessage = Res.GetString("6a3b4609-dde4-49a4-8fe2-1f70860591be", "Additional Info Codes ‘NIIMP’ and ‘NIDOM’ are mutually exclusive.");
			var niimpRequiredMessageForIOSSNumberAndProcedureCode = Res.GetString("31e2361a-726e-43e2-8e4c-3197f0b74929", "If ‘Seller IOSS Number’ has been provided and ‘Additional Procedure Code’ contains the value ‘F48’ then ‘Additional Info’ Code must contain the value ‘NIIMP’.");

			Parent.ClearRowNotificationsContaining(nidomRequiredMessage);
			Parent.ClearRowNotificationsContaining(niimpRequiredMessage);
			Parent.ClearRowNotificationsContaining(niimpRequiredMessageForIOSSNumberAndProcedureCode);

			var addInfoNIIMPCode = Parent.AdditionalInfos.FirstOrDefault(x => x.CSI_Code == GBCommonConstants.AdditonalInfoCodes.NIIMP);
			addInfoNIIMPCode?.ClearRowNotificationsContaining(mutuallyExclusiveMessage);
			var addInfoNIDOMCode = Parent.AdditionalInfos.FirstOrDefault(x => x.CSI_Code == GBCommonConstants.AdditonalInfoCodes.NIDOM);
			addInfoNIDOMCode?.ClearRowNotificationsContaining(mutuallyExclusiveMessage);

			if (addInfoNIIMPCode != null && addInfoNIDOMCode != null)
			{
				addInfoNIDOMCode.AddRowMessageError(mutuallyExclusiveMessage);
				addInfoNIIMPCode.AddRowMessageError(mutuallyExclusiveMessage);
			}
			else
			{
				var shouldHaveNIIMPCode = (!Parent.Header?.PortOfLoading?.IsInGreatBritain ?? false) && (Parent.Header?.PortOfDischarge?.IsInNorthernIreland ?? false);
				var shouldHaveNIDOMCode = (Parent.Header?.PortOfLoading?.IsInGreatBritain ?? false) && (Parent.Header?.PortOfDischarge?.IsInNorthernIreland ?? false);

				if (shouldHaveNIDOMCode && addInfoNIDOMCode == null)
				{
					Parent.AddRowMessageError(nidomRequiredMessage);
				}

				if (shouldHaveNIIMPCode && addInfoNIIMPCode == null)
				{
					Parent.AddRowMessageError(niimpRequiredMessage);
				}
			}

			if (addInfoNIIMPCode == null
				&& IOSSNumberProvided
				&& Parent.AdditionalProcedureCodes.AsString.Contains(ProcedureCodes.Concession.F48))
			{
				Parent.AddRowMessageError(niimpRequiredMessageForIOSSNumberAndProcedureCode);
			}
		}

		protected override void CheckGoodsLocationDescription()
		{
			if (Parent.GoodsLocationDescription.IsEmpty)
			{
				Parent.GoodsLocationDescriptionInfo.AddMessageError(Res.GetString("43b709fb-55f8-4740-b918-2b256e223926", "You have not entered a Location of Goods."));
			}
			else
			{
				base.CheckGoodsLocationDescription();
			}
		}

		protected override void CheckABL_TransportValue()
		{
			if (Parent.ABL_TransportValue == 0)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.ABL_TransportValueInfo);
			}
		}

		protected override void CheckABL_RL_NKOrigin()
		{
			base.CheckABL_RL_NKOrigin();
			if (IsForBIRDS)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RL_NKOriginInfo);
			}
		}

		protected override void CheckABL_GrossWeight()
		{
		}

		protected override void CheckABL_ShipperName()
		{
			base.CheckABL_ShipperName();
			ValidateMandatoryField(Parent.ABL_ShipperNameInfo, Res.GetString("e515ec0d-7778-4086-99b8-7b61985fefde", "Exporter Name"));
		}

		protected override void CheckABL_ShipperStreet1()
		{
			base.CheckABL_ShipperStreet1();
			ValidateShipperStreet(Parent.ABL_ShipperStreet1Info);
		}

		protected override void CheckABL_ShipperStreet2()
		{
			base.CheckABL_ShipperStreet2();
			ValidateShipperStreet(Parent.ABL_ShipperStreet2Info);
		}

		protected override void CheckABL_ShipperCity()
		{
			base.CheckABL_ShipperCity();
			ValidateMandatoryField(Parent.ABL_ShipperCityInfo, Res.GetString("797644e5-2dd0-499d-b45f-17ffb7332442", "Exporter City"));
		}

		protected override void CheckABL_RN_NKShipperCountry()
		{
			base.CheckABL_RN_NKShipperCountry();
			ValidateMandatoryField(Parent.ABL_RN_NKShipperCountryInfo, Res.GetString("8f9177b1-978d-4ab0-a5ca-06bc61b513cc", "Exporter Country/Region"));
		}

		protected override void CheckABL_ShipperPostcode()
		{
			base.CheckABL_ShipperPostcode();
			ValidateMandatoryField(Parent.ABL_ShipperPostcodeInfo, Res.GetString("278e124b-e6b5-4dfc-9bbe-7a7e531aa436", "Exporter Postcode"));
		}

		protected override bool NeedCheckSellerRegNoEnteredAndValid => Parent.ABL_SellerRegNoType == OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration
				&& Parent.AdditionalInfos.Any(x => x.CSI_Code == GBCommonConstants.AdditonalInfoCodes.NIIMP)
				&& Parent.AdditionalProcedureCodes.AsString.Contains(ProcedureCodes.Concession.F48);

		void ValidateMandatoryField(ZPropertyInfo propertyInfo, ZString propertyDescription)
		{
			if (propertyInfo.Value.IsEmpty)
			{
				var message = MandatoryValidation.MustBeEnteredMessage(propertyDescription);
				propertyInfo.AddMessageError(message);
			}
		}

		void ValidateShipperStreet(ZPropertyInfo streetPropertyInfo)
		{
			if (Parent.ABL_ShipperStreet1.IsEmpty && Parent.ABL_ShipperStreet2.IsEmpty)
			{
				var message = MandatoryValidation.MustBeEnteredMessage(Res.GetString("82626b8b-e4c4-4cb2-9a2e-4e18dcb3f4c6", "Exporter Street Address"));
				streetPropertyInfo.AddMessageError(message);
			}
		}

		public void ValidatePostponedVatAccountingCheck()
		{
			ValidateCalculatedProperty(Parent.PostponedVatAccountingCheckInfo);
		}

		protected void CheckPostponedVatAccountingCheck()
		{
			if (IOSSNumberProvided && Parent.PostponedVatAccountingCheck)
			{
				var message = Res.GetString("7749b576-0576-4d0d-a447-54e84fcaa24c", "Importer VAT number and Seller IOSS number cannot be combined on a single customs declaration.");
				Parent.PostponedVatAccountingCheckInfo.AddMessageError(message);
			}
		}
	}
}

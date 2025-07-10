using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.GB.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public partial class JobDeclarationValidation : AutoGBJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent; }
		}

		protected override bool ShouldValidatePackagesActualPackageCount
		{
			get { return false; }  // No need for box 6 to equal the sum of the box 31 line packages
		}

		protected override CargoWise.ComponentModel.INotificationType JE_VesselNameNotificationSeverity => CargoWise.ComponentModel.NotificationType.Warning;

		protected override void CheckJE_UCR()
		{
			base.CheckJE_UCR();
			var ducr = Parent.JE_UCR;
			if (!ducr.IsEmpty)
			{
				if (!Regex.IsMatch(ducr, @"^[0-9]GB[0-9]{12}-[0-9A-Z\-()]{1,19}$"))
				{
					Parent.JE_UCRInfo.AddMessageError("DUCR must match format defined in section 1.8.10 of the Tariff Vol 3");
				}
			}
		}

		public override ZString ErrorMessageTextForDuplicateUCR => "This DUCR is already in use on another declaration. Ensure uniqueness by adding an alpha suffix to the end, e.g. " + Parent.JE_UCR + "A.  Do not add a part suffix (/1, /2, etc).";

		public const string E02944_ErrorMessage = "A warehouse without a registration number has been selected. This will cause the following message to be received: 'E02944 - CPC requires the declaration of a Premise Identifier.'  You need to edit the warehouse's details to ensure that it has a registration number.";
		public const string InvalidSupervisingOffice_ErrorMessage = "The supervising office selected does not have a valid registration number.";
		public const string SupervisingOfficeDoesNotExist_ErrorMessage = "The registration number associated with this supervising office is not valid.";

		public override void ValidateWarehouseDocAddressForeignKey()
		{
			base.ValidateWarehouseDocAddressForeignKey();

			if (Parent.WarehouseDocAddress.Organisation != null)
			{
				if (Parent.WarehouseDocAddress.Organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, Core.Constants.CountryCodes.UnitedKingdom, Parent.WarehouseDocAddress.E2_OA_Address).IsEmpty)
				{
					Parent.WarehouseDocAddress.E2_OA_AddressInfo.AddMessageError(E02944_ErrorMessage);
				}
			}
		}

		public void ValidateSupervisingOfficeDocAddress()
		{
			ValidateSupervisingOfficeDocAddressCore();
		}

		protected virtual void ValidateSupervisingOfficeDocAddressCore()
		{
		}

		protected override void CheckJE_TransportMode()
		{
			if (WantsValidationForTransportMode)
			{
				base.CheckJE_TransportMode();
			}
		}

		bool WantsValidationForTransportMode
		{
			get
			{
				switch (Parent.JE_DeclarationType)
				{
					case ImportSADDeclarationTypeList.Codes.ImportFullDeclaration:
					case ImportSADDeclarationTypeList.Codes.ImportFullWarehouse:
					case ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration:
					case ImportSADDeclarationTypeList.Codes.ImportSupplementaryWarehouse:
					case ExportSADDeclarationTypeList.Codes.ExportFullDeclaration:
					case ExportSADDeclarationTypeList.Codes.ExportLCPPreShipment:
					case ExportSADDeclarationTypeList.Codes.ExportSDPPreShipment:
					case ExportSADDeclarationTypeList.Codes.ExportSupplementaryDeclaration:
						return true;

					default:
						return false;
				}
			}
		}

		protected override void CheckJE_PaymentMethodLogicForEU()
		{
		}

		protected override void CheckJE_HouseBill()
		{
			base.CheckJE_HouseBill();

			if (Parent.IsInventoryControlledAirImport && !Parent.JE_HouseBill.IsEmpty && Parent.JE_HouseBill.Length != 8)
			{
				Parent.JE_HouseBillInfo.AddMessageError("House Number must be 8 characters.");
			}
		}

		protected virtual void CheckJE_MasterUCR()
		{
			if (Parent.JE_MasterUCR.IsEmpty)
			{
				if (Parent.IsImport && (Parent.HasPortInventoryAttribute || (Parent.IsInventoryControlledAirImport && Parent.IsCCSUK)) && !GBCustomsDataRegistry.Instance.DisableBlueValidationOnMUCRForInventoryLinkedPortsImport.Value)
				{
					Parent.JE_MasterUCRInfo.AddMessageError("Inventory linking will not occur if you do not enter a Master UCR (inventory consignment reference, ICR)");
				}
				else if (Parent.IsExport && !GBCustomsDataRegistry.Instance.DisableBlueValidationOnMUCRForExport.Value)
				{
					Parent.JE_MasterUCRInfo.AddMessageError("Inventory linking will not occur if you do not enter a Master UCR. You may associate this entry to a MUCR later by sending an ASS (associate) message for one entry, or using Operational Actions to associate many entries to a MUCR in bulk.");
				}
				else
				{
					Parent.JE_MasterUCRInfo.AddWarning("Inventory linking will not occur if you do not enter a Master UCR");
				}
			}
			else if (!Parent.MasterUCR.IsDeleted)
			{
				Parent.MasterUCR.Validation.ValidateCE_EntryNum();
			}
		}

		protected override void CheckJE_ApplicationCode()
		{
			base.CheckJE_ApplicationCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_ApplicationCodeInfo);

			if (!Parent.WasSavedAsCHIEF && DeclarationApplicationCodeList.Codes.CHIEF.Equals(Parent.JE_ApplicationCode.ToString(), StringComparison.InvariantCultureIgnoreCase))
			{
				Parent.JE_ApplicationCodeInfo.AddError("You cannot convert a non-CHIEF JobDeclaration to the CHIEF type or create a new CHIEF JobDeclaration.");
			}
		}

		protected void CheckSubLocation()
		{
			ListValidation.WarnIfInvalidCode(Parent.SubLocationInfo);
		}

		public void ValidateSubLocation()
		{
			ValidateCalculatedProperty(Parent.SubLocationInfo);
		}

		public void ValidateJE_DeclarationType()
		{
			ValidateCalculatedProperty(Parent.JE_DeclarationTypeInfo);
		}

		public void ValidateJE_EntrySubStyle()
		{
			ValidateCalculatedProperty(Parent.JE_EntrySubStyleInfo);
		}

		protected override void CheckJE_MessageSubType()
		{
			base.CheckJE_MessageSubType();
			ValidateJE_EntrySubStyle();
		}

		protected override void CheckJE_LocationQualifier()
		{
			base.CheckJE_LocationQualifier();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationQualifierInfo, Parent.Lookups.LocationQualifiers);
		}

		protected void CheckJE_Calc_LocationOtherInformationCountry()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.JE_Calc_LocationOtherInformationCountryInfo);
			if (!Parent.JE_Calc_LocationOtherInformationCountry.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_Calc_LocationOtherInformationCountryInfo, twoCharacterCodesValidationError);
			}
		}
		static ResourceString twoCharacterCodesValidationError => ResString.GetMultilingualString("197617be-2e0b-4263-892c-8cffb9f4adcc", "Two-character codes must match those in the Tariff, Vol III, appendix C1, excluding those already known in [Maintain > Location > Countries].");

		public void ValidateJE_Calc_LocationOtherInformationCountry()
		{
			ValidateCalculatedProperty(Parent.JE_Calc_LocationOtherInformationCountryInfo);
		}

		protected void CheckJE_Calc_LocationOtherInformationType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_Calc_LocationOtherInformationTypeInfo, Parent.Lookups.LocationOfGoodsTypes);
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.JE_Calc_LocationOtherInformationTypeInfo);
		}

		public void ValidateJE_Calc_LocationOtherInformationType()
		{
			ValidateCalculatedProperty(Parent.JE_Calc_LocationOtherInformationTypeInfo);
		}

		protected virtual void CheckJE_GoodsLocation()
		{
		}

		public void ValidateJE_GoodsLocation()
		{
			ValidateCalculatedProperty(Parent.JE_GoodsLocationInfo);
		}

		public void ValidateJE_CHIEF_GoodsLocation()
		{
			ValidateCalculatedProperty(Parent.JE_CHIEF_GoodsLocationInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateSubLocation();
			ValidateJE_Calc_LocationOtherInformationCountry();
			ValidateJE_Calc_LocationOtherInformationType();
			ValidateJE_GoodsLocation();
			ValidateJE_CHIEF_GoodsLocation();
			ValidateJE_DeclarationType();
			ValidateJE_EntrySubStyle();
			ValidateCourierConsignmentType();
			ValidateJE_EidrType();
			ValidateJE_MasterUCR();
		}

		public void ValidateJE_MasterUCR()
		{
			ValidateCalculatedProperty(Parent.JE_MasterUCRInfo);
		}

		protected override void CheckJE_MessageType()
		{
			base.CheckJE_MessageType();
			ValidateJE_DeclarationType();
		}

		protected override void CheckJE_IATALoadPort()
		{
			base.CheckJE_IATALoadPort();
			if (Parent.IsExport)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JE_IATALoadPortInfo, "foreign airport code. It is not needed for exports");
			}
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			ListValidation.IfInvalidCode(NotificationType.MessageError, Parent.JE_CustomsOfficeInfo, Parent.Lookups.CustomsOffices, "The code you have selected is not in the list.");
		}

		public void ValidateCourierConsignmentType()
		{
			ValidateCalculatedProperty(Parent.CourierConsignmentTypeInfo);
		}
		protected void CheckCourierConsignmentType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CourierConsignmentTypeInfo);
		}

		protected override void CheckJE_NorthernIrelandMode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_NorthernIrelandModeInfo);
		}

		protected override void CheckJE_EidrType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_EidrTypeInfo);
		}

		protected override void CheckJE_EntryAuthorisationDate()
		{
			base.CheckJE_EntryAuthorisationDate();
			if (Parent.IsSupplementaryDeclarationType && Parent.JE_EntryAuthorisationDate.IsEmpty)
			{
				Parent.JE_EntryAuthorisationDateInfo.AddMessageError("Date should not be empty for CHIEF supplementary declarations");
			}
		}

		protected override void CheckJE_CustomsProfile()
		{
			base.CheckJE_CustomsProfile();
			if (Parent.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Customs_Declaration_Services && Parent.JE_GB.IsValid)
			{
				var badgeSetting = GBCustomsDataRegistry.Instance.BadgeCodes.GetValueWithoutFallback(Guid.Empty, Parent.JE_GB.ToGuid(), Guid.Empty)
					.FindByBadgeCode(Parent.JE_CustomsProfile, Parent.JE_MessageType);
				if (badgeSetting != null && badgeSetting.ApplicationCodeIsChief)
				{
					Parent.JE_CustomsProfileInfo.AddMessageError("The selected profile is set to be a CHIEF profile. Select a different profile or application");
				}
			}
		}
	}
}

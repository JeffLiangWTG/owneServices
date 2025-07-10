using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.CA.Business.CusBondDetailCollection;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ImportJobDocAddressValidation : JobDocAddressValidation
	{
		public ImportJobDocAddressValidation(AutoJobDocAddress parent, JobDeclaration declaration)
			: base(parent)
		{
			this.declaration = declaration;
		}

		#region CheckE2_Contact

		protected override void CheckE2_Contact()
		{
			base.CheckE2_Contact();
			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.ImporterPickupDeliveryAddress:
					if (declaration.IsOGD && declaration.CA_OGDCFIA)
					{
						bool overriddenContactIsNotValid = Parent.E2_AddressOverride && Parent.E2_Contact.IsEmpty;
						bool orgContactIsNotValid = !Parent.E2_AddressOverride && (Parent.ContactPK.IsEmpty || Parent.Contact.OC_Phone.IsEmpty || Parent.Contact.OC_Fax.IsEmpty);
						if ((overriddenContactIsNotValid || (orgContactIsNotValid && !Parent.OrganisationPKInfo.HasNotifications())) &&
								(Parent.E2_Phone.IsEmpty || Parent.E2_Fax.IsEmpty))
						{
							Parent.E2_ContactInfo.AddMessageError(Res.GetString("684d7b35-0482-4ec0-8c9a-e7a6ad67b161", "Delivery contact with Phone and Fax is required for CFIA shipments."));
						}
					}
					break;
			}

			var messageError = ContactNameValidataionHelper.ValidationContactName(Parent.E2_Contact);
			if (!messageError.IsEmpty)
			{
				Parent.E2_ContactInfo.AddMessageError(messageError);
			}
		}

		#endregion

		#region CheckE2_Phone

		protected override void CheckE2_Phone()
		{
			base.CheckE2_Phone();
			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.ImporterPickupDeliveryAddress:
					if (declaration.IsOGD && declaration.CA_OGDCFIA && Parent.E2_AddressOverride && Parent.E2_Phone.IsEmpty)
					{
						Parent.E2_PhoneInfo.AddMessageError(Res.GetString("9c1dc4bb-2041-4806-a5d0-2a3a0230d108", "Phone number is required for CFIA shipments."));
					}
					break;
			}
		}

		#endregion

		#region CheckE2_Fax

		protected override void CheckE2_Fax()
		{
			base.CheckE2_Fax();
			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.ImporterPickupDeliveryAddress:
					if (declaration.IsOGD && declaration.CA_OGDCFIA && Parent.E2_AddressOverride && Parent.E2_Fax.IsEmpty)
					{
						Parent.E2_FaxInfo.AddMessageError(Res.GetString("1938cce2-aa6e-47df-94a8-e713d4ab2d9f", "Fax number is required for CFIA shipments."));
					}
					break;
			}
		}

		#endregion

		#region CheckOrganisationPK

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.CustomsWarehouseAddress:
					ValidateWarehouseAddress();
					OrganisationValidation.ValidateNamesAndAddressesSpecial(Parent.OrganisationPKInfo, Parent.Organisation);
					break;
				case DocAddressTypes.Codes.ImporterDocumentaryAddress:
					if (!declaration.ImporterOfRecordAddress.HasRealOrganisation)
					{
						((ImportJobDeclarationValidation)declaration.Validation).ValidateImporterDocumentaryAddress(Parent.OrganisationPKInfo);
					}
					break;
				case DocAddressTypes.Codes.ImporterPickupDeliveryAddress:
					if (declaration.IsOGD && declaration.CA_OGDCFIA)
					{
						CAAddressValidator.ValidateMandatory(Parent, Parent.OrganisationPKInfo, Res.GetString("19c441a7-21e8-4851-ace6-8f5a1fb4b45e", "Importer Delivery"));
						if (Parent.OrganisationPKInfo.HasNotifications())
						{
							ValidateE2_Contact();
						}

						if (Parent.Contact == null)
						{
							if (Parent.E2_Phone.IsEmpty || Parent.E2_Fax.IsEmpty)
							{
								Parent.OrganisationPKInfo.AddMessageError(cFIAPhoneAndFaxMessage);
							}
						}
					}
					OrganisationValidation.ValidateNamesAndAddressesSpecial(Parent.OrganisationPKInfo, Parent.Organisation);
					break;
				case DocAddressTypes.Codes.SupplierDocumentaryAddress:
					if (!declaration.IsLVS && !Parent.E2_AddressOverride)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo, Res.GetString("9a3bc5da-c227-4dad-afa7-b8ad1d5d0306", "Main Vendor"));
					}
					OrganisationValidation.ValidateNamesAndAddressesSpecial(Parent.OrganisationPKInfo, Parent.Organisation);
					break;
				case DocAddressTypes.Codes.ImporterOfRecord:
					if (!declaration.IsLVS && Parent.HasRealOrganisation)
					{
						var importerOfRecord = Parent.Organisation;
						ImportJobDeclarationValidation.CheckHasImporterBusinessNumberForImportExport(importerOfRecord, Parent.OrganisationPKInfo, declaration.IsCADEnabled, declaration.IsExistingEffectiveCasualImport, Res.GetString("22968030-52c0-4842-9a1a-020935a85725", "Importer of Record"), declaration.Importer);
						((ImportJobDeclarationValidation)declaration.Validation).ValidateEffectiveImporter(importerOfRecord, Parent.OrganisationPKInfo);
						AuthorityToActValidationHelper.Validate(Parent.OrganisationPKInfo, declaration, () => importerOfRecord, ImportExportCodeList.Codes.Import, declaration.JE_CustomsOffice);

						IBondDetailsDefault bondDefault = declaration;
						var bondDetails = bondDefault.ImporterOfRecord?.BondDetails;
						if (bondDetails != null)
						{
							if (bondDetails.GetBondDetailsStatus(BondTypeList.Codes.NotOnPortal, bondDefault.EffectiveDate) == BondDetailsStatus.BondExist)
							{
								ImportAddInfoJobDeclarationValidation.ValidateBondType(declaration, Parent.OrganisationPKInfo, BondTypeList.Codes.NotOnPortal);
							}

							if (bondDetails.GetBondDetailsStatus(BondTypeList.Codes.OnPortal, bondDefault.EffectiveDate) == BondDetailsStatus.BondExist)
							{
								ImportAddInfoJobDeclarationValidation.ValidateBondType(declaration, Parent.OrganisationPKInfo, BondTypeList.Codes.OnPortal);
							}
						}
					}
					OrganisationValidation.ValidateNamesAndAddressesSpecial(Parent.OrganisationPKInfo, Parent.Organisation);
					break;
				case DocAddressTypes.Codes.CommercialInvoiceOriginator:
					OrganisationValidation.ValidateNamesAndAddressesSpecial(Parent.OrganisationPKInfo, Parent.Organisation);
					break;
			}
		}

		internal static string cFIAPhoneAndFaxMessage
		{
			get { return Res.GetString("61fafd4c-a3fe-49a6-929d-88dd1a11aae9", "Delivery address with Phone and Fax is required for CFIA shipments."); }
		}

		#region ValidateWarehouseAddress

		void ValidateWarehouseAddress()
		{
			if (declaration.IsCADEnabled ? CADEntryTypeList.WarehouseEntryTypes.Contains(declaration.JE_MessageSubType) : B3EntryTypeList.WarehouseEntryTypes.Contains(declaration.JE_MessageSubType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo);
				if (!Parent.OrganisationPKInfo.HasMessageErrors() && Parent.Organisation != null
					&& Parent.Organisation.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, Constants.CountryCodes.Canada) == null)
				{
					Parent.OrganisationPKInfo.AddMessageError(Res.GetString("56ba1702-0da1-4aa3-8127-8a48bd72d286", "CBSA Warehouse Code (CPW) must be specified on Organization Registration Numbers tab."));
				}
			}
		}

		#endregion

		#endregion

		#region CheckE2_OA_Address

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (!Parent.E2_AddressOverride)
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.ImporterOfRecord:
						if (!declaration.IsLVS && !Parent.OrganisationPK.IsEmpty && Parent.HasRealAddress)
						{
							CAAddressValidator.ValidateAddressInCanada(declaration, Parent, Parent.E2_OA_AddressInfo, Res.GetString("22968030-52c0-4842-9a1a-020935a85725", "Importer of Record"));
						}
						declaration.Validation.ValidateJE_OH_Importer();
						break;
				}
			}
		}

		#endregion

		#region CheckE2_RN_NKCountryCode

		protected override void CheckE2_RN_NKCountryCode()
		{
			base.CheckE2_RN_NKCountryCode();
			ValidateE2_State();
			ValidateE2_Postcode();
		}

		#endregion

		#region CheckE2_Postcode

		protected override void CheckE2_Postcode()
		{
			base.CheckE2_Postcode();
			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.SupplierDocumentaryAddress:
					if (Parent.E2_AddressOverride && !declaration.IsLVS)
					{
						CAAddressValidator.MaxLengthValidation(Parent.E2_PostcodeInfo, 9, propertyDescription: CAAddressValidator.PostCodeCaption);
						CAAddressValidator.PostCodeValidation(Parent.E2_Postcode, Parent.E2_PostcodeInfo, Parent.E2_RN_NKCountryCode);
					}
					break;
			}
		}

		#endregion

		#region CheckE2_State

		protected override void CheckE2_State()
		{
			base.CheckE2_State();
			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.SupplierDocumentaryAddress:
					if (Parent.E2_AddressOverride && !declaration.IsLVS)
					{
						CAAddressValidator.MaxLengthValidation(Parent.E2_StateInfo, 9, propertyDescription: CAAddressValidator.StateCaption);
						CAAddressValidator.StateValidation(Parent.E2_State, Parent.E2_StateInfo, Parent.E2_RN_NKCountryCode);
					}
					break;
			}

			if (declaration.IsIID &&
				(Parent.E2_RN_NKCountryCode == Constants.CountryCodes.Canada || Parent.E2_RN_NKCountryCode == Constants.CountryCodes.UnitedStates) &&
				Parent.E2_State.Length > 2)
			{
				Parent.E2_StateInfo.AddMessageError(StateCodeMustBe2Characters);
			}
		}

		internal static string StateCodeMustBe2Characters
		{
			get
			{
				return Res.GetString("c21b3c7d-0e3e-49bc-8c7d-e4c7a73b5d65", "The state code must be 2 characters. Please press F3 in below organization field and change");
			}
		}

		#endregion

		#region CheckE2_City

		protected override void CheckE2_City()
		{
			base.CheckE2_City();
			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.SupplierDocumentaryAddress:
					if (Parent.E2_AddressOverride && !declaration.IsLVS)
					{
						CAAddressValidator.MaxLengthValidation(Parent.E2_CityInfo, 35, propertyDescription: CAAddressValidator.CityCaption, mandatory: true);
					}
					break;
			}

			if (declaration.IsIID && !Parent.OrganisationPK.IsEmpty && Parent.HasRealAddress && Parent.E2_City.IsEmpty)
			{
				Parent.E2_CityInfo.AddMessageError(CityNameMustBeEntered);
			}
		}

		internal static string CityNameMustBeEntered
		{
			get
			{
				return Res.GetString("6ef975ed-5781-432c-9881-be47bcd9978b", "The city name must be entered. Please press F3 in below organization field and change");
			}
		}

		#endregion

		#region CheckE2_Address1

		protected override void CheckE2_Address1()
		{
			base.CheckE2_Address1();
			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.SupplierDocumentaryAddress:
					if (Parent.E2_AddressOverride && !declaration.IsLVS)
					{
						if (Parent.E2_Address2.IsEmpty)
						{
							CAAddressValidator.SplitMaxLengthValidation(Parent.E2_Address1, 70, 35, Parent.E2_Address1Info, CAAddressValidator.Address1Caption, true);
						}
						else
						{
							CAAddressValidator.MaxLengthValidation(Parent.E2_Address1, 35, Parent.E2_Address1Info, CAAddressValidator.Address1Caption, true);
						}
					}
					break;
			}
		}

		#endregion

		#region CheckE2_Address2

		protected override void CheckE2_Address2()
		{
			base.CheckE2_Address2();
			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.SupplierDocumentaryAddress:
					if (Parent.E2_AddressOverride && !declaration.IsLVS && !Parent.E2_Address2.IsEmpty)
					{
						CAAddressValidator.MaxLengthValidation(Parent.E2_Address2, 35, Parent.E2_Address2Info, CAAddressValidator.Address2Caption);
					}
					break;
			}
		}

		#endregion

		#region CheckE2_CompanyName

		protected override void CheckE2_CompanyName()
		{
			base.CheckE2_CompanyName();
			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.SupplierDocumentaryAddress:
					if (Parent.E2_AddressOverride && !declaration.IsLVS)
					{
						CAAddressValidator.SplitMaxLengthValidation(Parent.E2_CompanyNameTruncated, 70, 35, Parent.E2_CompanyNameInfo, CAAddressValidator.NameCaption, true);
					}
					break;
			}
		}

		#endregion

		readonly JobDeclaration declaration;
	}
}

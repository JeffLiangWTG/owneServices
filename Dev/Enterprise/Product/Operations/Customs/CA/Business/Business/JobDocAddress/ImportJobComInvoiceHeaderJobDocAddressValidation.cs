using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ImportJobComInvoiceHeaderJobDocAddressValidation : JobDocAddressValidation
	{
		public ImportJobComInvoiceHeaderJobDocAddressValidation(AutoJobDocAddress parent, JobComInvoiceHeader invoiceHeader)
			: base(parent)
		{
			this.invoiceHeader = invoiceHeader;
		}
		readonly JobComInvoiceHeader invoiceHeader;

		#region CheckE2_OA_Address (both)

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (!Parent.E2_AddressOverride)
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.SupplierDocumentaryAddress:
						CheckE2_OA_AddressForSupplierDocumentaryAddress();
						break;
					case DocAddressTypes.Codes.SupplierPickupDeliveryAddress:
						CheckE2_OA_AddressForSupplierPickupDeliveryAddress();
						break;
					case DocAddressTypes.Codes.FinalConsigneeAddress:
						CheckE2_OA_AddressForFinalConsigneeAddress();
						break;
					case DocAddressTypes.Codes.Exporter:
						CheckE2_OA_AddressForExporterDocumentaryAddress();
						break;
					case DocAddressTypes.Codes.BuyerDocumentaryAddress:
						CheckE2_OA_AddressForBuyerDocumentaryAddress();
						break;
				}
			}
		}

		void CheckE2_OA_AddressForSupplierDocumentaryAddress()
		{
			var declaration = invoiceHeader.JobDeclaration;
			if (!Parent.OrganisationPK.IsEmpty && Parent.Organisation != null && declaration != null)
			{
				var declarationValidator = declaration.DeclarationValidator;
				if (declarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS))
				{
					CAAddressValidator.ValidateMandatory(Parent, Parent.E2_OA_AddressInfo, Vendor);
				}
				if (declarationValidator.IsValidationRequired(ValidateForMessageType.B3CUSDEC))
				{
					CAAddressValidator.ValidateMandatoryName30Only(Parent, Parent.E2_OA_AddressInfo, Vendor);

					if (invoiceHeader.IsUSCountryOfExport && invoiceHeader.VendorStateAndZip.State.IsEmpty)
					{
						Parent.E2_OA_AddressInfo.AddMessageError(VendorStateIsInvalid);
					}
				}

				if (declaration.IsIID && invoiceHeader.Manufacturer == null)
				{
					var requiredMessageAdded = false;
					var vendorFallBackInvoices = invoiceHeader.InvoiceLines.OfType<JobComInvoiceLine>().Where(x => x.JI_OA_ManufacturerAddress.IsEmpty);
					if (invoiceHeader.HasInvoiceLinesWithWENIndOnECCCPGA || invoiceHeader.HasInvoiceLinesWithPESIndOnHCPGA)
					{
						if (vendorFallBackInvoices.Any(x => new PGAInvoiceLineValidator(x).IsJI_OA_ManufacturerAddressPhoneAndEmailRequired))
						{
							OrganisationValidation.ValidateCAPContactOrAddressPhoneAndEmailRequired(Parent.E2_OA_AddressInfo, Parent);
							requiredMessageAdded = true;
						}
					}

					if (!requiredMessageAdded && invoiceHeader.HasInvoiceLinesWithCPRIndOnHCPGA && vendorFallBackInvoices.Any(x => new PGAInvoiceLineValidator(x).IsAddressPhoneOrEmailRecommended))
					{
						OrganisationValidation.ValidateCAPContactOrAddressPhoneAndEmailRecommened(Parent.E2_OA_AddressInfo, Parent);
					}
				}
			}
		}

		void CheckE2_OA_AddressForSupplierPickupDeliveryAddress()
		{
			if (!Parent.OrganisationPK.IsEmpty && Parent.Organisation != null)
			{
				CAAddressValidator.ValidateMandatory(Parent, Parent.E2_OA_AddressInfo, Res.GetString("947b2518-38dd-4bea-b2eb-4c4d4a54effd", "Shipper"));
			}
		}

		void CheckE2_OA_AddressForFinalConsigneeAddress()
		{
			var parent = Parent;
			if (!parent.OrganisationPK.IsEmpty && parent.Organisation != null)
			{
				var invoiceHeader = this.invoiceHeader;

				CAAddressValidator.ValidateMandatory(parent, parent.E2_OA_AddressInfo, Res.GetString("12a38d65-6278-4425-973d-4b45b5a5099e", "Consignee"));
				if (invoiceHeader.JobDeclaration?.IsIID ?? false)
				{
					var capAllocation = OrganisationValidation.GetCAPContactAllocation(parent.Organisation);
					if (capAllocation == null)
					{
						OrganisationValidation.ValidateAddressPhone(invoiceHeader.FinalConsigneeAddress?.Address, parent.E2_OA_AddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired, CargoWise.EntityFramework.NotificationType.MessageError);
					}
				}
				var address = parent.Address;
				if (invoiceHeader.IsImport && address != null && address.OA_RN_NKCountryCode != Core.Constants.CountryCodes.Canada)
				{
					parent.E2_OA_AddressInfo.AddWarning(ImportJobComInvoiceLineValidation.ConsigneeAddrIsNotCA);
				}
			}
		}

		void CheckE2_OA_AddressForExporterDocumentaryAddress()
		{
			var declaration = invoiceHeader.JobDeclaration;
			var isIID = declaration?.IsIID ?? false;
			if (!Parent.OrganisationPK.IsEmpty && Parent.Organisation != null
				|| Parent.OrganisationPK.IsEmpty
					&& invoiceHeader.JZ_OA_ExporterAddress.IsEmpty
					&& isIID
					&& invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().Any(IsRequiredExporter))
			{
				CAAddressValidator.ValidateMandatory(Parent, Parent.E2_OA_AddressInfo, Res.GetString("188e6b7d-2ae1-46da-8646-e911fbb491ae", "Exporter"));

				if (isIID && !declaration.DoesRequireExporterHasPGAContact)
				{
					var capAllocation = OrganisationValidation.GetCAPContactAllocation(Parent.Organisation);
					if (capAllocation == null)
					{
						OrganisationValidation.ValidateAddressPhone(invoiceHeader.ExporterDocumentaryAddress?.Address, Parent.E2_OA_AddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired, CargoWise.EntityFramework.NotificationType.MessageError);
					}
				}
			}
		}

		bool IsRequiredExporter(JobComInvoiceLine invoiceLine)
		{
			var cnscPGAHeader = invoiceLine?.CNSCPGAHeader;
			return cnscPGAHeader != null && (cnscPGAHeader.CA_Category == CNSCCategories.Codes.NE || cnscPGAHeader.CA_Category == CNSCCategories.Codes.CNS);
		}

		void CheckE2_OA_AddressForBuyerDocumentaryAddress()
		{
			if (!Parent.OrganisationPK.IsEmpty && Parent.Organisation != null)
			{
				CAAddressValidator.ValidateMandatory(Parent, Parent.E2_OA_AddressInfo, Res.GetString("087b38cd-7c94-475e-9473-16649065f545", "Purchaser"));

				if (invoiceHeader.JobDeclaration?.IsIID ?? false)
				{
					var capAllocation = OrganisationValidation.GetCAPContactAllocation(Parent.Organisation);
					if (capAllocation == null)
					{
						OrganisationValidation.ValidateAddressPhone(invoiceHeader.BuyerDocumentaryAddress?.Address, Parent.E2_OA_AddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired, CargoWise.EntityFramework.NotificationType.MessageError);
					}
				}
			}
		}

		internal static string VendorStateIsInvalid
		{
			get { return Res.GetString("E644B8BB-57D1-4F88-86B8-0BA66C317583", "Either Vendor or Exporter address must have a valid US state code, or the country/region code is PR, UM or VI."); }
		}

		static string Vendor
		{
			get { return Res.GetString("e956d8e5-9df1-4018-87fb-87be3c39504c", "Vendor"); }
		}
		static string Exporter
		{
			get { return Res.GetString("A3A4723B-90FF-418F-A537-46B7230B6E3D", "Exporter"); }
		}

		#endregion

		#region CheckOrganisationPK (both)

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			if (!Parent.E2_AddressOverride)
			{
				var isIID = invoiceHeader.JobDeclaration?.IsIID ?? false;
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.SupplierDocumentaryAddress:
						MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo, Vendor);
						if (isIID && invoiceHeader.Manufacturer == null)
						{
							if (invoiceHeader.HasInvoiceLinesWithWENIndOnECCCPGA)
							{
								OrganisationValidation.ValidateCAPAllocatedContact(Parent.Organisation, Parent.OrganisationPKInfo, PGACodes.Descriptions.ECCC, false, true);
							}
							else
							{
								OrganisationValidation.ValidateOrgContactPhone(Parent.Organisation, Parent.OrganisationPKInfo);
							}
						}
						break;
					case DocAddressTypes.Codes.FinalConsigneeAddress:
						if (isIID)
						{
							OrganisationValidation.ValidateOrgContactPhone(Parent.Organisation, Parent.OrganisationPKInfo);
						}
						break;
					case DocAddressTypes.Codes.Exporter:
						if (isIID)
						{
							if (invoiceHeader.HasInvoiceLinesWithWENIndOnECCCPGA)
							{
								MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo, Exporter);
								OrganisationValidation.ValidateCAPAllocatedContact(Parent.Organisation, Parent.OrganisationPKInfo, PGACodes.Descriptions.ECCC, false, true);
							}
							else
							{
								OrganisationValidation.ValidateOrgContactPhone(Parent.Organisation, Parent.OrganisationPKInfo);
							}
						}
						break;
					case DocAddressTypes.Codes.BuyerDocumentaryAddress:
						if (isIID)
						{
							OrganisationValidation.ValidateOrgContactPhone(Parent.Organisation, Parent.OrganisationPKInfo);
						}
						break;
				}
				OrganisationValidation.ValidateNamesAndAddressesSpecial(Parent.OrganisationPKInfo, Parent.Organisation);
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
			if (Parent.E2_AddressOverride)
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.SupplierDocumentaryAddress:
					case DocAddressTypes.Codes.SupplierPickupDeliveryAddress:
						CAAddressValidator.MaxLengthValidation(Parent.E2_PostcodeInfo, 9, propertyDescription: CAAddressValidator.PostCodeCaption);
						CAAddressValidator.PostCodeValidation(Parent.E2_Postcode, Parent.E2_PostcodeInfo, Parent.E2_RN_NKCountryCode);
						break;
				}
			}
		}

		#endregion

		#region CheckE2_State

		protected override void CheckE2_State()
		{
			base.CheckE2_State();
			if (Parent.E2_AddressOverride)
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.SupplierDocumentaryAddress:
					case DocAddressTypes.Codes.SupplierPickupDeliveryAddress:
						CAAddressValidator.MaxLengthValidation(Parent.E2_StateInfo, 9, propertyDescription: CAAddressValidator.StateCaption);
						CAAddressValidator.StateValidation(Parent.E2_State, Parent.E2_StateInfo, Parent.E2_RN_NKCountryCode);
						break;
				}

				var declarationValidator = invoiceHeader.JobDeclaration?.DeclarationValidator;
				if (Parent.E2_AddressType == DocAddressTypes.Codes.SupplierDocumentaryAddress && declarationValidator != null && declarationValidator.IsValidationRequired(ValidateForMessageType.B3CUSDEC))
				{
					if (invoiceHeader.IsUSCountryOfExport && invoiceHeader.VendorStateAndZip.State.IsEmpty)
					{
						Parent.E2_StateInfo.AddMessageError(VendorStateIsInvalid);
					}
				}
			}
		}

		#endregion

		#region CheckE2_City

		protected override void CheckE2_City()
		{
			base.CheckE2_City();
			if (Parent.E2_AddressOverride)
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.SupplierDocumentaryAddress:
					case DocAddressTypes.Codes.SupplierPickupDeliveryAddress:
						CAAddressValidator.MaxLengthValidation(Parent.E2_CityInfo, 35, propertyDescription: CAAddressValidator.CityCaption, mandatory: true);
						break;
				}
			}
		}

		#endregion

		#region CheckE2_Address1

		protected override void CheckE2_Address1()
		{
			base.CheckE2_Address1();
			if (Parent.E2_AddressOverride)
			{
				var declarationValidator = invoiceHeader.JobDeclaration?.DeclarationValidator;
				if (Parent.E2_AddressType == DocAddressTypes.Codes.SupplierPickupDeliveryAddress || Parent.E2_AddressType == DocAddressTypes.Codes.SupplierDocumentaryAddress && declarationValidator != null && declarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS))
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
			}
		}

		#endregion

		#region CheckE2_Address2

		protected override void CheckE2_Address2()
		{
			base.CheckE2_Address2();
			if (Parent.E2_AddressOverride && !Parent.E2_Address2.IsEmpty)
			{
				var declarationValidator = invoiceHeader.JobDeclaration?.DeclarationValidator;
				if (Parent.E2_AddressType == DocAddressTypes.Codes.SupplierPickupDeliveryAddress || Parent.E2_AddressType == DocAddressTypes.Codes.SupplierDocumentaryAddress && declarationValidator != null && declarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS))
				{
					CAAddressValidator.MaxLengthValidation(Parent.E2_Address2, 35, Parent.E2_Address2Info, CAAddressValidator.Address2Caption);
				}
			}
		}

		#endregion

		#region CheckE2_CompanyName

		protected override void CheckE2_CompanyName()
		{
			base.CheckE2_CompanyName();
			if (Parent.E2_AddressOverride)
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.SupplierDocumentaryAddress:
						var declarationValidator = invoiceHeader.JobDeclaration?.DeclarationValidator;
						if (declarationValidator != null)
						{
							if (declarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS))
							{
								CAAddressValidator.SplitMaxLengthValidation(Parent.E2_CompanyNameTruncated, 70, 35, Parent.E2_CompanyNameInfo, CAAddressValidator.NameCaption, true);
							}
							if (declarationValidator.IsValidationRequired(ValidateForMessageType.B3CUSDEC))
							{
								CAAddressValidator.MaxLengthValidation(Parent.E2_CompanyNameTruncated, 30, Parent.E2_CompanyNameInfo, CAAddressValidator.NameCaption, true);
							}
						}
						break;
					case DocAddressTypes.Codes.SupplierPickupDeliveryAddress:
						CAAddressValidator.SplitMaxLengthValidation(Parent.E2_CompanyNameTruncated, 70, 35, Parent.E2_CompanyNameInfo, CAAddressValidator.NameCaption, true);
						break;
				}
			}
		}

		#endregion

		#region CheckE2_Contact

		protected override void CheckE2_Contact()
		{
			base.CheckE2_Contact();
			if (invoiceHeader.IsImport)
			{
				var messageError = ContactNameValidataionHelper.ValidationContactName(Parent.E2_Contact);
				if (!messageError.IsEmpty)
				{
					Parent.E2_ContactInfo.AddMessageError(messageError);
				}
			}
		}

		#endregion
	}
}

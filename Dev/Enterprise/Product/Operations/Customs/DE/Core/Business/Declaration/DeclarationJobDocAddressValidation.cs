using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	sealed class DeclarationJobDocAddressValidation : JobDocAddressValidation
	{
		public DeclarationJobDocAddressValidation(JobDocAddress address, JobDeclaration declaration)
			: base(address)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			var parent = Parent;
			var info = parent.OrganisationPKInfo;
			var addressType = parent.E2_AddressType;
			var isImportAndNotStockMovement = declaration.IsImportAndNotStockMovement;
			var isSupplierDocumentaryAddress = addressType == DocAddressTypes.Codes.SupplierDocumentaryAddress;

			if (addressType == DocAddressTypes.Codes.ImporterDocumentaryAddress)
			{
				if (isImportAndNotStockMovement)
				{
					CheckOrganisationPKIsMandatoryForEZA();
					CheckOrganisationPK_ImporterIsGermanResident_IMP();
				}
			}
			else if (isSupplierDocumentaryAddress && isImportAndNotStockMovement)
			{
				CheckOrganisationPKIsMandatoryForEZA();
			}
			else if (IsExport)
			{
				if (addressType == DocAddressTypes.Codes.Carrier)
				{
					CheckOrganisationPK_CarrierEUBorderEoriDetailsAndTcuNumber();
				}
				else if (addressType == DocAddressTypes.Codes.SupplierPickupDeliveryAddress)
				{
					CheckOrganisationPK_PickupIsMandatory();
				}
				else if (addressType == DocAddressTypes.Codes.ContractualPartner)
				{
					var exportOrg = declaration.ExporterDocAddress.Organisation;

					if (parent.Organisation.HasSameEoriOrTcu(exportOrg))
					{
						info.AddMessageError(Res.GetString("fbdef21e-56e0-40f8-a8e6-22ec18ac6a13", "Contractual Partner and Exporter must not be equal."));
					}
				}
				else if (addressType == DocAddressTypes.Codes.Exporter)
				{
					var declarantOrg = (OrgHeader)declaration.JE_OA_DeclarantAddress_ZAddress.OrgHeader;

					if (parent.Organisation.HasSameEori(declarantOrg))
					{
						info.AddMessageError(Res.GetString("7b508ced-aa4e-4dfc-8c57-26643c4d54ac", "Exporter and [14] Declarant must not be equal."));
					}

					if (declaration.CustomsEntryInstructions.Any(i => i.Constellation2ndDigitIs1And4thDigitIs1()) && parent.Organisation.HasSameEori(declaration.Seller))
					{
						info.AddMessageError(Res.GetString("0ef74643-c5f9-444e-acb5-d663c38169d7", "Exporter and [2] Subcontractor must not be equal."));
					}
				}
				else if (isSupplierDocumentaryAddress && !declaration.IsInwardProcessingAVABR)
				{
					MandatoryValidation.WarnIfNotEntered(info, Res.GetString("59343FB8-8DE6-45D0-B514-51879C3017BF", "[2] Supplier"));
				}
			}

			void CheckOrganisationPKIsMandatoryForEZA()
			{
				if (CustomsEntryInstructions.Any(x => x.CEI_Style == ImportDeclarationTypeList.Codes.EZA))
				{
					MandatoryValidation.MessageErrorIfNotEntered(info);
				}
			}

			void CheckOrganisationPK_ImporterIsGermanResident_IMP()
			{
				if (declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.Concession == "F49"))
				{
					var countryOfResidence = (string)parent.Address?.OA_RN_NKCountryCode;
					if (countryOfResidence != Core.Constants.CountryCodes.Germany)
					{
						info.AddMessageError(Res.GetString("3481FBB5-3F3A-43EE-9ADF-38712B1C2761", "EU-Code 'F49' detected - [8] Importer must have an Organization Address in DE – Germany."));
					}
				}
			}

			void CheckOrganisationPK_CarrierEUBorderEoriDetailsAndTcuNumber()
			{
				var orgHeader = parent.Organisation;

				if (orgHeader.HasEUEoriRegNo())
				{
					if (parent.Address.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix).IsEmpty)
					{
						info.AddMessageError(Res.GetString("E679AA08-AC00-4149-86B7-2655AC730C2E", "Carrier EU Border is missing EORI Branch."));
					}
				}
				else if (orgHeader != null && orgHeader.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU).Length != 1)
				{
					info.AddMessageError(Res.GetString("1CF12A06-5211-4D78-AFB7-E71CD8E92BED", "Carrier EU Border is missing EORI or TCUI number."));
				}
			}

			void CheckOrganisationPK_PickupIsMandatory()
			{
				if (CustomsEntryInstructions.Any(x => x.GoodsLocation.CGL_Qualifier.In(new ZString[] { CusGoodsLocationQualifierList.Codes.UnLocode, CusGoodsLocationQualifierList.Codes.GnssCoordinates, CusGoodsLocationQualifierList.Codes.Address })))
				{
					MandatoryValidation.MessageErrorIfNotEntered(info);
				}
			}
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();

			var parent = Parent;
			var address = parent.Address;
			var info = parent.E2_OA_AddressInfo;
			if (IsExport && parent.E2_AddressType == DocAddressTypes.Codes.SupplierPickupDeliveryAddress && !parent.E2_AddressOverride && address != null)
			{
				if (parent.E2_Address1.Length > 35)
				{
					info.AddMessageError(address1MaxLengthMessage);
				}
				if (parent.E2_Address2.Length > 35)
				{
					info.AddMessageError(address2MaxLengthMessage);
				}
				if (parent.E2_Postcode.Length != 5)
				{
					info.AddMessageError(postCodeLengthMessage);
				}

				if (CustomsEntryInstructions.Any(x => x.GoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.GnssCoordinates))
				{
					if (address.OA_Longitude.IsEmpty && address.OA_Latitude.IsEmpty)
					{
						info.AddMessageError(Res.GetString("E3D79971-5E27-4399-9FA9-0090D36EAE6C", "Organization of Pickup Address has no GPS Coordinates."));
					}
				}
			}
		}

		protected override void CheckE2_Address1()
		{
			base.CheckE2_Address1();

			var parent = Parent;
			if (IsExport && parent.E2_AddressType == DocAddressTypes.Codes.SupplierPickupDeliveryAddress && parent.E2_AddressOverride && parent.E2_Address1.Length > 35)
			{
				parent.E2_Address1Info.AddMessageError(address1MaxLengthMessage);
			}
		}
		readonly ZString address1MaxLengthMessage = Res.GetString("EEE52605-A273-4772-8DD3-613DC317B747", "Address 1 should have a maximum length of 35 characters.");

		protected override void CheckE2_Address2()
		{
			base.CheckE2_Address2();

			var parent = Parent;
			if (IsExport && parent.E2_AddressType == DocAddressTypes.Codes.SupplierPickupDeliveryAddress && parent.E2_AddressOverride && parent.E2_Address2.Length > 35)
			{
				parent.E2_Address2Info.AddMessageError(address2MaxLengthMessage);
			}
		}
		readonly ZString address2MaxLengthMessage = Res.GetString("0BB1DE94-F8D7-463C-B4A9-2CE98D3A55DA", "Address 2 should have a maximum length of 35 characters.");

		protected override void CheckE2_Postcode()
		{
			base.CheckE2_Postcode();

			var parent = Parent;
			if (IsExport && parent.E2_AddressType == DocAddressTypes.Codes.SupplierPickupDeliveryAddress && parent.E2_AddressOverride && parent.E2_Postcode.Length != 5)
			{
				parent.E2_PostcodeInfo.AddMessageError(postCodeLengthMessage);
			}
		}
		readonly ZString postCodeLengthMessage = Res.GetString("CF44A9D1-E01B-4D39-A6F1-F3986F408A60", "Postcode should be exactly 5 characters.");

		protected override void CheckE2_AddressOverride()
		{
			base.CheckE2_AddressOverride();

			var parent = Parent;
			if (IsExport && parent.E2_AddressType == DocAddressTypes.Codes.SupplierPickupDeliveryAddress && parent.E2_AddressOverride)
			{
				if (CustomsEntryInstructions.Any(x => x.GoodsLocation.CGL_Qualifier.In(new ZString[] { CusGoodsLocationQualifierList.Codes.UnLocode, CusGoodsLocationQualifierList.Codes.GnssCoordinates })))
				{
					parent.E2_AddressOverrideInfo.AddMessageError(Res.GetString("C007145B-BA33-4866-BB60-5263929F12B8", "You cannot use Address Override if Qualifier of Identification contains 'U' or 'W'."));
				}
			}
		}

		ZBool IsExport => declaration.IsExport;

		ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => declaration.CustomsEntryInstructions;
	}
}

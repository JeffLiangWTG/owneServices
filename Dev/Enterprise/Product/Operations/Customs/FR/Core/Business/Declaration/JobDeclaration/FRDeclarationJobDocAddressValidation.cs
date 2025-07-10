using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.MessagesWrappers;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class FRDeclarationJobDocAddressValidation : JobDocAddressValidation
	{
		public FRDeclarationJobDocAddressValidation(JobDocAddress address, JobDeclaration declaration)
			: base(address)
		{
			this.declaration = declaration;
		}

		protected readonly JobDeclaration declaration;

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.ImporterDocumentaryAddress:
					CheckImporter();
					break;
				case DocAddressTypes.Codes.SupplierDocumentaryAddress:
					CheckSupplier();
					break;
				default:
					break;
			}

			if (Parent.E2_AddressType == DocAddressTypes.Codes.DefermentParty && !declaration.DefermentPartyDocAddress.OrganisationPK.IsEmpty)
			{
				CheckRuleNat_020(Parent, Parent.OrganisationPKInfo);
			}
		}

		void CheckRuleNat_020(JobDocAddress address, ZPropertyInfo propertyInfo)
		{
			if (declaration?.Validation.ValidationDecider is IDeclarationValidationDecider { IsRuleNAT_020Active: true })
			{
				string ruleCode = "NAT_020";
				DeltaIEDeclarationValidationHelper.CheckEORI(propertyInfo, address, ruleCode);
			}
		}

		void CheckImporter()
		{
			if (declaration.IsImport)
			{
				var importer = declaration.Importer;
				CheckImporterOrSupplier(declaration, importer, (NoResString)"importer");

				var registrationNumber = ZString.Empty;
				if (importer != null)
				{
					if (importer.GetRegoCodeOfThisOrg(OrgCusCode.FranceCodeTypes.Siret).IsEmpty)
					{
						Parent.OrganisationPKInfo.AddMessageError(ErrorCollectorHelper.ImporterSrtNotConfigured);
					}
				}
			}
			else if (declaration.IsExport && declaration.IsDeltaC)
			{
				bool notAllInvoicesHaveBuyers = declaration.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.JZ_OH_Buyer.IsEmpty);
				var addressDocumentary = declaration.SupplierDocumentaryAddress;
				var documentaryAddressNotProperlyProvided = addressDocumentary.IsNull || addressDocumentary.IsOverridenButEmpty || !addressDocumentary.E2_AddressOverride;
				if (documentaryAddressNotProperlyProvided && declaration.JE_OH_Importer.IsEmpty && notAllInvoicesHaveBuyers)
				{
					Parent.OrganisationPKInfo.AddMessageError(ImporterMandatory);
				}
			}
		}

		public static string ImporterMandatory => Res.GetString("773FD6FA-22D8-4DAD-87A9-BDC88BAF72E9", "Importer is mandatory. Either set a supplier at declaration level or set a buyer to all invoices within the declaration.");

		void CheckSupplier()
		{
			if (declaration.IsExport)
			{
				var supplier = declaration.Supplier;
				CheckImporterOrSupplier(declaration, supplier, (NoResString)"supplier");
				var registrationNumber = ZString.Empty;
				if (supplier != null)
				{
					if (supplier.GetRegoCodeOfThisOrg(OrgCusCode.FranceCodeTypes.Siret).IsEmpty && supplier.GetEORI().IsEmpty)
					{
						Parent.OrganisationPKInfo.AddMessageError(ErrorCollectorHelper.SupplierSrtNotConfigured);
					}
				}
			}
			else if (declaration.IsImport && declaration.IsDeltaC)
			{
				bool notAllInvoicesHaveSuppliers = declaration.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.JZ_OH_Supplier.IsEmpty);
				var addressDocumentary = declaration.ImporterDocumentaryAddress;
				var documentaryAddressNotProperlyProvided = addressDocumentary.IsNull || addressDocumentary.IsOverridenButEmpty || !addressDocumentary.E2_AddressOverride;
				if (documentaryAddressNotProperlyProvided && declaration.JE_OH_Supplier.IsEmpty && notAllInvoicesHaveSuppliers)
				{
					Parent.OrganisationPKInfo.AddMessageError(SupplierMandatory);
				}
			}
		}

		public static string SupplierMandatory => Res.GetString("0758502D-C133-4C9F-8B03-5EC92970F6FB", "Supplier is mandatory. Either set a supplier at declaration level or set a supplier to all invoices within the declaration.");

		void CheckImporterOrSupplier(JobDeclaration declaration, OrgHeader header, ZString type)
		{
			if (declaration.JE_DeclarantType != EU.Business.RepresentationTypeList.Codes._3Indirect)
			{
				if (header == null)
				{
					Parent.OrganisationPKInfo.AddMessageError(declaration.IsImport ? ErrorCollectorHelper.ImporterIsRequired : ErrorCollectorHelper.SupplierIsRequired);
				}
			}

			if (declaration.IsImport)
			{
				var temp = ZString.Empty;
				var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
				var isOverrided = importerDocumentaryAddress?.E2_AddressOverride ?? false;

				if ((isOverrided && ErrorCollectorHelper.DeliveryDepartmentCantBeDeterminateJobDocAddressPostCode(importerDocumentaryAddress, ref temp)) || (!isOverrided && ErrorCollectorHelper.DeliveryDepartmentCantBeDeterminate(importerDocumentaryAddress?.Address, ref temp)))
				{
					Parent.OrganisationPKInfo.AddMessageError(Res.GetString("E5142F21-E664-436D-9948-F856D755ACC9", "Configure post code of {0} main address.", type));
				}
				if ((isOverrided && importerDocumentaryAddress.E2_GovRegNum.IsEmpty) || (!isOverrided && header.GetEUVATCodeOfThisOrg().IsEmpty))
				{
					Parent.OrganisationPKInfo.AddMessageError(MessageBuilderHelper.MessageTvaCodeNotFound);
				}
			}
			else
			{
				var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
				var isOverrided = supplierDocumentaryAddress?.E2_AddressOverride ?? false;

				if ((isOverrided && supplierDocumentaryAddress.E2_GovRegNum.IsEmpty) || (!isOverrided && header.GetEUVATCodeOfThisOrg().IsEmpty))
				{
					Parent.OrganisationPKInfo.AddMessageError(MessageBuilderHelper.MessageTvaCodeNotFound);
				}
			}
		}
	}
}

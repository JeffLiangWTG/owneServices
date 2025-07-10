using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class DeclarationJobDocAddressValidation : JobDocAddressValidation
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
		}

		void CheckImporter()
		{
			if (declaration.IsImport && declaration.JE_OH_Importer.IsEmpty && declaration.HasAnyDiffT2CAndT2lEntry)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo);
			}
			else if (declaration.IsExport && declaration.JE_OH_Importer.IsEmpty && declaration.HasAnyDiffT2CEntry)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo);
			}
			if (declaration.IsImport && declaration.HasAnyH2Entry && declaration.HasAnyCPCStartWithList(new List<ZString> { "76", "77" }))
			{
				Parent.OrganisationPKInfo.AddWarning(Res.GetString("849EF970-E7DF-4A26-8E78-DEDF2B51A448", "Importer data will not be sent using CPC 76 or 77 - B3 message"));
			}
		}

		void CheckSupplier()
		{
			if (declaration.IsImport && !declaration.JE_OH_Supplier.IsEmpty && HasInInvHeaderAnyDifferentSupplier(declaration.JE_OH_Supplier))
			{
				Parent.OrganisationPKInfo.AddWarning(Res.GetString("2654E1F4-85AD-4F82-A13E-ABD88618E96C", "This value will not be declared in Import Entries. Please, check the Suppliers in the Invoice Headers."));
			}
			else if (declaration.IsExport && declaration.HasAnyDiffT2CEntry && declaration.JE_OH_Supplier.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo);
			}
			if (declaration.IsImport && declaration.HasAnyH2Entry && declaration.HasAnyCPCStartWithList(new List<ZString> { "71", "95" }))
			{
				Parent.OrganisationPKInfo.AddWarning(Res.GetString("174E2987-8506-4418-9C92-EFA6460DABA8", "Supplier data will not be sent using CPC 71(H2) or 95 (H5)"));
			}
		}

		bool HasInInvHeaderAnyDifferentSupplier(ZGuid supplier) => declaration.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.JZ_OH_Supplier != supplier);
	}
}

using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportDeclarationJobDocAddressValidation : DeclarationJobDocAddressValidation
	{
		public ImportDeclarationJobDocAddressValidation(JobDocAddress address, JobDeclaration declaration)
			: base(address, declaration)
		{
		}

		protected override void ValidateImporterDocumentaryOrganisationPKIsRequired()
		{
			base.ValidateImporterDocumentaryOrganisationPKIsRequired();

			ValidateOrganisationPKImporterDocumentary_BR3162();
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			switch (Parent.E2_AddressType)
			{
				case DocAddressTypes.Codes.SupplierDocumentaryAddress:
					ValidateOrganisationPKSupplierDocumentary_BR3001();
					break;
				case DocAddressTypes.Codes.ImporterDocumentaryAddress:
					ValidateImporterAdded();
					ValidateOrganisationPKImporterDocumentary_BR3162();
					break;
			}
		}

		void ValidateOrganisationPKSupplierDocumentary_BR3001()
		{
			var declarant = declaration.Declarant;
			var supplierPK = Parent.OrganisationPK;
			if (!supplierPK.IsEmpty && declarant != null && supplierPK == declarant.Header.PK
				&& !declaration.CustomsEntryInstructions.Any(instruction => HasAdditionalDocsWithFullType(instruction.AdditionalInfos, Constants.AdditionalInformationCodes._00400)))
			{
				Parent.OrganisationPKInfo.AddMessageError(Res.GetString("6A4BC0F9-0D8A-4368-9CF4-132691A3532A", "[BR3001] An Additional Information (Full Type: '00400') must be entered under Entry Instruction > Additional Documents when Exporter is the same as Declarant."));
			}
		}

		void ValidateOrganisationPKImporterDocumentary_BR3162()
		{
			var declarant = declaration.Declarant;
			var importerPK = Parent.OrganisationPK;
			if (!importerPK.IsEmpty && declarant != null && importerPK == declarant.Header.PK
				&& !declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(instruction => HasAdditionalDocsWithFullType(instruction.AdditionalInfos, Constants.AdditionalInformationCodes._00500))
				&& !declaration.Invoices.Cast<JobComInvoiceHeader>().Any(header => HasAdditionalDocsWithFullType(header.AdditionalInfos, Constants.AdditionalInformationCodes._00500)))
			{
				Parent.OrganisationPKInfo.AddMessageError(Res.GetString("01247717-2331-4BD5-8F3C-DC70BEDAF739", "[BR3162] Please enter an Additional Document where Kind is 'INF' and Full Type is '00500' under the Entry Instructions / Invoice Header > Additional Documents tab."));
			}
		}

		void ValidateImporterAdded()
		{
			var isEntryStyle_h1h2h3h4h5h6i1 = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x =>
			{
				return x.CEI_Style == ImportDeclarationTypeList.Codes.H1
				|| x.CEI_Style == ImportDeclarationTypeList.Codes.H2
				|| x.CEI_Style == ImportDeclarationTypeList.Codes.H3
				|| x.CEI_Style == ImportDeclarationTypeList.Codes.H4
				|| x.CEI_Style == ImportDeclarationTypeList.Codes.H5
				|| x.CEI_Style == ImportDeclarationTypeList.Codes.H6
				|| x.CEI_Style == ImportDeclarationTypeList.Codes.I1;
			});
			if (isEntryStyle_h1h2h3h4h5h6i1)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo, (NoResString)"Importer");
			}

			if (declaration.IsV1OrV2ApplicationCode &&
				Parent.Organisation is OrgHeader importer && importer.GetEORI().IsEmpty)
			{
				Parent.OrganisationPKInfo.AddMessageError(Res.GetString("F6D3E892-DD96-424B-8C1E-FD688C6BD476", "Importer's EORI is missing"));
			}
		}

		bool HasAdditionalDocsWithFullType(AdditionalInfoCollection additionalDocs, string fullType)
				=> additionalDocs.Cast<AdditionalInfo>().Any(d => d.IsAnAdditionalInformation && d.CSI_Code.Equals(fullType));
	}
}

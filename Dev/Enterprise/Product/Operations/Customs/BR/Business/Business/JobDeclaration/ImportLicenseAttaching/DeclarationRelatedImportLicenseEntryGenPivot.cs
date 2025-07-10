using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class DeclarationRelatedImportLicenseEntryGenPivot : CustomsGenPivot, Integration.Customs.BR.IDeclarationRelatedImportLicenseEntryGenPivot
	{
		public DeclarationRelatedImportLicenseEntryGenPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XX_RelationType = GenPivotTypeDecider.Types.JobDecRelatedImportLicenseEntryGenPivot;
			XX_Relation1TableCode = JobDeclarationSchema.Constants.Prefix;
			XX_Relation2TableCode = CusEntryInstructionSchema.Constants.Prefix;
		}

		public CusEntryInstruction EntryInstruction => Relation2Object as CusEntryInstruction;

		[ResourceStringData("BR.DeclarationRelatedImportLicenseEntryGenPivot|ImportLicenseJobNumber", Caption = "Job. No")]
		public ZString ImportLicenseJobNumber => EntryInstruction?.JobDeclaration?.JE_DeclarationReference ?? ZString.Empty;

		[ResourceStringData("BR.DeclarationRelatedImportLicenseEntryGenPivot|ImportLicenseEntryDescription", Caption = "Description")]
		public ZString ImportLicenseEntryDescription => EntryInstruction?.CEI_Description ?? ZString.Empty;

		[ResourceStringData("BR.DeclarationRelatedImportLicenseEntryGenPivot|ImportLicenseEntryMRN", Caption = "Number")]
		public ZString ImportLicenseEntryMRN => EntryInstruction?.EntryHeader?.MovementReferenceNumber ?? ZString.Empty;

		[ResourceStringData("BR.DeclarationRelatedImportLicenseEntryGenPivot|ImportLicenseEntryRegistrationDate", Caption = "Registration Date")]
		public ZDateTime ImportLicenseEntryRegistrationDate => EntryInstruction?.EntryHeader?.MovementReferenceNumberIssueDate ?? ZDateTime.Empty;

		[ResourceStringData("BR.DeclarationRelatedImportLicenseEntryGenPivot|ImportLicenseEntryStatusDescription", Caption = "License Status")]
		public ZString ImportLicenseEntryStatusDescription => EntryInstruction?.EntryHeader?.EntryHeaderStatusDescription ?? ZString.Empty;

		[ResourceStringData("BR.DeclarationRelatedImportLicenseEntryGenPivot|ImportLicenseConcessionDate", Caption = "Concession Date")]
		public ZDateTime ImportLicenseConcessionDate => EntryInstruction?.EntryHeader?.CH_EntryReleaseDate ?? ZDateTime.Empty;
	}
}

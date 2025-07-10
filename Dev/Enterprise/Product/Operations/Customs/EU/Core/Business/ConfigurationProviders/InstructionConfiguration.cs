using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business
{
	public class InstructionConfiguration
	{
		public ZBool FiscalReferencesSupport(BusinessObject businessObject) => FiscalReferencesSupportCore(businessObject);
		protected virtual ZBool FiscalReferencesSupportCore(BusinessObject businessObject)
		{
			var declaration = (JobDeclaration)businessObject;
			return declaration.IsUCC6AndIsImport;
		}

		public ZBool FiscalReferencesSupportOnCPC42And63Only(BusinessObject businessObject) => FiscalReferencesSupportOnCPC42And63OnlyCore(businessObject);
		protected virtual ZBool FiscalReferencesSupportOnCPC42And63OnlyCore(BusinessObject businessObject) => true;

		public ZBool AuthorisationsSupport(JobDeclaration declaration) => AuthorisationsSupportCore(declaration);
		protected virtual ZBool AuthorisationsSupportCore(JobDeclaration declaration) => true;

		public ZBool IsAESFullUCC6(CusEntryInstruction entryInstruction) => IsAESFullUCC6Core(entryInstruction);

		protected virtual ZBool IsAESFullUCC6Core(CusEntryInstruction entryInstruction)
		{
			return FuncsHelper.IsFunctionalityValid(Constants.FunctionalityTypes.AESPLUS, entryInstruction.CEI_DateForDuty);
		}

		public ZBool AdditionalSupplyChainActorSupport(JobDeclaration declaration) => AdditionalSupplyChainActorSupportCore(declaration);
		protected virtual ZBool AdditionalSupplyChainActorSupportCore(JobDeclaration declaration) => false;

		public ZBool GuaranteesSupport(JobDeclaration declaration, CusEntryInstruction entryInstruction) => (declaration?.IsUCC6 ?? false) && GuaranteesSupportCore(declaration, entryInstruction);
		protected virtual ZBool GuaranteesSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => false;

		public ZBool UseEoriForAuthorisationReference => UseEoriForAuthorisationReferenceCore;
		protected virtual ZBool UseEoriForAuthorisationReferenceCore => false;

		public ZBool SealsSupport(JobDeclaration declaration) => SealsSupportCore(declaration);
		protected virtual ZBool SealsSupportCore(JobDeclaration declaration) => false;

		public ZBool AdditionalInfosSupport(JobDeclaration declaration, CusEntryInstruction entryInstruction) => AdditionalInfosSupportCore(declaration, entryInstruction);
		protected virtual ZBool AdditionalInfosSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => false;

		public ZBool SupportingDocumentsSupport(JobDeclaration declaration) => SupportingDocumentsSupportCore(declaration);
		protected virtual ZBool SupportingDocumentsSupportCore(JobDeclaration declaration) => false;

		public ZBool PreviousDocumentsSupport(JobDeclaration declaration) => PreviousDocumentsSupportCore(declaration);
		protected virtual ZBool PreviousDocumentsSupportCore(JobDeclaration declaration) => false;

		public ZBool RequestedDocumentsSupport(JobDeclaration declaration) => RequestedDocumentsSupportCore(declaration);
		protected virtual ZBool RequestedDocumentsSupportCore(JobDeclaration declaration) => false;

		public ZBool SpecialProceduresSupport(JobDeclaration declaration, CusEntryInstruction entryInstruction) => SpecialProceduresSupportCore(declaration, entryInstruction);
		protected virtual ZBool SpecialProceduresSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction) => false;

		public IEntryInstructionValidationDecider GetValidationDecider(CusEntryInstruction cusEntryInstruction) => GetValidationDeciderCore(cusEntryInstruction);
		protected virtual IEntryInstructionValidationDecider GetValidationDeciderCore(CusEntryInstruction cusEntryInstruction)
		{
			if (cusEntryInstruction.JobDeclaration is JobDeclaration declaration && declaration.IsUCC6)
			{
				if (declaration.IsImport)
				{
					return GetImportValidationDecider(cusEntryInstruction);
				}
				else if (declaration.IsExport)
				{
					return GetExportValidationDecider(cusEntryInstruction);
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}
		protected virtual IEntryInstructionValidationDecider GetImportValidationDecider(CusEntryInstruction cusEntryInstruction) => new UCC6ImportEntryInstructionValidationDecider();
		protected virtual IEntryInstructionValidationDecider GetExportValidationDecider(CusEntryInstruction cusEntryInstruction) => new UCC6ExportEntryInstructionValidationDecider();

		public ISupportingDocumentValidationDecider GetSupportingDocumentValidationDecider(CusEntryInstruction entryInstruction) => GetSupportingDocumentValidationDeciderCore(entryInstruction);
		protected virtual ISupportingDocumentValidationDecider GetSupportingDocumentValidationDeciderCore(CusEntryInstruction entryInstruction) => (entryInstruction as IUcc6ValueProvider)?.IsUCC6AndIsImport() ?? false ? UCC6ImportSupportingDocumentValidationDecider : null;
		protected virtual ISupportingDocumentValidationDecider UCC6ImportSupportingDocumentValidationDecider => new UCC6ImportSupportingDocumentValidationDecider();

		public IAdditionalInfoValidationDecider GetAdditionalInfoValidationDecider(CusEntryInstruction entryInstruction) => GetAdditionalInfoValidationDeciderCore(entryInstruction);
		protected virtual IAdditionalInfoValidationDecider GetAdditionalInfoValidationDeciderCore(CusEntryInstruction entryInstruction) => (entryInstruction as IUcc6ValueProvider)?.IsUCC6AndIsImport() ?? false ? UCC6ImportAdditionalInfoValidationDecider : null;
		protected virtual IAdditionalInfoValidationDecider UCC6ImportAdditionalInfoValidationDecider => new UCC6ImportAdditionalInfoValidationDecider();

		public ICusFiscalReferenceValidationDecider GetCusFiscalReferenceValidationDecider(CusEntryInstruction entryInstruction) => GetCusFiscalReferenceValidationDeciderCore(entryInstruction);
		protected virtual ICusFiscalReferenceValidationDecider GetCusFiscalReferenceValidationDeciderCore(CusEntryInstruction entryInstruction) => (entryInstruction as IUcc6ValueProvider)?.IsUCC6AndIsImport() ?? false ? UCC6ImportCusFiscalReferenceValidationDecider : null;
		protected virtual ICusFiscalReferenceValidationDecider UCC6ImportCusFiscalReferenceValidationDecider => new UCC6ImportCusFiscalReferenceValidationDecider();

		public ICusAuthorizationUsageValidationDecider GetCusAuthorizationUsageValidationDecider(CusEntryInstruction entryInstruction) => GetCusAuthorizationUsageValidationDeciderCore(entryInstruction);
		protected virtual ICusAuthorizationUsageValidationDecider GetCusAuthorizationUsageValidationDeciderCore(CusEntryInstruction entryInstruction)
			=> entryInstruction.JobDeclaration switch {
				{ IsUCC6AndIsImport: true } => UCC6ImportCusAuthorizationUsageValidationDecider,
				{ IsUCC6AndIsExport: true } => UCC6ExportCusAuthorizationUsageValidationDecider,
				_ => null
			};
		protected virtual ICusAuthorizationUsageValidationDecider UCC6ImportCusAuthorizationUsageValidationDecider => new UCC6ImportCusAuthorizationUsageValidationDecider();
		protected virtual ICusAuthorizationUsageValidationDecider UCC6ExportCusAuthorizationUsageValidationDecider => new UCC6ExportCusAuthorizationUsageValidationDecider();

		public IPreviousDocumentValidationDecider GetPreviousDocumentValidationDecider(CusEntryInstruction entryInstruction) => GetPreviousDocumentValidationDeciderCore(entryInstruction);
		protected virtual IPreviousDocumentValidationDecider GetPreviousDocumentValidationDeciderCore(CusEntryInstruction entryInstruction) => (entryInstruction as IUcc6ValueProvider)?.IsUCC6AndIsImport() ?? false ? UCC6ImportPreviousDocumentValidationDecider : null;
		protected virtual IPreviousDocumentValidationDecider UCC6ImportPreviousDocumentValidationDecider => new UCC6ImportPreviousDocumentValidationDecider();
	}
}

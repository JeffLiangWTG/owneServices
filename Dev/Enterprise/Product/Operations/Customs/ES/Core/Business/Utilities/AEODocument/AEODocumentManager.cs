using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.ES.Business
{
	public class AEODocumentManager
	{
		public AEODocumentManager(JobDeclaration declaration, IAEODocumentMessageBoxProvider guiProvider)
		{
			this.declaration = declaration;
			this.guiProvider = guiProvider;
		}

		readonly JobDeclaration declaration;
		readonly IAEODocumentMessageBoxProvider guiProvider;

		public void AddUpdateOrRemoveAEODocumentInEntryInstructions()
		{
			var isImportAndH2 = declaration.IsImport && declaration.HasAnyH2Entry;
			AddUpdateOrRemoveAEODocumentInEntryInstructions(AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo, AEOSupportingDocumentTypeCodeList.Descriptions.Y025RepresentativeAeo, isImportAndH2);
			AddUpdateOrRemoveAEODocumentInEntryInstructions(AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo, AEOSupportingDocumentTypeCodeList.Descriptions.Y022SupplierAeo, isImportAndH2);
			AddUpdateOrRemoveAEODocumentInEntryInstructions(AEOSupportingDocumentTypeCodeList.Codes.Y023ImporterAeo, AEOSupportingDocumentTypeCodeList.Descriptions.Y023ImporterAeo, isImportAndH2);
			AddUpdateOrRemoveAEODocumentInEntryInstructions(AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo, AEOSupportingDocumentTypeCodeList.Descriptions.Y024DeclarantAeo, isImportAndH2);
		}

		void AddUpdateOrRemoveAEODocumentInEntryInstructions(ZString docType, ZString docTypeDescription, bool removeOnly = false)
		{
			var shouldAskToRemoveDocs = false;
			var entryInstructionsToRemoveDocs = new List<CusEntryInstruction>();
			var isExportUcc6 = declaration.IsUCC6AndIsExport;
			foreach (CusEntryInstruction entryInstruction in declaration.CustomsEntryInstructions)
			{
				var isT2LorT2C = entryInstruction.IsT2L || entryInstruction.IsT2C;
				if (!isT2LorT2C && (!isExportUcc6 || !entryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ))
				{
					var aeoDocumentData = ShouldAddUpdateOrRemoveAEODocument(entryInstruction, docType, removeOnly);
					if (aeoDocumentData.ShouldRemoveDoc)
					{
						shouldAskToRemoveDocs = true;
						entryInstructionsToRemoveDocs.Add(entryInstruction);
					}
					else if (!removeOnly && aeoDocumentData.Authorisation != null)
					{
						AddOrUpdateAEODocument(entryInstruction, docType, aeoDocumentData.Authorisation.CPH_Number, aeoDocumentData.Authorisation.CPH_StartDate);
					}
				}
			}

			if (shouldAskToRemoveDocs && guiProvider.AskIfShouldRemoveAEODocument(docTypeDescription))
			{
				foreach (var entryInstruction in entryInstructionsToRemoveDocs)
				{
					RemoveAEODocument(entryInstruction, docType);
				}
			}
		}

		AEODocumentData ShouldAddUpdateOrRemoveAEODocument(CusEntryInstruction entryInstruction, ZString docType, bool removeOnly)
		{
			CusAuthorisationHeader authorisation = null;
			var shouldRemoveDoc = false;

			switch (docType)
			{
				case AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo:
					authorisation = GetAuthorisationForAEODocumentY025();
					break;
				case AEOSupportingDocumentTypeCodeList.Codes.Y022SupplierAeo:
					authorisation = GetAuthorisationForAEODocumentY022(entryInstruction);
					break;
				case AEOSupportingDocumentTypeCodeList.Codes.Y023ImporterAeo:
					authorisation = GetAuthorisationForAEODocumentY023();
					break;
				case AEOSupportingDocumentTypeCodeList.Codes.Y024DeclarantAeo:
					authorisation = GetAuthorisationForAEODocumentY024();
					break;
			}

			if (removeOnly || authorisation == null)
			{
				shouldRemoveDoc = EntryInstructionHasAEODocument(entryInstruction, docType);
			}
			return new AEODocumentData() { Authorisation = authorisation, ShouldRemoveDoc = shouldRemoveDoc };
		}

		CusAuthorisationHeader GetAuthorisationForAEODocumentY025()
		{
			CusAuthorisationHeader authorisation = null;

			var declarantPK = declaration.Declarant?.OA_OH ?? ZGuid.Empty;
			var isDeclarantSameAsImporter = !declarantPK.IsEmpty && declaration.IsImport && declarantPK != declaration.JE_OH_Importer;
			var isDeclarantSameAsSupplier = !declarantPK.IsEmpty && declaration.IsExport && declarantPK != declaration.JE_OH_Supplier;

			if (isDeclarantSameAsImporter || isDeclarantSameAsSupplier)
			{
				authorisation = declaration.LoadAEOCusGuaranteeHeaderFromReference(declarantPK);
			}
			return authorisation;
		}

		CusAuthorisationHeader GetAuthorisationForAEODocumentY022(CusEntryInstruction entryInstruction)
		{
			var invoice = entryInstruction.Invoices.FirstOrDefault();
			var supplier = declaration.IsImport && invoice != null ? invoice.JZ_OH_Supplier : declaration.JE_OH_Supplier;
			return declaration.LoadAEOCusGuaranteeHeaderFromReference(supplier);
		}

		CusAuthorisationHeader GetAuthorisationForAEODocumentY023() => declaration.LoadAEOCusGuaranteeHeaderFromReference(declaration.JE_OH_Importer);

		CusAuthorisationHeader GetAuthorisationForAEODocumentY024()
		{
			var holderPK = ZGuid.Empty;

			var declarantPK = declaration.Declarant?.OA_OH ?? ZGuid.Empty;
			var isDeclarantSameAsImporter = !declarantPK.IsEmpty && declaration.IsImport && declarantPK != declaration.JE_OH_Importer;
			var isDeclarantSameAsSupplier = !declarantPK.IsEmpty && declaration.IsExport && declarantPK != declaration.JE_OH_Supplier;

			if (isDeclarantSameAsImporter || isDeclarantSameAsSupplier)
			{
				holderPK = declarantPK;
			}
			else if (declaration.IsImport && !declaration.JE_OH_Importer.IsEmpty)
			{
				holderPK = declaration.JE_OH_Importer;
			}
			else if (declaration.IsExport && !declaration.JE_OH_Supplier.IsEmpty)
			{
				holderPK = declaration.JE_OH_Supplier;
			}

			return declaration.LoadAEOCusGuaranteeHeaderFromReference(holderPK);
		}

		void AddOrUpdateAEODocument(CusEntryInstruction entryInstruction, ZString docType, ZString docRef, ZDate docIssueDate)
		{
			var existingDoc = entryInstruction.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == docType);
			if (existingDoc == null)
			{
				if (docType != AEOSupportingDocumentTypeCodeList.Codes.Y025RepresentativeAeo || (!entryInstruction.IsT2L && !entryInstruction.IsT2C))
				{
					var newDoc = entryInstruction.SupportingDocuments.AddNew();
					newDoc.CSI_Code = docType;
					newDoc.CSI_ReferenceNumber = docRef;
					newDoc.CSI_DateOfIssue = docIssueDate;
				}
			}
			else
			{
				existingDoc.CSI_ReferenceNumber = docRef;
				existingDoc.CSI_DateOfIssue = docIssueDate;
			}
		}

		bool EntryInstructionHasAEODocument(CusEntryInstruction entryInstruction, ZString documentType) => entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == documentType);

		void RemoveAEODocument(CusEntryInstruction entryInstruction, ZString docType)
		{
			var docToRemove = entryInstruction.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == docType);
			if (docToRemove != null)
			{
				entryInstruction.SupportingDocuments.RemoveAndDelete(docToRemove);
			}
		}
	}
}

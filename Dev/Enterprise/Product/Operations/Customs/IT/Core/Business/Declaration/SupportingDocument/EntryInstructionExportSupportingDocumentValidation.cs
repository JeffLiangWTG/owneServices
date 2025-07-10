using CargoWise.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class EntryInstructionExportSupportingDocumentValidation : SupportingDocumentValidation
{
	public EntryInstructionExportSupportingDocumentValidation(SupportingDocument parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateIfSupportingDocumentsAreAllowed();
		ValidateSupportingDocumentsCount();
	}

	internal void ValidateIfSupportingDocumentsAreAllowed()
	{
		var parent = Parent;
		if (!parent.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod)
		{
			return;
		}

		if (IsUcc6Export && (EntryInstructionSupportingDocuments?.Any() ?? false))
		{
			parent.AddRowMessageError(ValidationCaptions.EntryInstruction.SupportingDocumentsNotAllowedAtHeaderLevelInTransitionPeriod);
		}
	}

	internal void ValidateSupportingDocumentsCount()
	{
		var parent = Parent;
		if (IsUcc6Export && (EntryInstructionSupportingDocuments?.IsCountMoreThan(99) ?? false))
		{
			parent.AddRowMessageError(ValidationCaptions.EntryInstruction.SupportingDocumentCountForExportMustNotExceed99);
		}
	}

	CusEntryInstruction ParentEntryInstruction => Parent.Parent as CusEntryInstruction;

	bool IsUcc6Export => (ParentEntryInstruction as IUcc6ValueProvider)?.IsUCC6AndIsExport() ?? false;

	ISupportingDocumentCollection<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> EntryInstructionSupportingDocuments => (ParentEntryInstruction as ISupportingDocumentsProvider)?.SupportingDocuments;
}

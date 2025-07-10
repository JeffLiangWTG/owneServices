using System;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BE.Business;

public class ImportOperationProvider : IImportOperation
{
	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration jobDeclaration;
	readonly CusEntryInstruction entryInstruction;
	public ImportOperationProvider(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.jobDeclaration = Argument.NotNull(entryHeader.Declaration, $"{nameof(entryHeader)}.{nameof(CusEntryHeader.Declaration)}");
		this.entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, $"{nameof(entryHeader)}.{nameof(CusEntryHeader.EntryInstruction)}");
	}

	public string LRN => entryHeader.CH_BGMReference;

	public string MRN => MovementReferenceCusEntryNumber.CE_EntryNum;

	public string DeclarationType => jobDeclaration.JE_EntryStyle;

	public string AdditionalDeclarationType => entryInstruction.CEI_SubStyle;

	public DateTime? PresentationDateAndTime => jobDeclaration.ZG_PresentationStartDate.IsValid ? jobDeclaration.ZG_PresentationStartDate.ToDateTime() : null;

	public DateTime? DeclarationAcceptanceDate => MovementReferenceCusEntryNumber.CE_IssueDate.IsValid ? MovementReferenceCusEntryNumber.CE_IssueDate.ToDateTime() : null;

	public string LanguageCode => jobDeclaration.JE_DeclarationLanguage;

	CusEntryNumber MovementReferenceCusEntryNumber => movementReferenceCusEntryNumber ??= CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, jobDeclaration.CountryCode);
	CusEntryNumber movementReferenceCusEntryNumber;
}

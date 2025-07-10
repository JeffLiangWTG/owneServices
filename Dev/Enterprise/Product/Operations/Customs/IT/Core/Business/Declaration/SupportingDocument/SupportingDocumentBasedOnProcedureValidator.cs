using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class SupportingDocumentBasedOnProcedureValidator
{
	public SupportingDocumentBasedOnProcedureValidator(SupportingDocument supportingDocument)
	{
		this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
	}

	readonly SupportingDocument supportingDocument;

	public void CheckDocumentY04ShouldNotBeUsedWhenProcedureIs42or63()
	{
		if (supportingDocument.CSI_Code.In(new ZString[] { SupportingDocumentTypes.Y040, SupportingDocumentTypes.Y041, SupportingDocumentTypes.Y042 }) && DocumentY04ShouldNotBeUsed())
		{
			AddDocumentY04ShouldNotBeUsedWhenProcedureIs42or63MessageError();
		}

		bool DocumentY04ShouldNotBeUsed()
		{
			return GetRelatedEntryInstructions()
				.WhereNotNull()
				.Any(x => IsProcedure42or63(x) && IsDeclarationTypeH1orH5(x));
		}
	}

	#region Implementation

	IEnumerable<CusEntryInstruction> GetRelatedEntryInstructions()
	{
		switch (supportingDocument.Parent)
		{
			case JobDeclaration declaration:
				return declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>();

			case JobComInvoiceHeader invoice:
				return invoice.CusEntryInstructions.Cast<CusEntryInstruction>();

			case JobComInvoiceLine invoiceLine:
				return new CusEntryInstruction[] { invoiceLine.EntryInstruction };

			default:
				return Array.Empty<CusEntryInstruction>();
		}
	}

	void AddDocumentY04ShouldNotBeUsedWhenProcedureIs42or63MessageError()
	{
		var messageError = ZString.Empty;
		switch (supportingDocument.Parent)
		{
			case JobDeclaration _:
			case JobComInvoiceHeader _:
				messageError = ValidationCaptions.SupportingDocument.DocumentY04XShouldNotBeUsedAtDeclarationOrInvoiceLevel;
				break;

			case JobComInvoiceLine _:
				messageError = ValidationCaptions.SupportingDocument.DocumentY04XShouldNotBeUsedAtInvoiceLineLevel;
				break;

			default:
				break;
		}

		if (!messageError.IsEmpty)
		{
			supportingDocument.CSI_CodeInfo.AddMessageError(messageError);
		}
	}

	bool IsProcedure42or63(CusEntryInstruction entryInstruction)
	{
		return entryInstruction.CEI_Procedure.In(new ZString[]
		{
			RefCusProcedureCodes.ReleaseForFreeCirculationOfGoodsForConsumption42,
			RefCusProcedureCodes.ReimportationWithSimultaneousReleaseForConsumption63,
		});
	}

	bool IsDeclarationTypeH1orH5(CusEntryInstruction entryInstruction)
	{
		return entryInstruction.CEI_Style.In(new ZString[]
		{
			ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1,
			ImportUCC6DeclarationTypeList.Codes.DichiarazioneScambiTerritoriFiscaliSpecialiH5,
		});
	}

	#endregion
}

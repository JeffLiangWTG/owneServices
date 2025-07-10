using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class ImportPreviousDocumentValidation : PreviousDocumentValidation
{
	public ImportPreviousDocumentValidation(PreviousDocument parent) : base(parent)
	{
	}

	protected override void CheckCSI_SubType()
	{
	}

	protected override void CheckCSI_Status()
	{
	}

	protected override void CheckCSI_Tariff()
	{
	}

	protected override void CheckCSI_Quantity2()
	{
	}

	protected override void CheckCSI_UnitOfQuantity2()
	{
	}

	protected override void CheckCSI_Quantity3()
	{
	}

	protected override void CheckCSI_UnitOfQuantity3()
	{
	}

	protected override void CheckCSI_ReferenceNumber2()
	{
		base.CheckCSI_ReferenceNumber2();

		if (!Parent.CSI_ReferenceNumber2.IsEmpty && Parent.CSI_ReferenceNumber2.Length != 18)
		{
			Parent.CSI_ReferenceNumber2Info.AddMessageError(ValidationCaptions.PreviousDocument.ReferenceNumber2LengthMustBe18);
		}
	}

	protected override void CheckLineNoMessageErrorIfIsEntered(ZPropertyInfo lineNoInfo)
	{
		if (Parent.CSI_Procedure.In(proceduresRequiringLineNumber))
		{
			MandatoryValidation.MessageErrorIfNotEntered(lineNoInfo);
			return;
		}

		base.CheckLineNoMessageErrorIfIsEntered(lineNoInfo);
	}

	protected override IPreviousDocumentReferenceNumberValidator GetReferenceNumberValidator() => new ImportPreviousDocumentReferenceNumberValidator(Parent, GetSettings(Parent));

	readonly ImmutableArray<ZString> proceduresRequiringLineNumber = new ZString[]
	{
		PreviousDocumentProcedureList.Codes.DichiarazioneMeccanizzataDiTransito,
		ImportPreviousDocumentProcedureList.Codes.DichiarazioneNotificaMrn,
		ImportPreviousDocumentProcedureList.Codes.RiferimentoDataDiIscrizioneDelleMerciNelleScrittureDelDichiarante,
		ImportPreviousDocumentProcedureList.Codes.AltriDocumenti,
		ImportPreviousDocumentProcedureList.Codes.PartitaDiTemporaneaCustodiaA3,
	}.ToImmutableArray();
}

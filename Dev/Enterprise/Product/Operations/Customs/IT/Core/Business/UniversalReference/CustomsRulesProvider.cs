using System;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business;

public static class CustomsRulesProvider
{
	public static bool IncotermAllowItalianAgreedPlaceCode(ZString incotermCode)
	{
		switch (incotermCode)
		{
			case Core.Constants.IncoTerms.FreeOnBoard:
			case Core.Constants.IncoTerms.ExWorks:
			case Core.Constants.IncoTerms.FreeAlongsideShip:
			case Core.Constants.IncoTerms.FreeCarrier:
				return false;
			default:
				return true;
		}
	}

	public static bool IncotermIsValidForIT(ZString incotermCode)
	{
		switch (incotermCode)
		{
			case Core.Constants.IncoTerms.FreeCarrierBuyer:
			case Core.Constants.IncoTerms.FreeCarrierSeller:
				return false;
			default:
				return true;
		}
	}

	public static bool AgreedPlaceCodeRequiresInvoiceFreightCharges(ZString agreedPlaceCode)
	{
		return !agreedPlaceCode.StartsWith(Core.Constants.CountryCodes.Italy);
	}

	public static bool IsSummaryDeclarationDocument(ZString registerCode) => SummaryDeclarationDocumentList.Contains(registerCode);

	static readonly ImmutableArray<string> SummaryDeclarationDocumentList = ImmutableArray.Create(
		PreviousDocumentProcedureList.Codes.PartitaDiTemporaneaCustodiaA3,
		PreviousDocumentProcedureList.Codes.PartitaDiTemporaneaCustodiaPf,
		PreviousDocumentProcedureList.Codes.DichiarazioneMeccanizzataDiTransito,
		PreviousDocumentProcedureList.Codes.AltriDocumenti,
		PreviousDocumentProcedureList.Codes.LetteraDiVetturaFerroviariaModCim,
		PreviousDocumentProcedureList.Codes.ManifestoMerciArrivateViaMareModA44,
		PreviousDocumentProcedureList.Codes.ManifestoMerciInPartenzaViaMareModA45,
		PreviousDocumentProcedureList.Codes.CarnetATA,
		PreviousDocumentProcedureList.Codes.LetteraDiVetturaAerea,
		PreviousDocumentProcedureList.Codes.BollettinoDiConsegna,
		PreviousDocumentProcedureList.Codes.ListaDiCarico,
		PreviousDocumentProcedureList.Codes.LetteraVetturaAerea,
		PreviousDocumentProcedureList.Codes.ManifestoMerciTrasportatePerViaAerea,
		PreviousDocumentProcedureList.Codes.DocumentoDiTCInternoArticolo340QuaterPar1,
		PreviousDocumentProcedureList.Codes.CarnetTIR,
		PreviousDocumentProcedureList.Codes.BollettinoColliEspressi,
		PreviousDocumentProcedureList.Codes.DichiarazioneNonMeccanizzataDiTransitoComunitarioEsternoT1,
		PreviousDocumentProcedureList.Codes.DichiarazioneNonMeccanizzataDiTransitoComunitarioInternoT2
	);

	public static bool IsPreviousProcedureDocument(ZString registerCode) => PreviousProcedureDocumentList.Contains(registerCode);

	public static ZBool ConvertContainerModeFromCargoWiseToIT(string containerMode)
	{
		var containerModeIT = ZBool.False;
		switch (containerMode)
		{
			case Core.Constants.ContainerModes.FCL:
			case Core.Constants.ContainerModes.LCL:
			case Core.Constants.ContainerModes.ULD:
			case Core.Constants.ContainerModes.Containerised:
				containerModeIT = ZBool.True;
				break;
		}
		return containerModeIT;
	}

	public static ZString ConvertRepresentativeTypeToItalianCustomsFormat(ZString representativeType)
	{
		switch (representativeType)
		{
			case RepresentationTypeList.Codes._1Self:
				return SADConstants.RepresentationTypeList.Self;

			case RepresentationTypeList.Codes._2Direct:
				return SADConstants.RepresentationTypeList.Direct;

			case RepresentationTypeList.Codes._3Indirect:
				return SADConstants.RepresentationTypeList.Indirect;

			default:
				return ZString.Empty;
		}
	}

	public static ZString GetRegistrationCodeFromUcc6Mrn(ZString mrn)
	{
		if (mrn.IsEmpty || mrn.Length != MrnLength)
		{
			return ZString.Empty;
		}

		var registryCode = string.Join(" ", mrn.SubstringSafe(7, 2).TrimStart('0').ToString().ToCharArray()).PadRight(2);
		return FormattableString.Invariant($"{registryCode}-{mrn.SubstringSafe(9, 7)}");
	}

	const int MrnLength = 18;

	static readonly ImmutableArray<string> PreviousProcedureDocumentList = ImmutableArray.Create(
		PreviousDocumentProcedureList.Codes.Registro2DiTempEsportazione,
		PreviousDocumentProcedureList.Codes.Registro2DiTempEsportazioneInProceduraDiFallback,
		PreviousDocumentProcedureList.Codes.Registro2DiTempEsportazioneConSdoganamentoTelematico,
		PreviousDocumentProcedureList.Codes.Registro5DiTempImportazione,
		PreviousDocumentProcedureList.Codes.Registro5DiTempImportazioneInProceduraDiFallback,
		PreviousDocumentProcedureList.Codes.Registro5DiTempImportazioneConSdoganamentoTelematico,
		PreviousDocumentProcedureList.Codes.Registro7DiIntroduzioneInDeposito,
		PreviousDocumentProcedureList.Codes.Registro7DiIntroduzioneInDepositoInProceduraDiFallback,
		PreviousDocumentProcedureList.Codes.Registro7DiIntroduzioneInDepositoConSdoganamentoTelematico,
		ImportPreviousDocumentProcedureList.Codes.Registro1,
		ImportPreviousDocumentProcedureList.Codes.Registro1InProceduraDiFallback,
		ImportPreviousDocumentProcedureList.Codes.Registro1ConSdoganamentoTelematico
	);
}

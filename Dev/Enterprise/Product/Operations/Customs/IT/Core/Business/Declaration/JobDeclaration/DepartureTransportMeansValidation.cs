using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using TransportMeansCodes = Enterprise.Customs.Business.TransportMeansList.Codes;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class DepartureTransportMeansValidation
{
	public DepartureTransportMeansValidation(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	readonly JobDeclaration declaration;

	public void CheckJE_TransportIDInland()
	{
		var transportIDInlandInfo = declaration.JE_TransportIDInlandInfo;
		if (transportIDInlandInfo.Value.IsEmpty)
		{
			CheckTransportIDInlandAndTrailer1RegNoEmptyForRailInlandMOTC0834(declaration, transportIDInlandInfo, declaration.JE_Trailer1RegNo);
			CheckTransportIDInlandAndAircraftRegistrationInlandEmptyForAirInlandMOTC0834(declaration, transportIDInlandInfo, declaration.JE_AircraftRegistrationInland);
			CheckTransportIDInlandAndTrailer1RegNoAndTrailer2RegNoEmptyForRoadInlandMOTC0834(declaration, transportIDInlandInfo, declaration.JE_Trailer1RegNo, declaration.JE_Trailer2RegNo);
			CheckTransportIDInlandMandatoryWithInlandAndEntryStyleAndProcedureCodesRelatedMOTC0834(declaration, transportIDInlandInfo);
		}

		if (declaration.IsTransitionPeriodAES30)
		{
			CheckTransportIDInlandAndTrailer1RegNoFilledForRaiInlandMOTB1884(declaration, transportIDInlandInfo);
			CheckTransportIDInlandAndAircraftRegistrationInlandFilledForAirInlandMOTB1884(declaration, transportIDInlandInfo);
			return;
		}
		CheckTransportIDInlandAndAircraftRegistrationInlandFilledForAirInlandMOTR0855(declaration, transportIDInlandInfo);
		CheckTransportIDInlandCharacterCasingR0473(declaration, transportIDInlandInfo);
	}

	public void CheckJE_Trailer1RegNo()
	{
		var trailer1RegNoInfo = declaration.JE_Trailer1RegNoInfo;
		if (trailer1RegNoInfo.Value.IsEmpty)
		{
			CheckTransportIDInlandAndTrailer1RegNoEmptyForRailInlandMOTC0834(declaration, trailer1RegNoInfo, declaration.JE_TransportIDInland);
			CheckTransportIDInlandAndTrailer1RegNoAndTrailer2RegNoEmptyForRoadInlandMOTC0834(declaration, trailer1RegNoInfo, declaration.JE_TransportIDInland, declaration.JE_Trailer2RegNo);
		}

		if (declaration.IsTransitionPeriodAES30)
		{
			CheckTransportIDInlandAndTrailer1RegNoFilledForRaiInlandMOTB1884(declaration, trailer1RegNoInfo);
		}
	}

	public void CheckJE_Trailer2RegNo()
	{
		var trailer2RegNoInfo = declaration.JE_Trailer2RegNoInfo;
		if (trailer2RegNoInfo.Value.IsEmpty)
		{
			CheckTransportIDInlandAndTrailer1RegNoAndTrailer2RegNoEmptyForRoadInlandMOTC0834(declaration, trailer2RegNoInfo, declaration.JE_TransportIDInland, declaration.JE_Trailer1RegNo);
		}
	}

	public void CheckJE_AircraftRegistrationInland()
	{
		var aircraftRegistrationInlandInfo = declaration.JE_AircraftRegistrationInlandInfo;
		if (aircraftRegistrationInlandInfo.Value.IsEmpty)
		{
			CheckTransportIDInlandAndAircraftRegistrationInlandEmptyForAirInlandMOTC0834(declaration, aircraftRegistrationInlandInfo, declaration.JE_TransportIDInland);
		}

		if (declaration.IsTransitionPeriodAES30)
		{
			CheckTransportIDInlandAndAircraftRegistrationInlandFilledForAirInlandMOTB1884(declaration, aircraftRegistrationInlandInfo);
			return;
		}
		CheckTransportIDInlandAndAircraftRegistrationInlandFilledForAirInlandMOTR0855(declaration, aircraftRegistrationInlandInfo);
	}

	public void CheckJE_RN_NKTransportNationalityInland()
	{
		var transportNationalityInlandInfo = declaration.JE_RN_NKTransportNationalityInlandInfo;
		if (!declaration.IsTransitionPeriodAES30 && transportNationalityInlandInfo.Value.IsEmpty)
		{
			AddMessageErrorIfAnyDependentFieldIsFilledB2101(transportNationalityInlandInfo, declaration.JE_TransportIDInland);
		}
	}

	public void CheckJE_RN_NKTrailer1Nationality()
	{
		var trailer1NationalityInfo = declaration.JE_RN_NKTrailer1NationalityInfo;
		if (!declaration.IsTransitionPeriodAES30 && trailer1NationalityInfo.Value.IsEmpty)
		{
			AddMessageErrorIfAnyDependentFieldIsFilledB2101(trailer1NationalityInfo
				, declaration.JE_AircraftRegistrationInland
				, declaration.JE_Trailer1RegNo);
		}
	}

	public void CheckJE_RN_NKTrailer2Nationality()
	{
		var trailer2NationalityInfo = declaration.JE_RN_NKTrailer2NationalityInfo;
		if (!declaration.IsTransitionPeriodAES30 && trailer2NationalityInfo.Value.IsEmpty)
		{
			AddMessageErrorIfAnyDependentFieldIsFilledB2101(trailer2NationalityInfo, declaration.JE_Trailer2RegNo);
		}
	}

	public void CheckJE_TransportMeans()
	{
		var transportMeansInfo = declaration.JE_TransportMeansInfo;
		ListValidation.MessageErrorIfInvalidCode(transportMeansInfo);

		if (declaration.IsTransitionPeriodAES30 || !declaration.JE_TransportMeans.IsEmpty)
		{
			return;
		}

		if (declaration.IsSeaInland
			|| declaration.IsFixedInstallationInland
			|| declaration.IsWaterwayTransportsInland
			|| declaration.IsOwnPropulsionInland
			|| declaration.IsMailInland)
		{
			transportMeansInfo.AddMessageError(ValidationCaptions.JobDeclaration.FieldMustBeFilledB2101);
		}
	}

	#region Implementation

	void CheckTransportIDInlandAndTrailer1RegNoFilledForRaiInlandMOTB1884(JobDeclaration parent, ZPropertyInfo targetInfo)
	{
		if (parent.IsRailInland && !parent.JE_TransportIDInland.IsEmpty && !parent.JE_Trailer1RegNo.IsEmpty)
		{
			targetInfo.AddMessageError(ValidationCaptions.JobDeclaration.OnlyOneMeansOfTransportMustBeFilledInTransitionPeriodB1884);
		}
	}

	void CheckTransportIDInlandAndAircraftRegistrationInlandFilledForAirInlandMOTB1884(JobDeclaration parent, ZPropertyInfo targetInfo)
		=> CheckTransportIDInlandAndAircraftRegistrationInlandFilledForAirInlandMOT(parent, targetInfo, ValidationCaptions.JobDeclaration.OnlyOneMeansOfTransportMustBeFilledInTransitionPeriodB1884);

	void CheckTransportIDInlandAndAircraftRegistrationInlandFilledForAirInlandMOTR0855(JobDeclaration parent, ZPropertyInfo targetInfo)
		=> CheckTransportIDInlandAndAircraftRegistrationInlandFilledForAirInlandMOT(parent, targetInfo, ValidationCaptions.JobDeclaration.OnlyOneMeansOfTransportMustBeFilledR0855);

	void CheckTransportIDInlandAndAircraftRegistrationInlandFilledForAirInlandMOT(JobDeclaration parent, ZPropertyInfo targetInfo, string errorMessage)
	{
		if (parent.IsAirInland && !parent.JE_TransportIDInland.IsEmpty && !parent.JE_AircraftRegistrationInland.IsEmpty)
		{
			targetInfo.AddMessageError(errorMessage);
		}
	}

	void CheckTransportIDInlandAndTrailer1RegNoEmptyForRailInlandMOTC0834(JobDeclaration parent, ZPropertyInfo targetInfo, params IZType[] dependentFields)
	{
		if (!parent.IsRailInland)
		{
			return;
		}
		AddMessageErrorIfAllDependentFieldsAreEmptyAndEntryStyleAndProcedureCodesRelatedC0834(parent, targetInfo, dependentFields);
	}

	void CheckTransportIDInlandAndAircraftRegistrationInlandEmptyForAirInlandMOTC0834(JobDeclaration parent, ZPropertyInfo targetInfo, params IZType[] dependentFields)
	{
		if (!parent.IsAirInland)
		{
			return;
		}
		AddMessageErrorIfAllDependentFieldsAreEmptyAndEntryStyleAndProcedureCodesRelatedC0834(parent, targetInfo, dependentFields);
	}

	void CheckTransportIDInlandAndTrailer1RegNoAndTrailer2RegNoEmptyForRoadInlandMOTC0834(JobDeclaration parent, ZPropertyInfo targetInfo, params IZType[] dependentFields)
	{
		if (!parent.IsRoadInland)
		{
			return;
		}
		AddMessageErrorIfAllDependentFieldsAreEmptyAndEntryStyleAndProcedureCodesRelatedC0834(parent, targetInfo, dependentFields);
	}

	void CheckTransportIDInlandMandatoryWithInlandAndEntryStyleAndProcedureCodesRelatedMOTC0834(JobDeclaration parent, ZPropertyInfo targetInfo)
	{
		if (!DoesEntryStyleAndProcedureCodeRequireTransportMeansC0834(parent))
		{
			return;
		}

		if (parent.IsWaterwayTransportsInland || parent.IsOwnPropulsionInland || parent.IsSeaInland)
		{
			targetInfo.AddMessageError(ValidationCaptions.JobDeclaration.MeansOfTransportMustBeFilledC0834);
		}
	}

	void AddMessageErrorIfAllDependentFieldsAreEmptyAndEntryStyleAndProcedureCodesRelatedC0834(JobDeclaration parent, ZPropertyInfo targetInfo, params IZType[] dependentFields)
	{
		if (!DoesEntryStyleAndProcedureCodeRequireTransportMeansC0834(parent))
		{
			return;
		}

		var areAllDependentFieldsEmpty = dependentFields.All(x => x.IsEmpty);
		if (areAllDependentFieldsEmpty)
		{
			targetInfo.AddMessageError(ValidationCaptions.JobDeclaration.AtLeastOneMeansOfTransportMustBeFilledC0834);
		}
	}

	bool DoesEntryStyleAndProcedureCodeRequireTransportMeansC0834(JobDeclaration parent)
	{
		var entryInstructions = parent.CustomsEntryInstructions.Cast<CusEntryInstruction>();
		var isEntryStyleExportNormalAndProcedureCodeRelated = parent.JE_EntryStyle == EntryStyleListExport.Codes.ExportNormal
			&& entryInstructions.Any(x => x.CEI_Procedure.In(entryStyleExReleatedProcedureCodes));
		var isEntryStyleExportSpecialTerritoryAndProcedureCodeRelated = parent.IsEntryStyleExportToSpecialTerritory && entryInstructions.Any(x => x.CEI_Procedure.In(entryStyleCoReleatedProcedureCodes));

		return isEntryStyleExportNormalAndProcedureCodeRelated || isEntryStyleExportSpecialTerritoryAndProcedureCodeRelated;
	}

	void AddMessageErrorIfAnyDependentFieldIsFilledB2101(ZPropertyInfo targetInfo, params IZType[] dependentFields)
	{
		foreach (var dependentField in dependentFields)
		{
			if (!dependentField.IsEmpty)
			{
				targetInfo.AddMessageError(ValidationCaptions.JobDeclaration.FieldMustBeFilledB2101);
				break;
			}
		}
	}

	void CheckTransportIDInlandCharacterCasingR0473(JobDeclaration parent, ZPropertyInfo targetInfo)
	{
		var transportIDInlandHasLowerCaseLetters = Regex.IsMatch(parent.JE_TransportIDInland, "[a-z]");
		if (!transportIDInlandHasLowerCaseLetters)
		{
			return;
		}

		var transportMeans = parent.JE_TransportMeans;
		var inlandMotSeaAndTransportMeans10 = parent.IsSeaInland && transportMeans == TransportMeansCodes.ImoShipIdentificationNumber;
		var inlandMotsDoesNotAllowLowerCaseTransportIDInland = parent.IsWaterwayTransportsInland
			|| parent.IsFixedInstallationInland
			|| parent.IsOwnPropulsionInland
			|| parent.IsMailInland;

		if (inlandMotSeaAndTransportMeans10
			|| (inlandMotsDoesNotAllowLowerCaseTransportIDInland && transportMeans.In(transportMeansDoesNotAllowLowerCaseTransportIDInland)))
		{
			targetInfo.AddMessageError(ValidationCaptions.JobDeclaration.LowercaseLettersNotAllowedR0473);
		}
	}

	readonly ImmutableArray<ZString> transportMeansDoesNotAllowLowerCaseTransportIDInland = new ZString[]
	{
		TransportMeansCodes.ImoShipIdentificationNumber,
		TransportMeansCodes.WagonNumber,
		TransportMeansCodes.TrainNumber,
		TransportMeansCodes.RegistrationNumberOfTheRoadVehicle,
		TransportMeansCodes.RegistrationNumberOfTheRoadTrailer,
		TransportMeansCodes.IataFlightNumber,
		TransportMeansCodes.RegistrationNumberOfTheAircraft,
		TransportMeansCodes.EuropeanVesselIdentificationNumberEniCode,
	}.ToImmutableArray();

	readonly ImmutableArray<ZString> entryStyleExReleatedProcedureCodes = new ZString[]
	{
		UniversalReferenceConstants.RefCusProcedureCodes.FinalExport10,
		UniversalReferenceConstants.RefCusProcedureCodes.CompensatingProductsExport11,
		UniversalReferenceConstants.RefCusProcedureCodes.TemporaryGoodsExportToBeReintegratedAsTheyAre23,
		UniversalReferenceConstants.RefCusProcedureCodes.GoodsReExport31,
	}.ToImmutableArray();

	readonly ImmutableArray<ZString> entryStyleCoReleatedProcedureCodes = new ZString[]
	{
		UniversalReferenceConstants.RefCusProcedureCodes.PlacingOfGoodsIntoCustomsWarehousingOrIntoFreeZone76,
		UniversalReferenceConstants.RefCusProcedureCodes.StoringAndPlacingOfGoodsUnderCustomsControlWithPreFinancing77,
	}.ToImmutableArray();

	#endregion

}

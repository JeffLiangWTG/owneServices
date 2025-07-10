using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Argument = CargoWise.Common.Argument;
using TransportMeansList = Enterprise.Customs.Business.TransportMeansList.Codes;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public sealed class DepartureMeansOfTransportWrappersProvider
{
	public DepartureMeansOfTransportWrappersProvider(CusEntryInstruction entryInstruction, int? inlandTransportMode)
	{
		this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
		declaration = Argument.NotNull(entryInstruction.JobDeclaration, nameof(entryInstruction.JobDeclaration));
		this.inlandTransportMode = inlandTransportMode;
	}

	public IReadOnlyCollection<IMeansOfTransport> GetDepartureMeansOfTransports()
	{
		if (!IsDepartureMeansOfTransportRequired())
		{
			return Array.Empty<IMeansOfTransport>();
		}

		return GetDepartureMeansOfTransportsGivenTransportMode()
			.WhereNotNull()
			.ToCollection();
	}

	#region Implementation

	IEnumerable<IMeansOfTransport> GetDepartureMeansOfTransportsGivenTransportMode()
	{
		var transportId = declaration.JE_TransportIDInland;
		var transportNationality = declaration.JE_RN_NKTransportNationalityInland;

		switch (inlandTransportMode)
		{
			case TransportModeAir:
				yield return GetNewOrNullDepartureWrapper(transportId, TransportMeansList.IataFlightNumber, transportNationality);
				yield return GetNewOrNullDepartureWrapper(declaration.JE_AircraftRegistrationInland
					, TransportMeansList.RegistrationNumberOfTheAircraft
					, declaration.JE_RN_NKTrailer1Nationality);

				break;

			case TransportModeRai:
				yield return GetNewOrNullDepartureWrapper(transportId, TransportMeansList.TrainNumber, transportNationality);
				yield return GetNewOrNullDepartureWrapper(declaration.JE_Trailer1RegNo, TransportMeansList.WagonNumber, declaration.JE_RN_NKTrailer1Nationality);
				break;

			case TransportModeRoa:
				yield return GetNewOrNullDepartureWrapper(transportId, TransportMeansList.RegistrationNumberOfTheRoadVehicle, transportNationality);
				yield return GetNewOrNullDepartureWrapper(declaration.JE_Trailer1RegNo, TransportMeansList.RegistrationNumberOfTheRoadTrailer, declaration.JE_RN_NKTrailer1Nationality);
				yield return GetNewOrNullDepartureWrapper(declaration.JE_Trailer2RegNo, TransportMeansList.RegistrationNumberOfTheRoadTrailer, declaration.JE_RN_NKTrailer2Nationality);
				break;

			default:
				yield return GetNewOrNullDepartureWrapper(transportId, declaration.JE_TransportMeans, transportNationality);
				break;
		}
	}

	bool IsDepartureMeansOfTransportRequired() => inlandTransportMode.HasValue && (IsDepartureRequiredForEx() || IsDepartureRequiredForCo());

	bool IsDepartureRequiredForEx()
	{
		return EntryStyle == EntryStyleListExport.Codes.ExportNormal
			&& (ProcedureRequiresDepartureForEx() || (!IsTransportModeFixOrMai && ProcedureRequiresDepartureForExAndNotFixOrMaiMode()));
	}

	bool IsDepartureRequiredForCo()
	{
		return EntryStyle == EntryStyleListExport.Codes.ExportToSpecialTerritory
			&& !IsTransportModeFixOrMai
			&& ProcedureRequiresDepartureForCoAndNotFixOrMaiMode();
	}

	bool ProcedureRequiresDepartureForEx() => CargoWise.Common.IEnumerableExtensions.In(ProcedureCode, proceduresRequiringDepartureForEx.ToArray());

	bool ProcedureRequiresDepartureForExAndNotFixOrMaiMode() => CargoWise.Common.IEnumerableExtensions.In(ProcedureCode, proceduresRequiringDepartureForExAndNotFixOrMaiMode.ToArray());

	bool ProcedureRequiresDepartureForCoAndNotFixOrMaiMode() => CargoWise.Common.IEnumerableExtensions.In(ProcedureCode, proceduresRequiringDepartureForCoAndNotFixOrMaiMode.ToArray());

	IMeansOfTransport GetNewOrNullDepartureWrapper(string identificationNumber, string typeOfIdentification, string nationality)
	{
		return DepartureMeansOfTransportWrapper.NewOrNull(identificationNumber, typeOfIdentification, nationality);
	}

	ZString EntryStyle => declaration.JE_EntryStyle;

	ZString ProcedureCode => entryInstruction.CEI_Procedure;

	bool IsTransportModeFixOrMai => CargoWise.Common.IEnumerableExtensions.In(inlandTransportMode, TransportModeFix, TransportModeMai);

	#endregion

	readonly CusEntryInstruction entryInstruction;
	readonly JobDeclaration declaration;
	readonly int? inlandTransportMode;

	readonly ImmutableArray<ZString> proceduresRequiringDepartureForEx = new ZString[]
	{
		UniversalReferenceConstants.RefCusProcedureCodes.FinalExport10,
		UniversalReferenceConstants.RefCusProcedureCodes.CompensatingProductsExport11,
		UniversalReferenceConstants.RefCusProcedureCodes.TemporaryGoodsExportToBeReintegratedAsTheyAre23,
		UniversalReferenceConstants.RefCusProcedureCodes.GoodsReExport31,
	}.ToImmutableArray();

	readonly ImmutableArray<ZString> proceduresRequiringDepartureForExAndNotFixOrMaiMode = new ZString[]
	{
		UniversalReferenceConstants.RefCusProcedureCodes.TemporaryExportUnderOutwardProcessingRegime21,
		UniversalReferenceConstants.RefCusProcedureCodes.TemporaryExportUnderOutwardProcessingOnTextileProducts22,
	}.ToImmutableArray();

	readonly ImmutableArray<ZString> proceduresRequiringDepartureForCoAndNotFixOrMaiMode = new ZString[]
	{
		UniversalReferenceConstants.RefCusProcedureCodes.PlacingOfGoodsIntoCustomsWarehousingOrIntoFreeZone76,
		UniversalReferenceConstants.RefCusProcedureCodes.StoringAndPlacingOfGoodsUnderCustomsControlWithPreFinancing77,
	}.ToImmutableArray();

	const int TransportModeAir = 4;
	const int TransportModeRai = 2;
	const int TransportModeRoa = 3;
	const int TransportModeFix = 7;
	const int TransportModeMai = 5;
}

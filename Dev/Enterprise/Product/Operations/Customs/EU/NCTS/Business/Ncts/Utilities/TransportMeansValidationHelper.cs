using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class TransportMeansValidationHelper
	{
		public static void CheckOfficeHasRequiredPurpose(NctsDepartureMovementHeader movementHeader, ZPropertyInfo customsOfficeInfo)
		{
			var value = (ZString)customsOfficeInfo.Value;
			var customsOfficesForDeparture = movementHeader.IsPhase5 ? movementHeader.CustomsOfficesForDeparture : movementHeader.Header.CustomsOfficesForDeparture;
			var hasMatchingOffice = customsOfficesForDeparture.Cast<NctsEuOfficeCode>().Any(co => co.CY_Data == value && co.CY_Code.In(
				new ZString[] { OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit }));

			if (!hasMatchingOffice)
			{
				customsOfficeInfo.AddMessageError(Res.GetString("9A072A63-2486-4FEB-BE22-5658C2DFB6CC", "[TR0052] Customs Office at Border must be equal to at least one Customs Office with Purpose DES/TRA/TXT."));
			}
		}

		public static void CheckReferenceNumber(NctsDepartureMovementHeader header, ZString typeOfIdentification, ZPropertyInfo referenceNumberInfo)
		{
			if (header.BM_ExportTransportMode == ModeOfTransportList.Codes._4_AirTransport
			&& header.BM_TypeOfSecurity != NctsTypeOfSecurityList.Codes.NON
			&& (typeOfIdentification == NctsTransportTypeOfIdList.Codes._40 || typeOfIdentification == NctsTransportTypeOfIdList.Codes._41))
			{
				MandatoryValidation.MessageErrorIfNotEntered(referenceNumberInfo);
			}
		}

		public static readonly ImmutableHashSet<string> TransportTypesRequiringUpperCaseIDs = new HashSet<string>(new[]
		{
			NctsTransportTypeOfIdList.Codes._10,
			NctsTransportTypeOfIdList.Codes._21,
			NctsTransportTypeOfIdList.Codes._30,
			NctsTransportTypeOfIdList.Codes._40,
			NctsTransportTypeOfIdList.Codes._41,
			NctsTransportTypeOfIdList.Codes._80
		}).ToImmutableHashSet();
	}
}

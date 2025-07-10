using System.Collections.Immutable;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public static class JobDeclarationUCC6ExportValidation
	{
		public static void CheckJE_TransportIDInland(JobDeclaration declaration)
		{
			var targetInfo = declaration.JE_TransportIDInlandInfo;
			if (declaration.IsAirInland)
			{
				CheckMutualExclusiveNumbers(targetInfo, declaration.JE_AircraftRegistrationInland, Res.GetString("11cc06ec-b27d-4b24-85ed-2cae73d99b7b", "Flight Number"), FlightNumberAndAircraftIDMutualExclusiveMessage);
			}
			else if (declaration.IsRailInland && declaration.IsTransitionPeriodAES30)
			{
				CheckMutualExclusiveNumbers(targetInfo, declaration.JE_Trailer1RegNo, Res.GetString("0c604987-dcbf-4e4d-a9d2-63eb23ecaec4", "Train Number"), TrainAndWagonNumberMutualExclusiveMessage);
			}
			else if (!declaration.JE_TransportMeans.IsEmpty && (declaration.IsFixedInstallationInland || declaration.IsMailInland))
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo, Res.GetString("43209429-E395-486A-9A7C-533EFF75F184", "Transport ID"));
			}
		}

		public static void CheckJE_Trailer1RegNo(JobDeclaration declaration)
		{
			if (declaration.IsRailInland && declaration.IsTransitionPeriodAES30)
			{
				CheckMutualExclusiveNumbers(declaration.JE_Trailer1RegNoInfo, declaration.JE_TransportIDInland, Res.GetString("8de015d5-f60d-4991-a5e4-7678b306bad0", "Wagon Number"), TrainAndWagonNumberMutualExclusiveMessage);
			}
		}

		public static void CheckJE_AircraftRegistrationInland(JobDeclaration declaration)
		{
			if (declaration.IsAirInland)
			{
				CheckMutualExclusiveNumbers(declaration.JE_AircraftRegistrationInlandInfo, declaration.JE_TransportIDInland, Res.GetString("4E6BE7DC-7A0E-45BD-8D44-03B4F2E7E4AC", "Aircraft ID"), FlightNumberAndAircraftIDMutualExclusiveMessage);
			}
		}

		public static void CheckJE_VesselName(JobDeclaration declaration)
		{
			var transportModesForMandatoryVessel = ImmutableHashSet.Create(Customs.Business.TransportTypeList.Codes.Sea, Customs.Business.TransportTypeList.Codes.Rail, Customs.Business.TransportTypeList.Codes.Road,
				Customs.Business.TransportTypeList.Codes.OwnPropulsion, Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport, Customs.Business.TransportTypeList.Codes.Mail, Customs.Business.TransportTypeList.Codes.FixedTransportInstallations);
			var transportMode = declaration.JE_TransportMode;
			var vessel = declaration.JE_VesselName;
			if (vessel.IsEmpty)
			{
				if (!declaration.ZG_BorderTransportMeans.IsEmpty && transportModesForMandatoryVessel.Contains(transportMode))
				{
					var targetInfo = declaration.JE_VesselNameInfo;
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(targetInfo.HumanReadableName));
				}
			}
			else
			{
				var targetInfo = declaration.JE_VesselNameInfo;
				if (vessel.Length > 27)
				{
					targetInfo.AddWarning(Res.GetString("D7FC77B3-92EC-4033-866B-75158EF17013", "The maximum length for [21] Vessel is 27 characters."));
				}

				if (transportMode == Customs.Business.TransportTypeList.Codes.Sea && declaration.ZG_BorderTransportMeans == ExportBorderTransportMeansList.Codes._10 && (declaration.Vessel?.RV_LloydsNumber.IsEmpty ?? false))
				{
					targetInfo.AddMessageError(Res.GetString("79413FF4-596C-4921-A593-DAE8C4643DD1", "There is no Lloyds/IMO number stored in the Vessel data."));
				}
			}
		}

		public static void CheckJE_RN_NKTransportNationalityInland(JobDeclaration declaration)
		{
			if (!declaration.JE_TransportMeans.IsEmpty && (declaration.IsFixedInstallationInland || declaration.IsMailInland))
			{
				MandatoryValidation.MessageErrorIfNotEntered(declaration.JE_RN_NKTransportNationalityInlandInfo);
			}
		}

		static void CheckMutualExclusiveNumbers(ZPropertyInfo propertyInfo, ZString relatedNumber, ZString mandatoryMessage, ZString exclusiveMessage)
		{
			var targetNumberIsEmpty = propertyInfo.Value.IsEmpty;
			var relatedNumberIsEmpty = relatedNumber.IsEmpty;
			if (targetNumberIsEmpty && relatedNumberIsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, mandatoryMessage);
			}
			else if (!targetNumberIsEmpty && !relatedNumberIsEmpty)
			{
				propertyInfo.AddMessageError(exclusiveMessage);
			}
		}

		static string FlightNumberAndAircraftIDMutualExclusiveMessage => Res.GetString("8A5F6CB6-6A0C-4356-8017-9D61362D8969", "You may only enter a Flight Number or an Aircraft ID.");

		static string TrainAndWagonNumberMutualExclusiveMessage => Res.GetString("77f134c3-0f75-4ea7-973d-65c30fdaa51c", "You may only enter a Train or a Wagon Number.");
	}
}

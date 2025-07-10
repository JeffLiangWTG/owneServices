using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class ArrivalTransportMeansWrapper : CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces.IArrivalTransportMeans
	{
		ArrivalTransportMeansWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		readonly JobDeclaration declaration;

		public string IdentificationNumber => identificationNumber ??= GetIdentificationNumber();
		string identificationNumber;

		public string TypeOfIdentification => typeOfIdentification ??= declaration.JE_TransportMeans;
		string typeOfIdentification;

		public static ArrivalTransportMeansWrapper New(JobDeclaration addInfo) => addInfo == null ? null : new ArrivalTransportMeansWrapper(addInfo);

		string GetIdentificationNumber()
		{
			var identificationMap = new Dictionary<string, string>
			{
				{ TransportTypeList.Codes.Air + TransportMeansList.Codes.IataFlightNumber, declaration.JE_VoyageFlightNo },
				{ TransportTypeList.Codes.InlandWaterwayTransport + TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel, declaration.JE_VesselName },
				{ TransportTypeList.Codes.Rail + TransportMeansList.Codes.WagonNumber, declaration.JE_VesselName },
				{ TransportTypeList.Codes.Road + TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, declaration.JE_VesselName },
				{ TransportTypeList.Codes.Sea + TransportMeansList.Codes.NameOfTheSeaGoingVessel, declaration.JE_VesselName },
				{ TransportTypeList.Codes.Sea + TransportMeansList.Codes.ImoShipIdentificationNumber, declaration?.Vessel?.RV_LloydsNumber ?? string.Empty }
			};

			var key = declaration.JE_TransportMode + declaration.JE_TransportMeans;

			return identificationMap.TryGetValue(key, out var identification) ? string.Concat(identification.Where(c => !char.IsWhiteSpace(c))) : string.Empty;
		}
	}
}

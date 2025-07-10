using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class DeclarationBorderTransportMeansWrapper : IDeclarationBorderTransportMeans
	{
		DeclarationBorderTransportMeansWrapper(TransportMean transport)
		{
			this.transport = Argument.NotNull(transport, nameof(transport));
		}

		internal static DeclarationBorderTransportMeansWrapper NewOrNull(TransportMean transport)
		=> transport == null ? null : new DeclarationBorderTransportMeansWrapper(transport);

		#region IDeclarationBorderTransportMeans

		IIDType IDeclarationBorderTransportMeans.Id => id ?? (id = IDTypeWrapper.NewOrNull(transport.JW_Vessel));
		IIDType id;

		IIDType IDeclarationBorderTransportMeans.FirstArrivalLocationId => firstArrivalLocationID ?? (IDTypeWrapper.NewOrNull(transport.JW_RL_NKDiscPort));
		readonly IIDType firstArrivalLocationID;

		public string ArrivalDateTime => transport.JW_ETA.IsEmpty ? null : transport.JW_ETA.ToCustomsDateTimeString();

		public string DepartureDateTime => transport.JW_ATD.IsEmpty ? null : transport.JW_ATD.ToCustomsDateTimeString();

		public IIDType JourneyId => null;

		public ITextType Name => null;

		public IIDType RegistrationNationalityId => registrationNationalityID ?? (IDTypeWrapper.NewOrNull(transport.VehicleCountry));
		readonly IIDType registrationNationalityID;

		public ICollection<IDeclarationBorderTransportMeansTransportMeansOperator> TransportMeansOperator => null;

		ICodeType IDeclarationBorderTransportMeans.TypeCode => typeCode ?? (CodeTypeWrapper.NewOrNull(transport.TruckKind));
		readonly ICodeType typeCode;

		ICollection<IDeclarationBorderTransportMeansItinerary> IDeclarationBorderTransportMeans.Itinerary
			=> new List<IDeclarationBorderTransportMeansItinerary>() { DeclarationBorderTransportMeansItineraryWrapper.NewOrNull(null) }.AsReadOnly();

		#endregion

		readonly TransportMean transport;
	}
}

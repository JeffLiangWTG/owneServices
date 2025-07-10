using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class IncidentLocationWrapper : ILocation
	{
		IncidentLocationWrapper(EU.NCTS.Business.EnRouteIncident incident)
		{
			this.incident = Argument.NotNull(incident, nameof(incident));
		}

		readonly EU.NCTS.Business.EnRouteIncident incident;

		public static IncidentLocationWrapper New(EU.NCTS.Business.EnRouteIncident incident) => incident  == null ? null : new IncidentLocationWrapper(incident);

		public string QualifierofIdentification => qualifierofIdentification ?? (qualifierofIdentification = incident.GoodsLocation.CGL_Qualifier);
		string qualifierofIdentification;

		public string UNLoCode => unlocode ?? (unlocode = incident.GoodsLocation.Unlocode);
		string unlocode;

		public string Country => country ?? (country = incident.GoodsLocation.Address.E2_RN_NKCountryCode);
		string country;

		public IGnss GNSS => gnss ?? (gnss = GnssWrapper.New((EU.NCTS.Business.CusGoodsLocationAddress)incident.GoodsLocation.Address));
		IGnss gnss;

		public IAddress Address => address ?? (address = CusGoodsLocationAddressWrapper.New((EU.NCTS.Business.CusGoodsLocationAddress)incident.GoodsLocation.Address));
		IAddress address;
	}
}

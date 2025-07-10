using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSIncidentProvider : NCTSIncidentBaseProvider, INCTSIncident
	{
		public new static NCTSIncidentProvider NewOrNull(EnRouteIncident incident) => incident == null ? null : new NCTSIncidentProvider(incident);

		protected NCTSIncidentProvider(EnRouteIncident incident) : base(incident)
		{
			goodsLocation = Argument.NotNull(incident.GoodsLocation as CusGoodsLocation, nameof(incident.GoodsLocation));
			incidentCode = incident.BN_IncidentCode;
		}
		readonly CusGoodsLocation goodsLocation;
		readonly ZString incidentCode;

		public string QualifierOfIdentification => goodsLocation.CGL_Qualifier;

		public string UNLocode => QualifierOfIdentification == Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode ? goodsLocation.Unlocode.ValueOrNullIfEmpty() : null;

		public string Country => incident.BN_EventCountryCode;

		public string Longitude => CachedValueHelper.GetValue(ref longitude, () =>
		{
			string result = null;
			if (LocationQualifierIsW)
			{
				result = incident.GoodsLocation.Address.E2_Longitude.ToString("+000.0000000;-000.0000000");
			}
			return result;
		});
		CachedValue<string> longitude;

		public string Latitude => CachedValueHelper.GetValue(ref latitude, () =>
		{
			string result = null;
			if (LocationQualifierIsW)
			{
				result = incident.GoodsLocation.Address.E2_Latitude.ToString("+00.0000000;-00.0000000");
			}
			return result;
		});
		CachedValue<string> latitude;

		public string StreetAndNumber => LocationQualifierIsZ ? goodsLocation.Address.E2_Address1AndE2_Address2.ValueOrNullIfEmpty() : null;

		public string Postcode => LocationQualifierIsZ ? goodsLocation.Address.E2_Postcode.ValueOrNullIfEmpty() : null;

		public string City => LocationQualifierIsZ ? goodsLocation.Address.E2_City.ValueOrNullIfEmpty() : null;

		public IReadOnlyCollection<CargoWise.Customs.DE.MessageContracts.NCTS.INCTSTransportEquipment> TransportEquipments => transportEquipments ?? (transportEquipments = incidentCode != IncidentCodeList.Codes._1 && incidentCode != IncidentCodeList.Codes._5
			? incident.IncidentContainers.Cast<NctsContainer>()
				.Where(c => IncidentCodeIs3Or6 && c.BC_Mode == Core.Constants.ContainerModes.Containerised || !IncidentCodeIs3Or6)
				.Select(c => NCTSIncidentTransportEquipmentProvider.NewOrNull(c))
				.ToArray()
			: Array.Empty<CargoWise.Customs.DE.MessageContracts.NCTS.INCTSTransportEquipment>());
		IReadOnlyCollection<CargoWise.Customs.DE.MessageContracts.NCTS.INCTSTransportEquipment> transportEquipments;

		public bool ContainerIndicator => IncidentCodeIs3Or6 && incident.IncidentContainers.Cast<NctsContainer>().Any(x => x.BC_Mode == Core.Constants.ContainerModes.Containerised);

		public string TypeOfIdentification => IncidentCodeIs3Or6 ? incident.BN_TransportAtDepartureType.ValueOrNullIfEmpty() : null;

		public string IdentificationNumber => IncidentCodeIs3Or6 ? incident.BN_TransportAtDepartureID.ValueOrNullIfEmpty() : null;

		public string Nationality => IncidentCodeIs3Or6 ? incident.BN_RN_NKTransportAtDepartureIDNationality.ValueOrNullIfEmpty() : null;

		bool LocationQualifierIsW => QualifierOfIdentification == Customs.Business.CusGoodsLocationQualifierList.Codes.GnssCoordinates;

		bool LocationQualifierIsZ => QualifierOfIdentification == Customs.Business.CusGoodsLocationQualifierList.Codes.Address;

		bool IncidentCodeIs3Or6 => incidentCode == IncidentCodeList.Codes._3 || incidentCode == IncidentCodeList.Codes._6;
	}
}

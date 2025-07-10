using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE007IncidentProvider : IIE007Incident, IIE007Endorsement
	{
		public IE007IncidentProvider(EnRouteIncident incident)
		{
			this.incident = Argument.NotNull(incident, "incident");
		}
		readonly EnRouteIncident incident;

		#region IIE007Incident Members
		public string Code => incident.BN_IncidentCode;

		public string Text => incident.BN_Information;

		public IIE007Endorsement Endorsement => this;

		public IIE007IncidentLocation LocationOfGoods => locationOfGoods ?? (locationOfGoods = new IE007IncidentLocationProvider((CusGoodsLocation)incident.GoodsLocation));
		IIE007IncidentLocation locationOfGoods;

		public IReadOnlyCollection<ITransportEquipmentWithSeals> TransportEquipments => transportEquipments ?? (transportEquipments = isSendTransportEquipments ? TransportEquipmentWithSealsProvider.GetEquipments(incident) : Array.Empty<ITransportEquipmentWithSeals>());
		IReadOnlyCollection<ITransportEquipmentWithSeals> transportEquipments;

		bool isSendTransportEquipments => HasContainer && !Code.In(codesNotRequireTransportEquipment);

		public bool HasContainer => CachedValueHelper.GetValue(ref hasContainerCached, () => incident.IncidentContainers.Count > 0 && incident.IncidentContainers[0].BC_Mode.EqualsIgnoringCase(Core.Constants.ContainerModes.Containerised));
		CachedValue<bool> hasContainerCached;

		public IIE007Transhipment Transhipment => CachedValueHelper.GetValue(ref transhipmentCached, () => IsSendTranshipment ? new IE007TranshipmentProvider(incident, HasContainer) : null);
		CachedValue<IIE007Transhipment> transhipmentCached;

		bool IsSendTranshipment => !Code.In(codesNotRequireTranshipment);

		#endregion

		#region IIE007Endorsement Members
		public DateTime Date => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(incident.BN_EndorsementDate.ToZDateTime(), removeMillisecond: true);

		public string Authority => incident.BN_EndorsementAuthority;

		public string Place => incident.BN_EndorsementPlace;

		public string Country => incident.BN_EndorsementCountryCode;
		#endregion

		readonly string[] codesNotRequireTransportEquipment = new[] { IncidentCodeList.Codes._1, IncidentCodeList.Codes._5 };
		readonly string[] codesNotRequireTranshipment = new[] { IncidentCodeList.Codes._1, IncidentCodeList.Codes._2, IncidentCodeList.Codes._4, IncidentCodeList.Codes._5 };
	}
}

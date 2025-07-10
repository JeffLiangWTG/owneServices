using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPhase5DepartureShipmentToTransportDocumentSynchroniser : BusinessObjectSynchroniser
	{
		public NctsPhase5DepartureShipmentToTransportDocumentSynchroniser(NctsBillAdditionalDocument transportDocument, ForwardingShipment source)
		: base(transportDocument, source)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			Synchronisers.Add(new FieldSynchroniser(Destination.CSI_SubTypeInfo, GetSubTypeConverted, Array.Empty<ZPropertyInfo>));
			Synchronisers.Add(new FieldSynchroniser(Destination.CSI_CodeInfo, GetCodeConverted, Array.Empty<ZPropertyInfo>));
			Synchronisers.Add(new FieldSynchroniser(Destination.CSI_ReferenceNumberInfo, Source.JS_HouseBillInfo));
		}

		new NctsBillAdditionalDocument Destination => (NctsBillAdditionalDocument)base.Destination;

		new ForwardingShipment Source => (ForwardingShipment)base.Source;

		static IZType GetSubTypeConverted() => new ZString(AdditionalInfoSubTypeList.Codes.TransportDocument);

		static IZType GetCodeConverted() => new ZString(NctsConstants.AdditionalInfoCodes.HouseBillOfLading);
	}
}

using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.GB.ICS.CodeDescriptionPairLists
{
	public partial class ICSManifestTypes
	{
		public IManifestType SAS => sas ?? (sas = new ManifestType(
			Codes.SAS,
			Descriptions.SAS,
			new[] { GBSSTransportTypeList.Codes.AirFreight, GBSSTransportTypeList.Codes.SeaFreight, GBSSTransportTypeList.Codes.RoadFreight, GBSSTransportTypeList.Codes.RailFreight, GBSSTransportTypeList.Codes.InlandWaterTransport, GBSSTransportTypeList.Codes.RoroUnaccompanied },
			new[] { ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine, ManifestBase.ApplicationCodeTypeList.Codes.Consolidator },
			MessageLevel.Manifest,
			ShipmentTypeList.Import23Only()));
		IManifestType sas;

		public new IReadOnlyList<IManifestType> All => new[] { ICS, SAS };
	}
}

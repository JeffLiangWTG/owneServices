using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CustomsManifestLineSequenceCollection : CusCodeDataCollection<CustomsManifestLineSequence>
	{
		public CustomsManifestLineSequenceCollection(CommonShipment shipment)
			: base(shipment, CusCodeDataTypeList.Codes.CustomsManifestLineSequence)
		{
		}

		public CustomsManifestLineSequenceCollection(IHVLVConsignment consignment)
			: base(consignment as BusinessObject, CusCodeDataTypeList.Codes.CustomsManifestLineSequence)
		{
		}
	}
}

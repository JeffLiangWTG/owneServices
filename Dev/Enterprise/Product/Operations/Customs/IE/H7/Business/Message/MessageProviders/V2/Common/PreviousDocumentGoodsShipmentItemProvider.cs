using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public class PreviousDocumentGoodsShipmentItemProvider : MPreviousDocumentProvider, IPreviousDocumentGoodsShipmentItem
	{
		public PreviousDocumentGoodsShipmentItemProvider(CusSupportingInfo supportingInfo) : base(supportingInfo)
		{
		}

		public string TypeOfPackages => null;

		public string NumberOfPackages => null;

		public string MeasurementUnitAndQualifier => null;

		public decimal Quantity => 0;

		public string GoodsItemIdentifier => null;
	}
}

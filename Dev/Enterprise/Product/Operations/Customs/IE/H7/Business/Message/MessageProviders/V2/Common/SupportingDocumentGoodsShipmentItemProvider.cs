using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public class SupportingDocumentGoodsShipmentItemProvider : SupportingDocumentProvider, ISupportingDocumentGoodsShipmentItem
	{
		public SupportingDocumentGoodsShipmentItemProvider(CusSupportingInfo supportingInfo) : base(supportingInfo)
		{
		}

		public string MeasurementUnitAndQualifier => null;

		public decimal Quantity => 0;

		public string Currency => null;

		public decimal Amount => 0;
	}
}

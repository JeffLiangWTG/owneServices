using CargoWise.Customs.IL.MessageDefinitions.DLO;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Customs.IL.Business
{
	public class CargoIdentifierWrapper : ICargoIdentifier
	{
		CargoIdentifierWrapper(DeliveryOrderDocDataObject deliveryOrderDocData)
		{
			this.deliveryOrderDocData = deliveryOrderDocData;
		}

		public static ICargoIdentifier NewOrNull(DeliveryOrderDocDataObject deliveryOrderDocData)
			=> deliveryOrderDocData == null ? null : new CargoIdentifierWrapper(deliveryOrderDocData);

		string ICargoIdentifier.CargoIdentifierKey1
			=> deliveryOrderDocData.ManifestNumber;

		string ICargoIdentifier.CargoIdentifierKey2
			=> deliveryOrderDocData.DealNumber;

		int ICargoIdentifier.CargoIdentifierType
			=> int.TryParse(deliveryOrderDocData.CargoIdentifierType.Code, out var result) ? result : ZInt.Zero;

		readonly DeliveryOrderDocDataObject deliveryOrderDocData;
	}
}

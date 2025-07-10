using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5SILine : IImport5SILine
	{
		public string ParcelCustomsNumber { get; set; }
		public string ParcelNumber { get; set; }
		public string DeliveryType { get; set; }

		ZString IImport5SILine.ParcelCustomsNumber => ParcelCustomsNumber;
		ZString IImport5SILine.ParcelNumber => ParcelNumber;
		ZString IImport5SILine.DeliveryType => DeliveryType;
	}
}

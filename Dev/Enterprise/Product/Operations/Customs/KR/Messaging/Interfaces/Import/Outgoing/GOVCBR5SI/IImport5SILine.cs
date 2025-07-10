using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5SILine
	{
		ZString ParcelCustomsNumber { get; }
		ZString ParcelNumber { get; }
		ZString DeliveryType { get; }
	}
}

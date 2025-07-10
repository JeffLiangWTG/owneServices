using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface ITermOfDeliveryGroup
{
	ZString IncotermCode { get; }
	ZString ComplementaryCode { get; }
	ZString ComplementOfInfo { get; }
	ZString ComplementOfInfoLng { get; }
}

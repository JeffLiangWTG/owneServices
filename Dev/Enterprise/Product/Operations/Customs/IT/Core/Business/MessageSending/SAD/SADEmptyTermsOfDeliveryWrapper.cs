using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADEmptyTermsOfDeliveryWrapper : ITermOfDeliveryGroup
{
	public ZString IncotermCode => ZString.Empty;

	public ZString ComplementaryCode => ZString.Empty;

	public ZString ComplementOfInfo => ZString.Empty;

	public ZString ComplementOfInfoLng => ZString.Empty;
}

using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class IMHeaderEmptyEntryCustomsOfficeWrapper : IIMHeaderEntryCustomsOffice
{
	public ZString Nationality => ZString.Empty;

	public ZString ReferenceNumber => ZString.Empty;

	public ZString Name => ZString.Empty;
}

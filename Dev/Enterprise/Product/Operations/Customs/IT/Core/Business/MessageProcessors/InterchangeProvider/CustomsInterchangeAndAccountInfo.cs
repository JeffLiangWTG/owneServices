using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Registry;

namespace Enterprise.Customs.IT.Business;

public class CustomsInterchangeAndAccountInfo
{
	public CustomsInterchangeHeader Header { get; set; }
	public ZString Staff { get; set; }
	public Account Account { get; set; }

	public ZStringBuilder ErrorCollector { get; private set; } = new ZStringBuilder();
}

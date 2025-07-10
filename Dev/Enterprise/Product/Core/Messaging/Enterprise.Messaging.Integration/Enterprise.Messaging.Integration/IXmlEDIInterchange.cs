using CargoWise.Types;

namespace Enterprise.Messaging.Integration
{
	public interface IXmlEDIInterchange : IEDIInterchange
	{
		ZString GetInterchangeTypeFromFileFormat(string fileFormat);
		IXmlEDIMessage AddNeweHubMessage();
		void AddMessage(IEDIMessage message);
	}
}

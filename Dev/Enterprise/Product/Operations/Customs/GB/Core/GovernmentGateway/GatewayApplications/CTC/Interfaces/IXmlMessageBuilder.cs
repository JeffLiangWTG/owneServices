using CargoWise.Types;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Interfaces
{
	public interface IXmlMessageBuilder
	{
		ZString GetXMLMessageWithoutNamespaces();
	}
}

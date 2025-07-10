using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IDataContextManagerFromEDIMessage
	{
		void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject);
	}
}

using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common
{
	public interface IDeclarationWithConsolShipmentDetailExporter
	{
		IValueObject ExportDeclarationWithRelatedConsolShipmentDetails(Integration.Customs.IBaseJobDeclaration declaration, EventsWithSourceType eventsWithSourceType, ProcessTaskNotification action);
	}
}

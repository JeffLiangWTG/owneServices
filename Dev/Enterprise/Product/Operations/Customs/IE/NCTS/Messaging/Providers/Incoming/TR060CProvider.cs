using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR060C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging;

public class TR060CProvider
{
	public TR060CProvider(Tr060C xmlObject)
	{
		XmlObject = xmlObject;
	}
	Tr060C XmlObject { get; }

	public ZString CustomsOfficeOfDestination => XmlObject.CustomsOfficeOfDestination?.ReferenceNumber ?? ZString.Empty;

	public ZString MRN => XmlObject.TransitOperation.Mrn ?? ZString.Empty;

	public ZDateTime ControlNotificationDateAndTime => XmlObject.TransitOperation.ControlNotificationDateAndTime;

	public ZString NotificationType => XmlObject.TransitOperation.NotificationType ?? ZString.Empty;

	public IReadOnlyCollection<TR060CTypeOfControlProvider> ControlTypes => controlTypesCached ??= XmlObject.TypeOfControls?.Select(x => new TR060CTypeOfControlProvider(x)).ToArray() ?? [];
	IReadOnlyCollection<TR060CTypeOfControlProvider> controlTypesCached;

	public IReadOnlyCollection<TR060CRequestedDocumentProvider> RequestedDocuments => requestedDocumentCached ??= XmlObject.RequestedDocument?.Select(x => new TR060CRequestedDocumentProvider(x)).ToArray() ?? [];
	IReadOnlyCollection<TR060CRequestedDocumentProvider> requestedDocumentCached;
}

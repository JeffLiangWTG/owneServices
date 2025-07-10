using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR054C;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Extensions = Enterprise.Customs.IE.Messaging.Extensions;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class TR054CProvider
	{
		Tr054C XmlObject { get; }
		public TR054CProvider(Tr054C xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString ReferenceNumber => XmlObject.CustomsOfficeOfDeparture?.ReferenceNumber ?? ZString.Empty;
		public ZString MRN => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;
		public ZBool AdviceRequested => Extensions.IsTrueOrFalse(XmlObject.TransitOperation?.AdviceRequested);
		public ZDateTime AdviceRequestDateAndTime => (XmlObject.TransitOperation?.AdviceRequestDateAndTime).ConvertToZDateTime();
	}
}

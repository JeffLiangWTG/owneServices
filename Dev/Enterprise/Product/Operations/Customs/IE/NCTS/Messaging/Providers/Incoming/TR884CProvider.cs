using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR884C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class TR884CProvider
	{
		Tr884C XmlObject { get; }

		public TR884CProvider(Tr884C xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.Declaration?.Mrn ?? ZString.Empty;
		public ZString CaseId => XmlObject.Declaration?.CaseId ?? ZString.Empty;
		public ZString PresentationRequestCancellationReason => XmlObject.Declaration?.PresentationRequestCancellationReason ?? ZString.Empty;
	}
}

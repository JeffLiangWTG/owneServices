using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR864C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class TR864CProvider
	{
		Tr864C XmlObject { get; }
		public TR864CProvider(Tr864C xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.Declaration?.Mrn ?? ZString.Empty;
		public ZString CaseId => XmlObject.Declaration?.CaseId ?? ZString.Empty;
		public ZString InvalidationRequestCancellationReason => XmlObject.Declaration?.InvalidationRequestCancellationReason ?? ZString.Empty;
	}
}

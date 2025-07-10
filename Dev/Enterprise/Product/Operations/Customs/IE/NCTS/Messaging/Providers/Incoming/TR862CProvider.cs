using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR862C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class TR862CProvider
	{
		Tr862C XmlObject { get; }

		public TR862CProvider(Tr862C xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.Declaration?.Mrn ?? ZString.Empty;
		public ZString CaseId => XmlObject.Declaration?.CaseId ?? ZString.Empty;
		public ZString AmendmentRequestCancellationReason => XmlObject.Declaration?.AmendmentRequestCancellationReason ?? ZString.Empty;
	}
}

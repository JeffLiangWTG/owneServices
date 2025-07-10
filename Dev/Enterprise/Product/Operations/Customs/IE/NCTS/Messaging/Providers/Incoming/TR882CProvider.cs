using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR882C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class TR882CProvider
	{
		Tr882C XmlObject { get; }

		public TR882CProvider(Tr882C xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.Declaration?.Mrn ?? ZString.Empty;
		public ZString CaseId => XmlObject.Declaration?.CaseId ?? ZString.Empty;
		public ZString UploadRequestCancellationReason => XmlObject.Declaration?.UploadRequestCancellationReason ?? ZString.Empty;
	}
}

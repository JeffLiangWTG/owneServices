using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR064C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class TR064CProvider
	{
		Tr064C XmlObject { get; }
		public TR064CProvider(Tr064C xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.Declaration?.Mrn ?? ZString.Empty;

		public ZString CaseId => XmlObject.Declaration.CaseId ?? ZString.Empty;

		public ZString Remarks => XmlObject.Declaration.Remarks ?? ZString.Empty;
	}
}

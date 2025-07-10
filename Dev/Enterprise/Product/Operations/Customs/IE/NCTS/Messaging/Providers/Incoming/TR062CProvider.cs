using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR062C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class TR062CProvider
	{
		Tr062C XmlObject { get; }
		public TR062CProvider(Tr062C xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.Declaration?.Mrn ?? ZString.Empty;
		public ZString CaseID => XmlObject.Declaration?.CaseId ?? ZString.Empty;
		public ZString Remarks => XmlObject.Declaration?.Remarks ?? ZString.Empty;
	}
}

using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC045C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC045CProvider
	{
		Cc045CType XmlObject { get; }

		public CC045CProvider(Cc045CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;
		public ZDate WriteOffDate => new ZDate(XmlObject.TransitOperation?.WriteOffDate);
	}
}

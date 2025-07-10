using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC028C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC028CProvider
	{
		Cc028CType XmlObject { get; }

		public CC028CProvider(Cc028CType xmlObject)
		{
			XmlObject = xmlObject;
		}
		public ZString MRN => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;
		public ZDate DeclarationAcceptanceDate => new ZDate(XmlObject.TransitOperation?.DeclarationAcceptanceDate);
	}
}

using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC628C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC628CProvider
	{
		public CC628CProvider(Cc628C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc628C xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ExportOperation?.Lrn;
		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZDate DeclarationAcceptanceDate => new ZDate(xmlObject.ExportOperation?.DeclarationAcceptanceDate);
	}
}

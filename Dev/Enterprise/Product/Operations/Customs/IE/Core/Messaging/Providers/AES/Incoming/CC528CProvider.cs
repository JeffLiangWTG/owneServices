using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC528C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC528CProvider
	{
		public CC528CProvider(Cc528C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc528C xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ExportOperation?.Lrn;
		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZDate DeclarationAcceptanceDate => new ZDate(xmlObject.ExportOperation?.DeclarationAcceptanceDate);
	}
}

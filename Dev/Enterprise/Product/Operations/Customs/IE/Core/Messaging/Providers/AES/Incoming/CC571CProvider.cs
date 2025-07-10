using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC571C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC571CProvider
	{
		public CC571CProvider(Cc571C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc571C xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ExportOperation?.Lrn;
		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZDate ReExportNotificationRegistrationDate => new ZDate(xmlObject.ExportOperation?.ReExportNotificationRegistrationDate);
	}
}

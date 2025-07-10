using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC582C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC582CProvider
	{
		public CC582CProvider(Cc582C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc582C xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZDate ResponseDateLimit => new ZDate(xmlObject.ExportOperation?.LimitForResponseDate);
		public ZDate NonExitedExportRequestDate => new ZDate(xmlObject.ExportOperation?.RequestOnNonExitedExportDate);
	}
}

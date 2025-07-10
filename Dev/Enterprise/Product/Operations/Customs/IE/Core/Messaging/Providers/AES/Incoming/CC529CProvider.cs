using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC529C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC529CProvider
	{
		public CC529CProvider(Cc529C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc529C xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ExportOperation?.Lrn;
		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZDate ReleaseDate => new ZDate(xmlObject.ExportOperation?.ReleaseDate);
	}
}

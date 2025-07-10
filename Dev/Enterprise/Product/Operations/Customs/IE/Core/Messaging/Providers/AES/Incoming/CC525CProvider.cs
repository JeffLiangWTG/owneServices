using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC525C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC525CProvider
	{
		public CC525CProvider(Cc525C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc525C xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;

		public ZDate ReleaseDate => new ZDate(xmlObject.ExportOperation?.ReleaseDate);

		public ZBool IsStored => Extensions.IsTrueOrFalse(xmlObject.ExportOperation?.StoringFlag);
	}
}

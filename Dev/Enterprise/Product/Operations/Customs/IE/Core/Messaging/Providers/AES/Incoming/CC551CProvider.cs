using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC551C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC551CProvider
	{
		public CC551CProvider(Cc551C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc551C xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;

		public ZDate ControlResultDate => new ZDate(xmlObject.ControlResult?.Date);

		public ZString ControlResultText => xmlObject.ControlResult?.Text;

		public ZString OtherThingsToReport => xmlObject.ExportOperation?.OtherThingsToReport;
	}
}

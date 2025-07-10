using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC522C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC522CProvider
	{
		public CC522CProvider(Cc522C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc522C xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZString ExitRejectionMotivationCode => xmlObject.ExportOperation?.ExitRejectionMotivationCode;
		public ZString ExitRejectionMotivation => xmlObject.ExportOperation?.ExitRejectionMotivation;
	}
}

using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC599C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC599CProvider
	{
		public CC599CProvider(Cc599C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc599C xmlObject;

		public ZString Status => ExitResultCode == ExitResultsCodeList.Codes.B1 ? AESEntryStatusList.Codes.Refused : AESEntryStatusList.Codes.Exported;
		public ZString LocalReferenceNumber => xmlObject.ExportOperation?.Lrn;
		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZString ExitResultCode => xmlObject.ExitControlResult?.Code;
		public ZDate ExitDate => new ZDate(xmlObject.ExitControlResult?.ExitDate);
		public ZDate ExitStoppedDate => new ZDate(xmlObject.ExitControlResult?.ExitStoppedDate);
		public ZString StateofSeals => xmlObject.ExitControlResult?.StateOfSeals;
	}
}

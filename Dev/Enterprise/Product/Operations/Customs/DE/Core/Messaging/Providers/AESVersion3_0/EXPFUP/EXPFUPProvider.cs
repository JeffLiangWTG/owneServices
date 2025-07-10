using System;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public class EXPFUPProvider : IEXPFUP
	{
		public EXPFUPProvider(DEXPFE xmlObject)
		{
			this.xmlObject = xmlObject;
			xmlExportOperation = xmlObject.ExportOperation;
		}
		readonly DEXPFE xmlObject;
		readonly DEXPFEExportOperation xmlExportOperation;

		public DateTime LatestPresentationDate => xmlExportOperation.limitForPresentationDate;

		public DateTime LatestResponseDate => xmlExportOperation.limitForResponseDate;

		public string MessageIdentifier => xmlObject.messageIdentification;

		public string LocalReferenceNumber => xmlExportOperation.LRN;

		public string MovementReferenceNumber => xmlExportOperation.MRN;

		public DateTime RequestDate => xmlExportOperation.requestOnNonExitedExportDate;

		public string FollowUpType => null;

		public string ReferencedMessageIdentifier => xmlObject.correlationIdentifier;
	}
}

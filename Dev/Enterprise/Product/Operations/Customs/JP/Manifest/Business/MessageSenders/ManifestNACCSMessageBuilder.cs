using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.Manifest.Business
{
	static class ManifestNACCSMessageBuilder
	{
		public static byte[] BuildNACCSMessage(IEnumerable<ManifestMessageSendingObject> sendingObjects)
		{
			var firstSendingObject = sendingObjects.Cast<ManifestMessageSendingObject>().FirstOrDefault();
			var messageHeader = new ManifestHeaderOutboundMessageHeaderProvider(firstSendingObject);
			var writer = NACCSFactoryService.GetOutboundMessageWriter(firstSendingObject.Bill.Factory);
			var result = messageHeader.ProcedureCode switch
			{
				JPProcedureCodeList.Codes.HCH01 => writer.Write<IHCH01>(messageHeader, new ManifestHeaderMesssageProvider(sendingObjects)),
				JPProcedureCodeList.Codes.HDF01 => writer.Write<IHDF01>(messageHeader, new ManifestHeaderMesssageProvider(sendingObjects)),
				JPProcedureCodeList.Codes.NVC01 => writer.Write<INVC01>(messageHeader, new ManifestHeaderMesssageProvider(sendingObjects)),
				JPProcedureCodeList.Codes.HDE => writer.Write<IHDE>(messageHeader, new ManifestHeaderMesssageProvider(sendingObjects)),
				JPProcedureCodeList.Codes.CHA => writer.Write<ICHA>(messageHeader, new ManifestHeaderMesssageProvider(sendingObjects)),
				_ => throw new DeveloperNotificationException($"Cannot generate message for {messageHeader.ProcedureCode}"),
			};
			return result;
		}
	}
}

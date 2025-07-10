using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.Business
{
	static class NACCSMessageBuilder
	{
		public static byte[] BuildNACCSMessage(JobDeclarationMessageSendingObject sendingObject)
		{
			var provider = new CusEntryHeaderOutboundMessageHeaderProvider(sendingObject);
			return BuildNACCSMessageCore(sendingObject, provider);
		}

		static byte[] BuildNACCSMessageCore(JobDeclarationMessageSendingObject sendingObject, IJPOutboundMessageHeader messageHeader)
		{
			var entryHeader = sendingObject.Header;
			var writer = NACCSFactoryService.GetOutboundMessageWriter(entryHeader.Factory);

			var result = messageHeader.ProcedureCode switch
			{
				JPProcedureCodeList.Codes.IDA => writer.Write<IIDAEntry>(messageHeader, new CusEntryHeaderMessageProvider(sendingObject as MessageSendingObject)),
				JPProcedureCodeList.Codes.IDC or JPProcedureCodeList.Codes.EDC => writer.Write<IEntrySubmission>(messageHeader, new CusEntryHeaderMessageProvider(sendingObject as MessageSendingObject)),
				JPProcedureCodeList.Codes.EAC  => writer.Write<IEAC>(messageHeader, new CusEntryHeaderMessageProvider(sendingObject as MessageSendingObject)),
				JPProcedureCodeList.Codes.CEW => writer.Write<ICEW>(messageHeader, new CusEntryHeaderMessageProvider(sendingObject as MessageSendingObject)),
				JPProcedureCodeList.Codes.EDA => writer.Write<IEDAEntry>(messageHeader, new CusEntryHeaderMessageProvider(sendingObject as MessageSendingObject)),
				JPProcedureCodeList.Codes.ECR => writer.Write<IECR>(messageHeader, new CusEntryHeaderMessageProvider(sendingObject as MessageSendingObject)),
				JPProcedureCodeList.Codes.MSX => writer.Write<IRegisterSupportingDocument>(messageHeader, new MSXMessageProvider(sendingObject as MSXMessageSendingObject)),
				_ => throw new DeveloperNotificationException($"Cannot generate message for {messageHeader.ProcedureCode}"),
			};

			return result;
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Chief.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public class CdsMucrInventoryLinkingMessageSender : CDSConsolMessageSender
	{
		public CdsMucrInventoryLinkingMessageSender(CusEntryHeader entry) : base(null)
		{
			this.entry = entry;
		}

		protected CusEntryHeader entry;

		protected override ConsolMessageManager GetConsolMessageManager(CustomsExportConsolIntegrationWrapper consolWrapper, GbDes242MessageFunction how, Customs.Business.ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			return new CdsMucrInventoryLinkingMessageManager(entry, consolWrapper, how, sendMessagesToCustoms);
		}
	}

	internal class CdsMucrInventoryLinkingMessageManager : CDSConsolMessageManager
	{
		public CdsMucrInventoryLinkingMessageManager(CusEntryHeader entry, CustomsExportConsolIntegrationWrapper consolWrapper, GbDes242MessageFunction how, Customs.Business.ISendsMessagesToCustoms sendMessagesToCustoms)
			: base(consolWrapper, how, sendMessagesToCustoms)
		{
			this.entry = entry;
		}

		protected CusEntryHeader entry;

		protected override IMessageBuilder GetMessageBuilder(BusinessObject ignore)
		{
			return new CdsMucrInventoryLinkingMessageBuilder(entry, how);
		}

		protected override BusinessObject Master => entry;
	}

	internal class CdsMucrInventoryLinkingMessageBuilder : IMessageBuilder
	{
		readonly CusEntryHeader cusEntryHeader;
		readonly GbDes242MessageFunction how;

		public CdsMucrInventoryLinkingMessageBuilder(CusEntryHeader cusEntryHeader, GbDes242MessageFunction how)
		{
			this.cusEntryHeader = cusEntryHeader;
			this.how = how;
		}

		public IMessageBuilderResult PopulateMessages()
		{
			var result = new MessageBuilderResult();
			var builderResult = PopulateMessage();
			if (builderResult != null)
			{
				result.AddBuilderResult(builderResult);
			}
			return result;
		}

		IBuilderResult PopulateMessage()
		{
			var message = cusEntryHeader.Messages.AddNew(typeof(CDSInventoryLinkingQueryRequestEDIMessage));
			message.EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest;
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Undefined;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageOwner = cusEntryHeader.Declaration.JE_CustomsProfile.Left(EDIMessage.Schema.EM_MessageOwnerMaxLength);
			message.MessageNumberStrategy = new GbMessageNumberStrategy(cusEntryHeader.Factory, ApplicationCodeList.Codes.GbCustomsDeclarationServices);
			message.EM_MessageText = CDSInventoryLinkingRequestMessageBuilder.NewMessageText(new GbChiefExportHeader(cusEntryHeader), how);
			message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCustomsDeclarationServices;

			var result = new BuilderResult(cusEntryHeader, System.Array.Empty<string>(), AfterFullSuccess)
			{
				Message = message
			};
			return result;
		}

		public virtual void AfterFullSuccess(IBuilderResult builderResult)
		{
			builderResult.Message.EM_MessageText = GbTransmissionMessageGenerator.PutBizoPkIntoSysCarPlaceholder(builderResult.Message.EM_MessageText, builderResult.Message);
		}
	}
}

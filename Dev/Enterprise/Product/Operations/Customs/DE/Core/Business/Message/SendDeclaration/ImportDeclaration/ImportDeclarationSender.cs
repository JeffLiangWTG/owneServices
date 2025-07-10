using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business
{
	public abstract class ImportDeclarationSender
	{
		protected ImportDeclarationSender(CusEntryHeader entryHeader, ZString messageName, IImportMessageHeader messageHeaderProvider)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			this.messageHeaderProvider = messageHeaderProvider;
			messageBuilder = ImportDeclarationMessageBuilderLoader.Instance.GetMessageBuilder(messageName, messageHeaderProvider);
		}
		protected readonly CusEntryHeader entryHeader;
		readonly IImportMessageHeader messageHeaderProvider;
		readonly IProduceMessageXml messageBuilder;

		public void Send()
		{
			if (PreSend())
			{
				var message = entryHeader.Factory.New<AtlasEDIMessage>();
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_MessageType = EDIMessageTypeList.Codes.Import;
				message.EM_MessageSubType = messageHeaderProvider.MessageGroup;
				message.SetEM_MessageTextOrDataSource(messageBuilder.GetXMLMessage());
				message.EM_LinkedObject = entryHeader;
				message.EM_ApplicationReference = messageBuilder.MessageTechnicalName;
				message.SetLogbookRegistrationNumber(LogbookRegistrationNumber);
				message.SetLogbookEORIBranchSuffix(messageHeaderProvider.InterchangeSender.EoriBranchSuffix);
				message.SetLogbookLocalReferenceNumber(DataProvider.LocalReferenceNumber);

				entryHeader.Messages.Add(message);
				entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.Sent;

				CreateCusReconEntry();
			}
		}

		protected IImportHeader DataProvider => messageHeaderProvider.Header;

		protected virtual ZString LogbookRegistrationNumber => ZString.Empty;

		protected virtual void CreateCusReconEntry()
		{
		}

		protected virtual bool PreSend() => true;

		protected void ResetEntryStatus()
		{
			entryHeader.CH_EntryStatus = ZString.Empty;
			entryHeader.ThrowAwayEntryNumber();

			entryHeader.AllEntryLines.Cast<CusEntryLine>().ForEach(x => x.ZG_CustomsStatus = ZString.Empty);
		}
	}
}

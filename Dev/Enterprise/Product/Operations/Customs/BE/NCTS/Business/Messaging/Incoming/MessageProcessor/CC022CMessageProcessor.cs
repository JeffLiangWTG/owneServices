using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC022C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC022CMessageProcessor : NCTSMessageProcessor<ICC022CDataProvider>
	{
		public CC022CMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC022C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC022C };

		protected override Type MessageInterpreterType => typeof(CC022CMessageInterpreter);

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC022CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByMRN(message.Factory, messageDataProvider, NctsTypeOfAdditionalDeclarationList.Codes.D)?.MovementHeader;

		protected override ICC022CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc022CType, CC022CDataProvider>();

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC022CDataProvider messageDataProvider)
		{
			base.PreProcessMessageWhenBOFoundCore(message, messageDataProvider);
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

			if (!moveHeader.IsAmendingDeclarationAllowed)
			{
				DiscardMessage(message, Res.GetString("ABFE513D-1AB2-4C0B-B95A-74FC139826ED", "The message was discarded, because the Status at Customs of the declaration is different from PRE, ACK, MRN and GIV. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to {3}.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC022CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;
			moveHeader.CustomsEntryStatusLogAdded += MovementHeader_CustomsEntryStatusLogAdded;

			moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}

		void MovementHeader_CustomsEntryStatusLogAdded(object sender, EventArgs e)
		{
			if (sender is NctsDepartureMovementHeader moveHeader)
			{
				moveHeader.Header.UnlockFileIfEnabledByConfiguration(Res.GetString("BDD4ED3F-4F5A-43D5-A2AD-5BE244804CD6", "The tabs are enabled for editing because a Notification to Amend was received."), EUJobMessageTypeList.Codes.NctsDeparture);
				moveHeader.CustomsEntryStatusLogAdded -= MovementHeader_CustomsEntryStatusLogAdded;
			}
		}
	}
}

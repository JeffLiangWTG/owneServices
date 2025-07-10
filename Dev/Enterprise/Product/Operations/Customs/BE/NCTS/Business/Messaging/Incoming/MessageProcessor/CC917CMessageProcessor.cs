using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders.NCTS;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC917C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC917CMessageProcessor : NCTSMessageProcessor<ICC917CDataProvider>
	{
		public CC917CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.CC917C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.CC917C };

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC917CDataProvider messageDataProvider)
		{
			var errorPointers = messageDataProvider.XMLErrorList.Select(x => x.ErrorPointer) ?? [];
			string headerType = null;
			if (!errorPointers.IsNullOrEmpty())
			{
				if (arrivalTypes.Any(x => errorPointers.Any(ep => ep.Contains(x))))
				{
					headerType = NctsMovementType.Codes.Arrival;
				}
				else if (departureTypes.Any(x => errorPointers.Any(ep => ep.Contains(x))))
				{
					headerType = NctsMovementType.Codes.Departure;
				}
			}

			return NctsMessageHelper.LocateHeaderByLRNOrMRNFallbackInterchange(message.Factory, messageDataProvider, message.Interchange, messageStatusArray: messageStatusArray, headerType: headerType);
		}

		protected string[] messageStatusArray = new string[2] { LogicalStatusList.Codes.Acknowledged,
																LogicalStatusList.Codes.Sent };

		protected override ICC917CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc917CType, CC917CDataProvider>();

		protected override Type MessageInterpreterType => typeof(CC917CMessageInterpreter);

		protected override void ProcessMessageCore(BEMessage message, ICC917CDataProvider messageDataProvider)
		{
			var nctsHeader = NctsMessageHelper.GetNctsHeaderFromLinkedObject(message);

			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Error;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}

		readonly HashSet<string> arrivalTypes = new HashSet<String> { Constants.MessageTypes.CC007C, Constants.MessageTypes.CC044C };
		readonly HashSet<string> departureTypes = new HashSet<String> { Constants.MessageTypes.CC013C, Constants.MessageTypes.CC014C, Constants.MessageTypes.CC015C, Constants.MessageTypes.CC054C, Constants.MessageTypes.CC141C, Constants.MessageTypes.CC170C };
	}
}

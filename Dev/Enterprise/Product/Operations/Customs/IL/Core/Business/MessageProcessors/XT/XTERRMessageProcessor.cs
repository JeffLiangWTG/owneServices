using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business.MessageProcessors
{
	public sealed class XTERRMessageProcessor : ILBranchCustomsApplicationTypeMessageProcessorBase
	{
		public XTERRMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("29EF567B-747F-4761-8AF6-75D1A046A85E", "IL xT Customs Error Message");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { ILMessageTypeList.Codes.XER };

		protected override void ProcessMessageCore(EDIMessage ediMessage)
		{
			if (ediMessage.EM_LinkedObject is IMessageAttachee attachee)
			{
				attachee.MessageStatus = EDIMessageStatusList.Codes.Error;
			}

			UpdateMessageStatus((ILEDIMessage)ediMessage, EDIMessage.Status.ProcessedOK);
		}

		protected override (ZGuid BranchPK, CargoWise.EntityFramework.BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObjectCore(EDIMessage message)
		{
			return (message.EM_GB, message.EM_LinkedObject, (NoResString)ZString.Empty);
		}

		public override ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger) => ProcessingResult.New(LinkedBusinessObjectMetaData.Empty);
	}
}

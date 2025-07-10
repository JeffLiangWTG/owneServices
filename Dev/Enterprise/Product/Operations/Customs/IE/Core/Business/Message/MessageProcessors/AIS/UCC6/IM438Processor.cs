using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM438Processor : AISMessageProcessor<AISInboundEDIMessage, IM438Provider>
	{
		public IM438Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("A150ED85-C320-40AD-9017-F4F25CD1B24C", "IM438: Reminder for Providing Additional Documents");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM438Provider provider) => LogicalStatusList.Codes.Accepted;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM438Provider provider)
		{
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				foreach (var requestedDocument in aisMessageAttachee.RequestedDocumentsProvider.RequestedDocuments.Where(x => x.IsOpen && x.CSI_DateOfExpiry != provider.ExpirationDate))
				{
					requestedDocument.CSI_DateOfExpiry = provider.ExpirationDate;
				}
			}
		}

		protected override Type MessageInterpreterType => typeof(IM438MessageInterpreter);
	}
}

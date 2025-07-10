using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM882;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM882Processor : AISMessageProcessor<AISInboundEDIMessage, IM882Provider>
	{
		public IM882Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM882Provider provider) =>
			xmlObjectType.IsAssignableFrom(typeof(AIS_H7_Version1_0.Im882)) ? LogicalStatusList.Codes.Accepted : null;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM882Provider provider) =>
			xmlObjectType.IsAssignableFrom(typeof(AIS_H7_Version1_0.Im882)) ? AISEntryStatusList.Codes.Control : null;

		protected override string MessageFriendlyNameCore => Res.GetString("13E4EBA8-36C4-483D-85B6-4F823556057E", "IM882: Documents Upload Request Cancellation");

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM882Provider provider)
		{
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				foreach (var requestedDocument in aisMessageAttachee.RequestedDocumentsProvider.RequestedDocuments.Where(x => x.IsOpen))
				{
					requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
				}
			}
		}

		protected override Type MessageInterpreterType => typeof(IM882MessageInterpreter);
	}
}

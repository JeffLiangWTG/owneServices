using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM882Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM882Provider>
	{
		public IM882Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("67714A48-AC5B-4FC2-82EA-E4F3E478AE1C", "IM882: Document Upload Request Cancellation");

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM882Provider provider) => LogicalStatusList.Codes.Accepted;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AISUCC5InboundEDIMessage message, IM882Provider provider)
		{
			base.ProcessMessageCore(factory, message, provider);
			var linkedObject = GetLinkedObject(factory, message);
			if (linkedObject is CusEntryHeader header && header.EntryInstruction is CusEntryInstruction entryInstruction)
			{
				foreach (var item in entryInstruction.RequestedDocuments.Where(x => x.CSI_Status.EqualsIgnoringCase(RequestedDocumentStatusList.Codes.RequestOpened)))
				{
					item.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
				}
			}
		}

		protected override bool NeedToSendEmailNotification(EDIMessage message) => true;

		protected override Type MessageInterpreterType => typeof(IM882MessageInterpreter);
	}
}

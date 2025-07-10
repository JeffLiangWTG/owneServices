using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM884Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM884Provider>
	{
		public IM884Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("163C57FC-6473-451B-89C1-13E9033A7EFE", "IM884: Document Presentation Request Cancellation");

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM884Provider provider) => LogicalStatusList.Codes.Accepted;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AISUCC5InboundEDIMessage message, IM884Provider provider)
		{
			base.ProcessMessageCore(factory, message, provider);
			var linkedObject = GetLinkedObject(factory, message);
			if (linkedObject is CusEntryHeader header && header.EntryInstruction is CusEntryInstruction entryInstruction)
			{
				foreach (var item in entryInstruction.RequestedDocuments.Where(x => x.CSI_Status.EqualsIgnoringCase(RequestedDocumentStatusList.Codes.PhysicallyPresentDocument)))
				{
					item.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
				}
			}
		}

		protected override bool NeedToSendEmailNotification(EDIMessage message) => true;

		protected override Type MessageInterpreterType => typeof(IM884MessageInterpreter);
	}
}

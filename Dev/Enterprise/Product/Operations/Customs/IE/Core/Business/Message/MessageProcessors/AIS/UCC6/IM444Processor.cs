using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Messaging;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(CL047_ControlResult))]

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM444Processor : AISMessageProcessor<AISInboundEDIMessage, IM444Provider>
	{
		public IM444Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		const string A = "A";

		protected override string MessageFriendlyNameCore => Res.GetString("FFD0BA72-1145-4636-AAE9-BFF3B5111675", "IM444: Control Results");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM444Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM444Provider provider)
		{
			var code = provider.Code;
			if (!code.StartsWith(A))
			{
				return AISEntryStatusList.Codes.Cancelled;
			}
			else if (code == CL047_ControlResult.Codes.MinorDiscrepancyDeclarationNeedsToBeAmendedAndThenForwardedToTheNextStage)
			{
				return AISEntryStatusList.Codes.AmendmentRequested;
			}
			else
			{
				return base.GetEntryStatus(message, messageAttachee, provider);
			}
		}

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM444Provider provider)
		{
			if (!provider.Code.StartsWith(A) && messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				foreach (var requestedDocument in aisMessageAttachee.RequestedDocumentsProvider.RequestedDocuments)
				{
					requestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
				}
			}
		}

		protected override Type MessageInterpreterType => typeof(IM444MessageInterpreter);
	}
}

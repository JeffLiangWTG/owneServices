using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM447Processor : AISMessageProcessor<AISInboundEDIMessage, IM447Provider>
	{
		public IM447Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		const string A = "A";

		protected override string MessageFriendlyNameCore => Res.GetString("5C177165-22A0-4C97-89A2-5F85C607821E", "IM447: Documentary Control Result");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM447Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM447Provider provider)
		{
			var code = provider.Code;
			if (!code.StartsWith(A))
			{
				return AISEntryStatusList.Codes.NotReleased;
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

		protected override Type MessageInterpreterType => typeof(IM447MessageInterpreter);
	}
}

using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CCF96A;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCCF96AProcessor : DTBaseProcessor<Ccf96AType>
	{
		public DTCCF96AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CCF96A processor";

		protected override ZString GetNewMessageStatus(Ccf96AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors;

		protected override ZString GetMessageInterpretation(Ccf96AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetMessageStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			sb.Append(GetFunctionalErrorInterpretation(messageObject));
			return sb.ToString();
		}
	}
}

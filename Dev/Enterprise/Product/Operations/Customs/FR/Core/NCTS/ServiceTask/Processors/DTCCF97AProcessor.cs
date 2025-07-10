using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CCF97A;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCCF97AProcessor : DTBaseProcessor<Ccf97AType>
	{
		public DTCCF97AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CCF97A processor";

		protected override ZString GetNewMessageStatus(Ccf97AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors;

		protected override ZString GetMessageInterpretation(Ccf97AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetMessageStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			sb.Append(GetXMLErrorInterpretation(messageObject));
			return sb.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Html strings")]
		ZString GetXMLErrorInterpretation(Ccf97AType messageObject)
		{
			var result = new ZStringBuilder();
			if (messageObject.Xmlerrer1 != null && messageObject.Xmlerrer1.Any())
			{
				result.Append("<p>");
				foreach (var error in messageObject.Xmlerrer1)
				{
					result.Append(ZString.Format("Error: {0}({1}, line {2}) - Original value : {3}<br>", error.ErrReaEr13?.Trim(), error.ErrLocEr14?.Trim(), error.ErrLinNumEr11?.Trim(), error.OriAttValEr15?.Trim()));
				}
				result.Append("</p>");
			}
			return result.ToString();
		}
	}
}

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Business
{
	public class EdiFactMessagePrettyFormatter : CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public EdiFactMessagePrettyFormatter(ICUSRESMessageProvider messageHelperProvider)
		{
			messageHelper = Argument.NotNull(messageHelperProvider, nameof(messageHelperProvider));
		}
		readonly ICUSRESMessageProvider messageHelper;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "") => CreateMessageDetailsAcceptedCore();
		protected virtual ZString CreateMessageDetailsAcceptedCore() => AcceptedDeclarationText;

		public ZString CreateMessageDetailsRejected()
		{
			var messageDetails = RejectedDeclarationText;
			var tableCreator = GetNewTableCreator();

			if (!messageHelper.FreeTextErrors.Skip(1).Any() && messageHelper.FreeTextErrors[0].Location.IsEmpty)
			{
				tableCreator.WriteRow(ErrorErrorColumnText, DescriptionColumnText);
				tableCreator.WriteRow(messageHelper.FreeTextErrors[0].Code, messageHelper.FreeTextErrors[0].Description);
			}
			else
			{
				tableCreator.WriteRow(ErrorErrorColumnText, ErrorLocationColumnText, DescriptionColumnText);

				foreach (var errorText in messageHelper.FreeTextErrors)
				{
					tableCreator.WriteRow(errorText.Code, errorText.Location, errorText.Description);
				}
			}

			messageDetails += tableCreator.ToHtml();

			return messageDetails;
		}

		protected virtual ZString GetCircuit() => messageFunctionList.ContainsKey(messageHelper.MessageFunction) ? GetColourCircuitStringEdifact(messageHelper.MessageFunction) : ZString.Empty;

		readonly ImmutableDictionary<string, (string Code, string Format)> messageFunctionList = new Dictionary<string, (string, string)>()
		{
			{ MessageFunctionCodeList.Codes.GreenCircuit, (CircuitCodeList.Descriptions.GREEN, green) },
			{ MessageFunctionCodeList.Codes.RedCircuit, (CircuitCodeList.Descriptions.RED, red) },
			{ MessageFunctionCodeList.Codes.OrangeCircuit, (CircuitCodeList.Descriptions.ORANGE, orange) }
		}.ToImmutableDictionary();

		protected ZString GetColourCircuitStringEdifact(ZString messageFunction) => HTMLColourString(messageFunctionList[messageFunction].Format, messageFunctionList[messageFunction].Code);

		protected ZString HTMLNoColourString(ZString text) => $@"<strong>{text}</strong>";
	}
}

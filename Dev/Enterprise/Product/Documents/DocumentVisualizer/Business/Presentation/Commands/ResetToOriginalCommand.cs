using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Business.Presentation;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class ResetToOriginalCommand : MessagingCommand
	{
		public override string Id => CommandIds.ResetToOriginal;

		public override bool Invoke(MacroMap map, IDocumentInfo info)
		{
			if (info == null)
			{
				return false;
			}

			var descriptor = info.Descriptor;
			var document = info.Document;
			var documentData = info.DocumentData;
			var services = info.Services;

			if (!CheckAllowSendMessage(services)
				|| CheckHasChanges(services, document, documentData))
			{
				return false;
			}

			var originalMessageSender = new OriginalMessageSender(
				descriptor.MessageInstructions,
				document,
				documentData,
				services,
				descriptor.MessageInstructions.DocumentName);

			if (map == null)
			{
				var recipient = string.IsNullOrWhiteSpace(info.Descriptor.MessageInstructions.Recipient)
					? Res.GetString("e5b9b2fe-8dfa-4c1f-ac0a-b718db5783ad", "recipient")
					: info.Descriptor.MessageInstructions.Recipient;
				var useDefiniteArticle = CheckDefiniteArticleIsRequiredInWarningMessage(recipient);
				var useSingularPossessiveCase = !recipient.EndsWith((NoResString)"s", StringComparison.OrdinalIgnoreCase);

				string warningMessage;
				string confirmationMessage;

				if (useDefiniteArticle)
				{
					warningMessage = useSingularPossessiveCase ? warningMessageWithDefiniteArticleAndSingularPossessiveCase : warningMessageWithDefiniteArticleAndPluralPossessiveCase;
					confirmationMessage = confirmationMessageWithDefiniteArticle;
				}
				else
				{
					warningMessage = useSingularPossessiveCase ? warningMessageWithIndefiniteArticleAndSingularPossessiveCase : warningMessageWithIndefiniteArticleAndPluralPossessiveCase;
					confirmationMessage = confirmationMessageWithIndefiniteArticle;
				}

				map = new MacroMap(new Dictionary<string, object>
				{
					[OriginalMessageSender.Parameters.WarningName] = string.Format(warningMessage, recipient),
					[OriginalMessageSender.Parameters.ConfirmationName] = string.Format(confirmationMessage, recipient)
				});
			}

			var parameters = OriginalMessageSender.Parameters.New(map);
			var eDocsInstructions = descriptor.EDocsInstructions;

			var res = originalMessageSender.Send(parameters);

			if (res
				&& eDocsInstructions.SaveCopyToEDocs)
			{
				var eDocDeliveryParameters = CreateEDocsDeliveryParameters(descriptor, eDocsInstructions);
				document.AddCopyToEDocs(eDocDeliveryParameters);
			}

			return res;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1098:DoNotInitializeStringFieldsWithResGetString", Justification = "There's no form designer for this class so using readonly is fine")]
		readonly string warningMessageWithIndefiniteArticleAndSingularPossessiveCase = Res.GetString("37e9a12c-c7aa-4e3a-989b-4378192fdce6", @"WARNING: Using this option without checking with {0} first might result in duplicate messages being processed by {0}.
Resetting to Original should only be required when there is a serious messaging failure at {0}'s end.
In the normal course of events, every message you send should be responded to so the system knows what kind of message to send automatically.
Before using this option, you should always check with {0} to make sure they have not already processed the message.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1098:DoNotInitializeStringFieldsWithResGetString", Justification = "There's no form designer for this class so using readonly is fine")]
		readonly string warningMessageWithIndefiniteArticleAndPluralPossessiveCase = Res.GetString("b16f1c57-9d48-402b-84f2-97277bd901e1", @"WARNING: Using this option without checking with {0} first might result in duplicate messages being processed by {0}.
Resetting to Original should only be required when there is a serious messaging failure at {0}' end.
In the normal course of events, every message you send should be responded to so the system knows what kind of message to send automatically.
Before using this option, you should always check with {0} to make sure they have not already processed the message.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1098:DoNotInitializeStringFieldsWithResGetString", Justification = "There's no form designer for this class so using readonly is fine")]
		readonly string warningMessageWithDefiniteArticleAndSingularPossessiveCase = Res.GetString("686ba927-6119-41af-9620-e0b0b05399ce", @"WARNING: Using this option without checking with the {0} first might result in duplicate messages being processed by the {0}.
Resetting to Original should only be required when there is a serious messaging failure at the {0}'s end.
In the normal course of events, every message you send should be responded to so the system knows what kind of message to send automatically.
Before using this option, you should always check with the {0} to make sure they have not already processed the message.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1098:DoNotInitializeStringFieldsWithResGetString", Justification = "There's no form designer for this class so using readonly is fine")]
		readonly string warningMessageWithDefiniteArticleAndPluralPossessiveCase = Res.GetString("49f10d1c-af2e-4954-bccc-dfad65ebca38", @"WARNING: Using this option without checking with the {0} first might result in duplicate messages being processed by the {0}.
Resetting to Original should only be required when there is a serious messaging failure at the {0}' end.
In the normal course of events, every message you send should be responded to so the system knows what kind of message to send automatically.
Before using this option, you should always check with the {0} to make sure they have not already processed the message.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1098:DoNotInitializeStringFieldsWithResGetString", Justification = "There's no form designer for this class so using readonly is fine")]
		readonly string confirmationMessageWithIndefiniteArticle = Res.GetString("e9b1427a-5e5b-4214-8eea-b88c73c40d61", @"I have confirmed with {0} that they did not process the Original message already sent.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1098:DoNotInitializeStringFieldsWithResGetString", Justification = "There's no form designer for this class so using readonly is fine")]
		readonly string confirmationMessageWithDefiniteArticle = Res.GetString("89d1efd8-b98a-4844-9b82-9bfac97d079f", @"I have confirmed with the {0} that they did not process the Original message already sent.");

		static bool CheckDefiniteArticleIsRequiredInWarningMessage(string recipient)
			=> !new string[] { (NoResString)"Customs" }.Contains(recipient, StringComparer.OrdinalIgnoreCase);
	}
}

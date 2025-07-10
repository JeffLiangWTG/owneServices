using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;
using ECB = Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business
{
	/// <summary>
	/// Any CSP, any message type, queue for sending to CHIEF on an entry. 
	/// sender = new GenericImmediateEntryMessageSender(entry);
	/// sender.Send(entry.Declaration, new ECB.SendsMessagesToCustomsShutterUpperer(false), queryFunction);
	/// </summary>
	public class GenericImmediateEntryMessageSender : GbDeclarationSenderChooser
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public GenericImmediateEntryMessageSender(CusEntryHeader entry, string interpretation, int delayMinutes)
		{
			this.entry = entry;
			this.interpretation = interpretation;
			this.delayMinutes = delayMinutes;
		}

		protected override void DoFinalSend(ECB.BaseJobDeclaration declaration, ECB.ISendsMessagesToCustoms sendToCustoms, ECB.CusdecMessageFunction how, ECB.IDeclarationMessageSender sender)
		{
			if (sender is GbDeclarationMessageSender gbSender)
			{
				transmissionGenerator = gbSender.GetTransmissionGenerator(how);
				var result = transmissionGenerator.Generate(entry);
				if (result.Message != null)
				{
					var proposedNudgeTime = ZDateTime.UtcNow.AddMinutes(delayMinutes);  // Extra minute to avoid rounding errors
					result.Message.EM_HeldUntilDate = proposedNudgeTime;
					result.Message.EM_MessageText = result.Message.EM_MessageText.Replace(CusEntryHeader.UCRReferencePlaceHolder, entry.DeclarationUCR);
					result.Message.EM_MessageText = result.Message.EM_MessageText.Replace(CusEntryHeader.UCRPartPlaceHolder, entry.DeclarationUCRPartSuffix);
					result.Message.EM_MessageText = result.Message.EM_MessageText.Replace(GbEDIMessage.SystemCommonAccessReferencePkPlaceholder, GbTransmissionMessageGenerator.GetBizoPkHexadecimalOnly(entry));
					if (!interpretation.IsEmpty)
					{
						result.Message.EM_MessageInterpretation = interpretation;
					}
					foreach (var taskCode in gbSender.GetCodesOfRelevantServiceTasksThatShouldBeCheckedBeforeSending())
					{
						ECB.ServiceTaskHelper.NudgeServiceTaskTime(null, taskCode, proposedNudgeTime, true);
					}

					result.Message.Saving -= message_Saving;
					result.Message.Saving += message_Saving;
				}
			}
		}

		void message_Saving(EDIMessage message)
		{
			if (message.IsTransmitMessage && !message.IsInDatabase)
			{
				CusEntryHeader entryHeader = message.EM_LinkedObject as CusEntryHeader;
				if (entryHeader != null)
				{
					var messageText = message.EM_MessageText;
					transmissionGenerator.PutReferenceNumberIntoMessageFromPlaceholder(message, messageText, entryHeader);
				}
			}
		}

		EU.Business.MessageBuilders.IMessageGenerator<EU.Business.Declaration.CusEntryHeader> transmissionGenerator;
		readonly CusEntryHeader entry;
		readonly ZString interpretation;
		readonly ZInt delayMinutes;
	}
}

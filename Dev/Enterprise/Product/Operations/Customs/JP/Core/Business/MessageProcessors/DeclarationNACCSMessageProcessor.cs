using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Business.MessageProcessors
{
	public class DeclarationNACCSMessageProcessor : NACCSMessageProcessor, Integration.Customs.JP.IDeclarationMessageProcessor
	{
		public DeclarationNACCSMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected sealed override void ProcessMessageCore(EDIMessage message)
		{
			var parseResult = NACCSFactoryService.GetInboundMessageParser(message.Factory).Parse(message.EM_MessageData);
			var entryHeader = message.EM_LinkedObject as CusEntryHeader;

			if (entryHeader != null)
			{
				NACCSStateMachineBuilder.Build(entryHeader).TransitOn(parseResult);
				entryHeader.TryImport(parseResult.MessageProvider);

				var responseHeader = parseResult.ResponseHeader;
				if (parseResult.HasResultCode)
				{
					if (parseResult.IsSuccess)
					{
						Func<ZString, ZString> getEmailAddressFromStaffKey = (staffKey) => message.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffKey)?.GS_EmailAddress ?? ZString.Empty;

						var emailTo = GetEmailAddressToSendToFromQueuedUser(GetRequestMessage(message.Factory, responseHeader));
						if (emailTo.IsEmpty)
						{
							emailTo = GetEmailRecipientsRelevantToLinkedObject(entryHeader).SkipWhile(x => x.IsEmpty).Select(re => getEmailAddressFromStaffKey(re)).FirstOrDefault(eml => !eml.IsEmpty);
						}

						if (!emailTo.IsEmpty)
						{
							SendEmail(message, responseHeader, emailTo);
						}

						entryHeader.AddEventWithReference(Events.InterchangeAcknowledged, message.EM_MessageData);
					}
					else
					{
						entryHeader.AddEventWithReference(Events.InterchangeRejected, message.EM_MessageData);
					}
				}

				var copyMessagesCodes = message.Factory.GetCachedValue<JPOutputInformationCodeList.JPCopyOutputInformationCodes>();
				if (responseHeader.OutputInformationCode != null && copyMessagesCodes.ContainsCode(responseHeader.OutputInformationCode))
				{
					entryHeader.AddEventIgnoringMsgNum(Events.CopyReceived, message);
				}

				message.EM_Status = EDIMessage.Status.ProcessedOK;
			}
			else
			{
				message.EM_Status = EDIMessage.Status.Discarded;
				Logger.LogWarning(FormattableString.Invariant($"NACCS response message {message.EM_MessageNum} discarded because it is not linked to an entry header."));
			}
		}

		IEnumerable<ZString> GetEmailRecipientsRelevantToLinkedObject(CusEntryHeader linkedObject)
		{
			yield return linkedObject.Declaration.JE_GS_NKCusAgent;
			yield return linkedObject.CH_SystemLastEditUser;
		}
	}
}

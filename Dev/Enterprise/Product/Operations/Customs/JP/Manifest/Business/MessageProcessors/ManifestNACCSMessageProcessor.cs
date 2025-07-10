using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Manifest.Business.MessageProcessors;

public class ManifestNACCSMessageProcessor(LoggingInformation logger) : NACCSMessageProcessor(logger), Integration.Customs.JP.IManifestHeaderMessageProcessor
{
	protected override void ProcessMessageCore(EDIMessage message)
	{
		if (message.EM_MessageType != JPMessageTypes.Codes.XER)
		{
			ProcessNACCSMessage(message);
		}
	}
	void ProcessNACCSMessage(EDIMessage message)
	{
		var messageData = message.EM_MessageData;
		var parseResult = NACCSFactoryService.GetInboundMessageParser(message.Factory).Parse(messageData);
		var manifestHeader = (AsycudaManifestHeader)message.EM_LinkedObject;
		try
		{
			manifestHeader.TryImport(parseResult, message);
		}
		catch (JPMessageImportException ex)
		{
			additionalWarning = ex.Message;
		}

		var responseHeader = parseResult.ResponseHeader;
		if (parseResult.HasResultCode)
		{
			if (parseResult.IsSuccess)
			{
				Func<ZString, ZString> getEmailAddressFromStaffKey = (staffKey) => message.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffKey)?.GS_EmailAddress ?? ZString.Empty;

				var emailTo = GetEmailAddressToSendToFromQueuedUser(GetRequestMessage(message.Factory, responseHeader));
				if (emailTo.IsEmpty)
				{
					emailTo = GetEmailRecipientsRelevantToLinkedObject(manifestHeader).SkipWhile(x => x.IsEmpty).Select(re => getEmailAddressFromStaffKey(re)).FirstOrDefault(eml => !eml.IsEmpty);
				}
				if (!emailTo.IsEmpty)
				{
					SendEmail(message, responseHeader, emailTo);
				}

				manifestHeader.AddEventWithReference(Events.InterchangeAcknowledged, messageData);
			}
			else
			{
				manifestHeader.AddEventWithReference(Events.InterchangeRejected, messageData);
			}
		}

		var copyMessagesCodes = message.Factory.GetCachedValue<JPOutputInformationCodeList.JPCopyOutputInformationCodes>();
		if (responseHeader.OutputInformationCode != null && copyMessagesCodes.ContainsCode(responseHeader.OutputInformationCode))
		{
			manifestHeader.AddEventIgnoringMsgNum(Events.CopyReceived, message);
		}

		message.EM_Status = EDIMessage.Status.ProcessedOK;
	}

	IEnumerable<ZString> GetEmailRecipientsRelevantToLinkedObject(AsycudaManifestHeader linkedObject)
	{
		yield return linkedObject.AMA_GS_NKCustomsAgent;
		yield return linkedObject.AMA_SystemLastEditUser;
	}
}

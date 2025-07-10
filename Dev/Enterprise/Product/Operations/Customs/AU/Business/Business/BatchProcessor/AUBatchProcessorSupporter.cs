namespace Enterprise.Customs.AU.Declaration.Business
{
	using System;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Edifact;
	using Enterprise.Edifact.D99B.Elements;
	using Enterprise.Edifact.D99B.Segments;
	using Enterprise.Environment;
	using Enterprise.Integration;
	using Enterprise.MailManager.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Messaging.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;

	public static class AUBatchProcessorSupporter
	{
		public static void SendEmail(BusinessObjectFactory factory, GlbBranch branch, string subject, string bodyText, IRegistryItem registry)
		{
			var groupPK = new ZGuid(registry.GetValueWithoutFallback(branch == null ? GlbCompany.CurrentCompany.PK.ToGuid() : branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty));
			if (!groupPK.IsValid || factory.Load<GlbGroup>(groupPK) == null)
			{
				groupPK = Core.Constants.Groups.PostMastersGroupPK;
			}

			if (!groupPK.IsValid || factory.Load<GlbGroup>(groupPK) == null)
			{
				ErrorReporter.ReportOnce("Cannot send error report - postmaster group deleted", "Cannot send error report - postmaster group deleted.");
			}
			else
			{
				var email = new EmailDef();
				email.Subject = subject;
				email.Body = bodyText;
				Env.OutgoingCustomsMailManager.Create(factory, email, groupPK.ToGuid(), GroupSourceLocator.GetFromRegistryItem(registry));
			}
		}

		public static bool IsATextMessage(ZString interchangeText, BatchProcessor.LoggingInformation logger, MailItem item)
		{
			var textToUpper = interchangeText.ToUpper();
			if (!textToUpper.Contains("UNB+UN") && textToUpper.Contains("DOCUMENT MESSAGE NUMBER:"))
			{
				var email = new EmailDef();
				email.Subject = "A Text message response has been received from Australian Customs";
				if (item == null)
				{
					email.Body = string.Format(@"An inbound message (please see below) has been identified as a TEXT RESPONSE from Australian Customs.
This will happen if you change your EDI Site ID details, in the ICS, to indicate 'Format Preference TEXT'. Enterprise is unable to
process these Text Responses. You should immediately change the setting back to 'Format Preference EDI'. 

Message Content:
 {0}", interchangeText);
				}
				else
				{
					email.Body = string.Format(@"An inbound email, with the following details, has been identified as a TEXT RESPONSE from Australian Customs.
This will happen if you change your EDI Site ID details, in the ICS, to indicate 'Format Preference TEXT'. Enterprise is unable to
process these Text Responses. You should immediately change the setting back to 'Format Preference EDI'. 

Received Time: {0}
From: {1}
To: {2}
Subject: {3}

Message Content:
 {4}", item.MI_ReceivedDateTime.ToLongTimeString(), item.MI_From, item.AllRecipients, item.MI_Subject, interchangeText);
				}
				Env.OutgoingCustomsMailManager.CreateAndSave(email, Core.Constants.Groups.PostMastersGroupPK, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
				logger.LogWarning(email.Body);
				return true;
			}
			return false;
		}

		public static void CreateCMRAcknowledgementMessage(EDIInterchange interchangeToAcknowledge)
		{
			Edifact.D99B.Messages.CONTRL.CONTRLMessage ackMessage = new Edifact.D99B.Messages.CONTRL.CONTRLMessage();

			UNHSegment uNH = ackMessage.UNH.InstantiateAChildAndAddItToChildrenCollection();
			uNH.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			uNH.MessageIdentifier.MessageType = MessageTypeList.SyntaxAndServiceReportMessage;
			uNH.MessageIdentifier.MessageVersionNumber = MessageVersionNumberList.DraftVersionUnEdifactDirectory;
			uNH.MessageIdentifier.MessageReleaseNumber = MessageReleaseNumberList.GetFromString("3");
			uNH.MessageIdentifier.ControllingAgency = ControllingAgencyList.UnCefact;

			UCISegment uCI = ackMessage.UCI.InstantiateAChildAndAddItToChildrenCollection();
			uCI.InterchangeControlReference = interchangeToAcknowledge.EI_InterchangeNum;
			uCI.InterchangeSender.SenderIdentification = interchangeToAcknowledge.EI_From; //Sender==Creator
			uCI.InterchangeSender.AddressForReverseRouting = interchangeToAcknowledge.EI_From;
			uCI.InterchangeRecipient.RecipientIdentification = interchangeToAcknowledge.EI_To;
			uCI.ActionCoded = ActionCodedList.InterchangeReceived;

			UNTSegment uNT = ackMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
			uNT.MessageReferenceNumber = "1";
			uNT.NumberOfSegmentsInTheMessage = "3";

			CMRMessage message = interchangeToAcknowledge.Factory.New<CMRMessage>();
			message.EM_MessageType = "CTL";
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = ackMessage.ToString(new UNOCCMRCharacterSet());
			message.EM_LinkedObject = interchangeToAcknowledge;
			message.EM_ApplicationReference = "1";
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
		}
	}
}

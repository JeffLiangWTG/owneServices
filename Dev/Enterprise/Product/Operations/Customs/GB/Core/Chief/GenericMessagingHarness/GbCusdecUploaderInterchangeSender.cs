

namespace Enterprise.Customs.GB.Chief
{
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.Customs.Common.EU;
	using Enterprise.Customs.EU.Business.Declaration;
	using Enterprise.Customs.GB.Business;
	using Enterprise.Customs.GB.Chief.CusRes;
	using Enterprise.Integration;
	using Enterprise.Messaging.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;
	public abstract class GbCusdecUploaderInterchangeSender : GbInterchangeSender
	{
		public GbCusdecUploaderInterchangeSender(ILogger serviceLogger) : base(serviceLogger)
		{
			this.iLogger = serviceLogger;
		}

		public int LockoutThreshold { get; set; }

		public static int MaximumRetriesBeforeFailing
		{
			get
			{
				return Enterprise.Customs.GB.Registry.GBCustomsDataRegistry.Instance.MaximumRetriesOfFailedCusdecs.Value;
			}
		}

		public abstract CusdecMessagePusher GetCusdecMessagePusher(EDIInterchange outgoingInterchange);

		protected override bool SendInt(EDIInterchange outgoingInterchange)
		{
			Argument.NotNull(outgoingInterchange, "outgoingInterchange");
			OutgoingInterchanges.Add(outgoingInterchange);
			string responseFromCspMayBeUkContrlOrCusRes = "";
			CusdecMessagePusher pusher = GetCusdecMessagePusher(outgoingInterchange);
			if (pusher.Credential == null)
			{
				lastErrorMessage = string.Format(@"Credentials for badge {0} were not found, cannot upload.   Registry credentials may have been modified since the message was sent.  Ensure you supply the credentials in the registry.", outgoingInterchange.EI_To);
				iLogger.Log(LogType.Error, lastErrorMessage);
				return false;
			}
			if (pusher.Credential.WebServiceFailureCount >= LockoutThreshold && LockoutThreshold > -1)
			{
				var c = pusher.Credential;
				iLogger.Log(LogType.Warning, string.Format("Uploads: Ignoring credential {0}/{1}/{2}/{3}, login failure count is too high ({4}). Reset its fail count in the registry", c.BadgeCode, c.Company, c.Username, c.Password, c.WebServiceFailureCount));
				return false;
			}
			responseFromCspMayBeUkContrlOrCusRes = pusher.UploadEdifactToCspReturningInterchangeString();

			if (pusher.CredentialsRequireRefreshing)
			{
				pusher.Credential = pusher.GetCredentialForThisInterchange(outgoingInterchange);
			}

			if (pusher.Status == CusdecMessagePusher.ErrorCodes.Success)
			{
				UpdateStatusToSent(outgoingInterchange);
				string responseID = "";
				if (!string.IsNullOrEmpty(responseFromCspMayBeUkContrlOrCusRes))
				{
					CreateNewInterchangeFromResponseString(outgoingInterchange, responseFromCspMayBeUkContrlOrCusRes, pusher.Credential.BadgeCode);
					var mostRecentlyReceivedInterchange = ResponseInterchanges.LastOrDefault();
					if (mostRecentlyReceivedInterchange != null)
					{
						responseID = "#" + mostRecentlyReceivedInterchange.EI_InterchangeNum + ", msg #" + mostRecentlyReceivedInterchange.ContainedMessages[0].EM_MessageNum;
					}
				}
				iLogger.Log(LogType.Information, delegate
				{ return string.Format(CultureInfo.InvariantCulture, "Uploaded message #{0} in interchange #{1} for badge {2}, received reply {3}", outgoingInterchange.ContainedMessages[0].EM_MessageNum, outgoingInterchange.EI_InterchangeNum, pusher.Credential.BadgeCode, responseID); });
			}
			else
			{
				iLogger.Log(LogType.Error, delegate
				{ return string.Format("Could not upload interchange #{0} to CSP: {1}", outgoingInterchange.EI_InterchangeNum, pusher.LastErrorMessageToLog); });
				this.lastErrorMessage = pusher.LastErrorMessageToLog;
			}
			return pusher.Status == CusdecMessagePusher.ErrorCodes.Success;
		}

		void CreateNewInterchangeFromResponseString(EDIInterchange outgoingUploadedInterchange, string responseFromCspMayBeUkContrlOrCusRes, ZString badge)
		{
			UkResponseInterchange ukResponseInterchange = new UkResponseInterchange(responseFromCspMayBeUkContrlOrCusRes,
																					CusResResponseProcessor.ApplicationCodeShared,  // Allows interchanges downloaded by MCP or CNS to both be parsed by same engine. 
																					outgoingUploadedInterchange.Factory);
			if (ukResponseInterchange != null)
			{
				ResponseInterchanges.Add(ukResponseInterchange.CreateInterchange(badge));
			}
			UpdateStatusOfReceivedDataBeforeSave();
		}

		public List<EDIInterchange> ResponseInterchanges
		{
			get { return responseInterchanges ?? (responseInterchanges = new List<EDIInterchange>()); }
		}
		public List<EDIInterchange> OutgoingInterchanges
		{
			get { return outgoingInterchanges ?? (outgoingInterchanges = new List<EDIInterchange>()); }
		}
		List<EDIInterchange> responseInterchanges;
		List<EDIInterchange> outgoingInterchanges;

		protected virtual void UpdateStatusOfReceivedDataBeforeSave()
		{
		}

		void UpdateStatusToSent(EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchange.Status.Sent;
			foreach (EDIMessage message in interchange.ContainedMessages)
			{
				message.EM_Status = EDIMessage.Status.Sent;
			}
		}

		protected override void OnInterchangeSendFailed(EDIInterchange interchange)
		{
			// This is raised by base if our SendInt() returns false. 
			interchange.EI_RetryCount += 1;
			interchange.EI_Status = StatusMeaningFailedToSend;
			if (interchange.EI_RetryCount > MaximumRetriesBeforeFailing)
			{
				WarnUserThatCusDecCouldNotBeSent(interchange);
				interchange.EI_Status = EDIInterchange.Status.Failed;

				foreach (EDIMessage message in interchange.ContainedMessages)
				{
					message.EM_Status = EDIMessage.Status.Failed;
					// Mark the entry as failed, otherwise it'll still there dumbly showing "Awaiting response"
					CusEntryHeader entry = message.EM_LinkedObject as CusEntryHeader;
					if (entry != null)
					{
						entry.CH_EntryStatus = MessageStatusList.Codes.FailedFromTransmission;
						entry.CH_Status = MessageStatusList.Codes.FailedFromTransmission;
					}
				}
			}
		}

		protected virtual ZString StatusMeaningFailedToSend
		{
			get { return EDIInterchange.Status.Queued; }
		}

		void WarnUserThatCusDecCouldNotBeSent(EDIInterchange interchange)
		{
			var pusher = GetCusdecMessagePusher(interchange);
			EmailDef email = new EmailDef();
			email.Body = string.Format(@"Many attempts to send an outbound declaration message directly to the CSP were unsuccessful. 
The outbound message/interchange has been marked as failed.  
Once you have investigated the problem and connectivity is restored, you may need to re-send the interchange or message. 
Please see the Service Task log for the {0} uploader to see the exact reason for the failures.
Connection endpoint is {5} 
The interchange's details are: #{1}, {2}-->{3}. 
The full error text is: \n {4}",
				this.ApplicationCode, interchange.EI_InterchangeNum, interchange.EI_From, interchange.EI_To, lastErrorMessage, pusher.GetEndpointUrlToWhichPusherConnects());
			email.Subject = "Unable to send UK customs declaration message";

			AttachmentDef att = new AttachmentDef("Failed Interchange.txt", GetBytesFromInterchangeText(interchange));
			email.Attachments.Add(att);
			Enterprise.Environment.Env.OutgoingCustomsMailManager.CreateAndSaveToPostmasterGroup(email, interchange.Factory);
		}

		byte[] GetBytesFromInterchangeText(EDIInterchange interchange)
		{
			StringBuilder text = new StringBuilder();
			text.Append(interchange.EI_HeaderText);
			text.Append(System.Environment.NewLine);
			text.Append(interchange.EI_BodyText);
			text.Append(System.Environment.NewLine);
			text.Append(interchange.EI_FooterText);
			return System.Text.Encoding.ASCII.GetBytes(text.ToString());
		}

		public override int NumberOfMessagesPerInterchange
		{
			get { return 1; }
		}

		internal void SetMaximumBatchSize(int p)
		{
			numberOfInterchangesToSendInThisRun = p;
		}

		int? numberOfInterchangesToSendInThisRun;
		protected override int NumberOfInterchangesToSendInThisRun
		{
			get { return numberOfInterchangesToSendInThisRun ?? base.NumberOfInterchangesToSendInThisRun; }
		}

		protected ILogger iLogger;
		string lastErrorMessage;
	}
}

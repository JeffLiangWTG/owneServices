using System;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class MXInterchangeProvider : InterchangeProviderBase
	{
		public MXInterchangeProvider(LoggingInformation logger, NonDependentEDIMessageCollection messages)
			: base(messages)
		{
			this.logger = logger;
		}
		readonly LoggingInformation logger;

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages)
		{
			return ZString.Empty;
		}

		protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count == 1)
			{
				var message = messages.Cast<EDIMessage>().Single();
				var sender = GlbCompany.CurrentCompany.LicenceKeyIdentifier;

				if (!message.EM_MessageText.IsEmpty)
				{
					var ei_To = MXMessageHelper.GetInterchangeMessageTo(message.EM_MessageType, message.EM_IsTestMessage);

					interchange.EI_TransportType = EDIInterchange.TransportType.xT;
					SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, ei_To, sender);
					interchange.EI_InterchangeNum = message.EM_MessageNum;
					interchange.EI_SessionGUID = ZGuid.NewZGuid();
					interchange.EI_GP = message.EM_GP;
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Discarded;

					var headerMessage = Res.GetString("47667760-1B89-4327-A557-B2872D3DF313",
						"Message {0} will be discarded for the following reason:",
						message.EM_MessageNum);
					var messageToLog = Res.GetString("3F309C67-DED7-45BC-BE4F-49D1A9962B6B",
						"The Interchange Body is empty even though there are {0} messages.",
						messages.Count.ToString(CultureInfo.InvariantCulture));

					ZStringBuilder stringBuilder = new ZStringBuilder();
					stringBuilder.Append(headerMessage);
					stringBuilder.Append(messageToLog);

					message.Notes.AddNew(true, ProcessingLogDescription, stringBuilder.ToStringWithNewLineBetweenAppends());
					logger.LogWarning(messageToLog);

					interchange.ContainedMessages.RemoveAll();
					interchange.Delete();
				}
			}
		}

		protected override ZString QueuedInterchangeStatusCode(EDIInterchange interchange) => EDIInterchange.Status.Queued;

		protected override string GetCollationKey(EDIMessage message) => DoNotCollateType;

		protected override Type InterchangeType => typeof(MXInterchange);

		protected override ZString GetMessageTextToMessageBody(EDIInterchange interchange, EDIMessage message)
		{
			if (!message.EM_GB.IsEmpty)
			{
				var company = message.Factory?.Load<GlbBranch>(message.EM_GB)?.Company;
				if (company != null)
				{
					var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).GetGlbExternalPassword<GlbCompanyCredential>(PasswordTypesList.Codes.MXB);
					if (credential != null)
					{
						return MXMessageHelper.EnvelopeSignedMessage(message.EM_MessageText, credential.GP_UserID, credential.GP_CurrentPassword, GetVucemURLForAirMode(), message.EM_MessageType);
					}
				}
			}
			return base.GetMessageTextToMessageBody(interchange, message);
		}

		ZString GetVucemURLForAirMode()
		{
			var wsVucem = MXCustomsDataRegistry.Instance?.WSVucem?.GetFallBackValueAtAllLevels((Guid)(GlbCompany.CurrentCompany?.PK.ToGuid()), Guid.Empty, Guid.Empty);
			return wsVucem?.AirModeWSResponse ?? ZString.Empty;
		}
	}
}

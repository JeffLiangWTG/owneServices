using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInterchangeProvider : InterchangeProviderBase
	{
		public CDSInterchangeProvider(LoggingInformation logger, NonDependentEDIMessageCollection messages) : base(messages)
		{
			this.logger = logger;
		}

		protected override string GetCollationKey(EDIMessage message) => message.PK.ToString();

		protected override ZString GetInterchangeFooter(int messageCount) => ZString.Empty;

		protected override Type InterchangeType => typeof(CDSInterchange);

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var cdsRequestMessage = messages.Cast<CDSEDIMessage>().Single();

			PopulateMessageOwnerIfRequired(cdsRequestMessage);

			SetInterchangeValuesForTransmit(interchange, messages, cdsRequestMessage.EM_MessageType, Constants.EDIInterchange.GBCustoms, cdsRequestMessage.EM_MessageOwner);

			if (interchange.EI_To.IsEmpty || interchange.EI_From.IsEmpty)
			{
				interchange.Delete();
			}
			else
			{
				var request = GBCustomsRequestFactory.New(cdsRequestMessage);

				var isValid = request != null && request.Provider != ProviderType.CCSUK;
				if (isValid && IsInventoryLinkingMessage(cdsRequestMessage) && cdsRequestMessage.EM_LinkedObject is ForwardingConsol && request.Provider == ProviderType.Direct && request.Credentials.Key.Count(x => x == '.') < 2)
				{
					logger.LogError($"Message {cdsRequestMessage.EM_MessageNum} cannot be packaged for delivery directly to CDS via eHub, as a suitable credentials key could not be determined. Most likely consol {request.JobNumber} lacks a sending forwarder with an EORI. The message is being set to failed.");
					isValid = false;
				}

				if (isValid)
				{
					interchange.EI_HeaderText = request.Serialize();
				}
				else
				{
					DeleteInterchange(interchange, cdsRequestMessage, request);
				}
			}
		}

		void PopulateMessageOwnerIfRequired(CDSEDIMessage cdsRequestMessage)
		{
			var dataProvider = GBCustomsRequestFactory.GetRequestDataProvider(cdsRequestMessage);
			if (dataProvider is GBAsycudaBillCustomsRequestDataProvider asycudaBillProvider)
			{
				cdsRequestMessage.EM_MessageOwner = asycudaBillProvider.GetBadgeCode();
			}
		}

		void DeleteInterchange(EDIInterchange interchange, CDSEDIMessage message, GBCustomsRequest request)
		{
			message.EM_Status = EDIMessageStatusList.Codes.Failed;
			interchange.ContainedMessages.Remove(message);
			interchange.Delete();
			if (request != null && request.Provider == ProviderType.CCSUK)
			{
				_ = message.Notes.AddNew(isCustomDescription: true, "Unable to deliver CDS EDI message",
					$"EDIMessage number {message.EM_MessageNum} could not be delivered due to a mismatch between its properties and its parent declaration's properties; " +
					"this is most likely due to changing the declaration's profile and gateway after message generation.");
			}
		}

		bool IsInventoryLinkingMessage(CDSEDIMessage message)
		{
			switch (message.EM_MessageType)
			{
				case CDSEDIMessageTypeList.Codes.InventoryLinkingConsolidationRequest:
				case CDSEDIMessageTypeList.Codes.InventoryLinkingMovementRequest:
				case CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest:
				case CDSEDIMessageTypeList.Codes.ArrivalNotification:
				case CDSEDIMessageTypeList.Codes.MasterQueryDeclaration:
					return true;
				default:
					return false;
			}
		}

		protected override StringBuilder MessageBody(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var messageBody = base.MessageBody(messages, interchange);

			if (interchange.EI_InterchangeType == Constants.InterchangeType.CargoMessageType)
			{
				messageBody = new StringBuilder("<pnt:InventoryMessage xmlns:pnt='http://www.myvan.descartes.com/pnt/2018/r1'>").Append(messageBody);
				messageBody.Append("</pnt:InventoryMessage>");
			}

			return messageBody;
		}

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages)
		{
			var result = ZString.Empty;
			var message = messages.FirstOrDefault() as EDIMessage;
			if (message != null)
			{
				var consol = message.EM_LinkedObject as ForwardingConsol;
				if (consol != null)
				{
					var credential = GetCredential(consol);
					result = ISCCSUKConsolMessage(credential?.CSP ?? ZString.Empty) ? (ZString)(GBCustomsDataRegistry.Instance.GetCdsCcsukProcessingInstruction(false, credential?.PIMA ?? ZString.Empty, interchange.PK.ToAlphanumericOnlyString())) : base.GetFooterText(interchange, messages);
				}
			}

			return result;
		}

		static CredentialsSetting GetCredential(ForwardingConsol consol)
		{
			var consolWrapper = new CustomsExportConsolIntegrationWrapper(consol, null);
			var badge = consolWrapper?.MawbExportHelper?.ME_Profile ?? string.Empty;
			var credential = CredentialsSetting.GetCredentialsForBadge(badge, GlbCompany.CurrentCompany.PK);
			return credential;
		}

		protected override void MarkMessagesInThisInterchangeAsFailed(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			foreach (EDIMessage message in messages.ToArray())
			{
				var errorMessage = new ZStringBuilder();

				if (interchange.EI_To.IsEmpty)
				{
					errorMessage.Append(string.Format(CultureInfo.InvariantCulture, "There is no customs interchange recipient id (Company - {0}, Branch - {1}).", GlbCompany.CurrentCompany.GC_Code, GlbBranch.CurrentBranch.GB_Code) + "\r\n");
				}

				if (interchange.EI_From.IsEmpty)
				{
					errorMessage.Append(string.Format(CultureInfo.InvariantCulture, "There is no customs interchange sender id (EM_MessageOwner)."));
				}

				logger.LogError(string.Format(CultureInfo.InvariantCulture, "Failed to create interchange for CDS Message #" + message.EM_MessageNum + " : " + errorMessage.ToString()));
				message.EM_Status = EDIMessage.Status.Failed;
			}
		}

		static bool ISCCSUKConsolMessage(ZString csp) => string.Equals(csp, "CCSUK");

		protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

		protected readonly LoggingInformation logger;
	}
}

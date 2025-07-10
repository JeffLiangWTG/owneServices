using System;
using System.Linq;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N99;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3N99MessageProcessor : MessageProcessorWithEmailNotification<Ie3N99Type>
	{
		public IE3N99MessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("5A57EC92-C6FB-42DF-9319-FF1908E2E43C", "Error Message");

		protected override Func<Ie3N99Type, string> GetLocalReferenceNumber => messageObject => messageObject.FunctionalReference;

		protected override void ProcessMessageSendEmailNotification(BusinessObjectFactory factory, AsycudaManifestHeader manifestHeader, Ie3N99Type messageObject)
		{
			SendEmailNotification(factory, manifestHeader, messageObject);
			manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;
			UpdateRequestHeadersIfNeeded(manifestHeader);
		}

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3N99Type messageObject) => Res.GetString("56BD28B6-CB5F-4F14-BA83-281DF991FCD2", "ICS2 Error Notification for {0}/{1}", manifestHeader.AMA_JobReference, manifestHeader.AMA_MasterBill);

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3N99Type messageObject)
		{
			var htmlBuilder = new ZStringBuilder();
			htmlBuilder.AppendLine();

			var messageInfoTableBuilder = new HtmlTableCreator(new string[] { Res.GetString("7491CEE0-B2E3-436F-86DA-424485B130FC", "Code"), Res.GetString("7271F7E3-48CD-48A4-BD81-EFFA3637656E", "Description"), Res.GetString("65D88416-76DD-40ED-93F5-E8F1B7CEC414", "Technical Error Message"), Res.GetString("2237F247-5ED4-486E-BA67-A7C94954F8FD", "Message Element Path") });

			foreach (var item in messageObject.Error)
			{
				messageInfoTableBuilder.WriteRow(item.ValidationCode, item.Description ?? string.Empty, item.TechnicalErrorMessage ?? string.Empty, item.Pointer?.MessageElementPath ?? string.Empty);
			}

			htmlBuilder.Append(messageInfoTableBuilder.ToHtml());
			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2ErrorsTo;

		void UpdateRequestHeadersIfNeeded(AsycudaManifestHeader manifestHeader)
		{
			foreach (var requestHeader in manifestHeader.RequestHeaders.Cast<RequestHeader>().Where(x => x.EUS_Type == EUICS2ReferralRequestTypeList.Codes.CL735_RFS))
			{
				requestHeader.EUS_Status = MessageStatusCodeList.Codes.Error;
			}
		}
	}
}

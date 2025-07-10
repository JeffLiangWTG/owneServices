using System;
using System.Linq;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N05;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3N05MessageProcessor : MessageProcessorWithEmailNotification<Ie3N05Type>
	{
		public IE3N05MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("E54A143E-C437-4173-9FBD-9FC5F12291AA", "High Risk Cargo & Mail Screening Request Notification");

		protected override Func<Ie3N05Type, string> GetMasterReferenceNumber => messageObject => messageObject.Mrn;

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3N05Type messageObject)
		{
			var htmlBuilder = new ZStringBuilder(Res.GetString("E54A143E-C437-4173-9FBD-9FC5F12291AA", "High Risk Cargo & Mail Screening Request Notification"));
			htmlBuilder.AppendLine();
			htmlBuilder.AppendLine(Res.GetString("4A682274-D302-4FCF-804D-AFE30A327EB1", "Document Issue Date: {0}", GetISO8601DateTimeWithSecondsPrecision(messageObject.DocumentIssueDate.DateTime)));
			htmlBuilder.AppendLine(Res.GetString("3431E812-1854-41F6-87FA-DD998FAE18F4", "MRN: {0}", messageObject.Mrn));
			htmlBuilder.AppendLine(Res.GetString("77FA58CA-A5A1-44A0-8BFA-E20DE95329F4", "Country: {0}", messageObject.ResponsibleMemberState?.Country));
			htmlBuilder.AppendLine(Res.GetString("59A458A1-F3C1-4EB9-9200-C8FC10739D7E", "Carrier: {0}", messageObject.Carrier.IdentificationNumber));

			if (messageObject.TransportDocument != null)
			{
				htmlBuilder.AppendLine(Res.GetString("05D3D778-D322-4A1F-B51A-369FB11064BB", "Transport document (Master level) Reference: {0}", messageObject.TransportDocument.DocumentNumber));
				htmlBuilder.AppendLine(Res.GetString("6BF425DE-8FD9-454B-B9FB-CAE2A99475F6", "Transport document (Master level) Type: {0}", messageObject.TransportDocument.Type));
			}

			htmlBuilder.AppendLine();

			htmlBuilder.AppendLine(Res.GetString("49BB9033-7BD0-4520-B771-576924A1D92E", "Referral request details"));

			htmlBuilder.AppendLine();

			HtmlTableCreator recommendedHRCMScreeningMethodBuilder = null;
			if (messageObject.ReferralRequestDetails.Any(detail => detail.RecommendedHrcmScreeningMethod.Count > 0))
			{
				recommendedHRCMScreeningMethodBuilder = new HtmlTableCreator(
				new string[]
				{
					Res.GetString("826B5FB2-3391-4C5C-8B37-D0AEFC87EBDD", "Referral Request Reference"),
					Res.GetString("21448482-AEF4-420D-AC79-3D579FD24970", "Method"),
				});
			}

			var referralRequestDetailsBuilder = new HtmlTableCreator(
				new string[]
				{
					Res.GetString("826B5FB2-3391-4C5C-8B37-D0AEFC87EBDD", "Referral Request Reference"),
					Res.GetString("62F658F5-E75C-4113-A938-2AA0BAF0934C", "Request Type"),
					Res.GetString("3B0AEAF8-F0CD-4784-B3AB-D5916E5ED46E", "Transport document (House level) Reference"),
					Res.GetString("E807AB2A-A885-44C8-8D37-9633302DC0BB", "Transport document (House level) Type"),
				});

			foreach (var detail in messageObject.ReferralRequestDetails)
			{
				referralRequestDetailsBuilder.WriteRow(detail.ReferralRequestReference, detail.RequestType, detail.TransportDocumentHouse.DocumentNumber, detail.TransportDocumentHouse.Type);

				foreach (var screeningMethod in detail.RecommendedHrcmScreeningMethod)
				{
					recommendedHRCMScreeningMethodBuilder?.WriteRow(detail.ReferralRequestReference, screeningMethod.Method);
				}
			}

			htmlBuilder.Append(referralRequestDetailsBuilder.ToHtml());

			if (recommendedHRCMScreeningMethodBuilder != null)
			{
				htmlBuilder.AppendLine();
				htmlBuilder.AppendLine(Res.GetString("145209F8-AE05-4BCE-A420-74B7BA8A536C", "Recommended HRCM Screening Method"));
				htmlBuilder.Append(recommendedHRCMScreeningMethodBuilder.ToHtml());
			}

			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3N05Type messageObject)
		{
			return Res.GetString("EAC96693-2253-4C3A-8717-6442396F87C9", "ICS2 - High Risk Cargo & Mail Screening Request Notification for {0}/{1}", manifestHeader.AMA_JobReference, messageObject.Mrn);
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3N05Type messageObject)
		{
			manifestHeader.RegistrationStatus = EUICS2CustomsStatusList.Codes.HRC;
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2RequestsTo;
	}
}

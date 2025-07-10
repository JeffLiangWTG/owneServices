using System;
using System.Linq;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N04;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3N04MessageProcessor : MessageProcessorWithEmailNotification<Ie3N04Type>
	{
		public IE3N04MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("873C24B6-446B-46DB-B4EE-EF55B4FCEC97", "ICS2 Additional Information Request Notification");

		protected override Func<Ie3N04Type, string> GetMasterReferenceNumber => messageObject => messageObject.Mrn;

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2RequestsTo;

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3N04Type messageObject)
		{
			var htmlBuilder = new ZStringBuilder(Res.GetString("873C24B6-446B-46DB-B4EE-EF55B4FCEC97", "ICS2 Additional Information Request Notification"));
			htmlBuilder.AppendLine();
			htmlBuilder.AppendLine(Res.GetString("7EF852E4-1A7E-452D-BECF-FDF0A62C0C20", "Issue Date: {0}", GetISO8601DateTimeWithSecondsPrecision(messageObject.DocumentIssueDate.DateTime)));
			htmlBuilder.AppendLine(Res.GetString("B32574BB-C995-48F9-BFBF-081C14DC6627", "MRN: {0}", messageObject.Mrn));
			htmlBuilder.AppendLine(Res.GetString("EE68FB86-07E6-44B6-B1B8-8634E730C962", "Country: {0}", messageObject.ResponsibleMemberState?.Country));
			htmlBuilder.AppendLine(Res.GetString("59A458A1-F3C1-4EB9-9200-C8FC10739D7E", "Carrier: {0}", messageObject.Carrier.IdentificationNumber));
			htmlBuilder.AppendLine();

			htmlBuilder.AppendLine(Res.GetString("8C546DC8-CA0B-4E8B-AA47-6DF11D321C24", "Referral Request Details"));

			var messageInfoTableBuilder = new HtmlTableCreator(
				new string[]
				{
					Res.GetString("65BA6ACE-67D6-4AA0-B813-E8977BC89809", "Reference"),
					Res.GetString("246D4C68-487F-456A-A3E0-68EA54BA1A16", "Type"),
					Res.GetString("69B689D8-CFC6-45BF-9F51-8DFAC2A5F28B", "Code"),
					Res.GetString("9D9FB29D-5C9D-4550-88AD-E1710E2C5308", "Info Type"),
					Res.GetString("88FBB631-7DCA-4F0C-A134-4C4EB34B1118", "Text")
				});

			HtmlTableCreator messageDocumentTableBuilder = null;
			if (messageObject.ReferralRequestDetails.Any(detail => detail.SupportingDocuments.Count > 0))
			{
				messageDocumentTableBuilder = new HtmlTableCreator(
					new string[]
					{
						Res.GetString("65BA6ACE-67D6-4AA0-B813-E8977BC89809", "Reference"),
						Res.GetString("246D4C68-487F-456A-A3E0-68EA54BA1A16", "Type"),
						Res.GetString("A2A698CC-1B2D-4B04-B2CC-AEEC99F28E38", "Document Type"),
						Res.GetString("1CE59890-D57B-4CC0-BA88-D91E1B2D01CB", "Reference Number")
					});
			}

			foreach (var detail in messageObject.ReferralRequestDetails)
			{
				foreach (var addInfo in detail.AdditionalInformation)
				{
					messageInfoTableBuilder.WriteRow(detail.ReferralRequestReference, detail.RequestType, addInfo.Code, addInfo.Type, addInfo.Text);
				}

				foreach (var doc in detail.SupportingDocuments)
				{
					messageDocumentTableBuilder?.WriteRow(detail.ReferralRequestReference, detail.RequestType, doc.Type, doc.ReferenceNumber);
				}
			}

			htmlBuilder.Append(messageInfoTableBuilder.ToHtml());
			if (messageDocumentTableBuilder != null)
			{
				htmlBuilder.AppendLine();
				htmlBuilder.AppendLine(Res.GetString("5B16F78E-676F-4ACF-9F14-CE33F3273B0C", "Supporting Documents"));
				htmlBuilder.Append(messageDocumentTableBuilder.ToHtml());
			}

			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3N04Type messageObject)
		{
			return Res.GetString("096D80C0-DCC5-459B-9AE7-9DCA1DAC333F", "ICS2 Additional Information Request Notification for {0}/{1}", manifestHeader.AMA_JobReference, messageObject.Mrn);
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3N04Type messageObject)
		{
			var state = messageObject.ResponsibleMemberState?.Country;

			foreach (var detail in messageObject.ReferralRequestDetails)
			{
				var identifier = detail.ReferralRequestReference;

				var header = manifestHeader.RequestHeaders.AddNew();

				header.EUS_Identifier = identifier;
				header.EUS_MessageElement = detail.Pointer?.MessageElementPath;
				header.EUS_Type = detail.RequestType;
				header.EUS_MemberState = state;
				header.EUS_TransportDocumentType = detail.TransportDocumentHouse?.Type;
				header.EUS_HouseBillNumber = detail.TransportDocumentHouse?.DocumentNumber;

				foreach (var doc in detail.SupportingDocuments)
				{
					var document = header.SupportingDocuments.AddNew();
					document.CSI_Code = doc.Type;
					document.CSI_ReferenceNumber = doc.ReferenceNumber;
				}

				foreach (var addInfo in detail.AdditionalInformation)
				{
					var information = header.RequestInformations.AddNew();
					information.CSI_Code = addInfo.Code;
					information.CSI_Description = addInfo.Text;
				}
			}
		}
	}
}

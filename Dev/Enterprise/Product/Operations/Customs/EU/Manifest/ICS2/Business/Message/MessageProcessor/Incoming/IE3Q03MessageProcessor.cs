using System;
using System.Linq;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3Q03;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3Q03MessageProcessor : MessageProcessorWithEmailNotification<Ie3Q03Type>
	{
		public IE3Q03MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("58EAE904-E811-4B24-A080-927A8AB961BE", "High Risk Cargo & Mail Screening Request");

		protected override Func<Ie3Q03Type, string> GetMasterReferenceNumber => messageObject => messageObject.Mrn;

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3Q03Type messageObject)
		{
			var htmlBuilder = new ZStringBuilder(Res.GetString("58EAE904-E811-4B24-A080-927A8AB961BE", "High Risk Cargo & Mail Screening Request"));
			htmlBuilder.AppendLine();
			htmlBuilder.AppendLine(Res.GetString("7EF852E4-1A7E-452D-BECF-FDF0A62C0C20", "Issue Date: {0}", GetISO8601DateTimeWithSecondsPrecision(messageObject.DocumentIssueDate.DateTime)));
			htmlBuilder.AppendLine(Res.GetString("B32574BB-C995-48F9-BFBF-081C14DC6627", "MRN: {0}", messageObject.Mrn));
			htmlBuilder.AppendLine(Res.GetString("EE68FB86-07E6-44B6-B1B8-8634E730C962", "Country: {0}", messageObject.ResponsibleMemberState?.Country));
			htmlBuilder.AppendLine(Res.GetString("173E02F3-063E-42E8-8062-63FE88AF871A", "Declarant: {0}", messageObject.Declarant.IdentificationNumber));
			htmlBuilder.AppendLine(Res.GetString("C977C15C-5A1C-4743-8886-7FA9D61BCAF1", "Representative: {0}", messageObject.Representative?.IdentificationNumber));
			htmlBuilder.AppendLine(Res.GetString("E0520F54-ADE3-4DEF-8262-15C59D4D339D", "Transport Document Reference Number: {0}", messageObject.TransportDocument?.DocumentNumber));
			htmlBuilder.AppendLine(Res.GetString("84C54A9C-F9EA-4CA0-813C-E17B2099DE8D", "Transport Document Type: {0}", messageObject.TransportDocument?.Type));
			htmlBuilder.AppendLine();

			htmlBuilder.AppendLine(Res.GetString("298F9F08-F5CA-42FB-9042-D398F835477D", "Referral Request Details:"));
			var messageInfoTableBuilder = new HtmlTableCreator(
				new string[]
				{
					Res.GetString("A499223B-9205-404C-A2C0-92C2529D60FE", "Referral Request Reference"),
					Res.GetString("43C01F22-D02D-4D56-9A22-CDF8751B1B98", "Request Type"),
					Res.GetString("281E256C-E918-4F11-8D64-06212CF8AF2F", "HRCM Screening Method"),
					Res.GetString("FD559960-EF99-4B61-B67E-0B87D80FF0B0", "Transport Document Reference Number"),
					Res.GetString("E48A17F0-7521-47DC-99CB-DBDB1FDFADB8", "Transport Document Type")
				});

			foreach (var detail in messageObject.ReferralRequestDetails)
			{
				var methodList = string.Empty;
				if (detail.RecommendedHrcmScreeningMethod.Count > 0)
				{
					methodList = string.Join(",", detail.RecommendedHrcmScreeningMethod.Select(x => x.Method).ToArray());
				}
				messageInfoTableBuilder.WriteRow(detail.ReferralRequestReference, detail.RequestType, methodList, detail.TransportDocumentHouse.DocumentNumber, detail.TransportDocumentHouse.Type);
			}

			htmlBuilder.Append(messageInfoTableBuilder.ToHtml());
			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3Q03Type messageObject)
		{
			return Res.GetString("25FA7929-4507-46C1-B121-F53A7CCED476", "ICS2 - High Risk Cargo & Mail Screening Request for {0}/{1}", manifestHeader.AMA_JobReference, messageObject.Mrn);
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3Q03Type messageObject)
		{
			var state = messageObject.ResponsibleMemberState?.Country;

			foreach (var detail in messageObject.ReferralRequestDetails)
			{
				var identifier = detail.ReferralRequestReference;

				var header = manifestHeader.RequestHeaders.AddNew();

				header.EUS_Identifier = identifier;
				header.EUS_Type = detail.RequestType;
				header.EUS_MemberState = state;
				header.EUS_TransportDocumentType = detail.TransportDocumentHouse?.Type;
				header.EUS_HouseBillNumber = detail.TransportDocumentHouse?.DocumentNumber;
				header.EUS_ScreeningMethod = string.Join(",", detail.RecommendedHrcmScreeningMethod.Select(x => x.Method).ToArray());

				if (detail.RequestType == EUICS2ReferralRequestTypeList.Codes.CL735_RFS)
				{
					header.EUS_IncludeScreeningDetails = true;
				}
			}

			manifestHeader.RegistrationStatus = EUICS2CustomsStatusList.Codes.HRC;
			manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2RequestsTo;
	}
}

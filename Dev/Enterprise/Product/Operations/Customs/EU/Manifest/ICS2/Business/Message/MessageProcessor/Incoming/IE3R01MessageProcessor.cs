using System;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3R01;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3R01MessageProcessor : MessageProcessorWithEmailNotification<Ie3R01Type>
	{
		public IE3R01MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("954BBADD-87F9-4892-9B1C-65C0EEF2107E", "ENS Registration Response");

		protected override Func<Ie3R01Type, string> GetLocalReferenceNumber => messageObject => messageObject.Lrn;

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3R01Type messageObject)
		{
			return Res.GetString("0A0830EC-020E-467D-A0D8-C8DF9BD174E8", "ICS2 - ENS Registration Response for {0}/{1}", manifestHeader.AMA_JobReference, messageObject.Mrn);
		}

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3R01Type messageObject)
		{
			var htmlBuilder = new ZStringBuilder(MessageFriendlyName);
			htmlBuilder.AppendLine();
			htmlBuilder.AppendLine(Res.GetString("05EDD4C6-42C4-4DB2-8E9D-7435CC43B1B6", "LRN: {0}", messageObject.Lrn));
			htmlBuilder.AppendLine(Res.GetString("6DF7ABC2-23C4-4E91-9E3E-1A4DD9F53E49", "MRN: {0}", messageObject.Mrn));
			htmlBuilder.AppendLine(Res.GetString("DF0F4ECE-2EB4-496F-B2E4-027179258DB5", "Declarant: {0}", messageObject.Declarant?.IdentificationNumber));
			htmlBuilder.AppendLine(Res.GetString("4BCA6B86-5979-4192-9DE6-A451EF251BDF", "Carrier: {0}", messageObject.Carrier?.IdentificationNumber));
			htmlBuilder.AppendLine(Res.GetString("59C749D4-8135-466B-AF6B-C9A6C7D6EC89", "Registration Date: {0}", GetISO8601DateTimeWithSecondsPrecision(messageObject.RegistrationDate?.DateTime)));
			htmlBuilder.AppendLine(Res.GetString("2BC9413F-918E-4D7D-BF2F-ED025826CE56", "Addressed Member State: {0}", messageObject.AddressedMemberState?.Country));
			htmlBuilder.AppendLine(Res.GetString("29A0861D-A392-4704-ADC3-0D6B518AEFB5", "Representative: {0}", messageObject.Representative?.IdentificationNumber));
			htmlBuilder.AppendLine(Res.GetString("5BA1DAD7-5E7D-4FFC-BDBA-E4CF6ECD64F2", "Transport Document Reference Number: {0}", messageObject.TransportDocument?.DocumentNumber));
			htmlBuilder.AppendLine(Res.GetString("5DCCEF6C-C23B-49BD-8064-F6D93913E665", "Transport Document Type: {0}", messageObject.TransportDocument?.Type));
			htmlBuilder.AppendLine(Res.GetString("EDAB20B0-B550-40AB-829B-9C394474CC72", "Customs Office of First Entry: {0}", messageObject.CustomsOfficeOfFirstEntry?.ReferenceNumber));
			htmlBuilder.AppendLine();

			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3R01Type messageObject)
		{
			if (messageObject.RegistrationDate.DateTime.HasValue)
			{
				// direct assignment manifestHeader.RegistrationDate = RegistrationDate.DateTime.Value breaks tests:
				// RegistrationDate.DateTime.Value.Kind is Utc, and apparently manifestHeader.RegistrationDate expects Local
				const string dateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
				var dateTimeStr = messageObject.RegistrationDate.DateTime.Value.ToString(dateTimeFormat);
				if (ZDateTime.TryParseExact(dateTimeStr, out var result, dateTimeFormat))
				{
					manifestHeader.RegistrationDate = result;
				}
			}

			manifestHeader.RegistrationNumber = messageObject.Mrn;
			manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
			manifestHeader.RegistrationStatus = EUICS2CustomsStatusList.Codes.REG;
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;
	}
}

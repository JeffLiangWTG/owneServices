using System.Collections.Specialized;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTS019ResponsePrettyFormatter : NCTS019EdiMessageXmlPrettier
	{
		public NCTS019ResponsePrettyFormatter(EDIMessage message) : base(message)
		{
		}

		protected override string MakeInboundPrettyForPhase5Interpretation()
		{
			var htmlBuilder = new ZStringBuilder();

			htmlBuilder.Append($"<h2>{Title}</h2>");

			var mainDataTable = new HtmlTableCreator(new NameValueCollection());
			WriteRow(mainDataTable, MrnCaption, $"{dataProvider.MRN}.{dataProvider.MRNVersion}");
			WriteRow(mainDataTable, LrnCaption, $"{dataProvider.LRN}");

			var departureCustomsOffice = GetCustomsOfficeDescription(dataProvider.CustomsOfficeOfDeparture);
			WriteRow(mainDataTable, CustomsOfficeDepartureCaption, $"{dataProvider.CustomsOfficeOfDeparture} {departureCustomsOffice}");
			htmlBuilder.Append(mainDataTable.ToHtml());

			htmlBuilder.Append($"<h3>{NotificationSubTitle}</h3>");
			var notificationTable = new HtmlTableCreator(new NameValueCollection());
			WriteRow(notificationTable, NotificationTextCaption, $"{dataProvider.DiscrepanciesNotificationText}");
			WriteRow(notificationTable, NotificationDateCaption, $"{dataProvider.DiscrepanciesNotificationDate:dd-MM-yyyy}");
			htmlBuilder.Append(notificationTable.ToHtml());

			htmlBuilder.Append($"<h4>{GuarantorSubTitle}</h4>");
			var guarantorTable = new HtmlTableCreator(new NameValueCollection());
			WriteRow(guarantorTable, GuarantorIdentificationNumberCaption, $"{dataProvider.GuarantorIdentificationNumber}");
			WriteRow(guarantorTable, GuarantorNameCaption, $"{dataProvider.GuarantorName}");
			if (dataProvider.GuarantorAddress != null)
			{
				var address = dataProvider.GuarantorAddress;
				var formattedAddress = $"{address.StreetAndNumber} {address.Postcode} {address.City}";
				WriteRow(guarantorTable, GuarantorAddressCaption, formattedAddress);
			}
			htmlBuilder.Append(guarantorTable.ToHtml());

			htmlBuilder.Append($"<h4>{HolderOfTheTransitProcedureSubTitle}</h4>");
			var holderTable = new HtmlTableCreator(new NameValueCollection());
			WriteRow(holderTable, HolderOfTheTransitProcedureCaption, $"{dataProvider.HolderOfTheTransitProcedure}");
			htmlBuilder.Append(holderTable.ToHtml());

			return htmlBuilder.ToString();
		}

		static string Title => Res.GetString("F7EF2C92-522B-4D7B-B6A5-E47D0F5D7CA5", "Discrepancies at Destination");
		static string NotificationSubTitle => Res.GetString("ABA6EF54-CE89-46B8-84FB-677C55DE5535", "Notification");
		static string GuarantorSubTitle => Res.GetString("AD5DA391-5768-43D7-935A-0DC2AF5B9D5B", "Guarantor");
		static string HolderOfTheTransitProcedureSubTitle => Res.GetString("6730D18D-DB11-489D-80AF-ADF0338A1BD2", "Holder Of The Transit Procedure");
		static string MrnCaption => Res.GetString("C51B29E2-4017-4F7E-8272-7F790649FFFD", "MRN:");
		static string LrnCaption => Res.GetString("7B5A8189-BE08-4740-A060-D119E61F0B29", "LRN:");
		static string CustomsOfficeDepartureCaption => Res.GetString("BA7E75ED-3A85-4F5D-8299-34D48909F414", "Customs Office of Departure:");
		static string NotificationTextCaption => Res.GetString("FE711EDB-12B1-42ED-A888-3D9B23CAEB65", "Notification Text:");
		static string NotificationDateCaption => Res.GetString("EA3D789A-1CA8-4164-BB94-8344001D90BD", "Notification Date:");
		static string GuarantorIdentificationNumberCaption => Res.GetString("0221F156-E805-4E56-BF96-06B85C8983F9", "Guarantor Identification Number:");
		static string GuarantorNameCaption => Res.GetString("2A800E0C-F32D-4C39-8B8C-CAFBFD3C3C94", "Guarantor Name:");
		static string GuarantorAddressCaption => Res.GetString("5F3323B7-D1B8-4422-86FD-F75887363A73", "Guarantor Address:");
		static string HolderOfTheTransitProcedureCaption => Res.GetString("B2E53F60-C32D-4EF6-A587-94C56253510F", "Holder Of The Transit Procedure Identification Number:");

		string GetCustomsOfficeDescription(ZString customsOfficeCode)
		{
			var dataGroupingCode = customsOfficeCode.SubstringSafe(0, 2);
			if (!dataGroupingCode.IsEmpty)
			{
				return Universal.ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(message.Factory, customsOfficeCode, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Now)?.ZZD_Description ?? ZString.Empty;
			}
			return ZString.Empty;
		}

		void WriteRow(HtmlTableCreator table, string caption, string v)
		{
			if (!string.IsNullOrEmpty(v))
			{
				table.WriteRow(caption, System.Net.WebUtility.HtmlEncode(v));
			}
		}
	}
}

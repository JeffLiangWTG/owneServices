using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class GenralFallbackStatusUpdateHandler
	{
		public GenralFallbackStatusUpdateHandler(string payload, EDIMessage inboundMessage)
		{
			this.payload = payload;
			this.inboundMessage = inboundMessage;
		}

		internal void DoAllProcessing()
		{
			// mask is MUCR A:12512345671 FALLBACK RELEASED 29 May 12 – 11:30 LHRBAC
			var regex = new Regex(@"MUCR (?<MUCR>A\:[0-9]{11}) FALLBACK (?<ACTION>HOLD|RELEASED) (?<DATE>.*?) (?<LOCATION>\w{6})");
			var matches = regex.Matches(payload);

			var mucr = ZString.Empty;
			var action = ZString.Empty;
			var date = ZString.Empty;
			var location = ZString.Empty;

			if (matches.Count == 0)
			{
				regex = new Regex(@"MUCR (?<MUCR>A\:[0-9]{11}) ");
				matches = regex.Matches(payload);
			}

			foreach (Match match in matches)
			{
				mucr = match.Groups["MUCR"].Value;
				action = match.Groups["ACTION"].Value;
				date = match.Groups["DATE"].Value;
				location = match.Groups["LOCATION"].Value;
			}

			if (!mucr.IsEmpty)
			{
				var queryForLoadingConsol = ErsReportProcessor.GetConsolLoadQuery(mucr);  // British consols whose MUCR is A:blah made in the last 12 months. Relies on validation of JK_MasterBill to ensure unique. 
				var consol = inboundMessage.Factory.LoadTop1<ForwardingConsol>(queryForLoadingConsol);
				if (consol != null)
				{
					var wrapper = new CustomsExportConsolIntegrationWrapper(consol, new Customs.Business.SendsMessagesToCustomsShutterUpperer(false));
					if (!date.IsEmpty)
					{
						var dateTime = ZDateTime.Empty;
						ZDateTime.TryParseExact(date, out dateTime, "dd MMM yy - HH:mm");  // from specs - "29 May 12 – 11:30 ", but with smart hyphen swapped for plain hyphen
						wrapper.MawbExportHelper.ME_ChiefCustomsActionDateFromFsn = dateTime;
					}
					if (!action.IsEmpty || !location.IsEmpty)
					{
						wrapper.MawbExportHelper.ME_ChiefCustomsActionTextFromFsn = string.Format("FBK {0} {1}", action, location);
					}
					consol.Messages.Add(inboundMessage);
				}
			}
		}

		readonly string payload;
		readonly EDIMessage inboundMessage;
	}
}

using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class PSRMessageInterpreter : BaseInboundMessageInterpreter<PSRProvider>
	{
		public PSRMessageInterpreter(CustomsAndExciseReportInboundMessage message, PSRProvider messageProvider) : base(message, messageProvider)
		{
		}

		protected override string Summary => CommonResStrings.PSRMessageFriendlyName;

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.EORI, provider.Eori);
			yield return (CommonResStrings.Period, provider.Period.ToShortDateString());
			yield return (CommonResStrings.TaxTotal, provider.TaxTotal.ToString("#,0.00"));
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			if (provider.HasTaxBreakdowns)
			{
				bool isSummaryAdd = false;
				foreach (var taxBreakdown in provider.TaxBreakdowns)
				{
					var summary = "";
					if (!isSummaryAdd)
					{
						summary = CommonResStrings.TaxBreakdowns;
						isSummaryAdd = true;
					}
					yield return (summary,
						new (string, string)[]
						{
							(CommonResStrings.TaxType, taxBreakdown.TaxType),
							(CommonResStrings.PayableAmount, taxBreakdown.PayableAmount.ToString("#,0.00"))
						});
				}
			}

			if (provider.HasDailyBreakdowns)
			{
				bool isSummaryAdd = false;
				foreach (var dailyBreakdown in provider.DailyBreakdowns)
				{
					var summary = "";
					if (!isSummaryAdd)
					{
						summary = Res.GetString("3C5E1B73-0613-40A1-BDEB-CB90531D6B53", "Daily Breakdowns");
						isSummaryAdd = true;
					}

					yield return (summary,
						new (string, string)[]
						{
							(CommonResStrings.Date, dailyBreakdown.Date.ToShortDateString()),
							(CommonResStrings.TaxTotal, dailyBreakdown.TaxTotal.ToString("#,0.00"))
						});
				}
			}
		}
	}
}

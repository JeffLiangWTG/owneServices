using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class DSRMessageInterpreter : BaseInboundMessageInterpreter<DSRProvider>
	{
		public DSRMessageInterpreter(CustomsAndExciseReportInboundMessage message, DSRProvider messageProvider) : base(message, messageProvider)
		{
		}

		protected override string Summary => CommonResStrings.DSRMessageFriendlyName;

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.EORI, provider.Eori);
			yield return (CommonResStrings.Date, provider.Date.ToShortDateString());
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
		}
	}
}

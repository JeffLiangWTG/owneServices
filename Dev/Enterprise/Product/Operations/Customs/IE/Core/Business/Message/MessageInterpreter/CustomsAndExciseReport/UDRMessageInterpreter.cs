using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class UDRMessageInterpreter : BaseInboundMessageInterpreter<UDRProvider>
	{
		public UDRMessageInterpreter(CustomsAndExciseReportInboundMessage message, UDRProvider messageProvider) : base(message, messageProvider)
		{
		}

		protected override string Summary => CommonResStrings.UDRMessageFriendlyName;

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.EORI, provider.Eori);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			if (provider.HasUnpaidOrders)
			{
				bool isSummaryAdd = false;
				foreach (var unpaidOrder in provider.UnpaidOrders)
				{
					var summary = "";
					if (!isSummaryAdd)
					{
						summary = Res.GetString("{C0B47AFF-C085-4326-81E8-3B5E4A12FB90}", "Unpaid Orders");
						isSummaryAdd = true;
					}
					yield return (summary,
						new (string, string)[]
						{
							(CommonResStrings.MovementReferenceNumber, unpaidOrder.Mrn),
							(CommonResStrings.Version, unpaidOrder.Version.ToString()),
							(CommonResStrings.TaxTotal, unpaidOrder.TaxTotal.ToString("#,0.00")),
						});
				}
			}
		}
	}
}

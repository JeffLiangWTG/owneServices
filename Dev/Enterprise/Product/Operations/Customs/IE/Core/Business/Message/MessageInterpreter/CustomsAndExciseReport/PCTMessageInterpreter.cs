using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class PCTMessageInterpreter : BaseInboundMessageInterpreter<PCTProvider>
	{
		public PCTMessageInterpreter(CustomsAndExciseReportInboundMessage message, PCTProvider messageProvider) : base(message, messageProvider)
		{
		}

		protected override string Summary => CommonResStrings.PCTMessageFriendlyName;

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.EORI, provider.Eori);
			yield return (CommonResStrings.Period, provider.Period.ToShortDateString());
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			if (provider.HasPaidOrders)
			{
				bool isSummaryAdd = false;
				foreach (var paidOrder in provider.PaidOrders)
				{
					var summary = "";
					if (!isSummaryAdd)
					{
						summary = CommonResStrings.PaidOrders;
						isSummaryAdd = true;
					}
					yield return (summary,
						new (string, string)[]
						{
							(CommonResStrings.MovementReferenceNumber, paidOrder.Mrn),
							(CommonResStrings.Version, paidOrder.Version.ToString()),
							(CommonResStrings.Amendment, paidOrder.Amendment.ToString()),
							(CommonResStrings.DeclarationMsgType, paidOrder.DeclarationMsgType),
							(CommonResStrings.Payer, paidOrder.Payer),
							(CommonResStrings.Importer, paidOrder.Importer),
							(CommonResStrings.ImporterName, paidOrder.ImporterName),
							(CommonResStrings.Declarant, paidOrder.Declarant),
							(CommonResStrings.DeclarantName, paidOrder.DeclarantName),
							(CommonResStrings.Recieved, paidOrder.DtReceived),
							(CommonResStrings.TaxTotal, paidOrder.TaxTotal.ToString("#,0.00")),
							(CommonResStrings.TotalDuty, paidOrder.TotalDuty.ToString("#,0.00")),
							(CommonResStrings.VatOnDuty, paidOrder.VatOnDuty.ToString("#,0.00")),
							(CommonResStrings.TotalExcise, paidOrder.TotalExcise.ToString("#,0.00")),
							(CommonResStrings.VatOnExcise, paidOrder.VatOnExcise.ToString("#,0.00")),
							(CommonResStrings.PostponedVat, paidOrder.PostponedVat.ToString("#,0.00")),
							(CommonResStrings.LocalReferenceNumber, paidOrder.Lrn),
							(CommonResStrings.UCR, paidOrder.Ucr),
							(CommonResStrings.CommercialTransportDoc, paidOrder.CommercialTransportDoc),
						});
				}
			}
		}
	}
}

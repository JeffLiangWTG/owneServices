using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class DTTMessageInterpreter : BaseInboundMessageInterpreter<DTTProvider>
	{
		public DTTMessageInterpreter(CustomsAndExciseReportInboundMessage message, DTTProvider messageProvider) : base(message, messageProvider)
		{
		}

		protected override string Summary => CommonResStrings.DTTMessageFriendlyName;

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.EORI, provider.Eori);
			yield return (CommonResStrings.Day, provider.Day.ToShortDateString());
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			if (provider.HasTaxDetails)
			{
				bool isSummaryAdd = false;
				foreach (var taxDetail in provider.TaxDetails)
				{
					var summary = "";
					if (!isSummaryAdd)
					{
						summary = CommonResStrings.TaxDetails;
						isSummaryAdd = true;
					}
					yield return (summary,
						new (string, string)[]
						{
							(CommonResStrings.MovementReferenceNumber, taxDetail.Mrn),
							(CommonResStrings.Version, taxDetail.Version.ToString()),
							(CommonResStrings._1D3, taxDetail._1D3.ToString()),
							(CommonResStrings._1A1, taxDetail._1A1.ToString()),
							(CommonResStrings._1B2, taxDetail._1B2.ToString()),
							(CommonResStrings._A00, taxDetail._A00.ToString()),
							(CommonResStrings._1B3, taxDetail._1B3.ToString()),
							(CommonResStrings._1D5, taxDetail._1D5.ToString()),
							(CommonResStrings._A45, taxDetail._A45.ToString()),
							(CommonResStrings._B00, taxDetail._B00.ToString()),
							(CommonResStrings._1D6, taxDetail._1D6.ToString()),
							(CommonResStrings._A35, taxDetail._A35.ToString()),
							(CommonResStrings._B00EX, taxDetail._B00EX.ToString()),
							(CommonResStrings._1S1, taxDetail._1S1.ToString()),
							(CommonResStrings._1E1, taxDetail._1E1.ToString()),
							(CommonResStrings._A40, taxDetail._A40.ToString()),
							(CommonResStrings._A30, taxDetail._A30.ToString()),
							(CommonResStrings._1C1, taxDetail._1C1.ToString()),
							(CommonResStrings._2E2, taxDetail._2E2.ToString()),
							(CommonResStrings._A20, taxDetail._A20.ToString()),
						});
				}
			}
		}
	}
}

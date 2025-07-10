using System;
using System.Collections.Generic;

namespace Enterprise.Customs.Common
{
	public class BorderWiseTariffExchangeModelV2
	{
		public BorderWiseTariffExchangeModelV2()
		{
			BorderWiseInvoiceLines = new List<BorderWiseInvoiceLine>();
			InvoicePksAction = new Dictionary<Guid, string>();
		}

		public Guid JobPk { get; set; }

		public Guid BranchPk { get; set; }

		public Dictionary<Guid, string> InvoicePksAction { get; }

		public string JobNumber { get; set; }

		public string JobStatus { get; set; }

		public string JobType { get; set; }

		public string TariffType { get; set; }

		public string CurrentlyEditedBy { get; set; }

		public string Message { get; set; }

		public List<BorderWiseInvoiceLine> BorderWiseInvoiceLines { get; }
	}
}

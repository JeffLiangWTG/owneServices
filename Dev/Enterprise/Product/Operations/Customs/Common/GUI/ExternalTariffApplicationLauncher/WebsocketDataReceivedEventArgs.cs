using System;
using System.Collections.Generic;

namespace Enterprise.Customs.Common.GUI.ExternalTariffApplicationLauncher
{
	public class WebSocketDataReceivedEventArgs : EventArgs
	{
		public Guid ClientId { get; set; }

		public string JobStatus { get; set; }

		public Guid JobPk { get; set; }

		public List<BorderWiseInvoiceLine> BorderWiseInvoiceLines { get; set; }

		public WebSocketDataReceivedEventArgs(
			Guid clientId,
			List<BorderWiseInvoiceLine> borderWiseInvoiceLine,
			Guid jobPk,
			string jobStatus = "")
		{
			ClientId = clientId;
			BorderWiseInvoiceLines = borderWiseInvoiceLine;
			JobPk = jobPk;
			JobStatus = jobStatus;
		}

		public WebSocketDataReceivedEventArgs(
			Guid clientId,
			List<BorderWiseInvoiceLine> borderWiseInvoiceLine)
		{
			ClientId = clientId;
			BorderWiseInvoiceLines = borderWiseInvoiceLine;
		}
	}
}

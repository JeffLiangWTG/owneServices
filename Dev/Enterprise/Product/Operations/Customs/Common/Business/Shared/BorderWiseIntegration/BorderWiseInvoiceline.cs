using System;

namespace Enterprise.Customs.Common
{
	public sealed class BorderWiseInvoiceLine
	{
		public BorderWiseInvoiceLine(
			string tariffCode,
			string statCode,
			Guid? invoiceLinePK = default,
			int? invoiceLineNumber = default,
			Guid? invoiceNumberPk = default,
			string invoiceNumber = "",
			string description = "",
			string productCode = "",
			bool isSelected = false)
		{
			TariffCode = tariffCode;
			StatCode = statCode;
			InvoiceLinePK = invoiceLinePK;
			InvoiceLineNumber = invoiceLineNumber;
			InvoiceNumberPk = invoiceNumberPk;
			InvoiceNumber = invoiceNumber;
			Description = description;
			ProductCode = productCode;
			IsSelected = isSelected;
		}

		public Guid? InvoiceLinePK { get; set; }

		public string TariffCode { get; set; }

		public string StatCode { get; set; }

		public int? InvoiceLineNumber { get; set; }

		public Guid? InvoiceNumberPk { get; set; }

		public string InvoiceNumber { get; set; }

		public string Description { get; set; }

		public string ProductCode { get; set; }

		public bool IsSelected { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class StlInvoiceFinder
	{
		public void Clear()
		{
			Invoices = Array.Empty<InvoicingBase>();
			BillLineUsageLinesList.Clear();
			BillLineFeeUsageList.Clear();
			BillLineInvoiceLineList.Clear();
			UsageLineToInvoiceMap?.Clear();
			FeeUsageToInvoiceMap?.Clear();
		}

		public void AddBillLine(SystemBill.BillLine billLine, IEnumerable<UsageLine> usageLines)
		{
			if (billLine != null && usageLines != null && usageLines.Any())
			{
				BillLineUsageLinesList.Add(new BillLineUsageLines(billLine, usageLines.ToArray()));
			}
		}

		public void AddBillLine(SystemBill.BillLine billLine, FeeUsage feeUsage)
		{
			if (billLine != null && feeUsage != null)
			{
				BillLineFeeUsageList.Add(new BillLineFeeUsage(billLine, feeUsage));
			}
		}

		public void AddBillLine(SystemBill.BillLine billLine, InvoicingLineBase invoiceLine)
		{
			if (invoiceLine != null && billLine != null)
			{
				BillLineInvoiceLineList.Add(new BillLineInvoiceLine(billLine, invoiceLine));
			}
		}

		public void BuildInvoiceMap(IEnumerable<InvoicingBase> invoices)
		{
			if (invoices == null || !invoices.Any())
			{
				throw new ArgumentOutOfRangeException(nameof(invoices));
			}

			Invoices = invoices?.ToArray() ?? Array.Empty<InvoicingBase>();

			if (Invoices.Count() < 2)
			{
				return;
			}

			var invoiceLineMap = Invoices.SelectMany(x => x.Lines.OfType<InvoicingLineBase>().Select(y => new { AL = y, AH = x }))
				.GroupBy(x => x.AL)
				.ToDictionary(k => k.Key, v => v.First().AH);

			var billLine2Invoice = BillLineInvoiceLineList.Select(x => new
			{
				x.BillLine,
				AH = invoiceLineMap.TryGetValue(x.InvoiceLine, out var invoice) ? invoice : null
			}).Where(x => x.AH != null)
			.GroupBy(x => x.BillLine)
			.ToDictionary(k => k.Key, v => v.First().AH);

			UsageLineToInvoiceMap = BillLineUsageLinesList.Select(x => new
			{
				AH = billLine2Invoice.TryGetValue(x.BillLine, out var invoice) ? invoice : null,
				x.UsageLines
			})
			.Where(x => x.AH != null)
			.SelectMany(x => x.UsageLines.Select(y => new { x.AH, UsageLine = y }))
			.GroupBy(x => x.UsageLine)
			.ToDictionary(k => k.Key, v => v.First().AH);

			FeeUsageToInvoiceMap = BillLineFeeUsageList.Select(x => new
			{
				AH = billLine2Invoice.TryGetValue(x.BillLine, out var invoice) ? invoice : null,
				x.FeeUsage
			})
			.Where(x => x.AH != null)
			.GroupBy(x => x.FeeUsage)
			.ToDictionary(k => k.Key, v => v.First().AH);
		}

		public InvoicingBase FindInvoice(UsageLine usageLine)
		{
			if (Invoices.Count() < 2 || usageLine == null)
			{
				return FirstInvoice;
			}

			return UsageLineToInvoiceMap.TryGetValue(usageLine, out var result) ? result : FirstInvoice;
		}

		public InvoicingBase FindInvoice(FeeUsage feeUsage)
		{
			if (Invoices.Count() < 2 || feeUsage == null)
			{
				return FirstInvoice;
			}

			return FeeUsageToInvoiceMap.TryGetValue(feeUsage, out var result) ? result : FirstInvoice;
		}

		public BusinessObjectFactory InvoiceFactory => FirstInvoice?.Factory;

		Dictionary<UsageLine, InvoicingBase> UsageLineToInvoiceMap;
		Dictionary<FeeUsage, InvoicingBase> FeeUsageToInvoiceMap;
		readonly List<BillLineUsageLines> BillLineUsageLinesList = new List<BillLineUsageLines>();
		readonly List<BillLineFeeUsage> BillLineFeeUsageList = new List<BillLineFeeUsage>();
		readonly List<BillLineInvoiceLine> BillLineInvoiceLineList = new List<BillLineInvoiceLine>();
		IEnumerable<InvoicingBase> Invoices = Array.Empty<InvoicingBase>();
		InvoicingBase FirstInvoice => Invoices.FirstOrDefault();

		class BillLineUsageLines
		{
			public BillLineUsageLines(SystemBill.BillLine billLine, IEnumerable<UsageLine> usageLines)
			{
				BillLine = billLine;
				UsageLines = usageLines;
			}

			public readonly SystemBill.BillLine BillLine;
			public readonly IEnumerable<UsageLine> UsageLines;
		}

		class BillLineFeeUsage
		{
			public BillLineFeeUsage(SystemBill.BillLine billLine, FeeUsage feeUsage)
			{
				BillLine = billLine;
				FeeUsage = feeUsage;
			}

			public readonly SystemBill.BillLine BillLine;
			public readonly FeeUsage FeeUsage;
		}

		class BillLineInvoiceLine
		{
			public BillLineInvoiceLine(SystemBill.BillLine billLine, InvoicingLineBase invoiceLine)
			{
				BillLine = billLine;
				InvoiceLine = invoiceLine;
			}

			public readonly SystemBill.BillLine BillLine;
			public readonly InvoicingLineBase InvoiceLine;
		}
	}
}

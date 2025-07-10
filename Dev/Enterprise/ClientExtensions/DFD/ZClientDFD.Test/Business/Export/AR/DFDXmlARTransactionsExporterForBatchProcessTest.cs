using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Export.Testing
{
	[TestedType(typeof(DFDXmlARTransactionsExporterForBatchProcess))]
	public class DFDXmlARTransactionsExporterForBatchProcessTest : DFDXmlARTransactionsExporterTest
	{
		public override void TestFinancialInvoiceTransactionExportFilter()
		{
			AssertEquals(typeof(DFDFinancialInvoiceTransactionExportFilterForBatchProcess), ((IExporter)Exporter).InvoiceBatchFilter.GetType());
		}

		protected override DFDXmlARTransactionsExporter Exporter
		{
			get
			{
				if (exporter == null)
				{
					exporter = new DFDXmlARTransactionsExporterForBatchProcessForTest(Factory);
				}

				return exporter;
			}
		}

		DFDXmlARTransactionsExporter exporter;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DFDXmlARTransactionsExporterForBatchProcess(Factory, ZDateTime.Now, ZDateTime.Now);
		}

		#region Implementation
		public class DFDXmlARTransactionsExporterForBatchProcessForTest : DFDXmlARTransactionsExporterForBatchProcess, IExporter
		{
			public DFDXmlARTransactionsExporterForBatchProcessForTest(BusinessObjectFactory factory) : base(factory, ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddDays(1))
			{
			}

			public new FinancialInvoiceTransactionExportFilter InvoiceBatchFilter
			{
				get
				{
					return base.InvoiceBatchFilter;
				}
			}

			public new ZBool ErrorHasOccuredCore
			{
				get
				{
					return base.ErrorHasOccuredCore;
				}
			}

			public new ZString GetDifferences(ZBool exportIsSuccessful, ZString description, ZInt transactionsInBatch, ZInt transactionsInStandardExport)
			{
				return base.GetDifferences(exportIsSuccessful, description, transactionsInBatch, transactionsInStandardExport);
			}
		}
		#endregion
	}
}

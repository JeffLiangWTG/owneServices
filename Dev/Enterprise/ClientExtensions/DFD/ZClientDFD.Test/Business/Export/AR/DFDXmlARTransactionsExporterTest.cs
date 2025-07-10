using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Client.DFD.Registry;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Export.Testing
{
	[TestedType(typeof(DFDXmlARTransactionsExporter))]
	public class DFDXmlARTransactionsExporterTest : XmlAccountingTransactionExporterTest
	{
		public void TestFilterProvider()
		{
			TransactionsTypesToExportBusinessObject transactionsTypesToExportBusinessObject = new TransactionsTypesToExportBusinessObject(Factory);
			transactionsTypesToExportBusinessObject.ARInvoice = true;
			transactionsTypesToExportBusinessObject.ARJobRelated = true;
			DFDDataRegistry.Instance.ARTransactionsTypesToExport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, transactionsTypesToExportBusinessObject);
			AssertEquals("IncludeARAdjustmentNotes", false, Exporter.FilterProvider.IncludeARAdjustmentNotes);
			AssertEquals("IncludeARCreditNotes", false, Exporter.FilterProvider.IncludeARCreditNotes);
			AssertEquals("IncludeARInvoices", true, Exporter.FilterProvider.IncludeARInvoices);
			AssertEquals("IncludeARCreditNotes", true, Exporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAR);
			AssertEquals("IncludeARInvoices", false, Exporter.FilterProvider.ExcludeJobRelatedTransactionsForAR);
		}

		public virtual void TestFinancialInvoiceTransactionExportFilter()
		{
			AssertEquals(typeof(DFDFinancialInvoiceTransactionExportFilter), ((IExporter)Exporter).InvoiceBatchFilter.GetType());
		}

		public void TestErrorHasOccuredCore()
		{
			Assert(!((IExporter)Exporter).ErrorHasOccuredCore);
		}

		public void TestGetDifferences()
		{
			AssertEquals(@"The following transactions were exported successfully: Invoice - 3
", ((IExporter)Exporter).GetDifferences(true, "Invoice", 323623, 3));
		}

		protected virtual DFDXmlARTransactionsExporter Exporter
		{
			get
			{
				if (exporter == null)
				{
					exporter = new DFDXmlARTransactionsExporterForTest(Factory);
				}

				return exporter;
			}
		}

		DFDXmlARTransactionsExporterForTest exporter;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DFDXmlARTransactionsExporter(Factory);
		}

		#region Implementation
		public class DFDXmlARTransactionsExporterForTest : DFDXmlARTransactionsExporter, IExporter
		{
			public DFDXmlARTransactionsExporterForTest(BusinessObjectFactory factory) : base(factory)
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

		public interface IExporter
		{
			FinancialInvoiceTransactionExportFilter InvoiceBatchFilter { get; }

			ZBool ErrorHasOccuredCore { get; }

			ZString GetDifferences(ZBool exportIsSuccessful, ZString description, ZInt transactionsInBatch, ZInt transactionsInStandardExport);
		}
		#endregion
	}
}

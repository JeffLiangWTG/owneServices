using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	[TestedType(typeof(IntercompanyTransactionFinancialInvoiceDataAdapterTestClass))]
	class IntercompanyTransactionFinancialInvoiceDataAdapterTest : UnapprovedTransactionFinancialInvoiceDataAdapterTest
	{
		protected override ValueObjectDataAdapter<InvoicingBase, TxnHeader> GetNewBizObjXmlDataAdapter()
		{
			return new IntercompanyTransactionFinancialInvoiceDataAdapterTestClass();
		}

		public void TestUseChargeDescAsLineDesc()
		{
			var adapter = new IntercompanyTransactionFinancialInvoiceDataAdapterTestClass();
			AssertEquals(true, adapter.GetTransactionBuilderConfig_ForTest().UseChargeDescAsLineDesc);
			AccountingConfigurationRegistry.Instance.RemoveItemFromCacheIfOlderThan("CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting", TimeSpan.MinValue);
			using (AccountingConfigurationRegistry.Instance.CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				Assert(!adapter.GetTransactionBuilderConfig_ForTest().UseChargeDescAsLineDesc);
			}
		}

		class IntercompanyTransactionFinancialInvoiceDataAdapterTestClass : IntercompanyTransactionFinancialInvoiceDataAdapter
		{
			protected override void SetTxnLineGuid(TxnLine xmlInvoiceLine, InvoicingLineBase invoiceLine)
			{
				xmlInvoiceLine.TxnLineGUID = "lineGUID";
			}

			public TransactionBuilderConfig GetTransactionBuilderConfig_ForTest()
			{
				return GetTransactionBuilderConfig(false);
			}
		}
	}
}

using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseConsolCostForImporting))]
	public class APInvoiceConsolCostForImportingTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			InvoicingBaseConsolCostCollectionForImporting collection = new InvoicingBaseConsolCostCollectionForImporting(Factory);
			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceConsolCosting costing = new APInvoiceConsolCosting(Factory, invoice);
			collection.Importer = new InvoicingBaseBulkConsolCostImporter(costing);
			InvoicingBaseConsolCostForImporting consolCost = Factory.New<InvoicingBaseConsolCostForImporting>();
			consolCost.E6_IsTaxAmountOverridden = true;
			collection.Add(consolCost);

			return consolCost;
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesInvoicingBaseConsolCostForImporting()
		{
			InvoicingBaseConsolCostForImporting invoice = Factory.New<InvoicingBaseConsolCostForImporting>();

			var exList = new List<string>
			{
				nameof(invoice.ExchangeRateInInvoiceCurrency)
			};

			var tester = new DecimalPlacesAttributeTester(invoice, invoice.Company);
			tester.CheckExchangeRate(exList, nameof(invoice.ExchangeRateDecimalPlaces));
		}

		public override void TestBizObjectFields()
		{
			Factory.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				base.TestBizObjectFields();
			}
			finally
			{
				Factory.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
		}
	}
}

using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Transactions.Invoicing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromWhsInvoice))]
	sealed class FreightWrapperFromWhsInvoiceTest : FreightWrapperTest
	{
		public void TestStorageFromStorageToAndBillingDates()
		{
			var invoice = Factory.New<WhsInvoice>();
			var year = ZDateTime.Now.Year;
			invoice.ET_BillingDate = new ZDateTime(year, 1, 1);
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 3, 1);

			var wrapper = FreightWrapper.New(invoice, Factory)[0];

			AssertEquals("BillingDate", new ZDateTime(year, 1, 1).ToString(), wrapper.BillingDate.ToString());
			AssertEquals("StorageFromDate", new ZDateTime(year, 2, 1).ToString(), wrapper.StorageFromDate.ToString());
			AssertEquals("StorageToDate", new ZDateTime(year, 3, 1).ToString(), wrapper.StorageToDate.ToString());
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "BillingDate", ZDateTime.Today.ToString() },
					{ "StorageToDate", ZDateTime.Today.ToString() }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
WarehouseJob : (No Default Field Value Available on WarehouseJob)";
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<WhsInvoice>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var invoice = Factory.New<WhsInvoice>();
			return new FreightWrapperFromWhsInvoice(invoice, Factory);
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}
	}
}

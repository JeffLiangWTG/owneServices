using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Warehouse.Testing
{
	[TestedType(typeof(DocWhsInvoice))]
	sealed class DocWhsInvoiceTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { InvoiceWrapper };
		}

		public void TestInvoiceNumber()
		{
			Invoice.ET_StorageJobNumber = "1234";
			AssertEquals("1234", InvoiceWrapper.InvoiceNumber);
		}

		public void TestStorageFromDate()
		{
			ZDateTime date = ZDateTime.Today.AddDays(-1);
			Invoice.ET_StorageFromDate = date;
			AssertEquals(date, InvoiceWrapper.StorageFromDate);
		}

		public void TestStorageToDate()
		{
			ZDateTime date = ZDateTime.Today.AddDays(1);
			Invoice.ET_StorageToDate = date;
			AssertEquals(date, InvoiceWrapper.StorageToDate);
		}

		public void TestWarehouseNameAndAddress()
		{
			if (Invoice.Warehouse == null)
			{
				Invoice.ET_WW = Factory.New(typeof(WhsWarehouse)).PK;
			}

			Invoice.Warehouse.WW_WarehouseName = "Warehouse One";

			var address1 = Factory.NewWithValidTestData<OrgAddress>();

			address1.OA_Address1 = "Address Line 1";
			address1.OA_Address2 = "Address Line 2";
			address1.OA_City = "SYDNEY";
			address1.OA_PostCode = "2000";

			Invoice.Warehouse.WW_OA_WarehouseAddress = address1.PK;

			ZString expectedResult = "Warehouse One\r\nHEADER\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY 2000";

			AssertEquals(expectedResult, InvoiceWrapper.WarehouseNameAndAddress);
		}

		public void TestWarehouseNameAndAddress_Translatable()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Warehouse One";
			Invoice.ET_WW = warehouse.PK;

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Address1 = "Address Line 1";
			address1.OA_Address2 = "Address Line 2";
			address1.OA_City = "SYDNEY";
			address1.OA_PostCode = "2000";

			warehouse.WW_OA_WarehouseAddress = address1.PK;

			var expectedResult = "Warehouse One\r\nHEADER\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY 2000";
			AssertEquals("WarehouseNameAndAddress in English", expectedResult, InvoiceWrapper.WarehouseNameAndAddress);

			var multiLingualString = warehouse.WW_WarehouseNameMultilingual;
			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "Warehouse One").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "仓库1"));
				expectedResult = "仓库1\r\nHEADER\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY 2000";
				AssertEquals("WarehouseNameAndAddress in Chinese", expectedResult, InvoiceWrapper.WarehouseNameAndAddress);
			}
		}

		#region Implementation

		WhsInvoice Invoice
		{
			get
			{
				if (invoice == null)
				{
					invoice = Factory.New<WhsInvoice>();
				}
				return invoice;
			}
		}
		WhsInvoice invoice;

		DocWhsInvoice InvoiceWrapper
		{
			get
			{
				if (invoiceWrapper == null)
				{
					invoiceWrapper = DocWhsInvoice.New(Invoice, Factory);
					AssertNotNull("Wrapper not null", InvoiceWrapper);
				}
				return invoiceWrapper;
			}
		}

		DocWhsInvoice invoiceWrapper;

		#endregion
	}
}

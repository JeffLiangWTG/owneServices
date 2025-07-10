using System;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class BondedWarehouseTransactionLineTestCase : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestCustomsQtyAndUQ()
		{
			Creator.InvoiceLine1.JI_CustomsUnitQty = "KG";
			Creator.InvoiceLine1.JI_CustomsQuantity = 5;
			AssertEquals("CustomsQty", new ZDecimal(5), new BondedWarehouseTransactionLineForTest(Creator.EntryLine1).CustomsQuantity);
			AssertEquals("CustomsQtyUQ", "KG", new BondedWarehouseTransactionLineForTest(Creator.EntryLine1).CustomsQuantityUnit);
			Creator.InvoiceLine1.AddInfo.ZA_WRU = "ML";
			Creator.InvoiceLine1.AddInfo.ZA_WRQ = 50;
			AssertEquals("CustomsQty", new ZDecimal(50), new BondedWarehouseTransactionLineForTest(Creator.EntryLine1).CustomsQuantity);
			AssertEquals("CustomsQtyUQ", "ML", new BondedWarehouseTransactionLineForTest(Creator.EntryLine1).CustomsQuantityUnit);
		}

		public void TestWarehouse()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
			Creator.Declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			Creator.Declaration.WarehouseDocAddress.E2_OA_Address = ZGuid.Empty;
			Creator.InvoiceLine1.AddInfo.ZA_OA_WarehouseAddress_Hidden = ZGuid.Empty;
			AssertNull(new BondedWarehouseTransactionLineForTest(Creator.EntryLine1).Warehouse);

			Creator.Declaration.WarehouseDocAddress.E2_OA_Address = OrgHeader.New(Factory).MainAddress.PK;
			AssertEquals(Creator.Declaration.WarehouseAddress, new BondedWarehouseTransactionLineForTest(Creator.EntryLine1).Warehouse);

			Creator.InvoiceLine1.AddInfo.ZA_OA_WarehouseAddress_Hidden = OrgHeader.New(Factory).MainAddress.PK;
			AssertEquals(Creator.InvoiceLine1.AddInfo.WarehouseAddress, new BondedWarehouseTransactionLineForTest(Creator.EntryLine1).Warehouse);
		}

		public void TestTILV()
		{
			Creator.InvoiceLine1.AddInfo.ZA_TILV = "23AUD";
			AssertEquals("TILV Amount", 23m, new BondedWarehouseTransactionLineForTest(Creator.EntryLine1).TILV.Amount);
			AssertEquals("TILV Currency", "AUD", new BondedWarehouseTransactionLineForTest(Creator.EntryLine1).TILV.Currency.Code);
		}

		public void TestCountryOfOriginComesFromHeaderIfNotOnLine()
		{
			Creator.InvoiceLine1.AddInfo.ZA_ORG = "";
			Creator.InvoiceLine1.InvoiceHeader.ZA_ORG = "FR";
			AssertEquals("FR", new BondedWarehouseTransactionLineForTest(Creator.EntryLine1).CountryOfOrigin.RN_Code);
		}

		public void TestGetEntryDateForWEA()
		{
			Creator.Declaration.JE_DateOfFirstArrival = new ZDateTime(2002, 2, 2);
			AssertEquals(new ZDateTime(2002, 2, 2), new BondedWarehouseTransactionLineForTest(Creator.EntryLine1).GetEntryDateForWEA());
		}

		public void TestGetEntryKeyFromInvoiceLine()
		{
			Creator.InvoiceLine1.AddInfo.ZA_WRN = "345ABC";
			AssertEquals("345ABC", new BondedWarehouseTransactionLineForTest(Creator.EntryLine1).GetEntryKeyFromInvoiceLine());
		}

		public void TestGetEntryLineNumberFromInvoiceLine()
		{
			Creator.InvoiceLine1.AddInfo.ZA_WRL = 23;
			AssertEquals((ZShort)23, new BondedWarehouseTransactionLineForTest(Creator.EntryLine1).GetEntryLineNumberFromInvoiceLine());
		}

		public void TestAddInfoString()
		{
			BondedWarehouseTransactionLineForTest line = new BondedWarehouseTransactionLineForTest(Creator.InvoiceLine1);

			Creator.InvoiceLine1.InvoiceHeader.ZA_ORG = "FR";
			Creator.InvoiceLine1.AddInfo.ZA_CVD = 5.5m;
			Creator.InvoiceLine1.AddInfo.ZA_DCX = "AA";
			Creator.InvoiceLine1.InvoiceHeader.AddInfo.ZA_HeaderREL_Hidden = "Y";

			AssertEquals("Has header info included", true, line.AddInfoString.Contains("ORG=FR"));
			AssertEquals("Has line info included", true, line.AddInfoString.Contains("CVD=5.5"));
			AssertEquals("Has line info included", true, line.AddInfoString.Contains("DCX=AA"));
			AssertEquals("Has header info converted", true, line.AddInfoString.Contains("REL_Hidden=Y"));

			Creator.InvoiceLine1.AddInfo.ZA_REL_Hidden = CMRRelatedTransaction.Default.Code;
			AssertEquals("Has header info converted", true, line.AddInfoString.Contains("REL_Hidden=Y"));
			Creator.InvoiceLine1.InvoiceHeader.AddInfo.ZA_HeaderREL_Hidden = "N";
			AssertEquals("Not related", true, line.AddInfoString.Contains("REL_Hidden=DEF"));
			Creator.InvoiceLine1.AddInfo.ZA_REL_Hidden = CMRRelatedTransaction.Yes.Code;
			AssertEquals("Has header info converted", true, line.AddInfoString.Contains("REL_Hidden=Y"));
			Creator.InvoiceLine1.InvoiceHeader.AddInfo.ZA_HeaderREL_Hidden = "Y";
			Creator.InvoiceLine1.AddInfo.ZA_REL_Hidden = CMRRelatedTransaction.No.Code;
			AssertEquals("Not related", true, line.AddInfoString.Contains("REL_Hidden=N"));
		}

		MergedDeclarationCreator Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new MergedDeclarationCreator(Factory);
				}

				return fCreator;
			}
		}
		MergedDeclarationCreator fCreator;
	}

	class BondedWarehouseTransactionLineForTest : BondedWarehouseTransactionLine
	{
		public BondedWarehouseTransactionLineForTest(JobComInvoiceLine line)
			: base(line)
		{
		}

		public BondedWarehouseTransactionLineForTest(CusEntryLine line)
			: base(line)
		{
		}

		new internal ZString AddInfoString => base.AddInfoString;
		new internal ZDateTime GetEntryDateForWEA() => base.GetEntryDateForWEA();
		new internal ZString GetEntryKeyFromInvoiceLine() => base.GetEntryKeyFromInvoiceLine();
		new internal ZShort GetEntryLineNumberFromInvoiceLine() => base.GetEntryLineNumberFromInvoiceLine();
	}
}

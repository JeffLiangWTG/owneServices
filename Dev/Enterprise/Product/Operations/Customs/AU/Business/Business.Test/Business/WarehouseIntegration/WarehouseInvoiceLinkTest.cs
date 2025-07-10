using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class WarehouseInvoiceLinkTest : WarehouseInvoiceLinkTestCase<JobDeclaration>
	{
		public override void TestSetupInvoiceLineFromTransactionLine()
		{
			JobDeclaration dec = JobDeclaration.New(Factory);
			dec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			JobComInvoiceLine invoiceLine = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var mock = new Mock<IWhsBondedWarehouseTransactionLine>();
			mock.Setup(m => m.EntryLineNumber).Returns(new ZShort((short)10));
			mock.Setup(m => m.EntryKey).Returns(new ZString("abc"));
			mock.Setup(m => m.ValueForDuty).Returns(new ZDecimal(1m));
			mock.Setup(m => m.Quantity).Returns(new ZDecimal(5m));
			mock.Setup(m => m.QuantityUnit).Returns(new ZString("NO"));
			mock.Setup(m => m.AddInfo).Returns(new ZString("VAN=CAR*IsPackToBondForLine_Hidden=Y*WRQ=10*WRU=NO"));
			mock.Setup(m => m.BondedWarehouseQuantity).Returns(new ZDecimal(0));
			mock.Setup(m => m.BondedWarehouseQuantityUnit).Returns(new ZString(""));
			mock.Setup(m => m.CustomsQuantity).Returns(new ZDecimal(15));
			mock.Setup(m => m.CustomsQuantityUnit).Returns(new ZString("KG"));
			mock.Setup(m => m.PartAttrib1).Returns(new ZString(""));
			mock.Setup(m => m.PartAttrib2).Returns(new ZString(""));
			mock.Setup(m => m.PartAttrib3).Returns(new ZString(""));
			mock.Setup(m => m.SerialNumber).Returns(new ZString(""));
			mock.Setup(m => m.TILV).Returns(new Money(54m, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD")));
			ZGuid key = ZGuid.NewZGuid();
			mock.Setup(m => m.UniqueKey).Returns(key);
			OrgAddress warehouse = OrgHeader.New(Factory).MainAddress;
			mock.Setup(m => m.Warehouse).Returns(warehouse);
			IWhsBondedWarehouseTransactionLine transactionLine = mock.Object;

			new WarehouseInvoiceLinkForTest(JobDeclaration.New(Factory)).SetupInvoiceLineFromTransactionLine(transactionLine, invoiceLine);
			AssertEquals("Should not be pack to bond", false, invoiceLine.IsGoingIntoBondedWarehouse);
			AssertEquals("EntryLineNumber", 10, invoiceLine.AddInfo.ZA_WRL);
			AssertEquals("EntryKey", "abc", invoiceLine.AddInfo.ZA_WRN);
			AssertEquals("TILV amount", new ZDecimal(54), invoiceLine.TransportAndInsurance.Amount);
			AssertEquals("TILV currency", "USD", invoiceLine.TransportAndInsurance.Currency.Code);
			CheckContainsCorrectValues(invoiceLine.JI_AddInfo);
			CheckContainsCorrectValues(invoiceLine.AddInfo.ToString());
			AssertEquals("Warehouse", warehouse, invoiceLine.AddInfo.WarehouseAddress);
			AssertEquals("InvoiceQty", 5m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("InvoiceUQ", "NO", invoiceLine.JI_InvoiceUQ);
			AssertEquals("WRQ", 15m, invoiceLine.AddInfo.ZA_WRQ); // since there is no UQ on the tariff
			AssertEquals("WRU", "KG", invoiceLine.AddInfo.ZA_WRU);
			AssertEquals("BondedLineKey", key, invoiceLine.JI_BondedWarehouseLineKey);
		}

		void CheckContainsCorrectValues(ZString toString)
		{
			AssertEquals("AddInfo", true, toString.Contains("VAN=CAR"));
			AssertEquals("AddInfo", true, toString.Contains("WRN=abc"));
			AssertEquals("AddInfo", true, toString.Contains("WRL=10"));
			AssertEquals("AddInfo", true, toString.Contains("WRU=KG"));
			AssertEquals("AddInfo", true, toString.Contains("WRQ=15"));
		}

		public override void TestEntryKeyTitle()
		{
			AssertEquals("WRN-WRL", new WarehouseInvoiceLinkForTest(JobDeclaration.New(Factory)).EntryKeyTitle);
		}
	}

	class WarehouseInvoiceLinkForTest : WarehouseInvoiceLink
	{
		public WarehouseInvoiceLinkForTest(JobDeclaration dec) : base(dec)
		{
		}

		new internal void SetupInvoiceLineFromTransactionLine(IWhsBondedWarehouseTransactionLine transactionLine, Customs.Business.BaseJobComInvoiceLine invoiceLine) => base.SetupInvoiceLineFromTransactionLine(transactionLine, invoiceLine);
	}
}

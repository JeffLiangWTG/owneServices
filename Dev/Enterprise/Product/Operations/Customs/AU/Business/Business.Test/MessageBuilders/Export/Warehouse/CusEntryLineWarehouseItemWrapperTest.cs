using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusEntryLineWarehouseItemWrapperTest : TestCaseWithFactory
	{
		public void TestAHECCCode()
		{
			AssertEquals("24021002", wrapper.AHECCCode);
		}

		public void TestNetQuantity()
		{
			AssertEquals(300m, wrapper.NetQuantity);
		}

		public void TestNetQuantityUnit()
		{
			AssertEquals("KG", wrapper.NetQuantityUnit);
		}

		public void TestGoodsDescription()
		{
			AssertEquals("Stuff", wrapper.GoodsDescription);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = header.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2402.10.02";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 300m;
			invoiceLine.JI_Description = "Stuff";

			declaration.DoMerge();

			wrapper = new CusEntryLineWarehouseItemWrapper(declaration.EntryHeader.MergedLines[0]);
		}

		WarehouseItemWrapper wrapper;
	}
}

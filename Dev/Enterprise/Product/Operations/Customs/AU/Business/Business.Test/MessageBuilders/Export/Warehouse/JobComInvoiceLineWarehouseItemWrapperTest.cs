using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class JobComInvoiceLineWarehouseItemWrapperTest : TestCaseWithFactory
	{
		public void TestAHECCCode()
		{
			AssertEquals("2402.10.02", wrapper.AHECCCode);
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

		WarehouseItemWrapper wrapper;
		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = header.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2402.10.02";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 300m;
			invoiceLine.JI_Description = "Stuff";

			wrapper = new JobComInvoiceLineWarehouseItemWrapper(invoiceLine);
		}
	}
}

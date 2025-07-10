using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsBillCustomsEntryIntegratorTest : TestCaseWithFactory
	{
		public void TestDoNotCopySupplementaryQuantity()
		{
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsSecondQuantity = 100.123456m;
			invoiceLine1.JI_CustomsSecondUnitQty = "ASV";
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsSecondQuantity = 150m;
			invoiceLine2.JI_CustomsSecondUnitQty = "ASV";

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			CombineAssertions(() =>
			{
				var goodsItem = houseConsignment.GoodsItems[0];
				AssertEquals("BY_CustomsSecondQuantity", 0m, goodsItem.BY_CustomsSecondQuantity);
				AssertEquals("BY_CustomsSecondUnitQty", "", goodsItem.BY_CustomsSecondUnitQty);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			houseConsignment = nctsHeader.Bills.AddNew();

			customsEntryIntegrator = new NctsBillCustomsEntryIntegrator(houseConsignment);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		NctsBill houseConsignment;
		EU.NCTS.Business.INctsCustomsEntryIntegrator customsEntryIntegrator;
	}
}

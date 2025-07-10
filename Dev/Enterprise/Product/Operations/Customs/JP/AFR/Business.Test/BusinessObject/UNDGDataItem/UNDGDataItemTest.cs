using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(UNDGDataItem))]
	sealed class UNDGDataItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBill()
		{
			var item = GetNewBusinessObject() as UNDGDataItem;
			AssertNotNull(item.Bill);
		}

		public void TestDI_Description()
		{
			var undg = DGSubstanceTestHelper.Create("1234", "a", "IMO", (d) =>
			{
				d.DG_PSN = "Test UNDG Substance";
			});

			var item = GetNewBusinessObject() as UNDGDataItem;
			AssertEquals("Default to empty", string.Empty, item.DI_Description);

			item.DI_DG = undg.PK;
			AssertEquals("Test UNDG Substance", item.DI_Description);
		}

		public void TestHumanReadableName()
		{
			var item = GetNewBusinessObject() as UNDGDataItem;
			AssertEquals("UNDG Item", item.HumanReadableName);
		}

		public void TestValidation()
		{
			var item = GetNewBusinessObject() as UNDGDataItem;
			AssertType<UNDGDataItemValidation>(item.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			return bill.UNDGs.AddNew();
		}
	}
}

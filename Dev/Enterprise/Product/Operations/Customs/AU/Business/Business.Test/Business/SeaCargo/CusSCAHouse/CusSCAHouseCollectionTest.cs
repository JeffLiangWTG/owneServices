using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAHouseCollection))]
	sealed class CusSCAHouseCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestByHouseBill()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.HouseBills.AddNew().CA_HouseBill = "A";
			oceanBill.HouseBills.AddNew().CA_HouseBill = "A";
			oceanBill.HouseBills.AddNew().CA_HouseBill = "B";

			AssertEquals(2, oceanBill.HouseBills.ByHouseBill["A"].Count());
			AssertEquals(1, oceanBill.HouseBills.ByHouseBill["B"].Count());

			oceanBill.HouseBills[0].CA_HouseBill = "C";
			AssertEquals(1, oceanBill.HouseBills.ByHouseBill["A"].Count());
			AssertEquals(1, oceanBill.HouseBills.ByHouseBill["B"].Count());
			AssertEquals(1, oceanBill.HouseBills.ByHouseBill["C"].Count());
			AssertEquals(0, oceanBill.HouseBills.ByHouseBill["D"].Count());
		}

		public void TestIndexer()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouseCollection houseBillCollection = new CusSCAHouseCollection(oceanBill, Factory);
			CusSCAHouse houseBill = Factory.New<CusSCAHouse>();
			houseBillCollection.Add(houseBill);
			AssertEquals(houseBill, houseBillCollection[0]);
		}

		public void TestAddNewHouseBillType()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBillCollection = new CusSCAHouseCollection(oceanBill, Factory);
			var houseBill = houseBillCollection.AddNew();
			AssertType(typeof(CusSCAHouse), houseBill);
		}

		public void TestDefaultValues()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_RL_NKPortOfLoading = "SGSIN";
			oceanBill.CB_RL_NKPortOfDischarge = "AUMEL";

			CusSCAHouse house = oceanBill.HouseBills.AddNew();
			AssertEquals("House Bill Origin Default", "SGSIN", house.CA_RL_NK_PortOfOrigin);
			AssertEquals("House Bill Destination Default", "AUMEL", house.CA_RL_NK_PortOfDestination);
			AssertEquals("House Bill Country/Region of Goods Origin", "SG", house.CA_RN_NKGoodsOrigin);
		}

		public void TestRemoveAndDeleteRemovesElementsIfCanBeDeletedIsTrue()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBillCol = new CusSCAHouseCollection(oceanBill, Factory);

			var elementToDelete = Factory.New<CusSCAHouseForTest>();
			elementToDelete.CanDeleteReturns = true;
			houseBillCol.Add(elementToDelete);
			AssertEquals("One item must exist", 1, houseBillCol.Count);
			houseBillCol.RemoveAndDelete(elementToDelete);
			AssertEquals("No item must remain", 0, houseBillCol.Count);
		}

		public void TestRemoveAndDeleteRemovesElementsIfCanBeDeletedIsFalse()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBillCol = new CusSCAHouseCollection(oceanBill, Factory);

			var elementNotToDelete = Factory.New<CusSCAHouseForTest>();
			elementNotToDelete.CanDeleteReturns = false;
			houseBillCol.Add(elementNotToDelete);
			AssertEquals("One item must exist", 1, houseBillCol.Count);
			houseBillCol.RemoveAndDelete(elementNotToDelete);
			AssertEquals("One item must remain", 1, houseBillCol.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			return new CusSCAHouseCollection(oceanBill, Factory);
		}

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}

		sealed class CusSCAHouseForTest : CusSCAHouse
		{
			public CusSCAHouseForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool CanDeleteReturns { get; set; }

			public override bool CanDelete => CanDeleteReturns;
		}
	}
}

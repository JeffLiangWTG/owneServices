using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusSCAPivotCollectionForHouse))]
	sealed class CusSCAPivotCollectionForHouseTest : ActiveBusinessObjectCollectionTestCase<CusSCAPivotCollectionForHouse>
	{
		public void TestIOverrideDefaultValuesCollectionMembers()
		{
			var houseBill = Factory.New<CusSCAOceanBill>().HouseBills.AddNew();
			houseBill.CA_OverrideFreightDefaults = true;
			IOverrideDefaultValuesCollection collection = houseBill.PackLines;
			AssertEquals(false, collection.IsOverrideDefaultValuesEnabled);

			var shipment = Factory.New<ForwardingShipment>();
			houseBill.CA_JS = shipment.PK;
			AssertEquals(true, collection.IsOverrideDefaultValuesEnabled);

			var info = collection.OverrideDefaultValuesInfo;
			AssertEquals("CA_OverrideFreightDefaults", info.Name);
			AssertEquals(true, info.Value);

			houseBill.CA_OverrideFreightDefaults = false;
			AssertEquals(false, info.Value);
		}

		public void TestFindByContainerAndShipmentPackLine()
		{
			var collection = GetCollectionToTest();
			var cusContainer = Factory.New<CusSCAContainer>();
			cusContainer.CN_ContainerNumber = "C1";
			var pivot = collection.AddNew();
			pivot.CV_CN = cusContainer.PK;
			pivot.CV_PackageType = "PLT";
			pivot.CV_GoodsDescription = "DESCRIPTION";
			var undg1 = pivot.UNDGs.AddNew();
			undg1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			var undg2 = pivot.UNDGs.AddNew();
			undg2.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").First().PK;

			var shipmentLine = Factory.New<PackLine>();
			shipmentLine.JL_F3_NKPackType = "PLT";
			shipmentLine.JL_Description = "DESCRIPTION";
			var shipmentUndg1 = shipmentLine.UNDGs.AddNew();
			shipmentUndg1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			var shipmentUndg2 = shipmentLine.UNDGs.AddNew();
			shipmentUndg2.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "b", "IMO").First().PK;

			AssertEquals("Pack Line Found", pivot.PK, collection.FindByContainerAndShipmentPackLine("C1", shipmentLine).PK);
			AssertNull("Pack Line Not Found", collection.FindByContainerAndShipmentPackLine("C2", shipmentLine));
			shipmentUndg2.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0327", "a", "IMO").First().PK;
			AssertNull("Pack Line Not Found", collection.FindByContainerAndShipmentPackLine("C1", shipmentLine));
			shipmentUndg2.Delete();
			AssertNull("Pack Line Not Found", collection.FindByContainerAndShipmentPackLine("C1", shipmentLine));
			undg2.Delete();
			AssertEquals("Pack Line Found", pivot.PK, collection.FindByContainerAndShipmentPackLine("C1", shipmentLine).PK);
			shipmentLine.UNDGs.DeleteAll();
			AssertNull("Pack Line Not Found", collection.FindByContainerAndShipmentPackLine("C1", shipmentLine));
			pivot.UNDGs.DeleteAll();
			AssertEquals("Pack Line Found", pivot.PK, collection.FindByContainerAndShipmentPackLine("C1", shipmentLine).PK);
			shipmentLine.JL_Description = "DESCRIPTION2";
			AssertNull("Pack Line Not Found", collection.FindByContainerAndShipmentPackLine("C1", shipmentLine));
			pivot.CV_GoodsDescription = "DESCRIPTION2";
			AssertEquals("Pack Line Found", pivot.PK, collection.FindByContainerAndShipmentPackLine("C1", shipmentLine).PK);
			shipmentLine.JL_F3_NKPackType = "NMB";
			AssertNull("Pack Line Not Found", collection.FindByContainerAndShipmentPackLine("C1", shipmentLine));
			collection.DeleteAll();
			AssertNull("Pack Line Not Found", collection.FindByContainerAndShipmentPackLine("C1", shipmentLine));
		}

		#region Implementation

		CusSCAHouse cusSCAHouse;
		CusSCAHouse CusSCAHouse
		{
			get
			{
				if (cusSCAHouse == null)
				{
					cusSCAHouse = Factory.New<CusSCAHouse>();
				}
				return cusSCAHouse;
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusSCAPivot>();
		}

		protected override CusSCAPivotCollectionForHouse GetCollectionToTest()
		{
			return new CusSCAPivotCollectionForHouse(CusSCAHouse);
		}

		#endregion
	}
}

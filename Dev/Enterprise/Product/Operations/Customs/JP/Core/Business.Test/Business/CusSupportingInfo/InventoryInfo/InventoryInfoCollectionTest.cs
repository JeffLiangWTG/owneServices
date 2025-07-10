using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(InventoryInfoCollection))]
	sealed class InventoryInfoCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<InventoryInfo>
	{
		protected override CusSupportingInfoCollection<InventoryInfo> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var moveInDestinationInfo = entryInstruction.MoveInDestinationInfos.AddNew();
			return new InventoryInfoCollection(entryInstruction, moveInDestinationInfo.PK);
		}

		public void TestSetDefaultValuesForNewChild_CSI_Quantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var moveInDestinationInfo = entryInstruction.MoveInDestinationInfos.AddNew();
			moveInDestinationInfo.CSI_Quantity = 10;

			var inventoryInfo1 = moveInDestinationInfo.InventoryNumbers.AddNew();
			AssertEquals(inventoryInfo1.CSI_Quantity, 10m);

			inventoryInfo1.CSI_Quantity = 5;
			var inventoryInfo2 = moveInDestinationInfo.InventoryNumbers.AddNew();
			AssertEquals(inventoryInfo2.CSI_Quantity, 5m);

			var inventoryInfo3 = moveInDestinationInfo.InventoryNumbers.AddNew();
			AssertEquals(inventoryInfo3.CSI_Quantity, 0m);

			inventoryInfo3.CSI_Quantity = 3;
			var inventoryInfo4 = moveInDestinationInfo.InventoryNumbers.AddNew();
			AssertEquals(inventoryInfo4.CSI_Quantity, 0m);
		}
	}
}

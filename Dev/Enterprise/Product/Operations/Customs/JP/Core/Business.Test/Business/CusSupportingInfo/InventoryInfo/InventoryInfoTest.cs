using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(InventoryInfo))]
	sealed class InventoryInfoTest : Customs.Business.Testing.CusSupportingInfoTest<InventoryInfo>
	{
		public void TestCSI_Code()
		{
			var inventoryInfo = Factory.New<InventoryInfo>();
			CombineAssertions(() =>
			{
				var resourceStringData = DataBoundResourceStrings.GetDataForProperty(inventoryInfo.CSI_CodeInfo);
				AssertEquals("CSI_Code Caption", "Inventory Management Number", resourceStringData.Caption);
				AssertEquals("CSI_Code MediumCaption", "Number", resourceStringData.MediumCaption);
				AssertEquals("Max Length", 10, inventoryInfo.CSI_CodeInfo.MaxLength);
			});
		}

		public void TestCSI_Quantity()
		{
			var inventoryInfo = Factory.New<InventoryInfo>();
			CombineAssertions(() =>
			{
				var resourceStringData = DataBoundResourceStrings.GetDataForProperty(inventoryInfo.CSI_QuantityInfo);
				AssertEquals("CSI_Quantity Caption", "Move-In Quantity", resourceStringData.Caption);
				AssertEquals("CSI_Quantity MediumCaption", "Quantity", resourceStringData.MediumCaption);
			});
		}

		public void TestSetDefaultValues()
		{
			var inventoryInfo = Factory.New<InventoryInfo>();
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Type", CusSupportingInfoTypeList.Codes.Inventory, inventoryInfo.CSI_Type);
				AssertEquals("CSI_ParentTableCode", CusEntryInstructionSchema.Constants.Prefix, inventoryInfo.CSI_ParentTableCode);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetBizObjsForCorrectlyTypeDecideTest(Factory).First();
		}

		protected override IEnumerable<InventoryInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var entryInstruction = factory.NewWithValidTestData<CusEntryInstruction>();
			yield return entryInstruction.MoveInDestinationInfos.AddNew().InventoryNumbers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var entryInstruction = factory.NewWithValidTestData<CusEntryInstruction>();
			var moveInDestinationInfo = entryInstruction.MoveInDestinationInfos.AddNew();
			moveInDestinationInfo.CSI_DataModel = "JP";
			var inventoryNumber = moveInDestinationInfo.InventoryNumbers.AddNew();
			inventoryNumber.CSI_DataModel = "JP";
			return inventoryNumber;
		}
	}
}

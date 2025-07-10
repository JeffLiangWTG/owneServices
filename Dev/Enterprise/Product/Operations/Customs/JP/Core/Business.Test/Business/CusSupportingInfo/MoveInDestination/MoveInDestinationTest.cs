using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(MoveInDestination))]
	sealed class MoveInDestinationTest : Customs.Business.Testing.CusSupportingInfoTest<MoveInDestination>
	{
		public void TestDelete()
		{
			var inventoryNumber1 = MoveInDestination.InventoryNumbers.AddNew();
			var inventoryNumber2 = MoveInDestination.InventoryNumbers.AddNew();
			var inventoryNumber3 = MoveInDestination.InventoryNumbers.AddNew();
			MoveInDestination.Delete();
			CombineAssertions(() =>
			{
				Assert("inventoryNumber1", inventoryNumber1.IsDeleted);
				Assert("inventoryNumber2", inventoryNumber2.IsDeleted);
				Assert("inventoryNumber3", inventoryNumber3.IsDeleted);
			});
		}

		public void TestCSI_Code()
		{
			MoveInDestination.CSI_CodeInfo.AssertHumanReadableNameAndMaxLength("Destination", 5);
		}

		public void TestCSI_DateOfIssue()
		{
			MoveInDestination.CSI_DateOfIssueInfo.AssertHumanReadableNameAndMaxLength("Date");
		}

		public void TestCSI_ReferenceNumber()
		{
			MoveInDestination.CSI_ReferenceNumberInfo.AssertHumanReadableNameAndMaxLength("Via", 5);
		}

		public void TestCSI_Quantity()
		{
			MoveInDestination.CSI_QuantityInfo.AssertHumanReadableNameAndMaxLength("Quantity");
		}

		public void TestCSI_Quantity2()
		{
			MoveInDestination.CSI_Quantity2Info.AssertHumanReadableNameAndMaxLength("Weight");
		}

		public void TestCSI_Quantity3()
		{
			MoveInDestination.CSI_Quantity3Info.AssertHumanReadableNameAndMaxLength("Volume");
		}

		public void TestCSI_Description()
		{
			MoveInDestination.CSI_DescriptionInfo.AssertHumanReadableNameAndMaxLength("Marks & Numbers");
		}

		public void TestInventoryNumbers()
		{
			var inventoryNumber = MoveInDestination.InventoryNumbers.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Collection Count", 1, MoveInDestination.InventoryNumbers.Count);
				AssertEquals("CSI_CSI_SupportingInfo", MoveInDestination.PK, inventoryNumber.CSI_CSI_SupportingInfo);
				AssertSame("Parent", MoveInDestination.Parent, inventoryNumber.Parent);
			});
		}

		MoveInDestination MoveInDestination => moveInDestination ??= GetNewBusinessObject() as MoveInDestination;
		MoveInDestination moveInDestination;

		protected override BusinessObject GetNewBusinessObject()
		{
			var entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			return entryInstruction.MoveInDestinationInfos.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var entryInstruction = factory.NewWithValidTestData<CusEntryInstruction>();
			return entryInstruction.MoveInDestinationInfos.AddNew();
		}

		protected override IEnumerable<MoveInDestination> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var entryInstruction = factory.NewWithValidTestData<CusEntryInstruction>();
			yield return entryInstruction.MoveInDestinationInfos.AddNew();
		}
	}
}

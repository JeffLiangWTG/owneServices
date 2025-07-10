using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsContainerItem))]
	sealed class NctsContainerItemTest : Customs.Business.Testing.CusCodeDataTest<NctsContainerItem>
	{
		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CY_Type", CusCodeDataTypeList.Codes.ItemNumber, itemNumber.CY_Type);
				AssertEquals("CY_Code", CusCodeDataTypeList.Codes.ItemNumber, itemNumber.CY_Code);
			});
		}

		public void TestGetNewLookups()
		{
			AssertType<CusCodeDataLookups>(itemNumber.Lookups);
		}

		public void TestCY_DataNumeric_ReadOnly()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_ExportFlag = EventFlagList.Codes.Yes;
			itemNumber = nctsHeader.EnRouteIncidents.AddNew().IncidentContainers.AddNew().ItemNumbers.AddNew();
			NCTSTestHelper.AssertArrivalNotificationReadOnlyForLockedDeclaration(itemNumber.CY_DataNumericInfo, nctsHeader);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			nctsHeader = factory.New<NctsHeader>();
			var container = nctsHeader.EnRouteIncidents.AddNew().IncidentContainers.AddNew();
			return container.ItemNumbers.AddNew();
		}

		protected override void SetUp()
		{
			itemNumber = (NctsContainerItem)GetNewBusinessObjectForDeleteTest(Factory);
		}
		NctsContainerItem itemNumber;
		NctsHeader nctsHeader;
	}
}

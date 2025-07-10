using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusSCAContainer))]
	sealed class CusSCAContainerBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookupsAndValidationType()
		{
			var container = Factory.New<CusSCAContainer>();
			AssertEquals(typeof(CusSCAContainerLookups), container.Lookups.GetType());
			AssertEquals(typeof(CusSCAContainerValidation), container.Validation.GetType());
		}

		public void TestReadOnlyProperties()
		{
			var container = Factory.New<CusSCAContainer>();
			Assert(!container.CN_RN_NKCountryOfRegistrationInfo.ReadOnly);
			Assert(!container.CN_ContainerSizeOrISOCodeInfo.ReadOnly);
			Assert(!container.CN_ContainerNumberInfo.ReadOnly);
			Assert(!container.CN_ContainerModeInfo.ReadOnly);
			Assert(!container.CN_RC_NKContainerTypeInfo.ReadOnly);
			container.CN_TypeOfContainer = CusSCAHouse.NonContaineriseID;
			Assert(container.CN_RN_NKCountryOfRegistrationInfo.ReadOnly);
			Assert(container.CN_ContainerSizeOrISOCodeInfo.ReadOnly);
			Assert(container.CN_ContainerNumberInfo.ReadOnly);
			Assert(container.CN_ContainerModeInfo.ReadOnly);
			Assert(container.CN_RC_NKContainerTypeInfo.ReadOnly);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var cusSCAOceanBill = Factory.New<CusSCAOceanBill>();
			var cusSCAContainer = Factory.NewWithValidTestData<CusSCAContainer>();
			cusSCAContainer.CN_CB = cusSCAOceanBill.PK;
			return cusSCAContainer;
		}

		#endregion

		public void TestSettingTypeSetsISOCode()
		{
			CusSCAContainer container = Factory.New<CusSCAContainer>();
			container.CN_RC_NKContainerType = "40GP";
			AssertEquals("42G0", container.CN_ContainerSizeOrISOCode);
		}

		public void TestDeletingContainerDeletesAllPivots()
		{
			var helper = new CusSCATestHelper();
			var container1 = helper.Container1;
			var house = helper.House;
			var packLine1 = helper.PackLine1;
			var packLine2 = helper.PackLine2;
			AssertEquals("Two containers on Ocean Bill (including NON)", 1, helper.OceanBill.Containers.Count);
			var cusContainer = helper.OceanBill.Containers[0];

			AssertEquals("Correct container", CusSCATestHelper.Container1Num, cusContainer.CN_ContainerNumber);
			AssertEquals("Container Ocean Bill OK", cusContainer.OceanBill, helper.OceanBill);
			AssertNotNull("Container Ocean Bill OK", cusContainer.OceanBill);
			AssertEquals("Two pivots on Ocean Bill", 2, helper.House.PackLines.Count);
			packLine1.CV_CN = cusContainer.PK;
			packLine2.CV_CN = cusContainer.PK;
			AssertEquals("Two pivots on Container", 2, cusContainer.AssociatedPackLines.Count);
			cusContainer.Delete();
			AssertEquals("No container on Ocean Bill", 0, helper.OceanBill.Containers.Count);
			AssertEquals("No pivots on Ocean Bill", 0, helper.House.PackLines.Count);
			Assert("Pivot 1 deleted", packLine1.IsDeleted);
			Assert("Pivot 2 deleted", packLine2.IsDeleted);
		}

		// CusSCAHouse TestSupplementaryCargoReportMessageBuildEndToEnd tests ISCRContainer members
	}
}

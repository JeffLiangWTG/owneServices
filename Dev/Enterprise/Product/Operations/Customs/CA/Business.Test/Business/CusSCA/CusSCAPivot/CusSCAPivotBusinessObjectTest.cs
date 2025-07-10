using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusSCAPivot))]
	sealed class CusSCAPivotBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOldAndNewHouseBillMarkAsNeedingValidation()
		{
			var oldHouseBill = Factory.NewWithValidTestData<CusSCAHouse>();
			var cusSCAPivot = oldHouseBill.PackLines.AddNew();
			var newHouseBill = Factory.NewWithValidTestData<CusSCAHouse>();

			oldHouseBill.MarkLightValidationAsValidForTesting();
			Assert(oldHouseBill.LightValidationIsValid);
			newHouseBill.MarkLightValidationAsValidForTesting();
			Assert(newHouseBill.LightValidationIsValid);

			cusSCAPivot.CV_CA = newHouseBill.PK;
			Assert(!oldHouseBill.LightValidationIsValid);
			Assert(!newHouseBill.LightValidationIsValid);
		}

		public void TestUNDGReadOnlyStatus()
		{
			var house = Factory.New<CusSCAHouse>();
			var pivot = house.PackLines.AddNew();
			house.PackLines.SetUNDGsReadOnly(true);
			Assert(pivot.UNDGs.ReadOnly);
			Assert(pivot.UNDGs.FirstItemForBinding.ReadOnly);
			Assert(pivot.UNDGs.FirstItemForBinding[0].DI_DGInfo.ReadOnly);
		}

		public void TestFieldsReadOnly()
		{
			CusSCATestHelper helper = new CusSCATestHelper();
			helper.Shipment.JS_GoodsDescription = "SHIPMENT DESC";
			var packLine = helper.Shipment.OuterPackLines[0];
			var c1 = helper.Container1;
			packLine.SetContainer(c1.PK);
			packLine.JL_PackageCount = new ZInt(5);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.BaleUncompressed;
			packLine.JL_ActualWeight = 250m;
			packLine.JL_ActualWeightUQ = "KG";
			packLine.JL_ActualVolume = 2.5m;
			packLine.JL_ActualVolumeUQ = "M3";
			packLine.JL_HarmonisedCode = "123456";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "XXXX";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			packLine.UNDGs.AddNew().LinkDefault(subs);
			packLine.JL_DetailedDescription = "DETAILED DESCRIPTION";
			packLine.JL_MarksAndNumbers = "MARKS";
			var house = helper.House;
			var pivot = house.PackLines[0];
			AssertFieldsReadOnly(pivot, true);
			Assert("Should not be able to delete pivot", !((ICanDelete)pivot).CanDelete);
			house.CA_OverrideFreightDefaults = true;
			AssertFieldsReadOnly(pivot, false);
			Assert("Should be able to delete pivot", ((ICanDelete)pivot).CanDelete);
		}

		void AssertFieldsReadOnly(CusSCAPivot pivot, bool shouldBeReadOnly)
		{
			AssertEquals("CV_PackageType", shouldBeReadOnly, pivot.CV_PackageTypeInfo.ReadOnly);
			AssertEquals("CV_WeightUQ", shouldBeReadOnly, pivot.CV_WeightUQInfo.ReadOnly);
			AssertEquals("CV_VolumeUQ", shouldBeReadOnly, pivot.CV_VolumeUQInfo.ReadOnly);
			AssertEquals("CV_Weight", shouldBeReadOnly, pivot.CV_WeightInfo.ReadOnly);
			AssertEquals("CV_Volume", shouldBeReadOnly, pivot.CV_VolumeInfo.ReadOnly);
			AssertEquals("CV_PackageCount", shouldBeReadOnly, pivot.CV_PackageCountInfo.ReadOnly);
			AssertEquals("CV_HarmonisedTariffNums", shouldBeReadOnly, pivot.CV_HarmonisedTariffNumsInfo.ReadOnly);
			AssertEquals("CV_GoodsDescription", shouldBeReadOnly, pivot.CV_GoodsDescriptionInfo.ReadOnly);
			AssertEquals("CV_MarksAndNumbers", shouldBeReadOnly, pivot.CV_MarksAndNumbersInfo.ReadOnly);
			AssertEquals("CV_AssociatedContainer", shouldBeReadOnly, pivot.CV_AssociatedContainerInfo.ReadOnly);
			AssertEquals("UNDGs", shouldBeReadOnly, pivot.UNDGs.ReadOnly);
		}

		public void TestContainerNumberWhenNoContainer()
		{
			var cusSCAContainer = Factory.NewWithValidTestData<CusSCAContainer>();
			cusSCAContainer.CN_ContainerNumber = "CNT001";
			var cusSCAOceanBill = Factory.New<CusSCAOceanBill>();
			cusSCAContainer.CN_CB = cusSCAOceanBill.PK;
			Factory.Save();

			var cusSCAHouse = Factory.New<CusSCAHouse>();
			cusSCAHouse.CA_CB = cusSCAOceanBill.PK;
			var cusSCAPivot = Factory.New<CusSCAPivot>();
			cusSCAPivot.CV_CA = cusSCAHouse.PK;
			cusSCAPivot.CV_CN = cusSCAContainer.PK;
			Factory.Save();

			AssertEquals("AssociatedContainer", "CNT001", cusSCAPivot.CV_AssociatedContainer);

			cusSCAPivot.CV_CN = ZGuid.Empty;
			AssertNull(cusSCAPivot.Container);
			AssertEquals("AssociatedContainer", "NCT", cusSCAPivot.CV_AssociatedContainer);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();

			CusSCAContainer container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "1";
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
			CusSCAPivot houseContainerPivot = houseBill.PackLines.AddNew();
			houseContainerPivot.CV_CN = container.PK;
			AssertNotNull(houseContainerPivot);
			return houseContainerPivot;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		#endregion

	}
}

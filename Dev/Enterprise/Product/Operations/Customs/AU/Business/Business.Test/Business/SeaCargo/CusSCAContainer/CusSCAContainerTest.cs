using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAContainer))]
	class CusSCAContainerTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			return oceanBill.Containers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		[ExpectNoExceptions]
		public void TestOceanBill_ScaContainerDeleted()
		{
			var ocean = Factory.New<CusSCAOceanBill>();
			var container = ocean.Containers.AddNew();
			var house = ocean.HouseBills.AddNew();
			var pivot = container.Pivots.AddNew();
			pivot.CV_AssociatedHouse = house.CA_HouseBill;

			container.Delete();
			Assert(pivot.IsDeleted);
		}

		public void TestContainerCase()
		{
			CusSCAContainer container = Factory.New<CusSCAContainer>();
			container.CN_ContainerNumber = "crxu1234567";
			AssertEquals("Lowercase should be forced to uppercase", "CRXU1234567", container.CN_ContainerNumber);
		}

		public void TestOceanBill()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			AssertEquals(oceanBill, container.OceanBill);
		}

		public void TestPivots()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();

			AssertEquals("PreCondition", 0, container.Pivots.Count);
			CusSCAPivot pivot1 = container.Pivots.AddNew();
			AssertEquals(1, container.Pivots.Count);
			CusSCAPivot pivot2 = container.Pivots.AddNew();
			AssertEquals(2, container.Pivots.Count);
		}

		public void TestContainerReadOnly()
		{
			CusSCAContainer container = Factory.New<CusSCAContainer>();
			AssertNotNull("Address control is lazy loaded now", container.CN_OA_UnderbondFrom_ZAddress);
			AssertNotNull("Address control is lazy loaded now", container.CN_OA_UnderbondTo_ZAddress);
			AssertEquals("Container Mode is not readonly", false, container.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("Container Number is not readonly", false, container.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("Container Move Underbond From is not readonly", false, container.CN_MoveUnderbondFromInfo.ReadOnly);
			AssertEquals("Container Move Underbond To is not readonly", false, container.CN_MoveUnderbondToInfo.ReadOnly);
			AssertEquals("Container Underbond From Key is readonly due to the Address Controller", true, container.CN_OA_UnderbondFromInfo.ReadOnly);
			AssertEquals("Container Underbond To Key is readonly due to the Address Controller", true, container.CN_OA_UnderbondToInfo.ReadOnly);
			AssertEquals("Container Container Type is readonly due to the Address Controller", false, container.CN_RC_NKContainerTypeInfo.ReadOnly);
			AssertEquals("Container Underbond Vessel is not readonly", true, container.CN_UnderbondVesselNameInfo.ReadOnly);
			AssertEquals("Container Seal Number is not readonly", false, container.CN_SealNumberInfo.ReadOnly);
			AssertEquals("Container Shipper Owned is not readonly", false, container.CN_ShipperOwnedContainerInfo.ReadOnly);
			AssertEquals("Container Time Up Underbond is not readonly", false, container.CN_TimeupUnderbondMoveInfo.ReadOnly);
			AssertEquals("Container Underbond Status is readonly", true, container.CN_UnderbondStatusInfo.ReadOnly);
			AssertEquals("Container Underbond By Sea Voyage is readonly", true, container.CN_UnderbondVoyageInfo.ReadOnly);
			container.CN_UnderbondBySea = true;
			AssertEquals("Container Underbond Vessel is readonly", false, container.CN_UnderbondVesselNameInfo.ReadOnly);
			AssertEquals("Container Underbond Status is readonly", false, container.CN_UnderbondVoyageInfo.ReadOnly);
		}

		public void TestContainerReadOnly_OverrideFreightDefaults()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var consol = Factory.New<ForwardingConsol>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var container = oceanBill.Containers.AddNew();

			AssertEquals("Container Number is readonly", true, container.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("Container Mode is readonly", true, container.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("Container Type is readonly", true, container.CN_RC_NKContainerTypeInfo.ReadOnly);
			AssertEquals("Container Seal Number is readonly", true, container.CN_SealNumberInfo.ReadOnly);

			container.CN_ContainerNumber = CusSCAPivot.Bulk;
			AssertEquals("Container Number is readonly", true, container.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("Container Mode is not readonly", false, container.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("Container Type is not readonly", false, container.CN_RC_NKContainerTypeInfo.ReadOnly);
			AssertEquals("Container Seal Number is not readonly", false, container.CN_SealNumberInfo.ReadOnly);

			container.CN_ContainerNumber = CusSCAPivot.BreakBulk;
			AssertEquals("Container Number is readonly", true, container.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("Container Mode is not readonly", false, container.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("Container Type is not readonly", false, container.CN_RC_NKContainerTypeInfo.ReadOnly);
			AssertEquals("Container Seal Number is not readonly", false, container.CN_SealNumberInfo.ReadOnly);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			container.CN_ContainerNumber = "CN123";
			AssertEquals("Container Number is readonly", true, container.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("Container Mode is readonly", true, container.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("Container Type is readonly", true, container.CN_RC_NKContainerTypeInfo.ReadOnly);
			AssertEquals("Container Seal Number is readonly", true, container.CN_SealNumberInfo.ReadOnly);

			oceanBill.OverrideFreightDefaults = true;
			AssertEquals("Container Number is not readonly", false, container.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("Container Mode is not readonly", false, container.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("Container Type is not readonly", false, container.CN_RC_NKContainerTypeInfo.ReadOnly);
			AssertEquals("Container Seal Number is not readonly", false, container.CN_SealNumberInfo.ReadOnly);

			container.CN_ContainerNumber = CusSCAPivot.Bulk;
			AssertEquals("Container Number is readonly", container is CusSCAContainer, container.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("Container Mode is not readonly", false, container.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("Container Type is not readonly", false, container.CN_RC_NKContainerTypeInfo.ReadOnly);
			AssertEquals("Container Seal Number is not readonly", false, container.CN_SealNumberInfo.ReadOnly);

			container.CN_ContainerNumber = CusSCAPivot.BreakBulk;
			AssertEquals("Container Number is readonly", container is CusSCAContainer, container.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("Container Mode is not readonly", false, container.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("Container Type is not readonly", false, container.CN_RC_NKContainerTypeInfo.ReadOnly);
			AssertEquals("Container Seal Number is not readonly", false, container.CN_SealNumberInfo.ReadOnly);
		}

		public void TestUnderbondReadOnly()
		{
			CusSCAContainer container = Factory.New<CusSCAContainer>();
			AssertEquals("Container Time Up Underbond is not readonly", false, container.CN_TimeupUnderbondMoveInfo.ReadOnly);
			AssertEquals("Container Underbond By Sea is not readonly", false, container.CN_UnderbondBySeaInfo.ReadOnly);

			Assert(!container.CN_UnderbondBySea);
			AssertEquals("Container Underbond Vessel is readonly", true, container.CN_UnderbondVesselNameInfo.ReadOnly);
			AssertEquals("Container Underbond By Sea Voyage is readonly", true, container.CN_UnderbondVoyageInfo.ReadOnly);

			container.CN_UnderbondBySea = true;
			AssertEquals("Container Underbond Vessel is readonly", false, container.CN_UnderbondVesselNameInfo.ReadOnly);
			AssertEquals("Container Underbond Status is readonly", false, container.CN_UnderbondVoyageInfo.ReadOnly);

			AssertEquals("Container Underbond From Key is not readonly", false, container.CN_OA_UnderbondFromInfo.ReadOnly);
			AssertEquals("Container Underbond To Key is not readonly", false, container.CN_OA_UnderbondToInfo.ReadOnly);
			AssertEquals("Container Move Underbond From is not readonly", false, container.CN_MoveUnderbondFromInfo.ReadOnly);
			AssertEquals("Container Move Underbond To is not readonly", false, container.CN_MoveUnderbondToInfo.ReadOnly);

			AssertNotNull("Address control is lazy loaded now", container.CN_OA_UnderbondFrom_ZAddress);
			AssertNotNull("Address control is lazy loaded now", container.CN_OA_UnderbondTo_ZAddress);

			AssertEquals("Container Underbond From Key is readonly", true, container.CN_OA_UnderbondFromInfo.ReadOnly);
			AssertEquals("Container Underbond To Key is readonly", true, container.CN_OA_UnderbondToInfo.ReadOnly);
			AssertEquals("Container Move Underbond From is not readonly", false, container.CN_MoveUnderbondFromInfo.ReadOnly);
			AssertEquals("Container Move Underbond To is not readonly", false, container.CN_MoveUnderbondToInfo.ReadOnly);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.MainAddress;
			container.CN_OA_UnderbondFrom = orgAddress.PK;
			container.CN_OA_UnderbondTo = orgAddress.PK;

			AssertEquals("Container Underbond From Key is not readonly", false, container.CN_OA_UnderbondFromInfo.ReadOnly);
			AssertEquals("Container Underbond To Key is not readonly", false, container.CN_OA_UnderbondToInfo.ReadOnly);
			AssertEquals("Container Move Underbond From is readonly", true, container.CN_MoveUnderbondFromInfo.ReadOnly);
			AssertEquals("Container Move Underbond To is readonly", true, container.CN_MoveUnderbondToInfo.ReadOnly);

			container.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.Bulk;
			AssertEquals("Container Move Underbond From is readonly", true, container.CN_MoveUnderbondFromInfo.ReadOnly);
			AssertEquals("Container Move Underbond To is readonly", true, container.CN_MoveUnderbondToInfo.ReadOnly);
			AssertEquals("Container Time Up Underbond is readonly", true, container.CN_TimeupUnderbondMoveInfo.ReadOnly);
			AssertEquals("Container Underbond By Sea is readonly", true, container.CN_UnderbondBySeaInfo.ReadOnly);
			AssertEquals("Container Underbond Vessel is readonly", true, container.CN_UnderbondVesselNameInfo.ReadOnly);
			AssertEquals("Container Underbond By Sea Voyage is readonly", true, container.CN_UnderbondVoyageInfo.ReadOnly);
			AssertEquals("Container Underbond From Key is readonly", true, container.CN_OA_UnderbondFromInfo.ReadOnly);
			AssertEquals("Container Underbond To Key is readonly", true, container.CN_OA_UnderbondToInfo.ReadOnly);

			container.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("Container Move Underbond From is readonly", true, container.CN_MoveUnderbondFromInfo.ReadOnly);
			AssertEquals("Container Move Underbond To is readonly", true, container.CN_MoveUnderbondToInfo.ReadOnly);
			AssertEquals("Container Time Up Underbond is readonly", true, container.CN_TimeupUnderbondMoveInfo.ReadOnly);
			AssertEquals("Container Underbond By Sea is readonly", true, container.CN_UnderbondBySeaInfo.ReadOnly);
			AssertEquals("Container Underbond Vessel is readonly", true, container.CN_UnderbondVesselNameInfo.ReadOnly);
			AssertEquals("Container Underbond By Sea Voyage is readonly", true, container.CN_UnderbondVoyageInfo.ReadOnly);
			AssertEquals("Container Underbond From Key is readonly", true, container.CN_OA_UnderbondFromInfo.ReadOnly);
			AssertEquals("Container Underbond To Key is readonly", true, container.CN_OA_UnderbondToInfo.ReadOnly);

			container.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.Liquid;
			AssertEquals("Container Move Underbond From is readonly", true, container.CN_MoveUnderbondFromInfo.ReadOnly);
			AssertEquals("Container Move Underbond To is readonly", true, container.CN_MoveUnderbondToInfo.ReadOnly);
			AssertEquals("Container Time Up Underbond is readonly", true, container.CN_TimeupUnderbondMoveInfo.ReadOnly);
			AssertEquals("Container Underbond By Sea is readonly", true, container.CN_UnderbondBySeaInfo.ReadOnly);
			AssertEquals("Container Underbond Vessel is readonly", true, container.CN_UnderbondVesselNameInfo.ReadOnly);
			AssertEquals("Container Underbond By Sea Voyage is readonly", true, container.CN_UnderbondVoyageInfo.ReadOnly);
			AssertEquals("Container Underbond From Key is readonly", true, container.CN_OA_UnderbondFromInfo.ReadOnly);
			AssertEquals("Container Underbond To Key is readonly", true, container.CN_OA_UnderbondToInfo.ReadOnly);

			container.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			AssertEquals("Container Move Underbond From is not readonly", true, container.CN_MoveUnderbondFromInfo.ReadOnly);
			AssertEquals("Container Move Underbond To is not readonly", true, container.CN_MoveUnderbondToInfo.ReadOnly);
			AssertEquals("Container Time Up Underbond is not readonly", false, container.CN_TimeupUnderbondMoveInfo.ReadOnly);
			AssertEquals("Container Underbond By Sea is not readonly", false, container.CN_UnderbondBySeaInfo.ReadOnly);
			AssertEquals("Container Underbond Vessel is not readonly", false, container.CN_UnderbondVesselNameInfo.ReadOnly);
			AssertEquals("Container Underbond By Sea Voyage is not readonly", false, container.CN_UnderbondVoyageInfo.ReadOnly);
			AssertEquals("Container Underbond From Key is not readonly", false, container.CN_OA_UnderbondFromInfo.ReadOnly);
			AssertEquals("Container Underbond To Key is not readonly", false, container.CN_OA_UnderbondToInfo.ReadOnly);

			var oceanBill = Factory.New<CusSCAOceanBill>();
			container.CN_CB = oceanBill.PK;
			AssertEquals("Container Underbond From Key is not readonly", false, container.CN_OA_UnderbondFromInfo.ReadOnly);
			AssertEquals("Container Underbond To Key is not readonly", false, container.CN_OA_UnderbondToInfo.ReadOnly);

			var consol = Factory.New<ForwardingConsol>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			AssertEquals(false, oceanBill.OverrideFreightDefaults);
			AssertEquals("Container Underbond From Key is readonly", true, container.CN_OA_UnderbondFromInfo.ReadOnly);
			AssertEquals("Container Underbond To Key is readonly", true, container.CN_OA_UnderbondToInfo.ReadOnly);

			oceanBill.OverrideFreightDefaults = true;
			AssertEquals("Container Underbond From Key is not readonly", false, container.CN_OA_UnderbondFromInfo.ReadOnly);
			AssertEquals("Container Underbond To Key is not readonly", false, container.CN_OA_UnderbondToInfo.ReadOnly);
		}

		public void TestMessages()
		{
			CusSCAContainer container = Factory.New<CusSCAContainer>();
			AssertNotNull(container.Messages);
		}

		public void TestNullContainerAndHouseBills()
		{
			CusSCAContainer container = Factory.GetNull<CusSCAContainer>();
			AssertEquals("House Bills Readonly state was causing the exception", true, container.ReadOnly);
		}

		public void TestTotalPackages()
		{
			OceanBillWithSingleContainerAndTwoHouseBills.HouseBills[0].Pivot[0].CV_PackageCount = 10;
			OceanBillWithSingleContainerAndTwoHouseBills.HouseBills[0].Pivot[1].CV_PackageCount = 20;
			AssertEquals("TotalPackages", 30, OceanBillWithSingleContainerAndTwoHouseBills.Containers[0].TotalPackages);
		}

		CusSCAOceanBill oceanBillWithSingleContainerAndTwoHouseBills;
		CusSCAOceanBill OceanBillWithSingleContainerAndTwoHouseBills
		{
			get
			{
				if (oceanBillWithSingleContainerAndTwoHouseBills == null)
				{
					oceanBillWithSingleContainerAndTwoHouseBills = Factory.New<CusSCAOceanBill>();
					CusSCAContainer container = oceanBillWithSingleContainerAndTwoHouseBills.Containers.AddNew();
					CusSCAHouse house1 = oceanBillWithSingleContainerAndTwoHouseBills.HouseBills.AddNew();
					CusSCAHouse house2 = oceanBillWithSingleContainerAndTwoHouseBills.HouseBills.AddNew();
					CusSCAPivot pivot1 = house1.Pivot.AddNew();
					CusSCAPivot pivot2 = house1.Pivot.AddNew();
					pivot1.CV_CN = container.PK;
					pivot2.CV_CN = container.PK;
				}
				return oceanBillWithSingleContainerAndTwoHouseBills;
			}
		}

		public void TestMostPrevelantPackageType()
		{
			OceanBillWithSingleContainerAndTwoHouseBills.HouseBills[0].Pivot[0].CV_PackageCount = 10;
			OceanBillWithSingleContainerAndTwoHouseBills.HouseBills[0].Pivot[1].CV_PackageCount = 20;
			OceanBillWithSingleContainerAndTwoHouseBills.HouseBills[0].Pivot[0].CV_PackageType = "BOX";
			OceanBillWithSingleContainerAndTwoHouseBills.HouseBills[0].Pivot[1].CV_PackageType = "CAN";
			AssertEquals("TotalPackages", "CAN", OceanBillWithSingleContainerAndTwoHouseBills.Containers[0].MostPrevelantPackageType);
		}

		public void TestMostPrevelantPackageTypeDeterministic()
		{
			OceanBillWithSingleContainerAndTwoHouseBills.HouseBills[0].Pivot[0].CV_PackageCount = 10;
			OceanBillWithSingleContainerAndTwoHouseBills.HouseBills[0].Pivot[1].CV_PackageCount = 10;
			OceanBillWithSingleContainerAndTwoHouseBills.HouseBills[0].Pivot[0].CV_PackageType = "BOX";
			OceanBillWithSingleContainerAndTwoHouseBills.HouseBills[0].Pivot[1].CV_PackageType = "CAN";
			AssertEquals("TotalPackages", "BOX", OceanBillWithSingleContainerAndTwoHouseBills.Containers[0].MostPrevelantPackageType);
			OceanBillWithSingleContainerAndTwoHouseBills.HouseBills[0].Pivot[0].CV_PackageType = "CAN";
			OceanBillWithSingleContainerAndTwoHouseBills.HouseBills[0].Pivot[1].CV_PackageType = "BOX";
			AssertEquals("TotalPackages", "BOX", OceanBillWithSingleContainerAndTwoHouseBills.Containers[0].MostPrevelantPackageType);
		}

		public void TestDeletePivots()
		{
			CusSCAOceanBill ocean = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = "Gibbiceps";
			CusSCAHouse house = ocean.HouseBills.AddNew();
			house.CA_HouseBill = "Cuckoo Squeaker";
			CusSCAPivot pivot = container.Pivots.AddNew();
			pivot.CV_AssociatedHouse = house.CA_HouseBill;
			pivot.Delete();
			AssertNotNull(container);
		}

		public void TestCN_UnderbondBySea()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			container.CN_UnderbondBySea = ZBool.True;
			var underbondVessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.NotEqual, ZString.Empty));
			container.CN_UnderbondVesselName = underbondVessel.RV_Code;
			container.CN_UnderbondVoyage = TestVoyage;

			AssertEquals("Underbond Vessel", underbondVessel.RV_Code, container.CN_UnderbondVesselName);
			AssertEquals("Underbond Voyage", TestVoyage, container.CN_UnderbondVoyage);
			container.CN_UnderbondBySea = ZBool.False;

			AssertEquals("Underbond Vessel", ZString.Empty, container.CN_UnderbondVesselName);
			AssertEquals("Underbond Voyage", ZString.Empty, container.CN_UnderbondVoyage);

			container.CN_UnderbondBySea = ZBool.True;
			AssertEquals("Underbond Vessel", underbondVessel.RV_Code, container.CN_UnderbondVesselName);
			AssertEquals("Underbond Voyage", TestVoyage, container.CN_UnderbondVoyage);
		}

		public void TestContainerSizeAndTypeReadOnly()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();

			AssertEquals("by default", false, container.CN_ContainerSizeOrISOCodeInfo.ReadOnly);
			AssertEquals("by default", false, container.CN_TypeOfContainerInfo.ReadOnly);

			container.CN_RC_NKContainerType = (Factory.NewWithValidTestData<RefContainer>()).RC_Code;
			AssertEquals("when rc_containertype set", true, container.CN_ContainerSizeOrISOCodeInfo.ReadOnly);
			AssertEquals("when rc_containertype set", true, container.CN_TypeOfContainerInfo.ReadOnly);

			container.CN_RC_NKContainerType = ZString.Empty;
			AssertEquals("when cleared", false, container.CN_ContainerSizeOrISOCodeInfo.ReadOnly);
			AssertEquals("when cleared", false, container.CN_TypeOfContainerInfo.ReadOnly);
		}

		public void TestContainerSize()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();

			container.CN_ContainerSizeOrISOCode = "0000";
			AssertEquals("when 0000", "0000", container.CN_ContainerSizeOrISOCode);
			AssertEquals("when 0000", false, container.CN_ContainerSizeOrISOCodeInfo.ReadOnly);

			container.CN_RC_NKContainerType = GetNewContainer().RC_Code;
			AssertEquals("when containertype set", "2008", container.CN_ContainerSizeOrISOCode);
			AssertEquals("when containertype set", true, container.CN_ContainerSizeOrISOCodeInfo.ReadOnly);

			container.CN_RC_NKContainerType = ZString.Empty;
			AssertEquals("when containertype cleared", ZString.Empty, container.CN_ContainerSizeOrISOCode);
			AssertEquals("when containertype cleared", false, container.CN_ContainerSizeOrISOCodeInfo.ReadOnly);
		}

		public void TestTypeOfContainer()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();

			container.CN_TypeOfContainer = CMRContainerTypes.Codes.RefrigeratedAContainerWhichIsUsedToTransportRefrigeratedCargo;
			AssertEquals("when refr", CMRContainerTypes.Codes.RefrigeratedAContainerWhichIsUsedToTransportRefrigeratedCargo, container.CN_TypeOfContainer);
			AssertEquals("when refr", false, container.CN_TypeOfContainerInfo.ReadOnly);

			container.CN_TypeOfContainer = CMRContainerTypes.Codes.OpenTopAContainerWithNoHardTopUsedToTransportCargoThatWouldNotNormallyFitInsideAConventionalContainer;
			AssertEquals("when otop", CMRContainerTypes.Codes.OpenTopAContainerWithNoHardTopUsedToTransportCargoThatWouldNotNormallyFitInsideAConventionalContainer, container.CN_TypeOfContainer);
			AssertEquals("when otop", false, container.CN_TypeOfContainerInfo.ReadOnly);

			container.CN_RC_NKContainerType = GetNewContainer().RC_Code;
			AssertEquals("when containertype set", CMRContainerTypes.Codes.MafiATypeOfWheeledTrailerOntoWhichCargoIsStrappedForTransportOnAVessel, container.CN_TypeOfContainer);
			AssertEquals("when containertype set", true, container.CN_TypeOfContainerInfo.ReadOnly);

			container.CN_RC_NKContainerType = ZString.Empty;
			AssertEquals("when containertype cleared", ZString.Empty, container.CN_TypeOfContainer);
			AssertEquals("when containertype cleared", false, container.CN_ContainerNumberInfo.ReadOnly);
		}

		public void TestSwitchTypeWhenBulkOrBreakBulkEntered()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();

			container.CN_ContainerNumber = "BULK";
			AssertEquals("when bulk", CMRImportCargoTypes.Codes.Bulk, container.CN_ContainerMode);

			container.CN_ContainerNumber = "BREAK BULK";
			AssertEquals(CMRImportCargoTypes.Codes.BreakBulk, container.CN_ContainerMode);
		}

		public void TestMultiAssociationFlags()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house1 = oceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "HOUSE1";
			house1.CA_ConsigneeName = "Consignee1";
			var house2 = oceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "HOUSE2";
			house2.CA_ConsigneeName = "Consignee2";
			var container = Factory.New<CusSCAContainerForTesting>();
			oceanBill.Containers.Add(container);
			var pivot1 = house1.Pivot.AddNew();
			pivot1.CV_CN = container.PK;
			pivot1.CV_AssociatedHouse = house1.CA_HouseBill;
			var pivot2 = house2.Pivot.AddNew();
			pivot2.CV_CN = container.PK;
			pivot2.CV_AssociatedHouse = house2.CA_HouseBill;
			container.Pivots.Load();

			AssertEquals("IsAssociatedWithMultipleHouseBills should be true for multiple CV_AssociatedHouse", true, container.IsAssociatedWithMultipleHouseBills);
			AssertEquals("IsAssociatedWithMultipleConsignees should be true for multiple CA_ConsigneeName", true, container.IsAssociatedWithMultipleConsignees);

			house1.CA_HouseBill = house2.CA_HouseBill = "HOUSE";
			house1.CA_ConsigneeName = house2.CA_ConsigneeName = "Consignee";
			pivot1.CV_AssociatedHouse = pivot2.CV_AssociatedHouse = "Consignee";
			AssertEquals("IsAssociatedWithMultipleHouseBills should be false for single CV_AssociatedHouse", false, container.IsAssociatedWithMultipleHouseBills);
			AssertEquals("IsAssociatedWithMultipleConsignees should be false for single CA_ConsigneeName", false, container.IsAssociatedWithMultipleConsignees);
		}

		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			var bizO = Factory.New<CusSCAContainerForTesting>();
			Assert("IsAutoLogged", bizO.IsAutoLogged);
		}
		#endregion

		internal class CusSCAContainerForTesting : CusSCAContainer
		{
			public CusSCAContainerForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			protected override bool IsAssociatedWithMultipleHouseBillsCore()
			{
				MultipleHouseBillsCalculated++;
				return base.IsAssociatedWithMultipleHouseBillsCore();
			}

			public int MultipleHouseBillsCalculated { get; set; }

			protected override bool IsAssociatedWithMultipleConsigneesCore()
			{
				MultipleConsigneesCalculated++;
				return base.IsAssociatedWithMultipleConsigneesCore();
			}

			public int MultipleConsigneesCalculated { get; set; }

			new public bool IsAutoLogged => base.IsAutoLogged;
		}

		public void TestRegisteredChildBehavior()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse house1 = oceanBill.HouseBills.AddNew();
			CusSCAHouse house2 = oceanBill.HouseBills.AddNew();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			container.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.LCL;
			container.CN_RC_NKContainerType = "40GP";
			container.CN_ContainerNumber = "FDCU34093930";
			CusSCAPivot pivot1 = house1.Pivot.AddNew();
			pivot1.CV_CN = container.PK;
			AssertEquals("House 1 Pivot Count", 1, house1.Pivot.Count);
			AssertEquals("House 2 Pivot Count", 0, house2.Pivot.Count);
			AssertEquals("Container Pivot Count", 1, container.Pivots.Count);
			CusSCAPivot pivot2 = house2.Pivot.AddNew();
			pivot2.CV_CN = container.PK;
			AssertEquals("House 1 Pivot Count", 1, house1.Pivot.Count);
			AssertEquals("House 2 Pivot Count", 1, house2.Pivot.Count);
			AssertEquals("Container Pivot Count", 2, container.Pivots.Count);
			pivot2.RunPreSaveValidation();
			AssertNoMessageErrors("House 1 should not have any validation errors as no action has been taken upon it", house1);
		}

		#region Old CMRCusSCAContainer Tests - adjusted to use CusSCAContainer

		[StressTest]
		public void TestPerformanceWhenDeleteCusSCAContainers()
		{
			var count = 2_000;
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			var firstContainer = oceanBill.Containers.AddNew();  // at least one container is required for the pivot, this container won't be deleted in the following test.
			var pivot = houseBill.Pivot.AddNew();
			pivot.CV_CN = firstContainer.PK;
			Enumerable.Range(0, count).ForEach(id =>
			{
				var container = oceanBill.Containers.AddNew();
				container.CN_ContainerNumber = $"CTN{id}";
			});

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentTableCode = pivot.TablePrefix;
			underbond.C4_ParentID = pivot.PK;

			Factory.Save();

			AssertEquals("We have only one underbond.", 1, oceanBill.AllUnderbonds.Count);
			AssertEquals("The underbond is linked to container.", pivot.PK, oceanBill.AllUnderbonds[0].C4_ParentID);

			var newFactory = new BusinessObjectFactory();
			var oceanBillInNewFactory = newFactory.Load<CusSCAOceanBill>(oceanBill.PK);
			_ = oceanBillInNewFactory.AllUnderbonds;
			var dbHitCount = newFactory.GetTableHitCount(CusUnderbondSchema.Constants.TableName);
			oceanBillInNewFactory.Containers.SkipWhile(x => x.PK == firstContainer.PK).Cast<CusSCAContainer>().ToList().ForEach(container =>
			{
				container.Delete();
				var newContainer = oceanBill.Containers.AddNew();
			});
			var dbHitCountAgain = newFactory.GetTableHitCount(CusUnderbondSchema.Constants.TableName);
			AssertEquals("Shoud not access the table CusUnderbond cuz no container have underbond linked.", dbHitCount, dbHitCountAgain);
		}

		public void TestUsesTranshipmentPortOnUnderbond()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();

			AssertEquals(true, ((ICusUnderbondDependentCollectionParent)container).UsesTranshipmentPortOnUnderbond);
			oceanBill.CB_RL_NKPortOfDischarge = ZString.Empty;
			AssertEquals(ZString.Empty, ((ICusUnderbondDependentCollectionParent)container).DefaultTranshipmentPort);
			oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			AssertEquals("NZAKL", ((ICusUnderbondDependentCollectionParent)container).DefaultTranshipmentPort);

			var underbond = container.Underbonds.AddNew();
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			AssertEquals(ZString.Empty, underbond.C4_RL_NKTranshipDestPort);
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
			AssertEquals(ZString.Empty, underbond.C4_RL_NKTranshipDestPort);
			oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			AssertEquals("NZAKL", underbond.C4_RL_NKTranshipDestPort);
			underbond.C4_RL_NKTranshipDestPort = ZString.Empty;
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
			AssertEquals(ZString.Empty, underbond.C4_RL_NKTranshipDestPort);
		}

		[ExpectNoExceptions]
		public void TestCN_TypeOfContainerLength()
		{
			Env.Registry.AUCustomsImportsMessagingMode = "DEF";
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			container.CN_TypeOfContainer = "GENN";
			AssertEquals("GENN", container.CN_TypeOfContainer);
		}

		public void TestIsBulk()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();

			AssertEquals("by default", false, container.IsBulk);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.Bulk;
			AssertEquals("when bulk", true, container.IsBulk);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			AssertEquals("when lcl", false, container.IsBulk);
		}

		public void TestCanSendWithoutDelay()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			Assert("Should be delayed because No CARSTS were found", !((ICusUnderbondDependentCollectionParent)container).CanSendWithoutDelay);
			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			container.Messages.Add(message);
			Assert("Should not be delayed due to carst.", ((ICusUnderbondDependentCollectionParent)container).CanSendWithoutDelay);
		}

		public void TestIsBreakBulk()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();

			AssertEquals("by default", false, container.IsBreakBulk);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.BreakBulk;
			AssertEquals("when break bulk", true, container.IsBreakBulk);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			AssertEquals("when lcl", false, container.IsBreakBulk);
		}

		public void TestContainerNumberBulkOrBreakBulkInCMR()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "123456";

			AssertEquals("by default", "123456", container.CN_ContainerNumber);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.Bulk;
			AssertEquals("when bulk", CMRImportCargoTypes.Descriptions.Bulk.ToUpper(), container.CN_ContainerNumber);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.BreakBulk;
			AssertEquals("when break bulk", CMRImportCargoTypes.Descriptions.BreakBulk.ToUpper(), container.CN_ContainerNumber);

			container.CN_ContainerMode = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertEquals("when fcl", ZString.Empty, container.CN_ContainerNumber);
		}

		public void TestCN_ContainerMode_List()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oceanBill.Containers.AddNew();

			AssertEquals("FreightAllKinds is not in the list", false, container.CN_ContainerMode_List.ContainsCode(Core.Constants.ContainerModes.FreightAllKind));
			AssertEquals("B/B is in the list", true, container.CN_ContainerMode_List.ContainsCode(CusSCAContainer.BreakBulkCodeForCMR));
			AssertEquals("BLK is in the list", true, container.CN_ContainerMode_List.ContainsCode(CusSCAContainer.BLK));
		}

		public void TestValidation()
		{
			CusSCAOceanBill oB = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oB.Containers.AddNew();
			AssertEquals("Validation", typeof(CusSCAContainerValidation), container.Validation.GetType());
		}

		public void TestBreakBulkContainerMode()
		{
			CusSCAOceanBill oB = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = oB.Containers.AddNew();
			container.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("Container Mode should have converted", CMRImportCargoTypes.Codes.BreakBulk, container.CN_ContainerMode);
		}

		public void TestCanBeDeleted()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = cMROceanBill.Containers.AddNew();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			CusSCAPivot pivot = container.Pivots.AddNew();

			AssertEquals("New Pivot should be able to be deleted", true, pivot.CanDelete);
			AssertEquals("New Container should be able to be deleted", true, container.CanDelete);

			CusUnderbond pivotUnderbond = pivot.Underbonds.AddNew();
			AssertEquals("Underbond count", 1, pivot.Underbonds.Count);
			AssertEquals("PivotUnderbond can be deleted", true, pivotUnderbond.CanDelete);

			pivotUnderbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals("Underbond should not be deletable", false, pivotUnderbond.CanDelete);
			AssertEquals("Pivot should not be able to be deleted as it has an attached underbond that cannot be deleted", false, pivot.CanDelete);
			AssertEquals("Container should not be deletable as it has an attached pivot that cannot be deleted", false, container.CanDelete);

			pivotUnderbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;
			AssertEquals("Underbond should now be deletable", true, pivotUnderbond.CanDelete);
			AssertEquals("Pivot should not be able to be deleted as it has an attached underbond that cannot be deleted", true, pivot.CanDelete);
			AssertEquals("Container should not be deletable as it has an attached pivot that cannot be deleted", true, container.CanDelete);

			CusUnderbond containerUnderbond = container.Underbonds.AddNew();
			containerUnderbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals("Underbond count", 1, container.Underbonds.Count);
			AssertEquals("Underbond should not be deletable", false, containerUnderbond.CanDelete);
			AssertEquals("Container should not be able to be deleted as it has an attached underbond that cannot be deleted", false, container.CanDelete);

			containerUnderbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;
			AssertEquals("Underbond should now be deletable", true, containerUnderbond.CanDelete);
			AssertEquals("Container should be able to be deleted now that underbond has been deleted", true, container.CanDelete);
		}

		public void TestCanBeDeletedHouseAcknowledged()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = cMROceanBill.Containers.AddNew();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			CusSCAPivot pivot = container.Pivots.AddNew();
			house.Pivot.Add(pivot);
			house.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			Factory.Save();
			AssertEquals("Container can not be deleted because it is packed in an acknoledged house", false, container.CanDelete);
		}

		public void TestDeleteDoesNotThrowException_00827659()
		{
			CusSCAOceanBill cMROceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAContainer container = cMROceanBill.Containers.AddNew();
			CusSCAHouse house = cMROceanBill.HouseBills.AddNew();
			CusSCAPivot pivot = container.Pivots.AddNew();
			AssertEquals("New Container should be able to be deleted", true, container.CanDelete);

			CusUnderbond underbond = container.Underbonds.AddNew();
			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals("Underbond count", 1, container.Underbonds.Count);
			AssertEquals("Underbond should not be deletable", false, underbond.CanDelete);
			AssertEquals("Container should not be able to be deleted as it has an attached underbond that cannot be deleted.", false, container.CanDelete);
			Assert(!container.CanDelete);

			CusUnderbond pivotUnderbond = pivot.Underbonds.AddNew();
			pivotUnderbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals("Underbond count", 1, pivot.Underbonds.Count);
			AssertEquals("Underbond should not be deletable", false, pivotUnderbond.CanDelete);
			AssertEquals("Container should not be able to be deleted as it has an attached pivot that cannot be deleted.", false, container.CanDelete);
			Assert(!container.CanDelete);

			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;
			Assert(!container.CanDelete);
			pivotUnderbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;

			container.Delete();
			AssertEquals("Container should have been deleted", true, container.IsDeleted);
			AssertEquals("Underbond should have been deleted", true, underbond.IsDeleted);
			AssertEquals("PivotUnderbond should have been deleted", true, pivotUnderbond.IsDeleted);
			AssertEquals("Pivot should have been deleted", true, pivot.IsDeleted);
		}

		#endregion

		#region Implementation

		RefContainer GetNewContainer()
		{
			RefContainer container = RefContainer.New(Factory);
			container.RC_Length = 20;
			container.RC_Height = 8;
			container.RC_Width = 8;
			container.RC_ContainerType = Core.Constants.ContainerTypes.MAFI;
			container.RC_Code = "foo";
			return container;
		}

		const string TestVoyage = "340";

		#endregion
	}
}

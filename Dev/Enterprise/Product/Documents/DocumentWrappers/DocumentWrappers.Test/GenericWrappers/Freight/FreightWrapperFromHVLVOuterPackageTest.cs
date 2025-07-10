using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromHVLVOuterPackage))]
	sealed class FreightWrapperFromHVLVOuterPackageTest : FreightWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();

			return new FreightWrapperFromHVLVOuterPackage(outerPackage, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.NewWithValidTestData<HVLVOuterPackage>();
		}

		public void TestGetWeight()
		{
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_WeightUQ = Core.Constants.Weight.Tonnes;

			var item1 = outerPackage.Items.AddNew();
			var item2 = outerPackage.Items.AddNew();
			var item3 = outerPackage.Items.AddNew();
			item1.HVI_ActualWeight = 1;
			item2.HVI_ActualWeight = 2;
			item3.HVI_ActualWeight = 3;
			item1.HVI_HVC_Consignment = consignment.PK;
			item2.HVI_HVC_Consignment = consignment.PK;
			item3.HVI_HVC_Consignment = consignment.PK;

			var outerPackageFreightWrapper = new FreightWrapperFromHVLVOuterPackage(outerPackage, Factory);

			AssertEquals("The weight should be 6.000 T", "6.000 T", outerPackageFreightWrapper.Weight.ValueAndUnitCode);
		}

		public void TestGetOuterPackagesQty()
		{
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_F3_NKPackageType = Core.Constants.PkgUnit.Coil;
			var item1 = outerPackage.Items.AddNew();
			var item2 = outerPackage.Items.AddNew();
			var item3 = outerPackage.Items.AddNew();

			item1.HVI_F3_NKPackType = Core.Constants.PkgUnit.Package;
			item2.HVI_F3_NKPackType = Core.Constants.PkgUnit.Package;
			item3.HVI_F3_NKPackType = Core.Constants.PkgUnit.Package;
			item1.HVI_IsActive = true;
			item2.HVI_IsActive = true;
			item3.HVI_IsActive = false;

			var outerPackageFreightWrapper = new FreightWrapperFromHVLVOuterPackage(outerPackage, Factory);

			AssertEquals("The package quantity should be three coils", "2 COI", outerPackageFreightWrapper.ShipmentOuterPacksQty.ValueAndUnitCode);
		}

		public void TestGetItemWeight_WhenItemActualWeightIsEmpty_ThenGetItemManifestedWeight()
		{
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_WeightUQ = Core.Constants.Weight.Pounds;

			var item1 = outerPackage.Items.AddNew();
			var item2 = outerPackage.Items.AddNew();
			var item3 = outerPackage.Items.AddNew();
			item1.HVI_ManifestedWeight = 1;
			item2.HVI_ActualWeight = 2;
			item3.HVI_ManifestedWeight = 3;
			item1.HVI_HVC_Consignment = consignment.PK;
			item2.HVI_HVC_Consignment = consignment.PK;
			item3.HVI_HVC_Consignment = consignment.PK;

			var outerPackageFreightWrapper = new FreightWrapperFromHVLVOuterPackage(outerPackage, Factory);

			AssertEquals("The weight should be 6.000 LB", "6.000 LB", outerPackageFreightWrapper.Weight.ValueAndUnitCode);
		}

		public void TestGetBarcodeText()
		{
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_PackageBarcode = "123456789";

			var outerPackageFreightWrapper = new FreightWrapperFromHVLVOuterPackage(outerPackage, Factory);

			AssertEquals("The barcode text should be 123456789", "123456789", outerPackageFreightWrapper.BarcodeText);
		}

		public void TestMasterBill()
		{
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_MasterBillNumber = "MB1234";
			outerPackage.HVO_HVL_LoadList = loadList.PK;

			var outerPackageFreightWrapper = new FreightWrapperFromHVLVOuterPackage(outerPackage, Factory);

			AssertEquals("The master bill number should be MB1234", "MB1234", outerPackageFreightWrapper.MasterBill);
		}

		public void TestMasterBill_WhenHVLVOuterPackageHasNoLoadList_ThenReturnEmptyString()
		{
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var outerPackageFreightWrapper = new FreightWrapperFromHVLVOuterPackage(outerPackage, Factory);

			AssertEquals("The master bill number should be empty", ZString.Empty, outerPackageFreightWrapper.MasterBill);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var outerPackage = Factory.New<HVLVOuterPackage>();

			return new FreightWrapperFromHVLVOuterPackage(outerPackage, Factory);
		}

		#region Destination Depot

		public void TestGetDeliveryAddress()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVOuterPackage;
			AssertEquals(ZString.Empty, wrapper.DeliveryAddress.CompanyCode);

			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.OH_IsShippingProvider = true;
			var lastMileCarrierAddress = lastMileCarrier.Addresses.AddNew();
			lastMileCarrierAddress.OA_OH = lastMileCarrier.PK;
			outerPackage.HVO_OH_LastMileCarrier = lastMileCarrierAddress.PK;
			wrapper = new FreightWrapperFromHVLVOuterPackage(outerPackage, Factory);
			AssertEquals(lastMileCarrierAddress.OA_Code, wrapper.DeliveryAddress.CompanyCode);
		}

		#endregion

		#region Last Mile Carrier

		public void TestGetCarrier()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVOuterPackage;
			AssertEquals(ZString.Empty, wrapper.Carrier.CompanyCode);

			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.OH_IsShippingProvider = true;
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_OH_LastMileCarrier = lastMileCarrier.PK;
			wrapper = new FreightWrapperFromHVLVOuterPackage(outerPackage, Factory);
			AssertEquals(lastMileCarrier.PK, wrapper.Carrier.WrappedObjectPK);
		}

		#endregion

		#region Last Mile Carrier Service Level

		public void TestGetCarrierServiceLevel()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVOuterPackage;
			AssertEquals(ZString.Empty, wrapper.CarrierServiceLevel.CodeAndDescription);

			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.OH_IsShippingProvider = true;
			outerPackage.HVO_OH_LastMileCarrier = lastMileCarrier.PK;
			outerPackage.HVO_PL_NKLastMileCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			wrapper = new FreightWrapperFromHVLVOuterPackage(outerPackage, Factory);
			AssertEquals("wrapper.CarrierServiceLevel.CodeAndDescription", "STD - Standard", wrapper.CarrierServiceLevel.CodeAndDescription);
		}

		#endregion
	}
}

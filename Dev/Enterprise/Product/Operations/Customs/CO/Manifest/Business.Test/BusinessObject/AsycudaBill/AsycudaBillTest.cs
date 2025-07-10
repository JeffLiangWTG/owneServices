using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.COManifest.IAsycudaBill>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaBill>(bizObj.PK).GetType());
		}

		public void TestHeader()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(bill.Header);
		}

		public void TestGetCountryCode()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(Core.Constants.CountryCodes.Colombia, bill.GetCountryCode());
		}

		public void TestPacks()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaPackCollection>(bill.Packs);
		}

		public void TestShipperRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "CO";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, "62318879");

			var org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "CO";
			org1.OH_FullName = "SECOND";
			var orgAddress1 = org1.MainAddress;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Shipper = ZGuid.Empty;
			Factory.Save();
			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertEquals("62318879", bill.ABL_ShipperRegNo);

			bill.ABL_OA_Shipper = orgAddress1.PK;
			AssertEquals(ZString.Empty, bill.ABL_ShipperRegNo);
		}

		public void TestShipperWithThreeRegNoPrecedenceOrder()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "CO";
			org.OH_FullName = "FULL NAME";

			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(ColombiaOrgCusCodeInfo.OrgCusCodes.CID, "123456789456");
			org.CustomsCodes.AddNew(ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, "62318879");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "J074878112");

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Shipper = ZGuid.Empty;
			Factory.Save();
			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertEquals("62318879", bill.ABL_ShipperRegNo);
		}

		public void TestConsigneeRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "CO";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, "62318879");

			var org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "CO";
			org1.OH_FullName = "SECOND";
			var orgAddress1 = org1.MainAddress;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Consignee = ZGuid.Empty;
			Factory.Save();
			bill.ABL_OA_Consignee = orgAddress.PK;
			AssertEquals("62318879", bill.ABL_ConsigneeRegNo);

			bill.ABL_OA_Consignee = orgAddress1.PK;
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNo);
		}

		public void TestGetWarningBeforeBeingDeletedForSentBills()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Accepted;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HB1";
			
			AssertEquals("This Bill is already sent to Customs.", bill1.GetWarningBeforeBeingDeleted());

			header.AMA_MessageStatus = MessageStatusCodeList.Codes.NotSent;
			AssertNotEquals("This Bill is already sent to Customs.", bill1.GetWarningBeforeBeingDeleted());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew();
		}

		public void TestValidation()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var mBill = header.MasterBill;
			var bill = header.Bills.AddNew();

			AssertType<AsycudaBillValidationForMasterChild>(mBill.Validation);
			AssertType<AsycudaBillValidationForRegularBill>(bill.Validation);
		}

		sealed class AsycudaBillForTest : AsycudaBill
		{
			public AsycudaBillForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new ZString GetCountryCode() => base.GetCountryCode();
			public new ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => base.CreateNewAsycudaPackCollection();
			public new Type GetPackTypeCore() => base.GetPackTypeCore();
		}
	}
}

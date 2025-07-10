using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ARManifest.IAsycudaBill>(bizObj.PK).GetType());
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
			AssertEquals(Core.Constants.CountryCodes.Argentina, bill.GetCountryCode());
		}

		public void TestPacks()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaPackCollection>(bill.Packs);
		}

		public void TestShipperRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AR";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, "62318879");

			var org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "AR";
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

		public void TestShipperWithThreeRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AR";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI, "62318879");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "J074878112");
			org.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL, "123456789456");

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Shipper = ZGuid.Empty;
			Factory.Save();
			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertEquals("123456789456", bill.ABL_ShipperRegNo);
		}

		public void TestConsigneeRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AR";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL, "62318879");

			var org1 = Factory.New<OrgHeader>();
			org1.OH_RL_NKClosestPort = "AR";
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

		public void TestConsigneeWithThreeRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AR";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL, "62318879");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "J074878112");
			org.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, "123456789456");

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Consignee = ZGuid.Empty;
			Factory.Save();
			bill.ABL_OA_Consignee = orgAddress.PK;
			AssertEquals("123456789456", bill.ABL_ConsigneeRegNo);
		}

		public void TestABL_E_DEP()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals(ZDateTime.Empty, bill.ABL_E_DEP);

			header.AMA_E_DEP = new ZDateTime(2021, 06, 22);
			var bill2 = header.Bills.AddNew();

			AssertEquals(ZDateTime.Empty, bill.ABL_E_DEP);
			AssertEquals("22-Jun-21 00:00:00", bill2.ABL_E_DEP.ToString());

			bill2.ABL_E_DEP = new ZDateTime(2021, 06, 21);

			AssertEquals("21-Jun-21 00:00:00", bill2.ABL_E_DEP.ToString());
		}

		public void TestBillCantBeDeleteWhenStatusIsAccepted()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "ACP";
			AssertEquals(false, bill.CanDelete);

			AssertEquals("This Bill is already sent and accepted. If you need to cancel it from Customs, you must use the Cancel option from Manifest menu.", bill.ReasonForNotAbleToDelete);

			bill.ABL_BillStatus = "";
			AssertEquals(true, bill.CanDelete);
		}

		public void TestBillCantBeDeleteWhenStatusIsSent()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "SNT";
			AssertEquals(false, bill.CanDelete);

			AssertEquals("This Bill is already sent and it is awaiting for a response.", bill.ReasonForNotAbleToDelete);

			bill.ABL_BillStatus = "";
			bill.ABL_MessageStatus = "";
			AssertEquals(true, bill.CanDelete);
		}

		public void TestBillCantBeDeleteWhenStatusIsCancelled()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "CAN";
			AssertEquals(false, bill.CanDelete);

			AssertEquals("This Bill is canceled. Must be kept for history purposes.", bill.ReasonForNotAbleToDelete);

			bill.ABL_BillStatus = "";
			AssertEquals(true, bill.CanDelete);
		}

		#region Implementation

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

		class AsycudaBillForTest : AsycudaBill
		{
			public AsycudaBillForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new ZString GetCountryCode() => base.GetCountryCode();
		}

		#endregion
	}
}

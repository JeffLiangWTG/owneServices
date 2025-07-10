using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	class AsycudaBillDocWrapperTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Origin", Wrapper.Origin, "AUSYD");
				AssertEquals("BillNumber", Wrapper.BillNumber, "billNumber");
				AssertEquals("GoodsDescription", Wrapper.GoodsDescription, "Desc");
				AssertEquals("Destination", Wrapper.Destination, "ADVLV");
				AssertEquals("Marks", Wrapper.Marks, "123");
				AssertEquals("Weight", Wrapper.Weight, "10\tKG");
				AssertEquals("Volume", Wrapper.Volume, "10\tM3");
				AssertEquals("Quantity", Wrapper.Quantity, "1\tBAG");
				AssertEquals("ConsignorPK", Wrapper.ConsignorPK, org.PK);
				AssertEquals("ConsigneePK", Wrapper.ConsigneePK, org.PK);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AR";
			org.OH_FullName = "FULL NAME";

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Bill = header.Bills.AddNew();
			Bill.ABL_RL_NKOrigin = "AUSYD";
			Bill.ABL_BillNumber = "billNumber";
			Bill.ABL_GoodsDescription = "Desc";
			Bill.ABL_RL_NKFinalDestination = "ADVLV";
			Bill.ABL_MarksAndNumbers = "123";
			Bill.ABL_GrossWeight = 10.0;
			Bill.ABL_GrossWeightUQ = "KG";
			Bill.ABL_Volume = 10.0;
			Bill.ABL_VolumeUQ = "M3";
			Bill.ABL_ManifestQty = 1;
			Bill.ABL_ManifestUQ = "BAG";
			Bill.ABL_OA_Shipper = org.PK;
			Bill.ABL_OA_Consignee = org.PK;

			Wrapper = new AsycudaBillDocWrapper(Bill);
		}

		public AsycudaBillDocWrapper Wrapper;
		public AsycudaBill Bill;
		public OrgHeader org;
	}
}

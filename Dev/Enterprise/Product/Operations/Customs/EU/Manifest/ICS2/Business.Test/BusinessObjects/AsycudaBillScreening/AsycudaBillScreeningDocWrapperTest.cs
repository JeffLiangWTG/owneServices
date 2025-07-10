using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	class AsycudaBillScreeningDocWrapperTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Result", Wrapper.Result, EUICS2ScreeningResultList.Descriptions.SR1);
				AssertEquals("PersonType", Wrapper.PersonType, EUICS2ScreeningAuthorizedPersonTypes.Descriptions.AP2);
				AssertEquals("PersonName", Wrapper.PersonName, "Name");
				AssertEquals("PersonIdentifier", Wrapper.PersonIdentifier, "12345");
				AssertEquals("POBox", Wrapper.POBox, "POBox");
				AssertEquals("SubDivision", Wrapper.SubDivision, "Sub");
				AssertEquals("Number", Wrapper.Number, "123456");
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
			BillScreening = Bill.BillScreenings.AddNew();
			BillScreening.ASR_Result = EUICS2ScreeningResultList.Codes.SR1;
			BillScreening.ASR_AuthorizedPersonType = EUICS2ScreeningAuthorizedPersonTypes.Codes.AP2;
			BillScreening.ASR_AuthorizedPersonName = "Name";
			BillScreening.ASR_AuthorizedPersonIdentifier = "12345";
			BillScreening.FacilityPlace.POBox = "POBox";
			BillScreening.FacilityPlace.SubDivision = "Sub";
			BillScreening.FacilityPlace.Number = "123456";

			Wrapper = new AsycudaBillScreeningDocWrapper(BillScreening);
		}

		public AsycudaBillScreeningDocWrapper Wrapper;
		public AsycudaBill Bill;
		public AsycudaBillScreening BillScreening;
		public OrgHeader org;
	}
}

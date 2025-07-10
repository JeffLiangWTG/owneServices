using CargoWise.Customs.MX.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	public class HouseWaybillWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;
		IHouseWaybill wrapper;

		public void TestHouseWaybillWrapper()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(wrapper.MessageHeader);
				AssertNotNull(wrapper.BusinessHeader);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.GC_BusinessRegNo = "7845";

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();

			AsycudaBill bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "23456ABCD";

			wrapper = new HouseWaybillWrapper(bill, MessageSubTypeCodes.Codes.Original);
		}

		void PopulateManifestHeader()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "123456";
			orgHeader1.OH_FullName = "K. WILSON";
			var orgAddress1 = orgHeader1.MainAddress;

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "210696";
			orgHeader2.OH_FullName = "J. LIM";
			var orgAddress2 = orgHeader2.MainAddress;
			orgAddress2.City = "City";

			header.ShippingAgentOrgPK = orgHeader1.PK;
			header.ShippingAgentOrg.Addresses.Add(orgAddress1);
			header.AMA_OA_Carrier = orgAddress2.PK;
			header.AMA_MasterBillIssueDate = new ZDate(2002, 07, 01);

			Factory.Save();
		}
	}
}

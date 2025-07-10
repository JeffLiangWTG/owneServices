using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.GovernmentGateway.Testing
{
	class NctsHeaderHelperTest : TestCaseWithFactory
	{
		public void TestIsConsignorDefinedAtGoodsItemLevel()
		{
			CombineAssertions(() =>
			{
				AssertEquals("departureHeaderLevel", false, NctsHeaderHelper.IsConsignorDefinedAtGoodsItemLevel(departureHeaderLevel));
				AssertEquals("departureLineLevel", true, NctsHeaderHelper.IsConsignorDefinedAtGoodsItemLevel(departureLineLevel));
				AssertEquals("arrival", false, NctsHeaderHelper.IsConsignorDefinedAtGoodsItemLevel(arrival));
			});
		}

		public void TestIsConsigneeDefinedAtGoodsItemLevel()
		{
			CombineAssertions(() =>
			{
				AssertEquals("departureHeaderLevel", false, NctsHeaderHelper.IsConsigneeDefinedAtGoodsItemLevel(departureHeaderLevel));
				AssertEquals("departureLineLevel", true, NctsHeaderHelper.IsConsigneeDefinedAtGoodsItemLevel(departureLineLevel));
				AssertEquals("arrival", false, NctsHeaderHelper.IsConsigneeDefinedAtGoodsItemLevel(arrival));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.CompanyName = "COMPANY NAME";
			orgAddress.OA_Address1 = "123 ABC";

			departureHeaderLevel = SetupNctsHeader(NctsMovementType.Codes.Departure);
			departureLineLevel = SetupNctsHeader(NctsMovementType.Codes.Departure, true);
			arrival = SetupNctsHeader(NctsMovementType.Codes.Arrival);
		}

		NctsHeader SetupNctsHeader(ZString movementType, bool addItemLevelItems = false)
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(movementType);

			if (addItemLevelItems)
			{
				var cargoDesc = header.Bills.AddNew().GoodsItems.AddNew();
				cargoDesc.Consignee.OrganisationPK = orgHeader.PK;
				cargoDesc.Consignor.OrganisationPK = orgHeader.PK;
			}

			return header;
		}

		OrgHeader orgHeader;
		NctsHeader departureHeaderLevel;
		NctsHeader departureLineLevel;
		NctsHeader arrival;
	}
}

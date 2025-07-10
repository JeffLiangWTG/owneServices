using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaContainerSailingSynchronisationTest : SailingSynchronisationTest
	{
		public void TestIsMatched()
		{
			header.Bills.RemoveAndDeleteAll();

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "AAA";

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "ABCD1111";

			header.BillOfLadingForSync = fCLSailingBill;

			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLadingContainer>)container;
			Assert("Should be true as the container number is same with the target container number on the bill of lading.", synchronisationTarget.IsMatched(sailingContainer));
		}

		public void TestSetBillOfLading()
		{
			header.Bills.RemoveAndDeleteAll();

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "AAA";

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "XXXX000";

			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLadingContainer>)container;
			synchronisationTarget.Set(sailingContainer);

			AssertEquals("Should update the value from the bill of lading container.", "ABCD1111", container.ACN_ContainerNumber);
		}

		public void TestSynchronise()
		{
			header.Bills.RemoveAndDeleteAll();

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "AAA";

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "ABCD1111";

			container.ACN_RC_ContainerType = Factory.NewWithValidTestData<RefContainer>().PK;
			container.ACN_Seal1 = "SEAL 001";
			container.ACN_Seal2 = "SEAL 002";
			container.ACN_Seal3 = "SEAL 003";
			container.ACN_CommodityCode = "052";
			container.ACN_GoodsWeight = 350m;
			container.ACN_GoodsWeightUQ = Core.Constants.Weight.Grams;
			container.ACN_StowageLocation = "Mascot";

			header.BillOfLadingForSync = fCLSailingBill;

			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLadingContainer>)container;
			synchronisationTarget.Synchronise();

			CombineAssertions(() =>
			{
				AssertSame("Should not reset to null after synchronisation.", fCLSailingBill, header.BillOfLadingForSync);

				AssertEquals("JC_RC", rContainer.PK, container.ACN_RC_ContainerType);
				AssertEquals("JC_SealNum", "123", container.ACN_Seal1);
				AssertEquals("JC_AdditionalSealNum", "456", container.ACN_Seal2);
				AssertEquals("JC_Additional2SealNum", "789", container.ACN_Seal3);
				AssertEquals("JC_RH_NKContainerCommodityCode", "ABC", container.ACN_CommodityCode);
				AssertEquals("JC_GrossWeight", 982m, container.ACN_GoodsWeight);
				AssertEquals("JC_GrossWeightUQ", Core.Constants.Weight.Kilograms, container.ACN_GoodsWeightUQ);
				AssertEquals("JC_StowagePosition", "Darnass", container.ACN_StowageLocation);
			});
		}

		public void TestSynchroniseACN_EmptyFullIndicator()
		{
			header.Bills.RemoveAndDeleteAll();

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "AAA";
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "ABCD1111";

			header.BillOfLadingForSync = fCLSailingBill;

			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLadingContainer>)container;

			CombineAssertions(() =>
			{
				sailingContainer.JC_IsEmptyContainer = true;
				synchronisationTarget.Synchronise();
				AssertEquals("Should update the ACN_EmptyFullIndicator as MT when the bill of lading container is marked as empty container.", EmptyFullIndicatorList.Codes.EmptyContainer, container.ACN_EmptyFullIndicator);

				sailingContainer.JC_IsEmptyContainer = false;
				sailingContainer.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
				synchronisationTarget.Synchronise();
				AssertEquals("Should update the value from the bill of lading container when ContainerMode is FCL.", EmptyFullIndicatorList.Codes.FullContainerLoad, container.ACN_EmptyFullIndicator);

				sailingContainer.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
				synchronisationTarget.Synchronise();
				AssertEquals("Should update the value from the bill of lading container when ContainerMode is LCL.", EmptyFullIndicatorList.Codes.LessThanFullContainerLoad, container.ACN_EmptyFullIndicator);

				sailingContainer.JC_ContainerMode = Core.Constants.ContainerModes.Bulk;
				synchronisationTarget.Synchronise();
				AssertEquals("Should not set the value from the bill of lading container when ContainerMode is not FCL or LCL.", "", container.ACN_EmptyFullIndicator);
			});
		}
	}
}

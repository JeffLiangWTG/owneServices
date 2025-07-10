using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsEntryNumberTypes_Contents()
		{
			var bill = Factory.NewWithValidTestData<AsycudaManifestHeader>().Bills.AddNew();

			AssertContainsExactElementsInAnyOrder(expectedEntryNumberTypes, bill.Lookups.CustomsEntryNumberTypes.GetAllCodes());
		}

		public void TestIncotermList_Contents()
		{
			var bill = Factory.NewWithValidTestData<AsycudaManifestHeader>().Bills.AddNew();

			AssertContainsExactElementsInAnyOrder(expectedIncotermListCodes, bill.Lookups.IncotermList.GetAllCodes());
		}

		public void TestABLEntryNumberTypes_Contents()
		{
			var bill = Factory.NewWithValidTestData<AsycudaManifestHeader>().Bills.AddNew();
			var ablEntryNumber = bill.CustomsEntryNumbers.AddNew();

			AssertContainsExactElementsInAnyOrder(expectedEntryNumberTypes, ablEntryNumber.Lookups.CustomsEntryNumberTypes.GetAllCodes());
		}

		public void TestABLProcedureList_Contents()
		{
			var asycudaBill = Factory.New<AsycudaBill>();
			var codeList = asycudaBill.Lookups.AdditionalProcedureList;

			CombineAssertions(() =>
			{
				AssertEquals("There are four elements in the list", 4, codeList.Count);
				AssertContainsExactElementsInAnyOrder(expectedAdditionalProcedureListCodes, codeList.GetAllCodes());
			});
		}

		public void TestABLContainerModeList_Contents()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertContainsExactElementsInAnyOrder(expectedContainerModeForAIR, bill.Lookups.ContainerModeList.GetAllCodes());

			header.AMA_TransportMode = Core.Constants.TransportModes.Mail;
			AssertContainsExactElementsInAnyOrder(expectedContainerModeForMAI, bill.Lookups.ContainerModeList.GetAllCodes());

			header.AMA_TransportMode = Core.Constants.TransportModes.Rail;
			AssertContainsExactElementsInAnyOrder(expectedContainerModeForRAI, bill.Lookups.ContainerModeList.GetAllCodes());

			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertContainsExactElementsInAnyOrder(expectedContainerModeForROA, bill.Lookups.ContainerModeList.GetAllCodes());

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertContainsExactElementsInAnyOrder(expectedContainerModeForSEA, bill.Lookups.ContainerModeList.GetAllCodes());
		}

		public void TestExpectedElementsAreFromMessagingProvider()
		{
			AssertContainsExactElementsInAnyOrder(MessagingProviderTest.expectedEntryNumberTypes, expectedEntryNumberTypes);
		}

		readonly string[] expectedEntryNumberTypes = new[] { "MRN", "LRN" };

		readonly string[] expectedIncotermListCodes = new[] { "CFR", "CIF", "CIP", "CPT", "DAP", "DAT", "DDP", "DPU", "EXW", "FAS", "FC1", "FC2", "FCA", "FOB" };

		readonly string[] expectedAdditionalProcedureListCodes = ["C07", "C08", "C07+F48", "C07+F49"];

		readonly string[] expectedContainerModeForAIR = ["LSE", "ULD", "NCT"];

		readonly string[] expectedContainerModeForMAI = ["CNT", "NCT"];

		readonly string[] expectedContainerModeForRAI = ["FCL", "LCL", "BLK", "LQD", "BBK", "CNT", "NCT"];

		readonly string[] expectedContainerModeForROA = ["FCL", "FTL", "LCL", "LTL", "CNT", "NCT"];

		readonly string[] expectedContainerModeForSEA = ["FCL", "LCL", "BLK", "LQD", "BBK", "ROR", "CNT", "NCT"];
	}
}

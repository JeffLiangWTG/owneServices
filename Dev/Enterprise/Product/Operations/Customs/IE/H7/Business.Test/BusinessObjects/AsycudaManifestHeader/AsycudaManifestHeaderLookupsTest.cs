using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	sealed class AsycudaManifestHeaderLookupsTest : TestCaseWithFactory
	{
		public void TestAgentTypeCodeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var codeList = header.Lookups.AgentTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("There are two countries in the list", 2, codeList.Count);
				Assert("Should include AU", codeList.ContainsCode("DIR"));
				Assert("Should include IE", codeList.ContainsCode("IND"));
			});
		}

		public void TestMethodOfPaymentList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var codeList = header.Lookups.MethodOfPaymentList;

			CombineAssertions(() =>
			{
				AssertEquals("There are four payment methods in the list", 4, codeList.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "A", "E", "J", "M" }, codeList.GetAllCodes());
			});
		}

		public void TestSubmitTypeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var codeList = header.Lookups.SubmitTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("There are two submit types in the list", 2, codeList.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "LV1", "LV2" }, codeList.GetAllCodes());
			});
		}

		public void TestRegistrationStatusList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var codeList = header.Lookups.RegistrationStatusList;

			CombineAssertions(() =>
			{
				AssertEquals("There are eighteen elements in the list", 18, codeList.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "ACC", "AMR", "AMA", "SUP", "CAR", "CAN", "CON", "GEN", "INS", "INV", "NOT", "PRE", "RAA", "RAJ", "RAR", "REG", "REJ", "REL" }, codeList.GetAllCodes());
			});
		}
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHContainerSynchroniserTest : TestCaseWithFactory
	{
		public void TestCusCAeMHContainerSynchroniser()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var refContainer = helper.Container1;
			var container = helper.MasterBill.Containers.AddNew();
			var synchroniser = new CusCAeMHContainerSynchroniser(container, refContainer);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals("20GP", container.BQ_RC_NKContainerType);

			refContainer.JC_ContainerNum = "MAEU78789";
			AssertEquals("MAEU78789", container.BQ_ContainerNumber);

			refContainer.JC_SealNum = "SEAL1";
			AssertEquals("SEAL1", container.BQ_Seal1);

			refContainer.JC_AdditionalSealNum = "SEAL2";
			AssertEquals("SEAL2", container.BQ_Seal2);
		}
	}
}

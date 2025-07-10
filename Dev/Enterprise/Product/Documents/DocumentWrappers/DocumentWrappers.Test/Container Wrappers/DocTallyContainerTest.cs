using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.DocumentWrappers.Testing.ContainerWrappers
{
	sealed class DocTallyContainerTest : TestCaseWithFactory
	{
		public void TestTotalShipments()
		{
			AssertEquals("TotalShipments", Tally.TotalShipments, TallyWrapper.TotalShipments);
		}

		#region Implementation

		TallyContainer Tally;
		DocTallyContainer TallyWrapper;

		protected override void SetUp()
		{
			Tally = Factory.New<TallyContainer>();
			TallyWrapper = DocTallyContainer.New(Tally, Factory);

			base.SetUp();
		}

		#endregion
	}
}

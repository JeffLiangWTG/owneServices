using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class ArrivalTransportInfosUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>), userControl.BindingSource.DataSourceType);
		}

		public void TestArrivalTransportInfosGridUserControl()
		{
			AssertNull("Before binding", userControl.ArrivalTransportInfosGridUserControl);
		}

		public void TestArrivalTransportInfosGridUserControlAfterBinding()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, string.Empty);

			CombineAssertions("After binding", () =>
			{
				AssertType<ArrivalTransportInfosGridUserControl>("Type", userControl.ArrivalTransportInfosGridUserControl);
				AssertEquals("BindingMember", ".", userControl.ArrivalTransportInfosGridUserControl.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ArrivalTransportInfosUserControl();
		}
		ArrivalTransportInfosUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}

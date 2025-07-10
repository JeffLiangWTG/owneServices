using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	sealed class DepartureDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType() => AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);

		public void TestArrivalGoodsLocationUserControl() => AssertType<ZCodeFindBox>(control.DepartureGoodsLocationCodeFindBox);

		public void TestTNNDocumentTypeDropEdit() => AssertType<ZDropEdit>(control.TNNDocumentTypeDropEdit);

		protected override void SetUp()
		{
			base.SetUp();
			control = new DepartureDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		DepartureDetailsUserControl control;
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class TransportDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestMasterBillAndIATAUserControl()
		{
			AssertType<MasterBillAndIATAUserControl>(control.MasterBillAndIATAUserControl);
		}

		public void TestTransportInlandRailUserControl()
		{
			AssertType<TransportInlandRailUserControl>(control.TransportInlandRailUserControl);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportDetailsUserControl control;
	}
}

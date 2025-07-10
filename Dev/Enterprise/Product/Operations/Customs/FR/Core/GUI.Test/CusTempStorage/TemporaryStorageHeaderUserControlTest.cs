using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	class TemporaryStorageHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestTransportDetailsUSerControlIsVisible()
		{
			using (var headerUserControl = new TemporaryStorageHeaderUserControl())
			{
				headerUserControl.SetDataBinding(header, string.Empty);
				var transportDetailsControl = headerUserControl.Controls.Find("TransportDetailsUserControl", true).Single();
				AssertEquals("Control exists and is visible", true, transportDetailsControl.Visible);
			}
		}

		public void TestCustomsDetailsUserControlIsVisible()
		{
			using (var headerUserControl = new TemporaryStorageHeaderUserControl())
			{
				headerUserControl.SetDataBinding(header, string.Empty);
				var customsDetailsControl = headerUserControl.Controls.Find("CustomsDetailsUserControl", true).Single();
				AssertEquals("Control exists and is visible", true, customsDetailsControl.Visible);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = FR.Business.CusTempStorage.CusTempStorageJobHeader.New(Factory);
		}
		CusTempStorageJobHeader header;
	}
}

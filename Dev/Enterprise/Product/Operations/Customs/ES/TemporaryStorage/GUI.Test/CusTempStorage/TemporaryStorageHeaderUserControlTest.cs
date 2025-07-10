using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.CusTempStorage;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	public class TemporaryStorageHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestTransportDetailsUserControlIsVisible()
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
			header = CusTempStorageJobHeader.New(Factory);
		}
		CusTempStorageJobHeader header;
	}
}

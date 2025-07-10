using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CusTempStorage;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class SumAUserControlTest : TestCaseWithFactory
	{
		public void TestControlVisibilityForSumAHeader()
		{
			header.SJH_AppCode = TemporaryStorageApplicationCodeList.Codes.SumA;
			using (var temporaryStorageUserControl = new TemporaryStorageUserControl())
			{
				temporaryStorageUserControl.SetDataBinding(header, string.Empty);
				AssertControlsVisibleAndDockFill(temporaryStorageUserControl, "SUMTransportDetailsUserControl", true);
				AssertControlsVisibleAndDockFill(temporaryStorageUserControl, "SUMCustomsDetailsUserControl", true);
				AssertControlsVisibleAndDockFill(temporaryStorageUserControl, "REXCustomsDetailsUserControl", false);
				AssertControlsVisibleAndDockFill(temporaryStorageUserControl, "REXCustomsDetailsUserControl", false);
			}
		}

		public void TestContolVisibilityForReExportHeader()
		{
			header.SJH_AppCode = TemporaryStorageApplicationCodeList.Codes.REX;
			using (var temporaryStorageUserControl = new TemporaryStorageUserControl())
			{
				temporaryStorageUserControl.SetDataBinding(header, string.Empty);
				AssertControlsVisibleAndDockFill(temporaryStorageUserControl, "SUMTransportDetailsUserControl", false);
				AssertControlsVisibleAndDockFill(temporaryStorageUserControl, "SUMCustomsDetailsUserControl", false);
				AssertControlsVisibleAndDockFill(temporaryStorageUserControl, "REXCustomsDetailsUserControl", true);
				AssertControlsVisibleAndDockFill(temporaryStorageUserControl, "REXCustomsDetailsUserControl", true);
			}
		}

		void AssertControlsVisibleAndDockFill(TemporaryStorageUserControl control, string controlName, bool visibility)
		{
			var foundControl = control.Controls.Find(controlName, true).First();
			AssertEquals(visibility, foundControl.Visible);
			AssertEquals(DockStyle.Fill, foundControl.Dock);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageJobHeader>();
		}
		CusTempStorageJobHeader header;
	}
}

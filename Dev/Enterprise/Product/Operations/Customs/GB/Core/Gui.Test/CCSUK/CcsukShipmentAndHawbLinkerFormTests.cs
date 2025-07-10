using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	[TestedType(typeof(CcsukShipmentAndHawbLinkerForm))]
	class CcsukShipmentAndHawbLinkerFormTests : ZFormBasherTest
	{
		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
		}

		protected override Form GetFormToBashCore()
		{
			var header = ShipmentToHawbMatcherManagerTest.CreateShipmentsAndHawbsForTest(Factory);
			return new CcsukShipmentAndHawbLinkerForm(header);
		}

		public void TestMatchManually()
		{
			var header = ShipmentToHawbMatcherManagerTest.CreateShipmentsAndHawbsForTest(Factory);
			using (var form = new CcsukShipmentAndHawbLinkerForm(header))
			{
				form.Show();
				header.Pivots[0].CS = header.Pivots[0].HawbsList[0].PK;
				header.Pivots[1].CS = header.Pivots[0].HawbsList[1].PK;
				var button = form.Controls.Find("buttonMakeLinks", true).FirstOrDefault();
				if (button != null)
				{
					((Button)button).PerformClick();
				}
			}
			var mawbMade = header.Consol.Factory.LoadTop1<CusMAWB>(new ZQuery());
			AssertNotNull(mawbMade);
			AssertEquals(3, mawbMade.ChildBills.Count);
			var h1 = mawbMade.ChildBills[0];
			var h2 = mawbMade.ChildBills[1];
			AssertEquals("00000001", h1.CS_HAWB);
			AssertEquals("SATL0001", h1.Shipment.JS_HouseBill);
			AssertEquals("ATL00002", h2.CS_HAWB);
			AssertEquals("SATL0002", h2.Shipment.JS_HouseBill);
		}
		public void TestMatchWithNewBill()
		{
			var header = ShipmentToHawbMatcherManagerTest.CreateShipmentsAndHawbsForTest(Factory);
			using (var form = new CcsukShipmentAndHawbLinkerForm(header))
			{
				form.Show();
				header.Pivots[0].CS = header.Pivots[0].HawbsList[0].PK;
				header.Pivots[1].CreateNewHawb = true;
				header.Pivots[1].HawbNumber = "ImNewPls";
				var button = form.Controls.Find("buttonMakeLinks", true).FirstOrDefault();
				if (button != null)
				{
					((Button)button).PerformClick();
				}
			}
			var mawbMade = header.Consol.Factory.LoadTop1<CusMAWB>(new ZQuery());
			AssertNotNull(mawbMade);
			AssertEquals(4, mawbMade.ChildBills.Count);
			var h1 = mawbMade.ChildBills[0];
			var h2 = mawbMade.ChildBills[1];
			var hNew = mawbMade.ChildBills[mawbMade.ChildBills.Count - 1];
			AssertEquals("00000001", h1.CS_HAWB);
			AssertEquals("SATL0001", h1.Shipment.JS_HouseBill);
			AssertEquals("IMNEWPLS", hNew.CS_HAWB);
			AssertEquals("SATL0002", hNew.Shipment.JS_HouseBill);
		}

		public void TestCheckBoxRemoveAllSplitsFromBasic()
		{
			var header = ShipmentToHawbMatcherManagerTest.CreateShipmentsAndHawbsForTest(Factory);
			header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR = false;

			using (var form = new CcsukShipmentAndHawbLinkerForm(header))
			{
				form.Show();
				var checkBox = form.Controls.Find("CheckBoxRemoveAllSplitsFromBasic", true).FirstOrDefault() as ZCheckBox;
				AssertNotNull("Must find the checkbox in the GUI window.", checkBox);
				checkBox.Checked = true;
				Assert("Property 'DeleteAnyExistingLocalSplitOnBasicIfStatusISR' must be true after the GUI checkbox was ticked.", header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR);
				checkBox.Checked = false;
				Assert("Property 'DeleteAnyExistingLocalSplitOnBasicIfStatusISR' must be false after the GUI checkbox was unticked.", !header.DeleteAnyExistingLocalSplitOnBasicIfStatusISR);
			}
		}
	}
}

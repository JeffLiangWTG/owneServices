using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.PlugIns.Testing
{
	[TestedType(typeof(HouseBillArrivalCertificationSelectionDialog))]
	class HouseBillArrivalCertificationSelectionDialogTest : ZFormBasherTest
	{
		public void TestHouseBillArrivalCertificationSelectionDialog()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			var rnsRequestBos = new RNSRequestBOCollection(consol);
			using (var form = new HouseBillArrivalCertificationSelectionDialog(rnsRequestBos))
			{
				form.Show();
				AssertEquals("Sending Messages", form.Text);
				var grid = form.FindSingle<ZGrid>("MessageToBeSendGrid");
				AssertNotNull(grid);
				AssertNotNull(grid.GetColumnStyle("Selected") as ZCheckBoxColumnStyleInfo);
				AssertNotNull(grid.GetColumnStyle("HouseBillNumber") as ZTextBoxColumnStyleInfo);
				AssertNotNull(grid.GetColumnStyle("CargoControlNumber") as ZTextBoxColumnStyleInfo);
				AssertNotNull(grid.GetColumnStyle("DateOfArrival") as ZDateEditColumnStyleInfo);
				AssertNotNull(grid.GetColumnStyle("OfficeCode") as ZCodeFindBoxColumnStyleInfo);
				AssertNotNull(grid.GetColumnStyle("SubLocationCode") as ZCodeFindBoxColumnStyleInfo);
			}
		}

		public void TestOKButton_Click()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var entryNumber1 = shipment1.Numbers.AddNew();
			entryNumber1.CE_EntryNum = "111";
			entryNumber1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			var shipment2 = consol.Shipments.AddNew();
			var entryNumber2 = shipment2.Numbers.AddNew();
			entryNumber2.CE_EntryNum = "222";
			entryNumber2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			Factory.Save();
			var rnsRequestBos = new RNSRequestBOCollection(consol);
			using (var form = new HouseBillArrivalCertificationSelectionDialog(rnsRequestBos))
			{
				form.Show();
				var okButton = form.FindSingle<ZButton>("OKButton");
				okButton.PerformClick();
				AssertEquals("Please select the required RNS Request information from the grid first, in order to proceed.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				var requestBO1 = rnsRequestBos[0];
				requestBO1.Selected = true;
				var requestBO2 = rnsRequestBos[1];
				requestBO2.Selected = true;
				okButton.PerformClick();
				AssertEquals("Dialog Result is 'OK'", DialogResult.OK, form.DialogResult);
			}
		}

		public void TestSelectAll_Button_ClickAndDeselectAll_Button_Click()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			var rnsRequestBos = new RNSRequestBOCollection(consol);
			AssertEquals(2, rnsRequestBos.Count);
			AssertEquals(0, rnsRequestBos.GetSelectedRequestBOs().Count());
			using (var form = new HouseBillArrivalCertificationSelectionDialog(rnsRequestBos))
			{
				form.Show();
				var selectAllButton = form.FindSingle<ZButton>("SelectAll_Button");
				selectAllButton.PerformClick();
				AssertEquals(2, rnsRequestBos.GetSelectedRequestBOs().Count());
				var deselectAllButton = form.FindSingle<ZButton>("DeselectAll_Button");
				deselectAllButton.PerformClick();
				AssertEquals(0, rnsRequestBos.GetSelectedRequestBOs().Count());
			}
		}

		protected override Form GetFormToBashCore()
		{
			var rnsRequestBos = new RNSRequestBOCollection(Factory.New<ForwardingConsol>());
			return new HouseBillArrivalCertificationSelectionDialog(rnsRequestBos);
		}
	}
}

using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoDepotOutturnUserControlTest : TestCaseWithFactory
	{
		public void TestNilOutturn()
		{
			var header = Factory.New<CusOutturnHeader>();
			var outturn = header.Outturns.AddNew();
			using (var form = new ZForm(header))
			using (var control = new SeaCargoDepotOutturnUserControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(header, "");
				form.Show();
				outturn.C5_OuterPacks = 7;
				outturn.C5_PackagesOutturned = 0;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				control.nilOutturnButton.PerformClick();
				AssertEquals(0, outturn.C5_PackagesOutturned);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				control.nilOutturnButton.PerformClick();
				AssertEquals(7, outturn.C5_PackagesOutturned);
			}
		}

		public void TestUpdateCurrentOutturnFromGrid()
		{
			var header = Factory.New<CusOutturnHeader>();
			var outturn1 = header.Outturns.AddNew();
			var outturn2 = header.Outturns.AddNew();
			Factory.Save();
			using (var testForm = new ZForm(header))
			using (var testControl = new SeaCargoDepotOutturnUserControl())
			{
				testForm.Controls.Add(testControl);
				testForm.Show();
				testControl.SetDataBinding(header, "");
				AssertEquals(outturn1, testControl.seaCargoOutturnDetailUserControl1.CurrentOutturn);
				testControl.OutturnsGrid.ListManager.Position = testControl.OutturnsGrid.List.IndexOf(outturn2);
				AssertEquals(testControl.seaCargoOutturnDetailUserControl1.CurrentOutturn, outturn2);
				testControl.OutturnsGrid.ListManager.Position = testControl.OutturnsGrid.List.IndexOf(outturn1);
				AssertEquals(testControl.seaCargoOutturnDetailUserControl1.CurrentOutturn, outturn1);
			}
		}

		public void TestOutturnMessages()
		{
			var header = Factory.New<CusOutturnHeader>();
			var outturn1 = header.Outturns.AddNew();
			var outturn2 = header.Outturns.AddNew();
			var message = Factory.New<CMRCARSTMessage>();
			message.EM_LinkedObject = outturn1;
			Factory.Save();
			using (var testForm = new ZForm(header))
			using (var testControl = new SeaCargoDepotOutturnUserControl())
			{
				testForm.Controls.Add(testControl);
				testForm.Show();
				testControl.SetDataBinding(header, "");
				AssertEquals(outturn1, testControl.seaCargoOutturnDetailUserControl1.CurrentOutturn);
				testControl.outturnTabControl.SelectedTab = testControl.outturnMessagesTabPage;
				BusinessObjectCollection gridDataSource = testControl.outturnMessagesControl.MessagesGrid.List as BusinessObjectCollection;
				AssertNotNull("Failed to get grid data source for Outturn 1", gridDataSource);
				AssertEquals("Data Source should have 1 message on Outturn 1", 1, gridDataSource.Count);
				testControl.OutturnsGrid.ListManager.Position = testControl.OutturnsGrid.List.IndexOf(outturn2);
				gridDataSource = testControl.outturnMessagesControl.MessagesGrid.List as BusinessObjectCollection;
				AssertNotNull("Failed to get grid data source for Outturn 2", gridDataSource);
				AssertEquals("Data Source should have 0 messages for outturn 2", 0, gridDataSource.Count);
			}
		}
	}
}

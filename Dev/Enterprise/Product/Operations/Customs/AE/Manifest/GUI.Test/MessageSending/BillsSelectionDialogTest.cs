using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.GUI.Testing;

[TestedType(typeof(BillsSelectionDialog))]
sealed class BillsSelectionDialogTest : ZFormBasherTest
{
	public void TestGridColumns()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var chooser = new MessageChooser(header, new[] { bill }, false);

		using (var chooserDialog = new BillsSelectionDialog(chooser, string.Empty))
		{
			chooserDialog.Show();
			var grid = chooserDialog.FindSingle<ZGrid>("ItemsGrid");
			var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
			AssertContainsExactElementsInAnyOrder(new string[] { "Checked", "EntryType", "BOLNumber", "CustomsStatus", "SubjectCode", "Subject", "CargoType" }, columnNames);
		}
	}

	public void TestHideSelectButtonAndLabel()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var chooser = new MessageChooser(header, new[] { bill }, false);

		using (var chooserDialog = new BillsSelectionDialog(chooser, string.Empty))
		{
			chooserDialog.Show();
			var selectButton = chooserDialog.FindSingle<ZButton>("SelectAllButton");
			var deSelectButton = chooserDialog.FindSingle<ZButton>("DeselectAllButton");
			var descPanel = chooserDialog.FindSingle<ZPanel>("DescPanel");
			CombineAssertions(() =>
			{
				AssertEquals("selectButton", false, selectButton.Visible);
				AssertEquals("deSelectButton", false, deSelectButton.Visible);
				AssertEquals("descPanel", false, descPanel.Visible);
			});
		}
	}

	public void TestGlobalMessageWithNoSelected()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var chooser = new MessageChooser(header, new[] { bill }, false);
		var message = "Cannot send as nothing has been selected.";

		using var chooserDialog = new BillsSelectionDialog(chooser, string.Empty);
		chooserDialog.Show();
		((MessageChooserItem)chooserDialog.BusinessEntity.ChooserItems.First()).EntryType = EntryTypes.Codes.Original;
		chooserDialog.DeSelectAll();

		chooserDialog.FindSingle<ZButton>("SendButton").PerformClick();
		AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestGlobalMessageWithSelected()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var chooser = new MessageChooser(header, new[] { bill }, false);
		var message = "Cannot send as nothing has been selected.";

		using var chooserDialog = new BillsSelectionDialog(chooser, string.Empty);
		chooserDialog.Show();
		((MessageChooserItem)chooserDialog.BusinessEntity.ChooserItems.First()).EntryType = EntryTypes.Codes.Original;
		chooserDialog.SelectAll();

		chooserDialog.FindSingle<ZButton>("SendButton").PerformClick();
		AssertNotEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
	}

	protected override Form GetFormToBashCore()
	{
		var header = Factory.New<AsycudaManifestHeader>();

		var bill1 = header.Bills.AddNew();
		bill1.ABL_BillNumber = "BILL1";

		var bill2 = header.Bills.AddNew();
		bill2.ABL_BillNumber = "BILL2";
		Factory.Save();

		var result = new BillsSelectionDialog(new MessageChooserForTest(header, new[] { bill1, bill2 }, false), "Bills");
		return result;
	}

	sealed class MessageChooserForTest : MessageChooser
	{
		public MessageChooserForTest(AsycudaManifestHeader header, IEnumerable<ASYCUDA.Business.ISelectionItem> items, bool showStatus) : base(header, items, showStatus)
		{
		}

		protected override void DefaultSelect(bool showStatus)
		{
		}
	}
}

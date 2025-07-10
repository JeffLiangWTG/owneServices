using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.ZGridInternals
{
	class CustomizeColumnsControlForTesting : CustomizeColumnsControl
	{
		public CustomizeColumnsControlForTesting()
			: base(nameof(HtmlTextWriterTag.Div))
		{
		}

		public CustomizeColumnsControlForTesting(string tag)
			: base(nameof(HtmlTextWriterTag.Div))
		{
		}

		internal ZListBox availableColumnsListBoxForTesting => availableColumnsListBox;
		internal ZListBox selectedColumnsListBoxForTesting => selectedColumnsListBox;
		internal void OnInitForTesting(EventArgs e) => OnInit(e);
	}

	sealed class CustomizeColumnsControlTest : WebControlTest
	{
		public void TestListBoxFilling()
		{
			using (Culture.SetTemporarily(Culture.Invariant))
			{
				var testControl = new CustomizeColumnsControlForTesting();
				testControl.AvailableColumns.Add(1, "one");
				testControl.AvailableColumns.Add(2, "two");
				testControl.AvailableColumns.Add(3, "three");
				testControl.AvailableColumns.Add(4, "four");
				testControl.AvailableColumns.Add(5, "five");
				testControl.AvailableColumns.Add(6, "six");

				testControl.SelectedColumns.Add(1);
				testControl.SelectedColumns.Add(2);
				testControl.SelectedColumns.Add(3);

				testControl.RequiredColumns.Add(3);
				testControl.RequiredColumns.Add(4);

				testControl.OnInitForTesting(null);

				Assert(testControl.selectedColumnsListBoxForTesting.Items.Count == 4);
				Assert(testControl.availableColumnsListBoxForTesting.Items.Count == 2);
				AssertEquals(testControl.selectedColumnsListBoxForTesting.Items[0].Text, "[four]");
				AssertEquals(testControl.selectedColumnsListBoxForTesting.Items[1].Text, "one");
				AssertEquals(testControl.selectedColumnsListBoxForTesting.Items[2].Text, "two");
				AssertEquals(testControl.selectedColumnsListBoxForTesting.Items[3].Text, "[three]");

				AssertEquals(testControl.availableColumnsListBoxForTesting.Items[0].Text, "five");
				AssertEquals(testControl.availableColumnsListBoxForTesting.Items[1].Text, "six");
			}
		}

		public void TestAvailableColumns_AlphabeticalOrder()
		{
			using (Culture.SetTemporarily(Culture.Invariant))
			{
				var testControl = new CustomizeColumnsControlForTesting();
				testControl.AvailableColumns.Add(1, "one");
				testControl.AvailableColumns.Add(2, "Two");
				testControl.AvailableColumns.Add(3, "three");
				testControl.AvailableColumns.Add(4, "four");
				testControl.AvailableColumns.Add(5, "Five");
				testControl.AvailableColumns.Add(6, "six");

				testControl.OnInitForTesting(null);

				AssertEquals(testControl.availableColumnsListBoxForTesting.Items[0].Text, "Five");
				AssertEquals(testControl.availableColumnsListBoxForTesting.Items[1].Text, "four");
				AssertEquals(testControl.availableColumnsListBoxForTesting.Items[2].Text, "one");
				AssertEquals(testControl.availableColumnsListBoxForTesting.Items[3].Text, "six");
				AssertEquals(testControl.availableColumnsListBoxForTesting.Items[4].Text, "three");
				AssertEquals(testControl.availableColumnsListBoxForTesting.Items[5].Text, "Two");
			}
		}

		public void TestSelectedColumns_ButtonOnClick()
		{
			var testControl = new CustomizeColumnsControlForTesting();
			testControl.OnInitForTesting(null);

			var selectedColumns = testControl.Controls[1];
			var moveAll = (HtmlButton)selectedColumns.Controls[0];
			var moveSelected = (HtmlButton)selectedColumns.Controls[2];
			var removeSelected = (HtmlButton)selectedColumns.Controls[4];
			var removeAll = (HtmlButton)selectedColumns.Controls[6];

			AssertEquals("moveAll(AvailableColumnsLabel, SelectedColumnsLabel, false); return false;", moveAll.Attributes["onClick"]);
			AssertEquals("moveSelected(AvailableColumnsLabel, SelectedColumnsLabel, false); return false;", moveSelected.Attributes["onClick"]);
			AssertEquals("moveSelected(SelectedColumnsLabel, AvailableColumnsLabel, true); return false;", removeSelected.Attributes["onClick"]);
			AssertEquals("moveAll(SelectedColumnsLabel, AvailableColumnsLabel, true); return false;", removeAll.Attributes["onClick"]);
		}

		protected override Control GetNewControl()
		{
			return new CustomizeColumnsControl();
		}
	}
}

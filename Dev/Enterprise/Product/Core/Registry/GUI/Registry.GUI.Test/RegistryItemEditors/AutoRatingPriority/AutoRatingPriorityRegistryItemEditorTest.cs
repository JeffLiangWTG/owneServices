using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AutoRatingPriorityRegistryItemEditor))]
	sealed class AutoRatingPriorityRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		[RequiresSTA]
		public void TestChangePriorityOrders()
		{
			foreach (var changeOrderTestData in TestDataFor_TestChangeOrder)
			{
				TestChangePriorityOrderWithTestData(changeOrderTestData);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1140:ColumnNameCaseAnalyzer", Justification = "AutoRatingPriorityControl.PopulateListControl() converts input to upper invariant")]
		static IEnumerable<ChangeOrderTestData> TestDataFor_TestChangeOrder
		{
			get
			{
				const string originalOrder = "TI_RH_NKCOMMODITYCODE,TI_RS_NKSERVICELEVEL_NI,TI_OH_TRANSPORTPROVIDER,TI_VIALRC";

				var message = "No item selected. Move up";
				var expectedMessage = "You have not selected a priority.";
				yield return new ChangeOrderTestData(message, originalOrder, -1, false, expectedMessage, originalOrder);

				message = "No item selected. Move down";
				yield return new ChangeOrderTestData(message, originalOrder, -1, true, expectedMessage, originalOrder);

				message = "First item selected. Move up";
				expectedMessage = "This priority is already at the top of the list.";
				yield return new ChangeOrderTestData(message, originalOrder, 0, false, expectedMessage, originalOrder);

				message = "First item selected. Move down";
				var expectedOrder = "TI_RS_NKSERVICELEVEL_NI,TI_RH_NKCOMMODITYCODE,TI_OH_TRANSPORTPROVIDER,TI_VIALRC";
				yield return new ChangeOrderTestData(message, originalOrder, 0, true, null, expectedOrder);

				message = "Middle item selected. Move up";
				expectedOrder = "TI_RH_NKCOMMODITYCODE,TI_OH_TRANSPORTPROVIDER,TI_RS_NKSERVICELEVEL_NI,TI_VIALRC";
				yield return new ChangeOrderTestData(message, originalOrder, 2, false, null, expectedOrder);

				message = "Middle item selected. Move down";
				expectedOrder = "TI_RH_NKCOMMODITYCODE,TI_RS_NKSERVICELEVEL_NI,TI_VIALRC,TI_OH_TRANSPORTPROVIDER";
				yield return new ChangeOrderTestData(message, originalOrder, 2, true, null, expectedOrder);

				message = "Last item selected. Move up";
				expectedOrder = "TI_RH_NKCOMMODITYCODE,TI_RS_NKSERVICELEVEL_NI,TI_VIALRC,TI_OH_TRANSPORTPROVIDER";
				yield return new ChangeOrderTestData(message, originalOrder, 3, false, null, expectedOrder);

				message = "Last item selected. Move down";
				expectedMessage = "This priority is already at the bottom of the list.";
				yield return new ChangeOrderTestData(message, originalOrder, 3, true, expectedMessage, originalOrder);
			}
		}

		struct ChangeOrderTestData
		{
			public ChangeOrderTestData(string assertionMessage, string originalOrder, int selectedIndex, bool moveDown, string expectedMessage, string expectedOrder)
			{
				AssertionMessage = assertionMessage;
				OriginalOrder = originalOrder;
				SelectedIndex = selectedIndex;
				MoveDown = moveDown;
				ExpectedMessage = expectedMessage;
				ExpectedOrder = expectedOrder;
			}

			public readonly string AssertionMessage;
			public readonly string OriginalOrder;
			public readonly int SelectedIndex;
			public readonly bool MoveDown;
			public readonly string ExpectedMessage;
			public readonly string ExpectedOrder;
		}

		void TestChangePriorityOrderWithTestData(ChangeOrderTestData testData)
		{
			using (var form = new ZForm())
			{
				var editorPane = (AutoRatingPriorityControl)Editor.NewWinFormsEditorPane();
				form.Controls.Add(editorPane);
				form.Show();

				var priorityListBox = editorPane.Controls.Find("PriorityListBox", false).First() as CargoWise.Windows.UI.KListBox;
				var moveUpButton = editorPane.Controls.Find("MoveUpButton", false).First() as ZButton;
				var moveDownButton = editorPane.Controls.Find("MoveDownButton", false).First() as ZButton;

				editorPane.Priorities = testData.OriginalOrder;
				if (testData.SelectedIndex >= 0)
				{
					priorityListBox.SetSelected(testData.SelectedIndex, true);
				}

				if (testData.MoveDown)
				{
					moveDownButton.PerformClick();
				}
				else
				{
					moveUpButton.PerformClick();
				}

				if (!string.IsNullOrWhiteSpace(testData.ExpectedMessage))
				{
					var unitTestNotification = UnitTestUserNotification.Instance;
					AssertEquals(testData.AssertionMessage + " - Message", testData.ExpectedMessage, unitTestNotification.LastMessage.Text);
				}

				if (!string.IsNullOrWhiteSpace(testData.ExpectedOrder))
				{
					AssertEquals(testData.AssertionMessage + " - Order", testData.ExpectedOrder, editorPane.Priorities);
				}
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new AutoRatingPriorityRegistryItemEditor(new StringRegistryDataType());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AutoRatingPriorityControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			StringRegistryItem result = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new AutoRatingPriorityRegistryEditorInfo();
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new[] { "TI_RH_NKCOMMODITYCODE,TI_RS_NKSERVICELEVEL_NI,TI_OH_TRANSPORTPROVIDER,TI_VIALRC" };
		}

		#endregion
	}
}

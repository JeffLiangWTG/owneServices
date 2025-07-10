using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(BillOfLadingNumberCustomisationControl))]
	sealed class BillOfLadingNumberCustomisationControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		public void TestOceanBillWording()
		{
			GenericWordingTest("V", "Ocean Bill", 10, "Shipment");
		}

		public void TestHouseBillWording()
		{
			GenericWordingTest("S", "House Bill", 15, "Shipmnt");
		}

		public void GenericWordingTest(string prefix, string typeName, int maxLength, string sequenceNumberName)
		{
			var dataType = CreateBillCustomisationRegistryDataType(prefix, typeName, maxLength, sequenceNumberName);

			using (var control = new BillOfLadingNumberCustomisationControl(dataType))
			{
				AssertControlText(control, "removeFountainPrefixCheckBox", string.Format("Remove the \'{0}\' Prefix", prefix));
				AssertControlText(control, "codingOfHouseBillNumberGroupBox", string.Format("Coding of {0}", typeName));
				AssertControlText(control, "maxLengthDividerLabel", string.Format("/{0}", maxLength));
				AssertControlText(control, "useShipmentSequenceNumberCheckBox", string.Format("Use {0} Sequence Number", sequenceNumberName));
			}
		}
		public void TestInitialize_EnableMacroInsertion()
		{
			var billingCustomisation = CreateBillCustomisationRegistryDataType("S", "House Bill", 15, "Shipmnt");

			using (var control = new BillOfLadingNumberCustomisationControl(billingCustomisation))
			{
				AssertNoExceptionThrown(() => control.Initialize(billingCustomisation));

				billingCustomisation.EnableMacroInsertion = true;
				AssertExceptionThrown<ArgumentNullException>(() => control.Initialize(billingCustomisation));

				billingCustomisation.MacroType = ObjectFactory.GetType<IForwardingShipment>();

				AssertNoExceptionThrown(() => control.Initialize(billingCustomisation));
			}
		}

		#region TestInsertFiledButton

		public void TestInsertFieldButton()
		{
			var mapTreePresentationManager =
				new Mock<IMapTreePresentationManager>(MockBehavior.Loose) { CallBase = true };
			var selectedMacro = "<dummy>";
			mapTreePresentationManager.Setup(m => m.ParentTypes).Returns((Type[])null);
			mapTreePresentationManager.Setup(m => m.GetUserSelectionMacro()).Returns(selectedMacro);
			mapTreePresentationManager.Setup(m => m.Dispose());

			var billingCustomisation = CreateBillCustomisationRegistryDataType("S", "House Bill", 15, "Shipmnt");
			billingCustomisation.EnableMacroInsertion = true;
			billingCustomisation.MacroType = ObjectFactory.GetType<IForwardingShipment>();

			var editor = new BillOfLadingNumberCustomisationRegistryItemEditor(billingCustomisation, null, Factory);

			using (ObjectFactory.Substitute(mapTreePresentationManager.Object))
			using (var dummyForm = new ZForm())
			using (var control = editor.NewWinFormsEditorPane())
			{
				editor.SetValueFromEditorPane(control, new BillOfLadingNumberCustomisation());
				dummyForm.Controls.Add(control);
				dummyForm.Show();
				var grid = control.Controls.Find("elementsGrid", true)[0] as ZGrid;
				var insertFieldButton = control.Controls.Find("insertFieldButton", true)[0] as ZButton;
				var codingOfHouseBillNumberGroupBox = control.Controls.Find("codingOfHouseBillNumberGroupBox", true)[0] as GroupBox;
				var detailStyle = grid.ColumnStyles.ToList<ZGridColumnInfo>().FirstOrDefault(zgridColumnInfo => zgridColumnInfo.ColumnName.Equals("Detail"));

				Assert("insertFieldButton should be visible", insertFieldButton.Visible);
				AssertEquals("codingOfHouseBillNumberGroupBox height should be increased", 91, codingOfHouseBillNumberGroupBox.Height);
				AssertEquals("CharacterCasing should be Normal", CharacterCasing.Normal, detailStyle.CharacterCasing);

				grid.CurrentCell = new DataGridCell(1, 3);
				grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")] = true;
				Assert("InsertField button should be enabled", insertFieldButton.Enabled);
				insertFieldButton.PerformClick();
				mapTreePresentationManager.Verify(m => m.GetUserSelectionMacro(), Times.Once);
				mapTreePresentationManager.Verify(m => m.Dispose(), Times.Once);
				var currentCellStringValue = new ZString(grid[grid.CurrentCell.RowNumber, grid.CurrentCell.ColumnNumber]);
				AssertEquals(selectedMacro.ToUpper(), currentCellStringValue.ToUpper());
				AssertEquals(false, grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")]);

				grid.CurrentCell = new DataGridCell(3, 3);
				grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")] = true;
				Assert("InsertField button should be enabled", insertFieldButton.Enabled);
				insertFieldButton.PerformClick();
				mapTreePresentationManager.Verify(m => m.GetUserSelectionMacro(), Times.Exactly(2));
				mapTreePresentationManager.Verify(m => m.Dispose(), Times.Exactly(2));
				currentCellStringValue = new ZString(grid[grid.CurrentCell.RowNumber, grid.CurrentCell.ColumnNumber]);
				AssertEquals(selectedMacro.ToUpper(), currentCellStringValue.ToUpper());
				AssertEquals(false, grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")]);

				grid.CurrentCell = new DataGridCell(4, 3);
				grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")] = true;
				Assert("InsertField button should be disabled", !insertFieldButton.Enabled);
				AssertEquals(true, grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")]);

				grid.CurrentCell = new DataGridCell(3, 0);
				grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")] = true;
				Assert("InsertField button should be disabled", !insertFieldButton.Enabled);
				insertFieldButton.PerformClick();
				mapTreePresentationManager.Verify(m => m.GetUserSelectionMacro(), Times.Exactly(2));
				mapTreePresentationManager.Verify(m => m.Dispose(), Times.Exactly(2));
				currentCellStringValue = new ZString(grid[grid.CurrentCell.RowNumber, grid.CurrentCell.ColumnNumber]);
				AssertEquals("Custom Element 3".ToUpper(), currentCellStringValue.ToUpper());
				AssertEquals(true, grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")]);
			}

			billingCustomisation.EnableMacroInsertion = false;
			editor = new BillOfLadingNumberCustomisationRegistryItemEditor(billingCustomisation, null, Factory);
			using (ObjectFactory.Substitute(mapTreePresentationManager.Object))
			using (var dummyForm = new ZForm())
			using (var control = editor.NewWinFormsEditorPane())
			{
				editor.SetValueFromEditorPane(control, new BillOfLadingNumberCustomisation());
				dummyForm.Controls.Add(control);
				dummyForm.Show();
				var grid = control.Controls.Find("elementsGrid", true)[0] as ZGrid;
				var insertFieldButton = control.Controls.Find("insertFieldButton", true)[0] as ZButton;
				var codingOfHouseBillNumberGroupBox = control.Controls.Find("codingOfHouseBillNumberGroupBox", true)[0] as GroupBox;
				var detailStyle = grid.ColumnStyles.ToList<ZGridColumnInfo>().FirstOrDefault(zgridColumnInfo => zgridColumnInfo.ColumnName.Equals("Detail"));

				Assert("insertFieldButton should not be visible", !insertFieldButton.Visible);
				AssertEquals("codingOfHouseBillNumberGroupBox height should be decreased", 68, codingOfHouseBillNumberGroupBox.Height);
				AssertEquals("CharacterCasing should be Upper", CharacterCasing.Upper, detailStyle.CharacterCasing);
			}
		}

		public void TestInsertFieldButton_WhsOrder()
		{
			var mapTreePresentationManager =
				new Mock<IMapTreePresentationManager>(MockBehavior.Loose) { CallBase = true };
			var selectedMacro = "<dummy>";
			mapTreePresentationManager.Setup(m => m.ParentTypes).Returns(new Type[] { ObjectFactory.GetType<IWhsOrder>() });
			mapTreePresentationManager.Setup(m => m.GetUserSelectionMacro()).Returns(selectedMacro);
			mapTreePresentationManager.Setup(m => m.Dispose());

			var billingCustomisation = CreateBillCustomisationRegistryDataType("W", "Order Docket ID Format", 25, "Order");
			billingCustomisation.EnableMacroInsertion = true;
			billingCustomisation.MacroType = ObjectFactory.GetType<IWhsOrder>();

			var editor = new BillOfLadingNumberCustomisationRegistryItemEditor(billingCustomisation, null, Factory);

			using (ObjectFactory.Substitute(mapTreePresentationManager.Object))
			using (var dummyForm = new ZForm())
			using (var control = editor.NewWinFormsEditorPane())
			{
				editor.SetValueFromEditorPane(control, new BillOfLadingNumberCustomisation());
				dummyForm.Controls.Add(control);
				dummyForm.Show();
				var grid = control.Controls.Find("elementsGrid", true)[0] as ZGrid;
				var insertFieldButton = control.Controls.Find("insertFieldButton", true)[0] as ZButton;
				var codingOfHouseBillNumberGroupBox = control.Controls.Find("codingOfHouseBillNumberGroupBox", true)[0] as GroupBox;
				var detailStyle = grid.ColumnStyles.ToList<ZGridColumnInfo>().FirstOrDefault(zgridColumnInfo => zgridColumnInfo.ColumnName.Equals("Detail"));

				Assert("insertFieldButton should be visible", insertFieldButton.Visible);
				AssertEquals("codingOfHouseBillNumberGroupBox height should be increased", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(91), codingOfHouseBillNumberGroupBox.Height);
				AssertEquals("CharacterCasing should be Normal", CharacterCasing.Normal, detailStyle.CharacterCasing);

				grid.CurrentCell = new DataGridCell(1, 3);
				grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")] = true;
				Assert("InsertField button should be enabled", insertFieldButton.Enabled);
				insertFieldButton.PerformClick();
				mapTreePresentationManager.Verify(m => m.GetUserSelectionMacro(), Times.Once);
				mapTreePresentationManager.Verify(m => m.Dispose(), Times.Once);
				mapTreePresentationManager.VerifySet(x => x.ParentTypes = new Type[] { ObjectFactory.GetType<IWhsOrder>() });
				var currentCellStringValue = new ZString(grid[grid.CurrentCell.RowNumber, grid.CurrentCell.ColumnNumber]);
				AssertEquals(selectedMacro.ToUpper(), currentCellStringValue.ToUpper());
				AssertEquals(false, grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")]);

				grid.CurrentCell = new DataGridCell(3, 3);
				grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")] = true;
				Assert("InsertField button should be enabled", insertFieldButton.Enabled);
				insertFieldButton.PerformClick();
				mapTreePresentationManager.Verify(m => m.GetUserSelectionMacro(), Times.Exactly(2));
				mapTreePresentationManager.Verify(m => m.Dispose(), Times.Exactly(2));
				mapTreePresentationManager.VerifySet(x => x.ParentTypes = new Type[] { ObjectFactory.GetType<IWhsOrder>() });
				currentCellStringValue = new ZString(grid[grid.CurrentCell.RowNumber, grid.CurrentCell.ColumnNumber]);
				AssertEquals(selectedMacro.ToUpper(), currentCellStringValue.ToUpper());
				AssertEquals(false, grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")]);

				grid.CurrentCell = new DataGridCell(4, 3);
				grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")] = true;
				Assert("InsertField button should be disabled", !insertFieldButton.Enabled);
				AssertEquals(true, grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")]);

				grid.CurrentCell = new DataGridCell(3, 0);
				grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")] = true;
				Assert("InsertField button should be disabled", !insertFieldButton.Enabled);
				insertFieldButton.PerformClick();
				mapTreePresentationManager.Verify(m => m.GetUserSelectionMacro(), Times.Exactly(2));
				mapTreePresentationManager.Verify(m => m.Dispose(), Times.Exactly(2));
				mapTreePresentationManager.VerifySet(x => x.ParentTypes = new Type[] { ObjectFactory.GetType<IWhsOrder>() });
				currentCellStringValue = new ZString(grid[grid.CurrentCell.RowNumber, grid.CurrentCell.ColumnNumber]);
				AssertEquals("Custom Element 3".ToUpper(), currentCellStringValue.ToUpper());
				AssertEquals(true, grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")]);
			}
		}
		public void TestInsertFieldButton_WhsReceive()
		{
			var mapTreePresentationManager =
				new Mock<IMapTreePresentationManager>(MockBehavior.Loose) { CallBase = true };
			var selectedMacro = "<dummy>";
			mapTreePresentationManager.Setup(m => m.ParentTypes).Returns(new Type[] { ObjectFactory.GetType<IWhsReceive>() });
			mapTreePresentationManager.Setup(m => m.GetUserSelectionMacro()).Returns(selectedMacro);
			mapTreePresentationManager.Setup(m => m.Dispose());

			var billingCustomisation = CreateBillCustomisationRegistryDataType("W", "Order Docket ID Format", 25, "Order");
			billingCustomisation.EnableMacroInsertion = true;
			billingCustomisation.MacroType = ObjectFactory.GetType<IWhsReceive>();

			var editor = new BillOfLadingNumberCustomisationRegistryItemEditor(billingCustomisation, null, Factory);

			using (ObjectFactory.Substitute(mapTreePresentationManager.Object))
			using (var dummyForm = new ZForm())
			using (var control = editor.NewWinFormsEditorPane())
			{
				editor.SetValueFromEditorPane(control, new BillOfLadingNumberCustomisation());
				dummyForm.Controls.Add(control);
				dummyForm.Show();
				var grid = control.Controls.Find("elementsGrid", true)[0] as ZGrid;
				var insertFieldButton = control.Controls.Find("insertFieldButton", true)[0] as ZButton;
				var codingOfHouseBillNumberGroupBox = control.Controls.Find("codingOfHouseBillNumberGroupBox", true)[0] as GroupBox;
				var detailStyle = grid.ColumnStyles.ToList<ZGridColumnInfo>().FirstOrDefault(zgridColumnInfo => zgridColumnInfo.ColumnName.Equals("Detail"));

				Assert("insertFieldButton should be visible", insertFieldButton.Visible);
				AssertEquals("codingOfHouseBillNumberGroupBox height should be increased", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(91), codingOfHouseBillNumberGroupBox.Height);
				AssertEquals("CharacterCasing should be Normal", CharacterCasing.Normal, detailStyle.CharacterCasing);

				grid.CurrentCell = new DataGridCell(1, 3);
				grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")] = true;
				Assert("InsertField button should be enabled", insertFieldButton.Enabled);
				insertFieldButton.PerformClick();
				mapTreePresentationManager.Verify(m => m.GetUserSelectionMacro(), Times.Once);
				mapTreePresentationManager.Verify(m => m.Dispose(), Times.Once);
				mapTreePresentationManager.VerifySet(x => x.ParentTypes = new Type[] { ObjectFactory.GetType<IWhsReceive>() });
				var currentCellStringValue = new ZString(grid[grid.CurrentCell.RowNumber, grid.CurrentCell.ColumnNumber]);
				AssertEquals(selectedMacro.ToUpper(), currentCellStringValue.ToUpper());
				AssertEquals(false, grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")]);

				grid.CurrentCell = new DataGridCell(3, 3);
				grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")] = true;
				Assert("InsertField button should be enabled", insertFieldButton.Enabled);
				insertFieldButton.PerformClick();
				mapTreePresentationManager.Verify(m => m.GetUserSelectionMacro(), Times.Exactly(2));
				mapTreePresentationManager.Verify(m => m.Dispose(), Times.Exactly(2));
				mapTreePresentationManager.VerifySet(x => x.ParentTypes = new Type[] { ObjectFactory.GetType<IWhsReceive>() });
				currentCellStringValue = new ZString(grid[grid.CurrentCell.RowNumber, grid.CurrentCell.ColumnNumber]);
				AssertEquals(selectedMacro.ToUpper(), currentCellStringValue.ToUpper());
				AssertEquals(false, grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")]);

				grid.CurrentCell = new DataGridCell(4, 3);
				grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")] = true;
				Assert("InsertField button should be disabled", !insertFieldButton.Enabled);
				AssertEquals(true, grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")]);

				grid.CurrentCell = new DataGridCell(3, 0);
				grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")] = true;
				Assert("InsertField button should be disabled", !insertFieldButton.Enabled);
				insertFieldButton.PerformClick();
				mapTreePresentationManager.Verify(m => m.GetUserSelectionMacro(), Times.Exactly(2));
				mapTreePresentationManager.Verify(m => m.Dispose(), Times.Exactly(2));
				mapTreePresentationManager.VerifySet(x => x.ParentTypes = new Type[] { ObjectFactory.GetType<IWhsReceive>() });
				currentCellStringValue = new ZString(grid[grid.CurrentCell.RowNumber, grid.CurrentCell.ColumnNumber]);
				AssertEquals("Custom Element 3".ToUpper(), currentCellStringValue.ToUpper());
				AssertEquals(true, grid[grid.CurrentCell.RowNumber, grid.Columns.IndexOf(column => column.ColumnName == "Fountain")]);
			}
		}

		#endregion

		#region TestHideUseShipmentCheckBoxIfRequired

		public void TestHideUseShipmentCheckBoxIfRequired()
		{
			var dataType = new BillCustomisationRegistryDataType();
			dataType.FountainPrefix = "XX";
			dataType.Categories = NumberCustomisationElementCategories.Default;

			using (var control = new BillOfLadingNumberCustomisationControl(dataType))
			{
				AssertEquals("Default Visibility", true, control.useShipmentSequenceNumberCheckBox.Visible);
				AssertNotEquals("Default Location", control.useShipmentSequenceNumberCheckBox.Location, control.removeFountainPrefixCheckBox.Location);
			}

			dataType.Categories = NumberCustomisationElementCategories.SupplierBooking;
			using (var control = new BillOfLadingNumberCustomisationControl(dataType))
			{
				AssertEquals("Supplier Booking Visibility", false, control.useShipmentSequenceNumberCheckBox.Visible);
				AssertEquals("Supplier Booking Location", control.useShipmentSequenceNumberCheckBox.Location, control.removeFountainPrefixCheckBox.Location);
			}

			dataType.Categories |= NumberCustomisationElementCategories.Standard;
			using (var control = new BillOfLadingNumberCustomisationControl(dataType))
			{
				AssertEquals("Supplier Booking Visibility", false, control.useShipmentSequenceNumberCheckBox.Visible);
				AssertEquals("Supplier Booking Location", control.useShipmentSequenceNumberCheckBox.Location, control.removeFountainPrefixCheckBox.Location);
			}

			dataType.Categories = NumberCustomisationElementCategories.ClientContract;
			using (var control = new BillOfLadingNumberCustomisationControl(dataType))
			{
				AssertEquals("Client Contract Visibility", false, control.useShipmentSequenceNumberCheckBox.Visible);
				AssertEquals("Client Contract Location", control.useShipmentSequenceNumberCheckBox.Location, control.removeFountainPrefixCheckBox.Location);
			}

			dataType.Categories = NumberCustomisationElementCategories.Domestic;
			using (var control = new BillOfLadingNumberCustomisationControl(dataType))
			{
				AssertEquals("Domestic Visibility", false, control.useShipmentSequenceNumberCheckBox.Visible);
				AssertEquals("Domestic Location", control.useShipmentSequenceNumberCheckBox.Location, control.removeFountainPrefixCheckBox.Location);
			}

			dataType.Categories = NumberCustomisationElementCategories.WarehouseJob;
			using (var control = new BillOfLadingNumberCustomisationControl(dataType))
			{
				AssertEquals("Warehouse Visibility", false, control.useShipmentSequenceNumberCheckBox.Visible);
				AssertEquals("Warehouse Location", control.useShipmentSequenceNumberCheckBox.Location, control.removeFountainPrefixCheckBox.Location);
			}
		}

		#endregion

		#region Implementation

		void AssertControlText(BillOfLadingNumberCustomisationControl control, string name, string expectedText)
		{
			AssertEquals(name, expectedText, GetControl(control, name).Text);
		}

		Control GetControl(BillOfLadingNumberCustomisationControl control, string name)
		{
			return (Control)typeof(BillOfLadingNumberCustomisationControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new BillOfLadingNumberCustomisation();
		}

		protected override RegistryZUserControl GetNewControl()
		{
			BillCustomisationRegistryDataType dataType = new BillCustomisationRegistryDataType();
			return new BillOfLadingNumberCustomisationControl(dataType);
		}

		BillCustomisationRegistryDataType CreateBillCustomisationRegistryDataType(string prefix, string typeName, int maxLength, string sequenceNumberName)
		{
			var dataType = new BillCustomisationRegistryDataType();
			dataType.FountainPrefix = prefix;
			dataType.GeneratedNumberName = (NoResString)typeName;
			dataType.MaxLength = maxLength;
			dataType.SequenceNumberName = (NoResString)sequenceNumberName;
			return dataType;
		}

		#endregion
	}
}

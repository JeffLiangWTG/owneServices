using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class HouseConsignmentAdditionalDocumentsOverviewUserControlTest : TestCaseWithFactory
	{
		public void TestGrid()
		{
			AssertType<ZGrid>(userControl.HouseConsignmentAdditionalDocumentsOverviewGrid);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { "CSI_LineNo", "CSI_Status", "CSI_SubType", "CSI_Code", "CSI_ReferenceNumber", "CSI_Description" },
						gridUserControl.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumnsWidth()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_LineNo", 80, gridUserControl.GetColumnStyle("CSI_LineNo").Width);
				AssertEquals("CSI_Status", 90, gridUserControl.GetColumnStyle("CSI_Status").Width);
				AssertEquals("CSI_SubType", 80, gridUserControl.GetColumnStyle("CSI_SubType").Width);
				AssertEquals("CSI_Code", 80, gridUserControl.GetColumnStyle("CSI_Code").Width);
				AssertEquals("CSI_ReferenceNumber", 160, gridUserControl.GetColumnStyle("CSI_ReferenceNumber").Width);
				AssertEquals("CSI_Description", 480, gridUserControl.GetColumnStyle("CSI_Description").Width);
			});
		}

		public void TestColumnsTypes()
		{
			var map = new Dictionary<string, Type>();
			map.Add("CSI_LineNo", typeof(ZTextBoxColumnStyleInfo));
			map.Add("CSI_Status", typeof(ZDropEditColumnStyleInfo));
			map.Add("CSI_SubType", typeof(ZDropEditColumnStyleInfo));
			map.Add("CSI_Code", typeof(ZCodeFindBoxColumnStyleInfo));
			map.Add("CSI_ReferenceNumber", typeof(ZTextBoxColumnStyleInfo));
			map.Add("CSI_Description", typeof(ZTextBoxColumnStyleInfo));
			var columnStyles = gridUserControl.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
			CombineAssertions(() =>
			{
				foreach (var columnName in map.Keys)
				{
					var columnStyle = columnStyles.Where(x => x.ColumnName == columnName).SingleOrDefault();
					var type = map[columnName];
					AssertType($"Column of name {columnName} should have a style of type {type}", type, columnStyle);
				}
			});
		}

		public void TestSequenceNumberDisabled()
		{
			AssertEquals("CSI_LineNo Disabled", true, gridUserControl.GetColumnStyle("CSI_LineNo").IsReadOnly);
		}

		public void TestUnloadedStateDisabled()
		{
			AssertEquals("CSI_Status Disabled", false, gridUserControl.GetColumnStyle("CSI_Status").IsReadOnly);
		}

		public void TestKindDisabled()
		{
			AssertEquals("CSI_SubType Disabled", false, gridUserControl.GetColumnStyle("CSI_SubType").IsReadOnly);
		}

		public void TestDocTypeDisabled()
		{
			AssertEquals("CSI_Code Disabled", false, gridUserControl.GetColumnStyle("CSI_Code").IsReadOnly);
		}

		public void TesFieldsDisabledForUnloadedStateDEC()
		{
			CusSupportingInfo supportingInfo = Factory.NewWithValidTestData<CusSupportingInfo>();

			using (var form = new ZForm(supportingInfo))
			using (var control = new HouseConsignmentAdditionalDocumentsOverviewUserControl())
			{
				supportingInfo.CSI_Status = "DEC";

				form.Controls.Add(control);
				form.Show();

				var sequenceNumberTextBox = control.FindSingle<ZTextBox>("SequenceNumbertextBox");
				var kindDropEdit = control.FindSingle<ZDropEdit>("KindDropEdit");
				var documentTypeDropEdit = control.FindSingle<ZDropEdit>("DocumentTypeDropEdit");
				var referenceNumberTextBox = control.FindSingle<ZTextBox>("ReferenceNumberTextBox");
				var textTextBox = control.FindSingle<ZTextBox>("TextTextBox");
				var unloadedStateTextBox = control.FindSingle<ZTextBox>("UnloadedStatetextBox");

				AssertEquals("Unloaded state text box should be disabled for unloaded state DEC", true, unloadedStateTextBox.ReadOnly);
				AssertEquals("Text text box should be disabled for unloaded state DEC", true, textTextBox.ReadOnly);
				AssertEquals("Reference number text box should be disabled for unloaded state DEC", true, referenceNumberTextBox.ReadOnly);
				AssertEquals("Document type drop edit should be disabled for unloaded state DEC", true, documentTypeDropEdit.ReadOnly);
				AssertEquals("Kind drop edit should be disabled for unloaded state DEC", true, kindDropEdit.ReadOnly);
				AssertEquals("Sequence number text box should be disabled for unloaded state DEC", true, sequenceNumberTextBox.ReadOnly);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HouseConsignmentAdditionalDocumentsOverviewUserControl();
			gridUserControl = userControl.HouseConsignmentAdditionalDocumentsOverviewGrid;
		}
		ZGrid gridUserControl;
		HouseConsignmentAdditionalDocumentsOverviewUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}

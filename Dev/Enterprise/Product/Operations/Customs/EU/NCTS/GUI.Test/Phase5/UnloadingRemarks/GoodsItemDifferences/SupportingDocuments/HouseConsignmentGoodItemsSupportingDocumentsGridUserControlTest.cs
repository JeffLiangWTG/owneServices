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
	class HouseConsignmentGoodItemsSupportingDocumentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestGrid()
		{
			AssertType<ZGrid>(houseConsigmentGoodItemsSupportingDocumentsGrid);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] { "CSI_LineNo", "CSI_Status", "CSI_Code", "CSI_ReferenceNumber", "CSI_ReferenceNumber2" },
				houseConsigmentGoodItemsSupportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumnsWidth()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_LineNo", 80, houseConsigmentGoodItemsSupportingDocumentsGrid.GetColumnStyle("CSI_LineNo").Width);
				AssertEquals("CSI_Status", 90, houseConsigmentGoodItemsSupportingDocumentsGrid.GetColumnStyle("CSI_Status").Width);
				AssertEquals("CSI_Code", 80, houseConsigmentGoodItemsSupportingDocumentsGrid.GetColumnStyle("CSI_Code").Width);
				AssertEquals("CSI_ReferenceNumber", 160, houseConsigmentGoodItemsSupportingDocumentsGrid.GetColumnStyle("CSI_ReferenceNumber").Width);
				AssertEquals("CSI_ReferenceNumber2", 480, houseConsigmentGoodItemsSupportingDocumentsGrid.GetColumnStyle("CSI_ReferenceNumber2").Width);
			});
		}

		public void TestColumnsTypes()
		{
			var map = new Dictionary<string, Type>();
			map.Add("CSI_LineNo", typeof(ZTextBoxColumnStyleInfo));
			map.Add("CSI_Status", typeof(ZDropEditColumnStyleInfo));
			map.Add("CSI_Code", typeof(ZCodeFindBoxColumnStyleInfo));
			map.Add("CSI_ReferenceNumber", typeof(ZTextBoxColumnStyleInfo));
			map.Add("CSI_ReferenceNumber2", typeof(ZTextBoxColumnStyleInfo));
			var columnStyles = houseConsigmentGoodItemsSupportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
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

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HouseConsignmentGoodItemsSupportingDocumentsGridUserControl();
			houseConsigmentGoodItemsSupportingDocumentsGrid = userControl.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid;
		}
		HouseConsignmentGoodItemsSupportingDocumentsGridUserControl userControl;
		ZGrid houseConsigmentGoodItemsSupportingDocumentsGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}

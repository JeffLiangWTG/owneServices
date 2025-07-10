using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	class ForeignOperatorFilterStripControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			var foreignOp = new CusBRForeignOperatorCollection(Factory);
			var filterBO = new ForeignOperatorFilterBusinessObject();
			using (var userControl = new ForeignOperatorFilterStripControl(foreignOp, filterBO))
			{
				var grid = userControl.FilteredGrid;
				CombineAssertions(() =>
				{
					AssertNotNull("BFR_OH_Owner", grid.GetColumnStyle("BFR_OH_Owner"));
					AssertNotNull("BFR_MessageStatus", grid.GetColumnStyle("BFR_MessageStatus"));
					AssertNotNull("BFR_CustomsStatus", grid.GetColumnStyle("BFR_CustomsStatus"));
					AssertNotNull("BFR_AuthorityIdentifier", grid.GetColumnStyle("BFR_AuthorityIdentifier"));
					AssertNotNull("BFR_AuthorityVersion", grid.GetColumnStyle("BFR_AuthorityVersion"));
					AssertNotNull("ForeignOperatorName", grid.GetColumnStyle("ForeignOperatorName"));
					AssertNotNull("ForeignOperatorCountry", grid.GetColumnStyle("ForeignOperatorCountry"));
				});
			}
		}

		public void TestGridOrderColumns()
		{
			var foreignOp = new CusBRForeignOperatorCollection(Factory);
			var filterBO = new ForeignOperatorFilterBusinessObject();

			using (var form = new ZForm())
			using (var userControl = new ForeignOperatorFilterStripControl(foreignOp, filterBO))
			{
				form.Controls.Add(userControl);
				form.Show();

				var grid = userControl.FilteredGrid;

				for (int i = 0; i < ExpectedColumnsOrder.Count; i++)
				{
					var expectedColumnName = ExpectedColumnsOrder[i];
					AssertEquals("Expected", expectedColumnName, grid.Columns[i].ColumnStyle.MappingName);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			var foreignOp = new CusBRForeignOperatorCollection(Factory);
			var filterBO = new ForeignOperatorFilterBusinessObject();
			using (var form = new ZForm())
			{
				var filterControl = new ForeignOperatorFilterStripControl(foreignOp, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}

		List<string> expectedColumnsOrder;

		List<string> ExpectedColumnsOrder
		{
			get
			{
				if (expectedColumnsOrder == null)
				{
					expectedColumnsOrder = new List<string>();
					expectedColumnsOrder.Add(CusBRForeignOperator.Schema.BFR_AuthorityIdentifier);
					expectedColumnsOrder.Add(CusBRForeignOperator.Schema.BFR_AuthorityVersion);
					expectedColumnsOrder.Add(CusBRForeignOperator.Schema.BFR_CustomsStatus);
					expectedColumnsOrder.Add(CusBRForeignOperator.Schema.BFR_OH_Owner);
					expectedColumnsOrder.Add(CusBRForeignOperator.Schema.ForeignOperatorName);
					expectedColumnsOrder.Add(CusBRForeignOperator.Schema.ForeignOperatorCountry);
					expectedColumnsOrder.Add(CusBRForeignOperator.Schema.BFR_MessageStatus);
				}
				return expectedColumnsOrder;
			}
		}
	}
}

using System;
using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZTimelineColumnTest : ZDateTimeColumnTest
	{
		protected override Type ExpectedColumnType
		{
			get { return typeof(ZTimelineColumn); }
		}

		public override void TestSortExpression()
		{
			AssertNotNull("Should be ZDateTimeColumnWithSuppression", TestColumn);
			AssertEquals("SortExpression should be start with " + CustomSorterSupportConst.IsCustomSorter, CustomSorterSupportConst.IsCustomSorter + DummyBusinessObject.Schema.Z0_Date + DummyBusinessObject.Schema.Z0_AnotherDate, TestColumn.SortExpression);
		}

		public void TestGetCustomSorter()
		{
			AssertNotNull("Should be ZDateTimeColumnWithSuppression", TestColumn);
			AssertEquals("Should be custom sorter with suppression", typeof(ActualEstimateCollectionSorter), TestColumn.GetCustomSorter(ListSortDirection.Ascending).GetType());
		}

		new ZTimelineColumn TestColumn
		{
			get
			{
				if (fTestColumn == null)
				{
					fTestColumn = new ZTimelineColumn("TestColumn", DummyBusinessObject.Schema.Z0_Date, DummyBusinessObject.Schema.Z0_AnotherDate, ZDateTimePickerFormat.Short);
				}
				return fTestColumn as ZTimelineColumn;
			}
		}
	}
}

using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class K84ReportsFilterControlTest : TestCaseWithFactory
	{
		public void TestFilterControl()
		{
			using (var module = new K84ReportsModule())
			{
				using (var form = new ZChildForm(module.GridCollection))
				{
					var control = (K84ReportsFilterControl)module.EmbeddedControl;
					form.Controls.Add(control);
					form.Show();
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.K84StatementDate));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.K84AccountingDate));

					var grid = control.FilteredGrid;
					grid.ResetColumns();
					Assert("K84ReportsFilterControl grid count must have at least " + ExpectedColumnNamesInSortOrderList.Count.ToString(), ExpectedColumnNamesInSortOrderList.Count <= grid.Columns.Count);
					for (int i = 0; i < ExpectedColumnNamesInSortOrderList.Count; i++)
					{
						var column = grid.Columns[i];
						AssertNotNull(column);
						var expectedColumnName = ExpectedColumnNamesInSortOrderList[i];
						AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
						AssertEquals("IsVisible", true, column.IsVisible);
					}
				}
			}
		}

		List<ZString> ExpectedColumnNamesInSortOrderList
		{
			get
			{
				if (expectedColumnNamesInSortOrderList == null)
				{
					expectedColumnNamesInSortOrderList = new List<ZString>();
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageType);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageSubType);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageSubTypeDescription);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.K84StatementDate);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.K84AccountingDate);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageDateTime);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_InterchangeSender);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_SendingUser);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_Status);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_SendOrReceiveHumanReadable);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageNum);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_DateTimeInterchangeSent);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_InterchangeNumber);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_InterchangeStatus);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_ApplicationCode);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_ApplicationReference);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageTextShort);
				}
				return expectedColumnNamesInSortOrderList;
			}
		}
		List<ZString> expectedColumnNamesInSortOrderList;
	}
}

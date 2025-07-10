using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class CAQueryMessagesFilterControlTest : TestCaseWithFactory
	{
		public void TestFilterControl()
		{
			using (var module = new CAQueryMessagesModule())
			{
				using (var form = new ZChildForm(module.GridCollection))
				{
					var control = (CAQueryMessagesFilterControl)module.EmbeddedControl;
					form.Controls.Add(control);
					form.Show();
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.BatchNumber));
					AssertEquals("BatchNumber width", 60, control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.BatchNumber));
					AssertEquals("Message Type width", 40, control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.EM_MessageType));
					AssertEquals("Sub Type Description width", 100, control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.EM_MessageSubTypeDescription));
					AssertEquals("Direction width", 55, control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.EM_SendOrReceiveHumanReadable));
					AssertEquals("Status width", 40, control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.EM_Status));
					AssertEquals("Message Date width", 95, control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.EM_MessageDateTime));
					AssertEquals("Sub Type width", 40, control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.EM_MessageSubType));
					AssertEquals("Message Held Until Date width", 95, control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.EM_HeldUntilDate));

					var grid = control.FilteredGrid;
					grid.ResetColumns();
					Assert("CAQueryMessagesFilterControl grid count must have at least " + ExpectedColumnNamesInSortOrderList.Count.ToString(), ExpectedColumnNamesInSortOrderList.Count <= grid.Columns.Count);
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
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.BatchNumber);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageType);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageSubTypeDescription);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_SendOrReceiveHumanReadable);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_Status);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageDateTime);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_InterchangeSender);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_SendingUser);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageNum);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageSubType);
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

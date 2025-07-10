using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class CAReleaseNotificationsFilterControlTest : TestCaseWithFactory
	{
		public void TestFilterControl()
		{
			using (var module = new CAReleaseNotificationsModule())
			{
				using (var form = new ZChildForm(module.GridCollection))
				{
					var control = (CAReleaseNotificationsFilterControl)module.EmbeddedControl;
					form.Controls.Add(control);
					form.Show();
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.LinkedObjectReference));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.TransactionNumber));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.CargoControlNumber));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.SubLocation));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.CBSAOffice));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.RNSProcessingDate));
					Assert(control.FilteredGrid.Columns.Contains(EDIMessage.Schema.RNSReleaseDate));
					AssertEquals("LinkedObjectReference width", ControlDpiScalingHelper.ScaleToCurrentDpiX(70), control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.LinkedObjectReference));
					AssertEquals("TransactionNumber width", ControlDpiScalingHelper.ScaleToCurrentDpiX(100), control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.TransactionNumber));
					AssertEquals("CargoControlNumber width", ControlDpiScalingHelper.ScaleToCurrentDpiX(100), control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.CargoControlNumber));
					AssertEquals("Message Type width", ControlDpiScalingHelper.ScaleToCurrentDpiX(40), control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.EM_MessageType));
					AssertEquals("Sub Type Description width", ControlDpiScalingHelper.ScaleToCurrentDpiX(100), control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.EM_MessageSubTypeDescription));
					AssertEquals("Direction width", ControlDpiScalingHelper.ScaleToCurrentDpiX(55), control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.EM_SendOrReceiveHumanReadable));
					AssertEquals("Status width", ControlDpiScalingHelper.ScaleToCurrentDpiX(40), control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.EM_Status));
					AssertEquals("Message Date width", ControlDpiScalingHelper.ScaleToCurrentDpiX(95), control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.EM_MessageDateTime));
					AssertEquals("Sub Type width", ControlDpiScalingHelper.ScaleToCurrentDpiX(40), control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.EM_MessageSubType));
					AssertEquals("Sub-Location", ControlDpiScalingHelper.ScaleToCurrentDpiX(50), control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.SubLocation));
					AssertEquals("CBSA Office", ControlDpiScalingHelper.ScaleToCurrentDpiX(50), control.FilteredGrid.GetColumnWidth(EDIMessage.Schema.CBSAOffice));

					var grid = control.FilteredGrid;
					grid.ResetColumns();
					Assert("CAReleaseNotificationsFilterControl grid count must have at least " + ExpectedColumnNamesInSortOrderList.Count.ToString(), ExpectedColumnNamesInSortOrderList.Count <= grid.Columns.Count);
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
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.CargoControlNumber);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.LinkedObjectReference);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.TransactionNumber);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.SubLocation);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.CBSAOffice);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageType);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_Status);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.RNSReleaseDate);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.RNSProcessingDate);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageSubType);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageSubTypeDescription);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageDateTime);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_InterchangeSender);
					expectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_SendingUser);
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

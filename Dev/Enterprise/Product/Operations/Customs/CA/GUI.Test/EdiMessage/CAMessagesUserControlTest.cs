using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CAMessagesUserControlTest : TestCaseWithFactory
	{
		public void TestCAMessagesUserControlDetails()
		{
			var boWithMessages = Factory.New<CusCAeMHHouse>();
			using (var form = new ZForm(boWithMessages))
			using (var control = new CAMessagesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Assert(control.MessagesBoundGrid.Columns.Contains(EDIMessage.Schema.CargoControlNumber));
				Assert(control.MessagesBoundGrid.Columns.Contains(EDIMessage.Schema.TransactionNumber));
				Assert(control.MessagesBoundGrid.Columns.Contains(EDIMessage.Schema.CBSAOffice));
				Assert(control.MessagesBoundGrid.Columns.Contains(EDIMessage.Schema.SubLocation));

				var grid = control.MessagesBoundGrid;
				grid.ResetColumns();
				Assert("CAMessagesUserControl grid count must have at least " + ExpectedColumnNamesList.Count.ToString(), ExpectedColumnNamesList.Count <= grid.Columns.Count);
				for (int i = 0; i < ExpectedColumnNamesList.Count; i++)
				{
					var column = grid.Columns[i];
					AssertNotNull(column);
					var expectedColumnName = ExpectedColumnNamesList[i];
					AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
					AssertEquals("IsVisible", true, column.IsVisible);
				}
			}
		}

		List<ZString> ExpectedColumnNamesList
		{
			get
			{
				if (expectedColumnNamesList == null)
				{
					expectedColumnNamesList = new List<ZString>();
					expectedColumnNamesList.Add(EDIMessage.Schema.EM_MessageType);
					expectedColumnNamesList.Add(EDIMessage.Schema.EM_MessageSubType);
					expectedColumnNamesList.Add(EDIMessage.Schema.EM_ReceiveTransmit);
					expectedColumnNamesList.Add(EDIMessage.Schema.EM_Status);
					expectedColumnNamesList.Add(EDIMessage.Schema.EM_InterchangeStatus);
					expectedColumnNamesList.Add(EDIMessage.Schema.EM_User);
					expectedColumnNamesList.Add(EDIMessage.Schema.EM_MessageDateTime);
					expectedColumnNamesList.Add(EDIMessage.Schema.CargoControlNumber);
					expectedColumnNamesList.Add(EDIMessage.Schema.TransactionNumber);
					expectedColumnNamesList.Add(EDIMessage.Schema.SubLocation);
					expectedColumnNamesList.Add(EDIMessage.Schema.CBSAOffice);
				}
				return expectedColumnNamesList;
			}
		}
		List<ZString> expectedColumnNamesList;
	}
}

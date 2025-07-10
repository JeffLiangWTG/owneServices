using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;
using CusPollingTransaction = Enterprise.Customs.KR.Business.CusPollingTransaction;
using EDIMessage = Enterprise.Customs.KR.Business.EDIMessage;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class EDIMessageMainUserControlTest : TestCaseWithFactory
	{
		public void TestRequestReceiveDocumentGridOrder()
		{
			var outgoing = Factory.New<EDIMessage>();
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			outgoing.EM_MessageNum = "1";
			using (var form = new DLTEDIMessageForm(outgoing))
			{
				form.Show();

				using (var control = form.FindSingle<EDIMessageMainUserControl>("EDIMessageMainUserControl"))
				{
					var grid = control.RequestReceiveDocumentGrid;
					var index = 0;

					AssertEquals(grid.Columns[index++].ColumnName, CusPollingTransaction.Schema.CPT_Status);
					AssertEquals(grid.Columns[index++].ColumnName, CusPollingTransaction.Schema.CPT_SystemCreateTimeUtc);
					AssertEquals(grid.Columns[index++].ColumnName, CusPollingTransaction.Schema.CPT_Reference);
					AssertEquals(grid.Columns[index++].ColumnName, CusPollingTransaction.Schema.CPT_TransactionID);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(CusPollingTransaction.DOCMessage) + "+" + EDIMessage.Schema.EM_MessageNum);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(CusPollingTransaction.DOCMessage) + "+" + nameof(EDIMessage.EM_StatusDescription));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(CusPollingTransaction.DOCMessage) + "+" + EDIMessage.Schema.EM_SystemLastEditTimeUtc);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(CusPollingTransaction.RCVMessage) + "+" + EDIMessage.Schema.EM_MessageNum);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(CusPollingTransaction.RCVMessage) + "+" + nameof(EDIMessage.EM_StatusDescription));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(CusPollingTransaction.RCVMessage) + "+" + EDIMessage.Schema.EM_SystemLastEditTimeUtc);

					AssertEquals(false, grid.GetColumnStyle(nameof(CusPollingTransaction.DOCMessage) + "+" + nameof(EDIMessage.Interchange) + "+" + EDIInterchange.Schema.EI_InterchangeNum).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(CusPollingTransaction.DOCMessage) + "+" + nameof(EDIMessage.Interchange) + "+" + EDIInterchange.Schema.EI_InterchangeType).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(CusPollingTransaction.DOCMessage) + "+" + nameof(EDIMessage.Interchange) + "+" + EDIInterchange.Schema.EI_Status).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(CusPollingTransaction.RCVMessage) + "+" + nameof(EDIMessage.Interchange) + "+" + EDIInterchange.Schema.EI_InterchangeNum).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(CusPollingTransaction.RCVMessage) + "+" + nameof(EDIMessage.Interchange) + "+" + EDIInterchange.Schema.EI_InterchangeType).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(CusPollingTransaction.RCVMessage) + "+" + nameof(EDIMessage.Interchange) + "+" + EDIInterchange.Schema.EI_Status).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(CusPollingTransaction.DOCMessage) + "+" + EDIMessage.Schema.EM_Status).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(CusPollingTransaction.RCVMessage) + "+" + EDIMessage.Schema.EM_Status).IsVisible);
				}
			}
		}
	}
}

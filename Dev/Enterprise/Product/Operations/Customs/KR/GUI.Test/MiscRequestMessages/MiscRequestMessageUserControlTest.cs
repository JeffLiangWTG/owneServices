using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class MiscRequestMessageUserControlTest : TestCaseWithFactory
	{
		public void TestMessageGrid()
		{
			var header = Factory.New<CusMiscRequestHeader>();
			header.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;

			using (var form = new CusMiscRequestHeaderViewForm(header))
			{
				form.Show();
				using (var tab = form.FindSingle<ZTabPage>("MessagesTabPage"))
				{
					form.FindSingle<ZTemplateTabControl>("MainTabControl").SelectedTab = tab;
					using (MiscRequestMessagesUserControl control = tab.FindSingle<MiscRequestMessagesUserControl>("MiscRequestMessagesUserControl"))
					{
						var grid = control.FindSingle<ZGrid>("MessagesGrid");
						var index = 0;

						AssertEquals(grid.Columns[index++].ColumnName, EDIMessage.Schema.EM_MessageNum);
						AssertEquals(grid.Columns[index++].ColumnName, nameof(EDIMessage.EM_ReceiveTransmitDescription));
						AssertEquals(grid.Columns[index++].ColumnName, nameof(EDIMessage.EM_StatusDescription));
						AssertEquals(grid.Columns[index++].ColumnName, nameof(EDIMessage.EM_MessageType));
						AssertEquals(grid.Columns[index++].ColumnName, nameof(EDIMessage.EM_MessageTypeDescription));
						AssertEquals(grid.Columns[index++].ColumnName, EDIMessage.Schema.EM_MessageDateTime);

						AssertEquals(false, grid.GetColumnStyle(EDIMessage.Schema.EM_ReceiveTransmit).IsVisible);
						AssertEquals(false, grid.GetColumnStyle(EDIMessage.Schema.EM_Status).IsVisible);
						AssertEquals(false, grid.GetColumnStyle(EDIMessage.Schema.EM_MessageSubType).IsVisible);
						AssertEquals(false, grid.GetColumnStyle(EDIMessage.Schema.EM_MessageSubTypeDescription).IsVisible);
						AssertEquals(false, grid.GetColumnStyle(EDIMessage.Schema.EM_SystemCreateTimeUtc).IsVisible);
						AssertEquals(false, grid.GetColumnStyle(EDIMessage.Schema.EM_ApplicationReference).IsVisible);
						AssertEquals(false, grid.GetColumnStyle(EDIMessage.Schema.EM_User).IsVisible);
						AssertEquals(false, grid.GetColumnStyle(EDIMessage.Schema.EM_InterchangeNumber).IsVisible);
						AssertEquals(false, grid.GetColumnStyle(EDIMessage.Schema.EM_DateTimeInterchangeSent).IsVisible);
						AssertEquals(false, grid.GetColumnStyle(EDIMessage.Schema.EM_InterchangeStatus).IsVisible);
						AssertEquals(false, grid.GetColumnStyle("Interchange+eHubID").IsVisible);

						AssertEquals(EDIMessage.Schema.EM_MessageInterpretation, control.FindSingle<Enterprise.Messaging.GUI.HtmlInterpretationBox>("HtmlInterpretationBox").BindTo);
						AssertEquals(EDIMessage.Schema.EM_FormattedMessageText, control.FindSingle<ZTextBox>("MessageTextTextBox").BindTo);
					}
				}
			}
		}
	}
}

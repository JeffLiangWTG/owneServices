using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class StatementMessagesUserControlTest : TestCaseWithFactory
	{
		public void TestMessagesUserControl()
		{
			var statement = Factory.New<CusStatementHeader>();
			var msg = statement.Messages.AddNew();

			using (var form = new Statements(statement))
			{
				form.Show();
				using (var tab = form.FindSingle<ZTabPage>("MessagesTabPage"))
				{
					form.FindSingle<ZTemplateTabControl>("MainTabControl").SelectedTab = tab;
					using (StatementMessagesUserControl control = tab.FindSingle<StatementMessagesUserControl>("StatementMessagesUserControl"))
					{
						int defaultColumnIndex = 0;
						var msgGrid = control.FindSingle<MessageZGrid>("MessagesGrid");
						var visibleColumnArr = msgGrid.ColumnStyles.OfType<ZGridColumnInfo>().Where(col => col.IsVisible).ToArray();
						AssertEquals("EM_MessageNum", visibleColumnArr[defaultColumnIndex++].ColumnName);
						AssertEquals("EM_StatusDescription", visibleColumnArr[defaultColumnIndex++].ColumnName);
						AssertEquals("EM_MessageType", visibleColumnArr[defaultColumnIndex++].ColumnName);
						AssertEquals("EM_MessageTypeDescription", visibleColumnArr[defaultColumnIndex++].ColumnName);
						AssertEquals("EM_MessageDateTime", visibleColumnArr[defaultColumnIndex++].ColumnName);

						AssertEquals(msg.EM_MessageInterpretation, control.FindSingle<HtmlInterpretationBox>("HtmlInterpretationBox").DocumentText);
						AssertEquals(msg.EM_FormattedMessageText, control.FindSingle<ZTextBox>("MessageTextTextBox").Text);

						AssertEquals(true, msgGrid.Columns.Contains("EM_MessageNum"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_ReceiveTransmitDescription"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_ReceiveTransmit"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_StatusDescription"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_Status"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_MessageType"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_MessageTypeDescription"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_MessageDateTime"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_MessageSubType"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_MessageSubTypeDescription"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_SystemCreateTimeUtc"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_ApplicationReference"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_User"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_InterchangeNumber"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_DateTimeInterchangeSent"));
						AssertEquals(true, msgGrid.Columns.Contains("EM_InterchangeStatus"));
						AssertEquals(true, msgGrid.Columns.Contains("Interchange+eHubID"));

						AssertEquals(true, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_MessageNum)).IsVisible);
						AssertEquals(false, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_ReceiveTransmitDescription)).IsVisible);
						AssertEquals(false, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_ReceiveTransmit)).IsVisible);
						AssertEquals(true, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_StatusDescription)).IsVisible);
						AssertEquals(false, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_Status)).IsVisible);
						AssertEquals(true, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_MessageType)).IsVisible);
						AssertEquals(true, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_MessageTypeDescription)).IsVisible);
						AssertEquals(true, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_MessageDateTime)).IsVisible);
						AssertEquals(false, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_MessageSubType)).IsVisible);
						AssertEquals(false, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_MessageSubTypeDescription)).IsVisible);
						AssertEquals(false, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_SystemCreateTimeUtc)).IsVisible);
						AssertEquals(false, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_ApplicationReference)).IsVisible);
						AssertEquals(false, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_User)).IsVisible);
						AssertEquals(false, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_InterchangeNumber)).IsVisible);
						AssertEquals(false, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_DateTimeInterchangeSent)).IsVisible);
						AssertEquals(false, msgGrid.GetColumnStyle(nameof(EDIMessage.EM_InterchangeStatus)).IsVisible);
						AssertEquals(false, msgGrid.GetColumnStyle("Interchange+eHubID").IsVisible);
					}
				}
			}
		}
	}
}

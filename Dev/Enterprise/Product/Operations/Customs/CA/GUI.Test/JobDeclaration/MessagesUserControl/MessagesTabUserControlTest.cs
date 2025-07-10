using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class MessagesTabUserControlTest : TestCaseWithFactory
	{
		public void TestSetupMessageColumns()
		{
			var testMessage = Factory.New<EDIMessage>();

			using (var form = new ZForm(testMessage))
			using (var userControl = new MessagesTabUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(testMessage, ".");
				form.Show();

				var messagesGrid = (ZGrid)userControl.Controls.Find("MessagesGrid", true).First();

				CombineAssertions(() =>
				{
					AssertNotNull("User control should have BatchNumber column", messagesGrid.Columns[EDIMessage.Schema.BatchNumber]);
					AssertNotNull("User control should have MessageScheduleDescription column", messagesGrid.Columns[EDIMessage.Schema.MessageScheduleDescription]);
					AssertNotNull("User control should have EM_SendWithMessageErrorsFormatted column", messagesGrid.Columns[EDIMessage.Schema.EM_SendWithMessageErrorsFormatted]);

					var interchangeStatusColumn = messagesGrid.Columns[EDIMessage.Schema.EM_InterchangeStatus];
					AssertNotNull("User control should have EM_InterchangeStatus column", interchangeStatusColumn);
					AssertEquals("EM_InterchangeStatus column is visible", true, interchangeStatusColumn.IsVisible);
				});
			}
		}
	}
}

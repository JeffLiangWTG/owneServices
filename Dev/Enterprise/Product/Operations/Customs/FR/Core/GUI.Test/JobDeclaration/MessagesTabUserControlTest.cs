using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	class MessagesTabUserControlTest : TestCaseWithFactory
	{
		public void TestSetupMessagesGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				CombineAssertions(() =>
				{
					var messagesGrid = (ZGrid)messageUserControl.Controls.Find("MessagesGrid", true).SingleOrDefault();
					AssertNotNull("MessagesGrid should have a Held Until Date column.", messagesGrid.GetColumnStyle(EDIMessage.Schema.EM_HeldUntilDate));
				});
			}
		}
	}
}

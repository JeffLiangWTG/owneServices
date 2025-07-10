using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Client.ELG.Testing
{
	public class SagGuiWrapperExportDirectorTest : SagAccountsExportDirectorTest
	{
		protected override void AssertNotificationsWhenSuccess()
		{
			base.AssertNotificationsWhenSuccess();
			string expectedMessage = "was exported successfully.";
			Assert("User notification message should contain the message: " + expectedMessage, lastMessage.Contains(expectedMessage));
		}

		protected override void AssertNotificationsWhenFailure()
		{
			base.AssertNotificationsWhenFailure();
			Assert("User notification message should contain the message: Empty file created: none.", lastMessage.Contains("File created: none."));
		}

		protected override void ExecuteExportDirector()
		{
			Director.EnableManualMode = true;
			Director.Execute();
		}

		protected override void AsserFileCreated()
		{
			string expectedMessage = "was exported successfully.";
			Assert("User notification message should contain the message: " + expectedMessage, lastMessage.Contains(expectedMessage));
			Assert("folder should not be empty", TempDirInfo.GetFiles().Length > 0);
		}

		SagGuiWrapperExportDirector Director
		{
			get
			{
				return director ?? (director = new SagGuiWrapperExportDirectorForTest(Factory, NotifyBuffer, tempPath));
			}
		}

		SagGuiWrapperExportDirector director;
		class SagGuiWrapperExportDirectorForTest : SagGuiWrapperExportDirector
		{
			readonly string folder;
			public SagGuiWrapperExportDirectorForTest(BusinessObjectFactory factory, INotifications notificationSubscriber, string path) : base(factory, notificationSubscriber)
			{
				folder = path;
			}

			protected override string GetFolderLocationFromBrowseDialog()
				=> folder;
		}
	}
}

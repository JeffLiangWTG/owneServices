using System.Threading;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class XMLDirectorTest : TestCase
	{
		public void TestRun()
		{
			XMLDirectorForTest director = new XMLDirectorForTest(null);
			director.Run();
			AssertEquals(1, director.Notify.Events.Length);
			AssertEquals("Import/Export has been run", true, director.Notify.Events[0].Message.IndexOf("Import/Export has been run") > -1);
		}

		public void TestNotifications()
		{
			XMLDirectorForTest director = new XMLDirectorForTest(null);
			director.Notify.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, "InvalidFileFormat"));

			AssertEquals("There should be 1 notification", 1, director.Notify.Events.Length);
			AssertEquals("Contains error", true, director.Notify.Events[0].Message.IndexOf("InvalidFileFormat") > -1);
		}

		public void TestResetNotifications()
		{
			XMLDirectorForTest director = new XMLDirectorForTest(null);
			director.Notify.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, "InvalidFileFormat"));

			AssertEquals("There should be 1 notification", 1, director.Notify.Events.Length);
			director.ResetNotifications();
			AssertEquals("No notifications", 0, director.Notify.Events.Length);
		}

		class XMLDirectorForTest : XMLDirector
		{
			public XMLDirectorForTest(INotifications notify) : base(notify) { }
			protected override void RunCore(CancellationToken token)
			{
				Notify.Notify(new WarningNotification("Import/Export has been run"));
			}
		}
	}
}

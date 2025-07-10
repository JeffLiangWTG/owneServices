using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class InstallationTest : TestCase
	{
		Installation installation;

		public void TestConfiguration()
		{
			Configuration config = new Configuration();
			Installation installation = new Installation(config);
			AssertEquals("Configuration", config, installation.Configuration);
		}

		public void TestNeedsToInstall()
		{
			AssertEquals("NeedsToInstall", true, Installation.NeedsToInstall());
		}

		public void TestOnCurrentTaskDescriptionChanged()
		{
			AssertNoExceptionThrown(delegate
			{ Installation.OnCurrentTaskDescriptionChanged(null); });
			bool eventRaised = false;
			Installation.CurrentTaskDescriptionChanged += delegate(object sender, TaskDescriptionChangedEventArgs e)
			{
				eventRaised = true;
				AssertEquals("sender", Installation, sender);
				AssertEquals("e.TaskDescription", "Hello!", e.TaskDescription);
			};
			Installation.OnCurrentTaskDescriptionChanged("Hello!");
			AssertEquals("eventRaised", true, eventRaised);
		}

		public void TestOnProgress()
		{
			AssertNoExceptionThrown(delegate
			{ Installation.OnProgress(); });
			bool eventRaised = false;
			Installation.Progress += delegate
			{ eventRaised = true; };
			Installation.OnProgress();
			AssertEquals("eventRaised", true, eventRaised);
		}

		Installation Installation
		{
			get { return installation ?? (installation = new Installation(new Configuration())); }
		}
	}
}

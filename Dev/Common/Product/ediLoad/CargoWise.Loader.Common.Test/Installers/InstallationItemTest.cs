using System;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class InstallationItemTest : TestCase
	{
		class AlwaysFailsAvailabilityInstallationItem : InstallationItem
		{
			public AlwaysFailsAvailabilityInstallationItem(Installation installation) : base(installation)
			{
			}

			protected override InstallationResult CheckAvailabilityExcludingDependencies()
			{
				return InstallationResult.Error("I always fail");
			}

			protected override bool NeedsToInstallCore()
			{
				return true;
			}
		}

		public class AlwaysWarnsAvailabilityInstallationItem : InstallationItem
		{
			public AlwaysWarnsAvailabilityInstallationItem(Installation installation) : base(installation)
			{
			}

			protected override InstallationResult CheckAvailabilityExcludingDependencies()
			{
				return InstallationResult.Warning("I always warn");
			}

			protected override bool NeedsToInstallCore()
			{
				return true;
			}
		}

		public class AlwaysFailsInstallInstallationItem : InstallationItem
		{
			public AlwaysFailsInstallInstallationItem(Installation installation) : base(installation)
			{
			}

			protected override InstallationResult InstallExcludingDependencies()
			{
				return InstallationResult.Error("I always fail");
			}

			protected override bool NeedsToInstallCore()
			{
				return true;
			}
		}

		public class AlwaysWarnsInstallInstallationItem : InstallationItem
		{
			public AlwaysWarnsInstallInstallationItem(Installation installation) : base(installation)
			{
			}

			protected override InstallationResult InstallExcludingDependencies()
			{
				return InstallationResult.Warning("I always warn");
			}

			protected override bool NeedsToInstallCore()
			{
				return true;
			}
		}

		public class DoesNotNeedToInstallItem : InstallationItem
		{
			public DoesNotNeedToInstallItem(Installation installation) : base(installation)
			{
			}

			protected override bool NeedsToInstallCore()
			{
				return false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Installation = new Installation(new MockConfiguration("Source", "Target"));
			TestItem = new DummyInstallationItem(Installation);
		}

		DummyInstallationItem TestItem;
		Installation Installation;
		const string expectedTaskDescription = "hello world";

		public void TestChangeCurrentTaskDescription()
		{
			Installation.CurrentTaskDescriptionChanged += new EventHandler<TaskDescriptionChangedEventArgs>(Installation_CurrentTaskDescriptionChanged);
			TestItem.ChangeCurrentTaskDescription(expectedTaskDescription);
		}

		void Installation_CurrentTaskDescriptionChanged(object sender, TaskDescriptionChangedEventArgs e)
		{
			AssertEquals(expectedTaskDescription, e.TaskDescription);
		}

		public void TestAddDependency()
		{
			InstallationItem otherItem1 = new DummyInstallationItem(Installation);
			InstallationItem otherItem2 = new DummyInstallationItem(Installation);
			AssertEquals("Count before adding", 0, TestItem.Dependencies.Count);
			TestItem.AddDependency(otherItem1);
			TestItem.AddDependency(otherItem2);
			AssertEquals("Count after adding", 2, TestItem.Dependencies.Count);
			AssertEquals("Item 1", otherItem1, TestItem.Dependencies[0]);
			AssertEquals("Item 2", otherItem2, TestItem.Dependencies[1]);
		}

		public void TestAddRemoteExclusiveDependency()
		{
			InstallationItem topItem = new DummyInstallationItem(Installation);
			InstallationItem otherItem1 = new DummyInstallationItem(Installation);
			InstallationItem otherItem2 = new DummyRemoteExclusiveInstallationItem(Installation);
			AssertEquals("Count before adding", 0, TestItem.Dependencies.Count);
			topItem.AddDependency(otherItem1);
			AssertEquals("Count after adding", 1, topItem.Dependencies.Count);
			AssertEquals("Item 1", otherItem1, topItem.Dependencies[0]);
			AssertExceptionThrown<InvalidOperationException>(() => topItem.AddDependency(otherItem2));
		}

		public void TestCheckAvailability()
		{
			InstallationItem temp;

			temp = new AlwaysFailsAvailabilityInstallationItem(Installation);
			temp.AddDependency(new AlwaysWarnsAvailabilityInstallationItem(Installation));
			TestItem.AddDependency(temp);

			temp = new AlwaysWarnsAvailabilityInstallationItem(Installation);
			temp.AddDependency(new DummyInstallationItem(Installation));
			temp.AddDependency(new DummyInstallationItem(Installation));
			TestItem.AddDependency(temp);

			InstallationResultCollection results = new InstallationResultCollection();
			TestItem.CheckAvailability(results);

			AssertEquals("Result count", 6, results.Count);
			AssertEquals("Error count", 1, results.ErrorCount);
			AssertEquals("Warning count", 2, results.WarningCount);
			AssertEquals("OK count", 3, results.OKCount);
		}

		public void TestInstall()
		{
			SetUpSixItemsTestData();

			InstallationResultCollection results = new InstallationResultCollection();
			TestItem.Install(results);

			AssertEquals("Result count", 2, results.Count);
			AssertEquals("Error count", 1, results.ErrorCount);
			AssertEquals("Warning count", 1, results.WarningCount);
			AssertEquals("OK count", 0, results.OKCount);
		}

		public void TestDoesNotNeedToInstallShouldNotInstallDependencies()
		{
			TestItem.AddDependency(new DummyInstallationItem(Installation));

			DoesNotNeedToInstallItem item = new DoesNotNeedToInstallItem(Installation);
			item.AddDependency(new AlwaysFailsInstallInstallationItem(Installation));
			TestItem.AddDependency(item);

			InstallationResultCollection results = new InstallationResultCollection();
			TestItem.Install(results);
			AssertEquals("Result count", 2, results.Count);
			AssertEquals("OK count", 2, results.OKCount);
		}

		public void TestConstructor()
		{
			AssertSame("Constructor should set Installation to the supplied Installation", Installation, TestItem.Installation);
		}

		public void TestTotalProgressCount()
		{
			SetUpSixItemsTestData();
			AssertEquals(12, TestItem.TotalProgressCount);
		}

		public void TestOnProgress()
		{
			SetUpSixItemsTestData();
			TestItem.Installation.Progress += new EventHandler(TestItem_Progress);

			TestItem.CheckAvailability(new InstallationResultCollection());
			AssertEquals(6, ProgressCount);

			TestItem.Install(new InstallationResultCollection());
			AssertEquals("don't raise progress for error", 7, ProgressCount);
		}

		public void TestFindItems()
		{
			SetUpSixItemsTestData();
			DummyItemToFind = new DummyInstallationItem(Installation);
			TestItem.Dependencies[0].AddDependency(DummyItemToFind);
			InstallationItemCollection results = TestItem.FindItems(new InstallationItemFilter(DummyFilter));
			AssertEquals(2, results.Count);
			AssertSame(DummyItemToFind, results[0]);
			AssertSame(TestItem, results[1]);
		}

		public void TestAddTerminalServerInstallModeDependency()
		{
			AssertEquals(0, TestItem.Dependencies.Count);
			TestItem.AddTerminalServerInstallModeDependency();
			AssertEquals(1, TestItem.Dependencies.Count);
			TerminalServerInstallModeTest.AssertDependsOnInstallMode(TestItem);
		}

		public void TestExceptionsAreHandled()
		{
			var dummy = new DummyInstallationItem(Installation);
			dummy.ExceptionToThrowDuringInstall = new Exception("Fail");
			TestItem.AddDependency(dummy);
			InstallationResultCollection results = new InstallationResultCollection();
			TestItem.Install(results);
			AssertEquals(1, results.ErrorCount);
			string errorMessages = results.GetErrorMessages();
			Assert("Expected errorMessages to contain exception details 'System.Exception: Fail'", errorMessages.Contains("System.Exception: Fail"));
			Assert("Expected errorMessages to contain stack trace 'DummyInstallationItem.InstallExcludingDependencies'", errorMessages.Contains("DummyInstallationItem.InstallExcludingDependencies"));
		}

		InstallationItem DummyItemToFind;

		bool DummyFilter(InstallationItem item)
		{
			return item == TestItem || item == DummyItemToFind;
		}

		void SetUpSixItemsTestData()
		{
			InstallationItem temp;

			temp = new AlwaysFailsInstallInstallationItem(Installation);
			temp.AddDependency(new AlwaysWarnsInstallInstallationItem(Installation));
			TestItem.AddDependency(temp);

			temp = new AlwaysWarnsInstallInstallationItem(Installation);
			temp.AddDependency(new DummyInstallationItem(Installation));
			temp.AddDependency(new DummyInstallationItem(Installation));
			TestItem.AddDependency(temp);
		}

		void TestItem_Progress(object sender, EventArgs e)
		{
			ProgressCount++;
		}

		int ProgressCount;
	}
}

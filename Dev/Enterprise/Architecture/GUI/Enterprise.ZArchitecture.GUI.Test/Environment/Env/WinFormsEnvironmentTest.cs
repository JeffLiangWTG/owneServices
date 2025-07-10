using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;

namespace Enterprise.Environment.Testing
{
	sealed class WinFormsEnvironmentTest : EnvTest
	{
		protected override void SetTestEnvironment()
		{
			var currentDepartment = Env.CurrentDepartment.PK;
			var currentBranch = Env.CurrentBranch.PK;
			new WinFormsEnvironmentProvider().Enable();
			userContextSwitch = Env.SetTemporaryUserContext(User.SupportUserName, currentBranch, currentDepartment);
		}

		protected override void TearDown()
		{
			userContextSwitch?.Dispose();
			base.TearDown();
		}

		IDisposable userContextSwitch;

		WinFormsEnvironment WinEnv
		{
			get { return (WinFormsEnvironment)Env.Instance; }
		}

		public void TestCurrentModule()
		{
			WinEnv.CurrentModule = "TestModule";
			AssertEquals("Current module not correct", "TestModule", WinEnv.CurrentModule);
		}

		public void TestSetTemporaryUserContext()
		{
			using (var env = new WinFormsEnvironment())
			{
				RecentItemManager oldRecentItemManager = RecentItemManager.Instance;

				using (env.SetTemporaryUserContext(new UserContext("Test", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
				{
					AssertNotEquals("Temporary Recent Item Manager", oldRecentItemManager, RecentItemManager.Instance);
				}

				AssertEquals("Recent Item Manager was reset", oldRecentItemManager, RecentItemManager.Instance);
			}
		}

		public void TestSetLanguageForContext()
		{
			var user = new UserForTest();
			user.Language = Core.SharedConstants.Languages.German;
			var oldContext = Env.Instance.CurrentUserContext;
			var newContext = new UserContext(user, Env.CurrentBranchPK, Env.CurrentDepartmentPK);

			using (var env = new WinFormsEnvironment())
			{
				var nextInstance = BusinessObjectFactory._NextInstance;
				using (env.SetTemporaryUserContext(new UserContext(user, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
				{
					AssertEquals("Language should be changed if value is valid", Core.SharedConstants.Languages.German, Res.CurrentLanguage);
					AssertEquals("Language should be found in hard-coded languages, not using additional Factory(2 BusinessFactories created in UserContext constructor and SetTemporaryUserContext RecentItemManager)", 2, BusinessObjectFactory._NextInstance - nextInstance);
				}

				user.Language = "CCC";
				using (env.SetTemporaryUserContext(new UserContext(user, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
				{
					AssertEquals("Language should be defaulted to English if value is invalid", Res.DefaultLanguage, Res.CurrentLanguage);
				}
			}
		}

		public void TestUserContextSwitchLoggerThreadSafety()
		{
			var threads = new List<Thread>();
			int threadCount = 20;
			int raceCount = 100000;
			var logger = new UserContextSwitchLogger();

			for (int i = 0; i < threadCount; ++i)
			{
				threads.Add(new Thread(() =>
				{
					for (int j = 0; j < raceCount; ++j)
					{
						// It's a race, it's a race!
						logger.Log(null);
					}
				}));
			}

			foreach (var thread in threads)
			{
				thread.Start();
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			Assert(true);
		}
	}
}

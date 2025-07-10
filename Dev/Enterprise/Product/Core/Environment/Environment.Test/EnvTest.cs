using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	abstract public class EnvTest : TestCase
	{
		protected abstract void SetTestEnvironment();

		protected EnvProvider OldEnvironmentProvider;
		protected override void SetUp()
		{
			base.SetUp();
			OldEnvironmentProvider = Env.Provider;
			SetTestEnvironment();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (!ReferenceEquals(OldEnvironmentProvider, Env.Provider))
			{
				Env.Provider.Dispose();
			}

			Env.Provider = OldEnvironmentProvider;
		}

		public void TestSetDelegate()
		{
			var oldProvider = Env.Provider;
			var newProvider = new ServiceManagerEnvProvider(usePooledConnection: true);
			try
			{
				AssertEquals(Env.Instance, oldProvider.Instance);
				newProvider.Enable();
				Assert(!newProvider.Instance.Equals(oldProvider.Instance));
				AssertEquals(newProvider.Instance, Env.Instance);
			}
			finally
			{
				newProvider.Dispose();
				oldProvider.Enable();
			}
			AssertEquals(Env.Instance, oldProvider.Instance);
		}

		public void TestTime()
		{
			AssertNotNull(Env.Time);
		}

		public void TestTempPath()
		{
			string[] splitPath = Env.TempPath.TrimEnd('\\').Split('\\');
			string lastSubDirectory = splitPath[splitPath.Length - 2];

			Assert("Temp Path should point to a WiseTechGlobal directory (since the folder name is no longer coupled with CW1). Temp path is " + Env.TempPath, lastSubDirectory.IndexOf("WiseTechGlobal") != -1);
			Assert("Temp Path directory should exist. Temp path is " + Env.TempPath, Directory.Exists(Env.TempPath));
		}

		public void TestGetTempFileName()
		{
			string fileName = Env.GetTempFileName();
			try
			{
				AssertNotNull(fileName);
			}
			finally
			{
				File.Delete(fileName);
			}
		}

		[ExpectException(typeof(System.IO.IOException))]
		public void TestGetTempFileNameForNonExistentDirectory()
		{
			Env.GetTempFileName(@"\zz\zz\zz\zz\zz\zz");
		}

		public void TestCurrentCompany()
		{
			AssertNotNull(Env.CurrentCompany);
		}

		public void TestCurrentBranch()
		{
			AssertNotNull(Env.CurrentBranch);
		}

		public void TestCurrentDepartment()
		{
			AssertNotNull(Env.CurrentDepartment);
		}

		public void TestEmailSender()
		{
			AssertNotNull(Env.OutgoingMailManager);
		}

		public void TestRegistry()
		{
			AssertNotNull(Env.Registry);
		}

		public void TestRawRegistry()
		{
			AssertNotNull(Env.Registry.RawRegistry);
		}

		public void TestCurrentUser()
		{
			AssertNotNull(Env.CurrentUser);
		}

		public void TestLoginController()
		{
			AssertNotNull(Env.LoginController);
		}

		public void TestNumberFountains()
		{
			AssertNotNull(Env.NumberFountains);
		}

		public void TestSecurity()
		{
			AssertNotNull(Env.Security);
		}

		public void TestLicence()
		{
			AssertNotNull(Env.Licence);
		}

		public void TestDisposeSetTemporaryUserContextDoesNotHitDb()
		{
			var context = Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty);
			using (Db.Connection.TrackExecutedCommands(includeStackTrace: true))
			{
				context.Dispose();
				AssertContainsExactElementsInAnyOrder("Dispose method of SetTemporaryUserContext should not hit the DB when no state changed", Array.Empty<string>(), Db.Connection.ExecutedCommands);
			}
		}

		public void TestAllowNestingOfLogging()
		{
			var logger = new UserContextSwitchLogger();
			using (Env.StartContextSwitchTrace(logger))
			{
				AssertNull(Env.StartContextSwitchTrace(logger));
			}
		}

		public void TestTraceContextSwitches()
		{
			var logger = new UserContextSwitchLogger();

			using (var contextSwitchTrace = Env.StartContextSwitchTrace(logger))
			{
				using (var switch1 = Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
				{
					SwitchContextInAnotherMethod();
				}
			}

			CombineAssertions(() =>
			{
				AssertNotNull("logger.Logs", logger.Logs);
				AssertEquals("logger.Logs.Count", 4, logger.Logs.Count);

				var callStackDump = string.Join("\r\n\r\n", logger.Logs.Select(o => o.Trace.ToString()));
				AssertContains("Context Switching Method should be in the Call Stacks", "SwitchContextInAnotherMethod", callStackDump);
			});
		}

		void SwitchContextInAnotherMethod()
		{
			Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)).Dispose();
		}

		public void TestUserContextInfoBehaviour()
		{
			var contexts = new List<UserContextInfo>
			{
				new UserContextInfo(null, null, null),
				new UserContextInfo("Branch1", null, null),
				new UserContextInfo("Branch1", "Company1", null),
				new UserContextInfo("Branch1", "Company1", "User1"),
				new UserContextInfo("branch1", "company1", "user1"),
				new UserContextInfo("branch1", "company2", "user1"),
				new UserContextInfo("branch1", "company1", "user2")
			};

			AssertEquals(true, contexts[3] == contexts[4]);
			AssertEquals(true, contexts[3].Equals(contexts[4]));
			AssertEquals(true, contexts[3].Equals((object)contexts[4]));
			AssertEquals(true, contexts[3] != contexts[2]);
			AssertEquals(true, contexts[0] != contexts[1]);
			AssertEquals(true, contexts[0] != contexts[1]);
			AssertEquals(true, contexts[4] != contexts[5]);
			AssertEquals(true, contexts[4] != contexts[6]);

			var hashCodes = new HashSet<int>();

			foreach (var userContextInfo in contexts)
			{
				hashCodes.Add(userContextInfo.GetHashCode());
			}

			AssertGreaterThan(hashCodes.Count, 5);
		}
	}
}

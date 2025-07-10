using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMExtensionMethodsTest : BMSTestCaseWithFactory
	{
		[TestDate(2022, 07, 03)]
		[TestTimeZoneUNLOCO("USCHI")]
		public void TestToLocalBranchTime()
		{
			TestDateAttribute.UseUNLOCO = true;
			var now = ZDateTime.Now;
			var utc = now.ToUniversalBranchTime();

			AssertEquals("Emtpy time converted to local time is always emtpy", ZDateTime.Empty, BMExtensionMethods.ToLocalBranchTime(ZDateTime.Empty, GlbBranch.CurrentBranch));
			AssertEquals("Emtpy time converted to local time is always emtpy", ZDateTime.Empty, BMExtensionMethods.ToLocalBranchTime(ZDateTime.Empty, null));

			var branch = Factory.New<GlbBranch>();
			branch.GB_RL_NKHomePort = "";

			AssertEquals("local to utc ~= utc to local", now, BMExtensionMethods.ToLocalBranchTime(utc, GlbBranch.CurrentBranch));
			AssertEquals("When no homeport, fallback to current user context branch.", now, BMExtensionMethods.ToLocalBranchTime(utc, branch));

			branch.GB_RL_NKHomePort = "AUBNE";
			AssertEquals("AUBNE is always +10 right?", utc.AddHours(10), BMExtensionMethods.ToLocalBranchTime(utc, branch));
		}

		public void TestGetView()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "65.9");

			var view = workflow.GetView();

			AssertNull("workflow hasn't been saved yet, so can't be loaded", view);
			AssertEquals("Should not hit db when workflow isn't in the database yet", 0, Factory.GetTableHitCount(ViewProcessHeaderSchema.Constants.TableName));

			Factory.Save();

			view = workflow.GetView();

			AssertNotNull(view);
			AssertEquals(1, Factory.GetTableHitCount(ViewProcessHeaderSchema.Constants.TableName));
		}

		public void TestAppendNextBracketedNumber()
		{
			AssertEquals("Booo [1]", "Booo".AppendNextBracketedNumber());
			AssertEquals("Booo [2]", "Booo [1]".AppendNextBracketedNumber());
			AssertEquals("Booo [1a] [1]", "Booo [1a]".AppendNextBracketedNumber());
			AssertEquals("Booo [[1]]] [1]", "Booo [[1]]]".AppendNextBracketedNumber());
			AssertEquals("Booo [667]", "Booo [666]".AppendNextBracketedNumber());
			AssertEquals("Bo [1]", "Booooooo".AppendNextBracketedNumber(maxLength: 6));
			AssertEquals("Bo [2]", "Booooooo [1]".AppendNextBracketedNumber(maxLength: 6));
			AssertEquals("Booooooo [1]", "Booooooo".AppendNextBracketedNumber(maxLength: 12));
		}

		public void TestAppendNextHighestBracketedNumber()
		{
			var existing = new List<string>
			{
				"Boooo [1]", "Boooo [10]", "Boooo [12]", "Boooo [99]", "Boooo [4]", "Boooo [31]"
			};

			AssertEquals("Boooo [100]", "Boooo".AppendNextHighestBracketedNumber(existing));
			AssertEquals("Boooo [1a] [1]", "Boooo [1a]".AppendNextHighestBracketedNumber(existing));
			AssertEquals("Boooo [100]", "Boooo [1]".AppendNextHighestBracketedNumber(existing));
			AssertEquals("Boooo [100]", "Boooo [31]".AppendNextHighestBracketedNumber(existing));
			AssertEquals("Boooo [1]", "Boooo".AppendNextHighestBracketedNumber(new List<string>()));

			AssertEquals("Booo [1]", "Booo".AppendNextHighestBracketedNumber(new List<string>()));
			AssertEquals("Booo [1]", "Booo".AppendNextHighestBracketedNumber(existing));
		}

		public void TestRequireBackgroundThread()
		{
			var wasDispatcherRegistered = RegisterDispatcher.IsDispatcherRegistered(Thread.CurrentThread.ManagedThreadId);
			if (!wasDispatcherRegistered)
			{
				RegisterDispatcher.Register();
			}
			try
			{
				AssertExceptionThrown<CrossThreadAccessException>(() => Thread.CurrentThread.RequireBackgroundThread());

				var task = Task.Factory.StartNew(() => Thread.CurrentThread.RequireBackgroundThread());
				task.Wait();
				AssertNull("Should not throw an exception when on background thread", task.Exception);
			}
			finally
			{
				if (!wasDispatcherRegistered)
				{
					RegisterDispatcher.Deregister();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestRequireNotOnVisualBoardFormThread_OnMainFormThread()
		{
			Thread.CurrentThread.RequireNotOnVisualBoardFormThread();
		}

		public void TestRequireNotOnVisualBoardFormThread_OnBackgroundThread()
		{
			var task = Task.Factory.StartNew(() =>
			{
				Thread.CurrentThread.Name = "Dat Thread";
				Thread.CurrentThread.RequireNotOnVisualBoardFormThread();
			});
			task.Wait();
			AssertNull("Should not throw an exception when on background thread", task.Exception);
		}

		[ExpectNoExceptions]
		public void TestRequireNotOnVisualBoardFormThread_OnVisualBoardFormThread()
		{
			var task = Task.Factory.StartNew(() =>
			{
				Thread.CurrentThread.Name = VisualBoardThreadHelper.GetVisualBoardFormThreadName("Poodle");
				Thread.CurrentThread.RequireNotOnVisualBoardFormThread();
			});
			NUnit.Framework.Assert.That(() => task.Wait(), CustomConstraints.InnermostExceptionThrown(typeof(CrossThreadAccessException)), "Should throw an exception when on VisualBoardForm thread");
		}

		public void TestToFriendlyTimeOverAgoString()
		{
			AssertEquals(string.Empty, TimeSpan.FromMinutes(-2).ToFriendlyTimeOverAgoString());
			AssertEquals("just now", TimeSpan.FromSeconds(0).ToFriendlyTimeOverAgoString());
			AssertEquals("just now", TimeSpan.FromSeconds(1).ToFriendlyTimeOverAgoString());

			AssertEquals("1 minute ago", TimeSpan.FromMinutes(1).ToFriendlyTimeOverAgoString());
			AssertEquals("2 minutes ago", TimeSpan.FromMinutes(2).ToFriendlyTimeOverAgoString());

			AssertEquals("over 1 hour ago", TimeSpan.FromHours(1).ToFriendlyTimeOverAgoString());
			AssertEquals("over 2 hours ago", TimeSpan.FromHours(2).ToFriendlyTimeOverAgoString());

			AssertEquals("over 1 day ago", TimeSpan.FromDays(1).ToFriendlyTimeOverAgoString());
			AssertEquals("over 2 days ago", TimeSpan.FromDays(2).ToFriendlyTimeOverAgoString());

			AssertEquals("over 1 week ago", TimeSpan.FromDays(8).ToFriendlyTimeOverAgoString());
			AssertEquals("over 2 weeks ago", TimeSpan.FromDays(15).ToFriendlyTimeOverAgoString());

			AssertEquals("over 1 month ago", TimeSpan.FromDays(32).ToFriendlyTimeOverAgoString());
			AssertEquals("over 2 months ago", TimeSpan.FromDays(64).ToFriendlyTimeOverAgoString());

			AssertEquals("over 1 year ago", TimeSpan.FromDays(380).ToFriendlyTimeOverAgoString());
			AssertEquals("over 2 years ago", TimeSpan.FromDays(800).ToFriendlyTimeOverAgoString());
		}

		public void TestToFriendlyTimeString_Zero()
		{
			AssertEquals("0 minutes", TimeSpan.FromSeconds(0).ToFriendlyTimeString(FriendlyMinutesDisplay.ShowZeroMinutes));
		}

		public void TestToFriendlyTimeString_Year()
		{
			AssertEquals("2 years", TimeSpan.FromDays(735).ToFriendlyTimeString());
			AssertEquals("1 year", TimeSpan.FromDays(380).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Year));
			AssertEquals("10 months", TimeSpan.FromDays(310).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Year));
		}

		public void TestToFriendlyTimeString_Month()
		{
			AssertEquals("16 months", TimeSpan.FromDays(480).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Month));
			AssertEquals("10 months", TimeSpan.FromDays(310).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Month));
			AssertEquals("2 weeks", TimeSpan.FromDays(20).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Month));
		}

		public void TestToFriendlyTimeString_Week()
		{
			AssertEquals("68 weeks", TimeSpan.FromDays(480).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Week));
			AssertEquals("44 weeks", TimeSpan.FromDays(310).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Week));
			AssertEquals("2 weeks", TimeSpan.FromDays(20).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Week));
			AssertEquals("6 days", TimeSpan.FromDays(6).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Week));
		}

		public void TestToFriendlyTimeString_Day()
		{
			AssertEquals("310 days", TimeSpan.FromDays(310).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Day));
			AssertEquals("20 days", TimeSpan.FromDays(20).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Day));
			AssertEquals("6 days", TimeSpan.FromDays(6).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Day));
			AssertEquals("2 hours", TimeSpan.FromHours(2).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Day));
		}

		public void TestToFriendlyTimeString_Hour()
		{
			AssertEquals("480 hours", TimeSpan.FromDays(20).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Hour));
			AssertEquals("144 hours", TimeSpan.FromDays(6).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Hour));
			AssertEquals("2 hours", TimeSpan.FromHours(2).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Hour));
			AssertEquals("2 minutes", TimeSpan.FromMinutes(2).ToFriendlyTimeString(maxUnitRollup: FriendlyMaxUnitRollup.Hour));
		}

		public void TestColorToHex_ShouldReturnHexColors()
		{
			AssertEquals("#FF0000", Color.Red.ToHex());
			AssertEquals("#FFA500", Color.Orange.ToHex());
			AssertEquals("#FFFF00", Color.Yellow.ToHex());
			AssertEquals("#008000", Color.Green.ToHex());
			AssertEquals("#0000FF", Color.Blue.ToHex());
			AssertEquals("#4B0082", Color.Indigo.ToHex());
			AssertEquals("#EE82EE", Color.Violet.ToHex());
		}

		public void TestAreDataVersionsLogged_ShouldNotComplainWhenAccessingDeletedWorkflow()
		{
			var job = (SalesEnquiry)BMSTestHelper.CreateJob<SalesEnquiry>(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var processHeader = jobHeader.ProcessHeaders.First() ?? jobHeader.ProcessHeaders.AddNew();

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var processHeaderInNewFactory = newFactory.Load<ProcessHeader>(processHeader.PK);
			processHeader.Delete();

			Factory.Save();

			AssertNoExceptionThrown(() => { var check = processHeaderInNewFactory.AreDataVersionsLogged(); });
			AssertEquals(false, processHeader.IsRowDeletedOrNull);
		}
	}
}

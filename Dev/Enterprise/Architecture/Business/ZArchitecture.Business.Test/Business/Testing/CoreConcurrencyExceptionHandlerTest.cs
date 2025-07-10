using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CoreConcurrencyExceptionHandlerTest : TestCaseWithDummy
	{
		public void TestUserFriendlyMessage_WithoutLogs()
		{
			AssertEquals("While you were editing your data, another user (Unknown) modified it.\r\nYour changes cannot be saved because they may conflict with the other user's changes.\r\nPlease close and open this form to try again.",
				HandlerWithoutLogs.UserFriendlyMessage);
		}

		public void TestUserFriendlyMessage_WithLogs()
		{
			AssertStringContains(HandlerWithLogs.UserFriendlyMessage, "While you were editing your data, another user (" + Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentUser.FullName + " @ ");
			AssertStringContains(HandlerWithLogs.UserFriendlyMessage, ") modified it.\r\nYour changes cannot be saved because they may conflict with the other user's changes.\r\nPlease close and open this form to try again.");
		}

		public void TestUserFriendlyMessage_WithLastEditInfo()
		{
			AssertStringContains(HandlerWithLastEditInfo.UserFriendlyMessage, "While you were editing your data, another user (CargoWise Service @ ");
			AssertStringContains(HandlerWithLastEditInfo.UserFriendlyMessage, ") modified it.\r\nYour changes cannot be saved because they may conflict with the other user's changes.\r\nPlease close and open this form to try again.");
		}

		public void TestUserFriendlyMessage_WithNullRow()
		{
			var handler = new DummyConcurrencyExceptionHandler(new DBConcurrencyException());
			AssertEquals("While you were editing your data, another user (Unknown) modified it.\r\nYour changes cannot be saved because they may conflict with the other user's changes.\r\nPlease close and open this form to try again.",
				handler.UserFriendlyMessage);
		}

		public void TestBasicInfoAboutRow()
		{
			AssertEquals(@$"
ROW INFORMATION
Table      = DummyBizo
PK         = {Dummy.PK.ToString()}
RowState   = Modified",
				HandlerWithoutLogs.BasicInfoAboutRow);
		}

		public void TestBasicInfoAboutRow_WithNullRow()
		{
			var handler = new DummyConcurrencyExceptionHandler(new DBConcurrencyException());
			AssertEquals("Row is null. Nothing known.", handler.BasicInfoAboutRow);
		}

		public void TestInfo_ConcurrencyTable()
		{
			string expectedXMLTable =
				"<ConcurrencyTableRows>" +
				"<ConcurrencyTableRow><Column>Z0_Bool</Column><Original>True</Original><DB>True</DB><Changed>False</Changed><Conflict>None</Conflict><Concurrency>Default - NotifyAndMerge</Concurrency></ConcurrencyTableRow>" +
			   @"<ConcurrencyTableRow><Column>Z0_Description</Column><Original /><DB /><Changed>noodle &amp; eldoon! it's apparently ""delicious"" and &gt; rice. though &lt; pasta</Changed><Conflict>None</Conflict><Concurrency>Ignore - OverwriteOtherUser</Concurrency></ConcurrencyTableRow>" +
				"<ConcurrencyTableRow><Column>Z0_IsValid</Column><Original>False</Original><DB>False</DB><Changed>False</Changed><Conflict>None</Conflict><Concurrency /></ConcurrencyTableRow>" +
				"<ConcurrencyTableRow><Column>Z0_Number</Column><Original>1</Original><DB>10</DB><Changed>5</Changed><Conflict>DB Changed</Conflict><Concurrency>Strict - Notify</Concurrency></ConcurrencyTableRow>" +
				"<ConcurrencyTableRow><Column>Z0_VarCharMax</Column><Original>Something</Original><DB>Something</DB><Changed /><Conflict>None</Conflict><Concurrency>Default - NotifyAndMerge</Concurrency></ConcurrencyTableRow>" +
				"</ConcurrencyTableRows>";
			ConcurrencyInfo.SetConcurrencyPolicy(Dummy, nameof(AutoDummyBizo.Z0_Description), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(Dummy, nameof(AutoDummyBizo.Z0_Number), ConcurrencyPolicy.Strict);
			AssertStringContains(HandlerWithoutLogs.Info, expectedXMLTable);
		}

		public void TestInfo_ConcurrencyDeletionMessage()
		{
			handlerWithoutLogs_DeleteRow = true;
			AssertStringContains(HandlerWithoutLogs.Info, "Row in database has been deleted.");
		}

		public void TestInfo_FactorySavingConncurrent()
		{
			try
			{
				Factory.Save();
				var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
				var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
				var dummyOnOne = factory1.Load<DummyBusinessObject>(Dummy.PK);
				var dummyOnTwo = factory2.Load<DummyBusinessObject>(Dummy.PK);

				dummyOnOne.Z0_Number = 69;
				dummyOnTwo.Z0_AnotherDecimal = 75.0;
				factory1.Save();
				factory2.Save();
			}
			catch (ZSaveConcurrencyException e)
			{
				var report = new ConcurrencyExceptionHandler(e);
				var reportInfo = report.Info;
				Assert(reportInfo.Contains("<Column>Z0_Number</Column><Original>0</Original><DB>69</DB><Changed>0</Changed>"));
				Assert(reportInfo.Contains("<Column>Z0_AnotherDecimal</Column><Original>0.000</Original><DB>0.000</DB><Changed>75</Changed>"));
			}
		}

		public void TestInfo_WithNullRow()
		{
			var handler = new DummyConcurrencyExceptionHandler(new DBConcurrencyException());
			AssertEquals(@"
--- Save Aborted Due to Concurrency Check ---
Row is null. Nothing known.
", handler.Info);
		}

		public void TestIsExceptionProcessed()
		{
			Assert(!HandlerWithLogs.IsExceptionProcessed);
			HandlerWithoutLogs.NotifyUserAndDevelopersIfNotAlreadyProcessed();
			ErrorReporter.Clear();
			Assert(!HandlerWithLogs.IsExceptionProcessed);
		}

		public void TestDeveloperNotNotifying()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.ReportConcurrencyErrors = false;
				HandlerWithoutLogs.NotifyUserAndDevelopersIfNotAlreadyProcessed();
				AssertEquals("Developer Error must not be reported", 0, ErrorReporter.ExceptionsThrown.Count);
			}
		}

		public void TestDevelopersNotifying()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.ReportConcurrencyErrors = true;
				HandlerWithoutLogs.NotifyUserAndDevelopersIfNotAlreadyProcessed();
				AssertEquals("Developer Error must be reported", 4, ErrorReporter.ExceptionsThrown.Count);
				AssertEquals("Developer Error must be reported", true, ErrorReporter.ExceptionsThrown.Any(ex => ex.Contains("--- Save Aborted Due to Concurrency Check ---")));
				ErrorReporter.Clear();
			}
		}

		void AssertStringContains(string bigString, string subString)
		{
			string message = "This string: \r\n\r\n" + bigString +
				"\r\n\r\nDoes not contain this sub-string: \r\n\r\n" + subString + "\r\n\r\n";
			Assert(message, bigString.IndexOf(subString) != -1);
		}

		public void TestNotifyUserWithoutErrorReport()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var exception = new ZSaveConcurrencyException(new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Test Message"), ((IBusinessObjectInternals)dummy).Row, Db.Connection), Factory), true);
			var report = new ConcurrencyExceptionHandler(exception);
			report.NotifyUserAndDevelopersIfNotAlreadyProcessed();
			AssertEquals(@"While you were editing your data, another user (Unknown) modified it.
Your changes cannot be saved because they may conflict with the other user's changes.
Please close and open this form to try again.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		bool handlerWithoutLogs_DeleteRow;
		DummyConcurrencyExceptionHandler fHandlerWithoutLogs;
		DummyConcurrencyExceptionHandler HandlerWithoutLogs
		{
			get
			{
				if (fHandlerWithoutLogs == null)
				{
					Factory.RefreshEnabled = false;
					Dummy.Z0_Bool = true;
					Dummy.Z0_Number = 1;
					Dummy.Z0_Description = string.Empty;
					Dummy.Z0_VarCharMax = "Something";
					Factory.Save();

					var factory2 = new BusinessObjectFactory();
					factory2.RefreshEnabled = false;
					var dummy2 = factory2.Load<DummyBusinessObject>(Dummy.PK);
					if (handlerWithoutLogs_DeleteRow)
					{
						dummy2.Delete();
					}
					else
					{
						dummy2.Z0_Number = 10;
					}
					factory2.Save();

					Dummy.Z0_Bool = false;
					Dummy.Z0_Number = 5;
					Dummy.Z0_Description = @"noodle & eldoon! it's apparently ""delicious"" and > rice. though < pasta";
					Dummy.Z0_VarCharMax = string.Empty;

					try
					{
						Factory.Save(); // should throw exception
					}
					catch (ZSaveConcurrencyException e)
					{
						fHandlerWithoutLogs = new DummyConcurrencyExceptionHandler(e);
					}
					if (fHandlerWithoutLogs == null)
					{
						Fail("Should have had DB concurrency error");
					}
				}
				return fHandlerWithoutLogs;
			}
		}

		DummyConcurrencyExceptionHandler fHandlerWithLogs;
		DummyConcurrencyExceptionHandler HandlerWithLogs
		{
			get
			{
				if (fHandlerWithLogs == null)
				{
					BusinessObjectFactory factory1 = new BusinessObjectFactory();
					factory1.RefreshEnabled = false;

					IGlbStaff staff1 = factory1.New<IGlbStaff>();

					staff1.GS_Title = "MR";
					staff1.GS_Code = "ZAC";
					factory1.Save();

					BusinessObjectFactory factory2 = new BusinessObjectFactory();
					factory2.RefreshEnabled = false;

					IGlbStaff staff2 = factory2.Load<IGlbStaff>(staff1.PK);

					staff2.GS_Title = "MRS";
					staff2.GS_Code = "BAS";
					factory2.Save();

					staff1.GS_Title = "DR";

					try
					{
						factory1.Save(); // should throw exception
					}
					catch (ZSaveConcurrencyException e)
					{
						fHandlerWithLogs = new DummyConcurrencyExceptionHandler(e);
					}
					if (fHandlerWithLogs == null)
					{
						Fail("Should have had DB concurrency error");
					}
				}
				return fHandlerWithLogs;
			}
		}

		DummyConcurrencyExceptionHandler handlerWithLastEditInfo;
		DummyConcurrencyExceptionHandler HandlerWithLastEditInfo
		{
			get
			{
				if (handlerWithLastEditInfo == null)
				{
					ZString currentUser = StaticCurrentFetcher.Instance.CurrentUser.GS_Code;
					AssertNotEquals("PRE", Enterprise.ZArchitecture.Environment.User.ServiceUserCode, currentUser);

					BusinessObjectFactory factory1 = new BusinessObjectFactory();
					factory1.RefreshEnabled = false;
					DummyLogged dummy1 = factory1.New<DummyLogged>();
					factory1.Save();

					BusinessObjectFactory factory2 = new BusinessObjectFactory();
					factory2.RefreshEnabled = false;
					DummyLogged dummy2 = factory2.Load<DummyLogged>(dummy1.PK);
					dummy2.ZL2_Description = "foo";
					StaticCurrentFetcher.Instance.CurrentUser.GS_Code = Enterprise.ZArchitecture.Environment.User.ServiceUserCode;
					factory2.Save();

					AssertEquals("another user made last edit", Enterprise.ZArchitecture.Environment.User.ServiceUserCode, dummy2.ZL2_SystemLastEditUser);

					StaticCurrentFetcher.Instance.CurrentUser.GS_Code = currentUser;
					dummy1.ZL2_Description = "bar";

					try
					{
						factory1.Save(); // should throw exception
					}
					catch (ZSaveConcurrencyException e)
					{
						handlerWithLastEditInfo = new DummyConcurrencyExceptionHandler(e);
					}
					if (handlerWithLastEditInfo == null)
					{
						Fail("Should have had DB concurrency error");
					}
				}
				return handlerWithLastEditInfo;
			}
		}

		class DummyConcurrencyExceptionHandler : ConcurrencyExceptionHandler
		{
			public DummyConcurrencyExceptionHandler(Exception e)
				: base(e)
			{
			}

			public new string UserFriendlyMessage
			{
				get { return base.UserFriendlyMessage; }
			}

			public new string BasicInfoAboutRow
			{
				get { return base.BasicInfoAboutRow; }
			}

			public new bool IsExceptionProcessed
			{
				get { return base.IsExceptionProcessed; }
			}
		}
	}
}

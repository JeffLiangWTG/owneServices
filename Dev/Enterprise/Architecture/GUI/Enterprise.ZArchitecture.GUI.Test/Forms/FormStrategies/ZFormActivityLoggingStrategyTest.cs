using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZFormActivityLoggingStrategyTest : TestCaseWithFactory
	{
		[TestDate(2006, 1, 1)]
		public void TestActivityLogging()
		{
			EnvProxy.Instance.Registry.UserEventTrackingEnterprise = true;

			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();

			using (Env.SetTemporaryUserContext(UserContextWithActivityTrackingStatusYes))
			{
				using (var testForm = new TestZFormWithCaption(bizo))
				{
					var stats = UserEventTracker.Instance.GetFormStatsForForm(testForm);
					AssertCollectionContains(stats, ZFormActivityLogger.Instance.StatLogs);

					AssertEquals(DateTime.MinValue, stats.ShownDateTimeUtc);
					AssertEquals(DateTime.MinValue, stats.ShownDateTimeUtc);

					testForm.Show();
					Application.DoEvents();

					AssertEquals("Sheep", stats.FormCaption);
					AssertEquals("", stats.ModuleName);

					testForm.Close();
					Application.DoEvents();
					AssertEquals(new DateTime(2006, 1, 1), stats.ShownDateTimeUtc);
					AssertEquals(new DateTime(2006, 1, 1), stats.CloseDateTimeUtc);
					AssertEquals(Guid.Empty, stats.BusinessObjectPK);
					AssertEquals(string.Empty, stats.BusinessObjectTableCode);
				}

				using (var testForm = new TestZFormWithCaption(bizo))
				{
					testForm.ControllerID = ControllerIDs.AccBankAccount;

					var stats = UserEventTracker.Instance.GetFormStatsForForm(testForm);
					AssertCollectionContains(stats, ZFormActivityLogger.Instance.StatLogs);

					AssertEquals(DateTime.MinValue, stats.ShownDateTimeUtc);
					AssertEquals(DateTime.MinValue, stats.ShownDateTimeUtc);

					testForm.Show();
					Application.DoEvents();

					AssertEquals("Sheep", stats.FormCaption);
					AssertEquals(ControllerIDs.AccBankAccount.Name, stats.ModuleName);

					AssertEquals(new DateTime(2006, 1, 1), stats.ShownDateTimeUtc);

					testForm.Close();
					Application.DoEvents();
					AssertEquals(new DateTime(2006, 1, 1), stats.ShownDateTimeUtc);
					AssertEquals(new DateTime(2006, 1, 1), stats.CloseDateTimeUtc);
					AssertEquals(Guid.Empty, stats.BusinessObjectPK);
					AssertEquals(string.Empty, stats.BusinessObjectTableCode);
				}

				using (var testForm = new TestZFormWithoutCaption(bizo))
				{
					testForm.Text = "Text Set";
					var stats = UserEventTracker.Instance.GetFormStatsForForm(testForm);
					AssertCollectionContains(stats, ZFormActivityLogger.Instance.StatLogs);

					testForm.Show();
					Application.DoEvents();

					AssertEquals("Text Set", stats.FormCaption);
					AssertEquals("", stats.ModuleName);
				}

				Factory.Save();
				using (var testForm = new ZForm(bizo))
				{
					var stats = UserEventTracker.Instance.GetFormStatsForForm(testForm);
					AssertCollectionContains(stats, ZFormActivityLogger.Instance.StatLogs);

					testForm.Show();
					Application.DoEvents();
					testForm.Close();
					Application.DoEvents();
					AssertEquals(bizo.PK.ToGuid(), stats.BusinessObjectPK);
					AssertEquals("Z0", stats.BusinessObjectTableCode);
				}
			}

			using (Env.SetTemporaryUserContext(UserContextWithActivityTrackingStatusNo))
			using (var testForm = new TestZFormWithCaption(bizo))
			{
				var stats = UserEventTracker.Instance.GetFormStatsForForm(testForm);
				AssertCollectionNotContains(stats, ZFormActivityLogger.Instance.StatLogs);
			}

			using (Env.SetTemporaryUserContext(UserContextWithActivityTrackingStatusCMP))
			using (var testForm = new TestZFormWithCaption(bizo))
			{
				var stats = UserEventTracker.Instance.GetFormStatsForForm(testForm);
				AssertCollectionContains(stats, ZFormActivityLogger.Instance.StatLogs);
			}

			EnvProxy.Instance.Registry.UserEventTrackingEnterprise = false;
			using (Env.SetTemporaryUserContext(UserContextWithActivityTrackingStatusYes))
			using (var testForm = new TestZFormWithCaption(bizo))
			{
				var stats = UserEventTracker.Instance.GetFormStatsForForm(testForm);
				AssertCollectionContains(stats, ZFormActivityLogger.Instance.StatLogs);
			}
		}

		public void TestInactiveDuration()
		{
			EnvProxy.Instance.Registry.UserEventTrackingEnterprise = true;

			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();

			using (Env.SetTemporaryUserContext(UserContextWithActivityTrackingStatusYes))
			using (var testFormActive = new TestZFormWithCaption(bizo) { SetFormCaptionForTest = "Active Form" })
			using (var testFormInactive = new TestZFormWithCaption(bizo) { SetFormCaptionForTest = "Inactive Form" })
			{
				var statsActive = UserEventTracker.Instance.GetFormStatsForForm(testFormActive);
				AssertCollectionContains(statsActive, ZFormActivityLogger.Instance.StatLogs);

				var statsInactive = UserEventTracker.Instance.GetFormStatsForForm(testFormInactive);
				AssertCollectionContains(statsInactive, ZFormActivityLogger.Instance.StatLogs);

				testFormInactive.Show();
				Application.DoEvents();
				testFormActive.Show();
				Application.DoEvents();

				System.Threading.Thread.Sleep(2500);

				testFormInactive.Close();
				testFormActive.Close();

				Assert("ActiveDuration on Active form should be more than 2 sec, but it was " + statsActive.ActiveDuration.TotalSeconds, statsActive.ActiveDuration > new TimeSpan(0, 0, 2));
				Assert("InactiveDuration on Active form should be less than 2 sec, but it was " + statsActive.InactiveDuration.TotalSeconds, statsActive.InactiveDuration < new TimeSpan(0, 0, 2));
				Assert("ActiveDuration on Inactive form should be less than 2 sec, but it was " + statsInactive.ActiveDuration.TotalSeconds, statsInactive.ActiveDuration < new TimeSpan(0, 0, 2));
				Assert("InactiveDuration on Inactive form should be more than 2 sec, but it was " + statsInactive.InactiveDuration.TotalSeconds, statsInactive.InactiveDuration > new TimeSpan(0, 0, 2));
			}
		}

		public void TestNoActivitiesLoggedWhenDatabaseUpgradedExceptionHasBeenThrown()
		{
			EnvProxy.Instance.Registry.UserEventTrackingEnterprise = true;
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();

			using (Db.Connection.SetDatabaseUpgradedExceptionHasBeenThrown_ForTest())
			using (Env.SetTemporaryUserContext(UserContextWithActivityTrackingStatusYes))
			using (var testFormActive = new TestZFormWithCaption(bizo) { SetFormCaptionForTest = "Active Form" })
			using (var testFormInactive = new TestZFormWithCaption(bizo) { SetFormCaptionForTest = "Inactive Form" })
			{
				ZFormActivityLogger.Instance.StatLogs.Clear();

				testFormInactive.Show();
				Application.DoEvents();
				testFormActive.Show();
				Application.DoEvents();

				System.Threading.Thread.Sleep(2500);

				testFormInactive.Close();
				testFormActive.Close();

				Assert(!ZFormActivityLogger.Instance.StatLogs.Any());
			}
		}

		public void TestActivityLogging_Dispose()
		{
			EnvProxy.Instance.Registry.UserEventTrackingEnterprise = true;
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();

			using (Env.SetTemporaryUserContext(UserContextWithActivityTrackingStatusYes))
			using (var testForm = new TestZFormWithCaption(bizo))
			{
				testForm.Show();
				Application.DoEvents();

				var stats = UserEventTracker.Instance.GetFormStatsForForm(testForm);
				AssertCollectionContains("Should have the form log", stats, ZFormActivityLogger.Instance.StatLogs);
			}

			ZFormActivityLogger.Instance.SavePendingLogs(false);
			Assert("Should NOT have the form log", !ZFormActivityLogger.Instance.StatLogs.Any(l => l.FormCaption != null && l.FormCaption == "Sheep"));
		}

		#region Implementation

		IUserContext UserContextWithActivityTrackingStatusYes
		{
			get
			{
				var user = new UserForTest { Language = "EN" };
				user.ActivityTrackingStatus = ActivityTrackingStatus.Yes;
				return new UserContextTest.UserContextForTest(user, Env.CurrentCompany);
			}
		}

		IUserContext UserContextWithActivityTrackingStatusCMP
		{
			get
			{
				var user = new UserForTest { Language = "EN" };
				user.ActivityTrackingStatus = ActivityTrackingStatus.BasedOnCompany;
				return new UserContextTest.UserContextForTest(user, Env.CurrentCompany);
			}
		}

		IUserContext UserContextWithActivityTrackingStatusNo
		{
			get
			{
				var user = new UserForTest { Language = "EN" };
				user.ActivityTrackingStatus = ActivityTrackingStatus.No;
				return new UserContextTest.UserContextForTest(user, Env.CurrentCompany);
			}
		}

		class TestZFormWithCaption : ZForm
		{
			public TestZFormWithCaption(IBusiness bizo) : base(bizo) { }

			public override string FormCaption
			{
				get { return formCaption; }
			}
			string formCaption = "Sheep";

			public string SetFormCaptionForTest
			{
				set { formCaption = value; }
			}
		}

		class TestZFormWithoutCaption : ZForm
		{
			public TestZFormWithoutCaption(IBusiness bizo) : base(bizo) { }

			public override string FormCaption
			{
				get { return ""; }
			}
		}

		#endregion
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	[TestedType(typeof(SystemShutdownDate))]
	internal class SystemShutdownDateTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNewSystemShutdownDate()
		{
			var expiry = ZDateTime.Today.AddDays(2);
			var db = Factory.New<LicenceDatabase>();
			db.LD_ManualLicenceExpiry = expiry;

			var shutdown = new SystemShutdownDate(db);
			AssertEquals(expiry, shutdown.NewSystemShutdownDate);

			shutdown.NewSystemShutdownDate = expiry.AddDays(10);
			AssertNoNotifications(shutdown.NewSystemShutdownDateInfo);
			AssertEquals("no change to DB", expiry, db.LD_ManualLicenceExpiry);

			shutdown.NewSystemShutdownDate = ZDate.Empty;
			AssertNoNotifications(shutdown.NewSystemShutdownDateInfo);
		}

		public void TestExpiredMessage()
		{
			var db = Factory.New<LicenceDatabase>();
			db.CustomExpiredNote.Text = "testme";
			var shutdown = new SystemShutdownDate(db);
			AssertEquals("testme", shutdown.ExpiredMessage);

			shutdown.ExpiredMessage = "testme2";
			AssertEquals("testme", db.CustomExpiredNote.Text);
		}

		public void TestExpiryWeekMessage()
		{
			var db = Factory.New<LicenceDatabase>();
			db.CustomExpiryWeekNote.Text = "testme";
			var shutdown = new SystemShutdownDate(db);
			AssertEquals("testme", shutdown.ExpiryWeekMessage);

			shutdown.ExpiryWeekMessage = "testme2";
			AssertEquals("testme", db.CustomExpiryWeekNote.Text);
		}

		public void TestExpiryMonthMessage()
		{
			var db = Factory.New<LicenceDatabase>();
			db.CustomExpiryMonthNote.Text = "testme";
			var shutdown = new SystemShutdownDate(db);
			AssertEquals("testme", shutdown.ExpiryMonthMessage);

			shutdown.ExpiryMonthMessage = "testme2";
			AssertEquals("testme", db.CustomExpiryMonthNote.Text);
		}

		public void TestValidateMessages()
		{
			var db = Factory.New<LicenceDatabase>();
			var shutdown = new SystemShutdownDate(db);
			shutdown.NewSystemShutdownDate = ZDateTime.Today;

			shutdown.ExpiredMessage = "test";
			shutdown.RunPreSaveValidation();
			AssertNoErrors(shutdown.ExpiredMessageInfo);
			AssertHasErrors(shutdown.ExpiryWeekMessageInfo);
			AssertHasErrors(shutdown.ExpiryMonthMessageInfo);

			shutdown.ExpiryWeekMessage = "test";
			shutdown.RunPreSaveValidation();
			AssertNoErrors(shutdown.ExpiredMessageInfo);
			AssertNoErrors(shutdown.ExpiryWeekMessageInfo);
			AssertHasErrors(shutdown.ExpiryMonthMessageInfo);

			shutdown.ExpiryMonthMessage = "test";
			shutdown.RunPreSaveValidation();
			AssertNoErrors(shutdown.ExpiredMessageInfo);
			AssertNoErrors(shutdown.ExpiryWeekMessageInfo);
			AssertNoErrors(shutdown.ExpiryMonthMessageInfo);

			shutdown.ExpiredMessage = "";
			shutdown.RunPreSaveValidation();
			AssertHasErrors(shutdown.ExpiredMessageInfo);
			AssertNoErrors(shutdown.ExpiryWeekMessageInfo);
			AssertNoErrors(shutdown.ExpiryMonthMessageInfo);
		}

		public void TestValidateNewSystemShutdownDate()
		{
			var shutdown = new SystemShutdownDate(null);
			shutdown.RunPreSaveValidation();
			AssertNoErrors(shutdown.NewSystemShutdownDateInfo);

			shutdown.NewSystemShutdownDate = ZDateTime.Today;
			AssertNoErrors(shutdown.NewSystemShutdownDateInfo);

			shutdown.NewSystemShutdownDate = ZDateTime.Today.AddYears(10).AddDays(-1);
			AssertNoErrors(shutdown.NewSystemShutdownDateInfo);

			shutdown.NewSystemShutdownDate = ZDateTime.Today.AddYears(10).AddDays(1);
			AssertHasErrors(shutdown.NewSystemShutdownDateInfo);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SystemShutdownDate(Factory.New<LicenceDatabase>());
		}

		#endregion
	}
}

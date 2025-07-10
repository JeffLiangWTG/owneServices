using System;
using System.Web.Http;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Integration;
using Enterprise.ZClientWebCargoWiseEDI;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace ZClientWebEDI.Test.WebApi.Controllers.IdentityAndSecurity
{
	public class SystemToSystemTrustHelperTest : TestCaseWithFactory
	{
		public void TestGetLicenceDatabase()
		{
			var trustedContext = new TrustedContextForHelperTest();
			var info = new SystemToSystemTrustedRegisteredInfoForTest
			{
				DatabaseNumber = Database.LD_DatabaseNumber.ToString(),
			};

			var db = SystemToSystemTrustHelper.GetLicenceDatabase(Factory, trustedContext, info);
			AssertEquals("Should load the database from the database number", Database, db);
			AssertEquals("Should not create any error messages", null, trustedContext.Messages);
		}

		public void TestGetLicenceDatabase_InvalidDatabaseNumber()
		{
			var trustedContext = new TrustedContextForHelperTest();
			var info = new SystemToSystemTrustedRegisteredInfoForTest
			{
				DatabaseNumber = "abra kadabra",
			};

			var db = SystemToSystemTrustHelper.GetLicenceDatabase(Factory, trustedContext, info);
			AssertEquals("Should not load any db", null, db);
			AssertEquals("Should create error message since database could not be loaded", SystemToSystemTrustHelper.DatabaseNumberNotValidMessage, trustedContext.Messages.Messages[0].Message);
		}

		public void TestGetLicenceDatabase_NoDatabaseMatch()
		{
			var trustedContext = new TrustedContextForHelperTest();
			var info = new SystemToSystemTrustedRegisteredInfoForTest
			{
				DatabaseNumber = "8001",
			};

			var db = SystemToSystemTrustHelper.GetLicenceDatabase(Factory, trustedContext, info);
			AssertEquals("Should not load any db", null, db);
			AssertEquals("Should create error message since database could not be loaded", SystemToSystemTrustHelper.DatabaseNumberNotValidMessage, trustedContext.Messages.Messages[0].Message);
		}

		class SystemToSystemTrustedRegisteredInfoForTest : SystemToSystemTrustedRegisteredInfo
		{
		}

		class TrustedContextForHelperTest : ITrustedContext
		{
			public string Product { get; set; }

			public bool Success { get; set; }
			public ErrorMessages Messages { get; set; }

			public TrustedController Controller { get; set; }

			public IHttpActionResult CreateHttpActionResult()
			{
				throw new NotImplementedException();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Logger = new LoggerForTest();

			var product = "CW1";

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_DatabaseNumber = 8000;
			db.LD_TenantID = string.Empty;
			db.LD_Product = product;
			db.LicEnterprise.LE_EnterpriseID = "E001001";
			Database = db;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			Factory.Save();
		}

		protected LicenceDatabase Database;
		protected LoggerForTest Logger;
	}
}

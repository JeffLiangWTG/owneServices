using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	[TestedType(typeof(LicenceDatabaseLogsRequest))]
	public class LicenceDatabaseLogsRequestTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2010, 1, 5)]
		public void TestSubject()
		{
			LicenceDatabaseLogsRequest request = new LicenceDatabaseLogsRequest(licenceDatabase);
			request.DateFrom = ZDateTime.Now.AddDays(-4);
			request.DateTo = ZDateTime.Now.AddDays(-1);
			request.ServiceTaskCode = "ABC";

			var dummyIncident = Factory.NewWithValidTestData<SupportIncident>();
			dummyIncident.IM_IncidentNumber = "CS01234567";
			Factory.Save();

			request.IncidentNumber = "CS01234567";

			AssertEquals("Request Service Task Logs 2010/01/01 2010/01/04 ABC CS01234567 LD_HostServerName! LD_HostDBName! LD_HostConnectionServerName!", request.Subject);

			licenceDatabase.LD_HostedLocation = "SYD";

			AssertEquals("Request Service Task Logs 2010/01/01 2010/01/04 ABC CS01234567 LD_HostServerName! LD_HostDBName! LD_HostConnectionServerName!", request.Subject);
		}

		[TestDate(2010, 1, 5)]
		public void TestDateFrom()
		{
			LicenceDatabaseLogsRequest request = new LicenceDatabaseLogsRequest(licenceDatabase);
			AssertEquals(new ZDateTime(2009, 12, 29), request.DateFrom);
			request.RunPreSaveValidation();
			AssertNoNotifications(request.DateFromInfo);

			TestDateAttribute.Date = new DateTime(2000, 1, 1, 23, 0, 0);
			request = new LicenceDatabaseLogsRequest(null);
			AssertEquals(new ZDateTime(1999, 12, 25, 23, 0, 0), request.DateFrom);

			TestDateAttribute.Date = new DateTime(2009, 12, 31, 23, 0, 0);
			request = new LicenceDatabaseLogsRequest(null);
			AssertEquals(new ZDateTime(2009, 12, 24, 23, 0, 0), request.DateFrom);

			request.DateFrom = new ZDateTime(2008, 2, 5);
			AssertEquals(new ZDateTime(2008, 2, 5), request.DateFrom);
			AssertNoNotifications(request.DateFromInfo);

			request.DateFrom = new ZDateTime(2009, 2, 25);
			AssertEquals(new ZDateTime(2009, 2, 25), request.DateFrom);
			AssertNoNotifications(request.DateFromInfo);

			request.DateFrom = new ZDateTime(1999, 12, 31);
			AssertHasError(request.DateFromInfo, "Date From year must be 2000 or more");

			request.DateFrom = ZDateTime.Now.AddDays(2);
			AssertHasError(request.DateFromInfo, "Date From must not be after today");
		}

		[TestDate(2010, 1, 5)]
		public void TestDateTo()
		{
			LicenceDatabaseLogsRequest request = new LicenceDatabaseLogsRequest(licenceDatabase);
			AssertEquals(new ZDateTime(2010, 1, 5), request.DateTo);
			request.RunPreSaveValidation();
			AssertNoNotifications(request.DateToInfo);

			TestDateAttribute.Date = new DateTime(2000, 1, 1, 23, 0, 0);
			request = new LicenceDatabaseLogsRequest(null);
			AssertEquals(new ZDateTime(2000, 1, 1, 23, 0, 0), request.DateTo);

			TestDateAttribute.Date = new DateTime(2009, 3, 5, 23, 0, 0);
			request = new LicenceDatabaseLogsRequest(null);
			AssertEquals(new ZDateTime(2009, 3, 5, 23, 0, 0), request.DateTo);

			request.DateTo = new ZDateTime(2008, 2, 5);
			AssertEquals(new ZDateTime(2008, 2, 5), request.DateTo);
			request.DateTo = new ZDateTime(2010, 7, 25);
			AssertEquals(new ZDateTime(2010, 7, 25), request.DateTo);

			request.DateTo = request.DateFrom.AddDays(-1);
			AssertHasError(request.DateToInfo, "Date To must not be less than Date From");

			request.DateTo = ZDateTime.Now.AddDays(2);
			AssertHasError(request.DateToInfo, "Date To must not be after today");
		}

		public void TestServiceTaskCode()
		{
			string error = "Enter into this field one or more service task codes (3 letters long each, or the HOST code) separated by commas (,)";

			LicenceDatabaseLogsRequest request = new LicenceDatabaseLogsRequest(licenceDatabase);
			AssertEquals(ZString.Empty, request.ServiceTaskCode);
			request.RunPreSaveValidation();
			AssertHasError(request.ServiceTaskCodeInfo, "Please enter a value.");

			request.ServiceTaskCode = "ABC";
			AssertEquals("ABC", request.ServiceTaskCode);
			AssertNoNotifications(request.ServiceTaskCodeInfo);

			request.ServiceTaskCode = "ABCD";
			AssertEquals("ABCD", request.ServiceTaskCode);
			AssertHasError(request.ServiceTaskCodeInfo, error);

			request.ServiceTaskCode = "AB";
			AssertEquals("AB", request.ServiceTaskCode);
			AssertHasError(request.ServiceTaskCodeInfo, error);

			request.ServiceTaskCode = "ABC DEF";
			AssertEquals("ABCDEF", request.ServiceTaskCode);
			AssertHasError(request.ServiceTaskCodeInfo, error);

			request.ServiceTaskCode = "ABC,DEF";
			AssertEquals("ABC,DEF", request.ServiceTaskCode);
			AssertNoNotifications(request.ServiceTaskCodeInfo);

			request.ServiceTaskCode = "ABC;DEF";
			AssertEquals("ABC;DEF", request.ServiceTaskCode);
			AssertHasError(request.ServiceTaskCodeInfo, error);

			request.ServiceTaskCode = "ABCD,EF";
			AssertEquals("ABCD,EF", request.ServiceTaskCode);
			AssertHasError(request.ServiceTaskCodeInfo, error);

			request.ServiceTaskCode = "ABC,DEF GHI";
			AssertEquals("ABC,DEFGHI", request.ServiceTaskCode);
			AssertHasError(request.ServiceTaskCodeInfo, error);

			request.ServiceTaskCode = " ABC, DEF, GHI ";
			AssertEquals("ABC,DEF,GHI", request.ServiceTaskCode);
			AssertNoNotifications(request.ServiceTaskCodeInfo);

			request.ServiceTaskCode = "HOST,ABC,DEF";
			AssertEquals("HOST,ABC,DEF", request.ServiceTaskCode);
			AssertNoNotifications(request.ServiceTaskCodeInfo);

			request.ServiceTaskCode = "HOST";
			AssertEquals("HOST", request.ServiceTaskCode);
			AssertNoNotifications(request.ServiceTaskCodeInfo);

			request.ServiceTaskCode = "ABC,DEF,HOST";
			AssertEquals("ABC,DEF,HOST", request.ServiceTaskCode);
			AssertNoNotifications(request.ServiceTaskCodeInfo);

			request.ServiceTaskCode = "HOST ABC";
			AssertEquals("HOSTABC", request.ServiceTaskCode);
			AssertHasError(request.ServiceTaskCodeInfo, error);

			request.ServiceTaskCode = "ABC HOST";
			AssertEquals("ABCHOST", request.ServiceTaskCode);
			AssertHasError(request.ServiceTaskCodeInfo, error);

			request.ServiceTaskCode = "TESHOSTABC";
			AssertEquals("TESHOSTABC", request.ServiceTaskCode);
			AssertHasError(request.ServiceTaskCodeInfo, error);

			request.ServiceTaskCode = "ABC,HOSTBC";
			AssertEquals("ABC,HOSTBC", request.ServiceTaskCode);
			AssertHasError(request.ServiceTaskCodeInfo, error);
		}

		public void TestIncident()
		{
			LicenceDatabaseLogsRequest request = new LicenceDatabaseLogsRequest(licenceDatabase);
			AssertEquals(ZString.Empty, request.IncidentNumber);
			AssertEquals(null, request.Incident);
			request.RunPreSaveValidation();
			AssertHasError(request.IncidentNumberInfo, "Please enter a value.");

			request.IncidentNumber = "0";
			AssertEquals("0", request.IncidentNumber);
			AssertEquals(null, request.Incident);
			AssertHasError(request.IncidentNumberInfo, "Enter a valid Incident.");

			var dummyIncident = Factory.NewWithValidTestData<SupportIncident>();
			dummyIncident.IM_IncidentNumber = "CS01234567";
			Factory.Save();

			request.IncidentNumber = "CS01234567";
			AssertEquals("CS01234567", request.IncidentNumber);
			AssertEquals(dummyIncident.PK, request.Incident.PK);
			AssertNoNotifications(request.IncidentNumberInfo);
		}

		public void TestVersionSupports()
		{
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.VersionNumber = LicenceDatabaseLogsRequest.FirstVersion;
			licenceDatabase.LD_HL_CurrentRunningVersion = build.PK;
			licenceDatabase.LD_ReleaseRing = "ALP";

			var validation = new NotificationCollection();

			Assert(LicenceDatabaseLogsRequest.IsSupportingVersion(licenceDatabase, validation));
			Assert(!validation.HasNotifications());

			licenceDatabase.LD_PublicEmailAddressForUpdate = "";
			Assert("not supported if no email", !LicenceDatabaseLogsRequest.IsSupportingVersion(licenceDatabase, validation));
			Assert("Notifications should contain invalid email error message", validation.HasNotifications());
			Assert("Notifications should contain invalid email error message", validation.Any(x =>
				x.Message == LicenceDatabaseLogsRequest.EmailValidationMessage));

			validation.Clear();
			licenceDatabase.LD_PublicEmailAddressForUpdate = "valid@test.com";
			licenceDatabase.LD_HL_CurrentRunningVersion = ZGuid.Empty;
			Assert("not supported if no build", !LicenceDatabaseLogsRequest.IsSupportingVersion(licenceDatabase, validation));
			Assert("Notifications should contain invalid version error message", validation.HasNotifications());
			Assert("Notifications should contain invalid version error message", validation.Any(x => x.Message == LicenceDatabaseLogsRequest.InvalidVersionValidationMessage));

			validation.Clear();
			licenceDatabase.LD_HL_CurrentRunningVersion = build.PK;
			build.VersionNumber = LicenceDatabaseLogsRequest.FirstVersion.AddRelease(-1);
			Assert("not supported if old build", !LicenceDatabaseLogsRequest.IsSupportingVersion(licenceDatabase, validation));
			Assert("Notifications should contain outdated version error message", validation.HasNotifications());
			Assert("Notifications should contain outdated version error message", validation.Any(x => x.Message == LicenceDatabaseLogsRequest.OutdatedVersionValidationMessage));

			validation.Clear();
			build.VersionNumber = LicenceDatabaseLogsRequest.FirstVersion.AddRelease(30);
			Assert(LicenceDatabaseLogsRequest.IsSupportingVersion(licenceDatabase, validation));
			Assert(!validation.HasNotifications());

			validation.Clear();
			licenceDatabase.LD_ReleaseRing = "GPR";
			build.VersionNumber = LicenceDatabaseLogsRequest.GprVersion.AddRelease(30);
			Assert(LicenceDatabaseLogsRequest.IsSupportingVersion(licenceDatabase, validation));
			Assert(!validation.HasNotifications());

			validation.Clear();
			build.VersionNumber = LicenceDatabaseLogsRequest.GprVersion.AddRelease(-1);
			Assert("not supported if old build", !LicenceDatabaseLogsRequest.IsSupportingVersion(licenceDatabase, validation));
			Assert("Notifications should contain outdated version error message", validation.HasNotifications());
			Assert("Notifications should contain outdated version error message", validation.Any(x => x.Message == LicenceDatabaseLogsRequest.OutdatedVersionValidationMessage));
		}

		public void TestIsSupportingNewVersion()
		{
			var build = Factory.New<ReleaseBuild>();
			build.VersionNumber = LicenceDatabaseLogsRequest.NewVersion;
			licenceDatabase.LD_HL_CurrentRunningVersion = build.PK;
			licenceDatabase.LD_ReleaseRing = "ALP";

			var validation = new NotificationCollection();

			Assert(LicenceDatabaseLogsRequest.IsSupportingNewVersion(licenceDatabase, validation));
			Assert(!validation.HasNotifications());

			licenceDatabase.LD_PublicEmailAddressForUpdate = "";
			Assert("not supported if no email", !LicenceDatabaseLogsRequest.IsSupportingNewVersion(licenceDatabase, validation));
			Assert("Notifications should contain invalid email error message",validation.HasNotifications());
			Assert("Notifications should contain invalid email error message", validation.Any(x =>
				x.Message == LicenceDatabaseLogsRequest.EmailValidationMessage));

			validation.Clear();
			licenceDatabase.LD_PublicEmailAddressForUpdate = "valid@test.com";
			build.VersionNumber = LicenceDatabaseLogsRequest.NewVersion.AddRelease(-1);
			Assert("not supported if old build", !LicenceDatabaseLogsRequest.IsSupportingNewVersion(licenceDatabase, validation));
			Assert("Notifications should contain outdated version error message", validation.HasNotifications());
			Assert("Notifications should contain outdated version error message", validation.Any(x => x.Message == LicenceDatabaseLogsRequest.OutdatedVersionValidationMessage));
		}

		protected override void SetUp()
		{
			base.SetUp();

			licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_PublicEmailAddressForUpdate = "valid@test.com";
			licenceDatabase.LD_HostServerName = "LD_HostServerName!";
			licenceDatabase.LD_HostDBInstance = "LD_HostDBInstance!";
			licenceDatabase.LD_HostDBName = "LD_HostDBName!";
			licenceDatabase.LD_HostConnectionServerName = "LD_HostConnectionServerName!";
			licenceDatabase.LD_HostedLocation = "NCW";
		}

		/*protected override void TearDown()
		{
			licenceDatabase.Delete();

			base.TearDown();
		}*/

		LicenceDatabase licenceDatabase;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new LicenceDatabaseLogsRequest(null);
		}
	}
}

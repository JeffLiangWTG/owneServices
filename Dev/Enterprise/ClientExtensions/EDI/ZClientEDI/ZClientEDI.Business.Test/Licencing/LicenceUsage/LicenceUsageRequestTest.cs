using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.ServiceManager.Tasks.StandardXMLProcessor;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	[TestedType(typeof(LicenceUsageRequest))]
	public class LicenceUsageRequestTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2010, 1, 5)]
		public void TestDateFrom()
		{
			LicenceUsageRequest request = new LicenceUsageRequest(null);
			AssertEquals(new ZDateTime(2009, 12, 1), request.DateFrom);
			request.RunPreSaveValidation();
			AssertNoNotifications(request.DateFromInfo);

			TestDateAttribute.Date = new DateTime(2000, 1, 1, 23, 0, 0);
			request = new LicenceUsageRequest(null);
			AssertEquals(new ZDateTime(1999, 12, 1), request.DateFrom);

			TestDateAttribute.Date = new DateTime(2009, 12, 31, 23, 0, 0);
			request = new LicenceUsageRequest(null);
			AssertEquals(new ZDateTime(2009, 11, 1), request.DateFrom);

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
			LicenceUsageRequest request = new LicenceUsageRequest(null);
			AssertEquals(new ZDateTime(2009, 12, 31), request.DateTo);
			request.RunPreSaveValidation();
			AssertNoNotifications(request.DateToInfo);

			TestDateAttribute.Date = new DateTime(2000, 1, 1, 23, 0, 0);
			request = new LicenceUsageRequest(null);
			AssertEquals(new ZDateTime(1999, 12, 31), request.DateTo);

			TestDateAttribute.Date = new DateTime(2009, 3, 5, 23, 0, 0);
			request = new LicenceUsageRequest(null);
			AssertEquals(new ZDateTime(2009, 2, 28), request.DateTo);

			request.DateTo = new ZDateTime(2008, 2, 5);
			AssertEquals(new ZDateTime(2008, 2, 5), request.DateTo);
			request.DateTo = new ZDateTime(2010, 7, 25);
			AssertEquals(new ZDateTime(2010, 7, 25), request.DateTo);

			request.DateTo = request.DateFrom.AddDays(-1);
			AssertHasError(request.DateToInfo, "Date To must not be less than Date From");

			request.DateTo = ZDateTime.Now.AddDays(2);
			AssertHasError(request.DateToInfo, "Date To must not be after today");
		}

		public void TestIsSupportingVersion()
		{
			EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "16.10.20.0");
			LicenceDatabase licenceDatabase = Factory.New<LicenceDatabase>();

			licenceDatabase.LD_PublicEmailAddressForUpdate = "valid@test.com";
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.VersionNumber = LicenceUsageRequest.FirstVersion;
			licenceDatabase.LD_HL_CurrentRunningVersion = build.PK;

			var validation = new NotificationCollection();
			Assert(LicenceUsageRequest.IsSupportingVersion(licenceDatabase, validation));
			Assert(!validation.HasNotifications());

			validation.Clear();
			licenceDatabase.LD_PublicEmailAddressForUpdate = "";
			Assert("not supported if no email", !LicenceUsageRequest.IsSupportingVersion(licenceDatabase, validation));
			Assert("Notifications should contain invalid email error message", validation.HasNotifications());
			Assert("Notifications should contain invalid email error message", validation.Any(x => x.Message == LicenceUsageRequest.EmailValidationMessage));

			validation.Clear();
			build.VersionNumber = new VersionNumber(16, 20, 20, 0);
			AssertEquals("supported if version uses SystemMessage even if no email", true, LicenceUsageRequest.IsSupportingVersion(licenceDatabase, validation));
			Assert(!validation.HasNotifications());

			build.VersionNumber = LicenceUsageRequest.FirstVersion;

			validation.Clear();
			licenceDatabase.LD_PublicEmailAddressForUpdate = "valid@test.com";
			licenceDatabase.LD_HL_CurrentRunningVersion = ZGuid.Empty;
			Assert("not supported if no build", !LicenceUsageRequest.IsSupportingVersion(licenceDatabase, validation));
			Assert("Notifications should contain invalid version error message", validation.HasNotifications());
			Assert("Notifications should contain invalid version error message", validation.Any(x => x.Message == LicenceUsageRequest.InvalidVersionValidationMessage));

			validation.Clear();
			licenceDatabase.LD_HL_CurrentRunningVersion = build.PK;
			build.VersionNumber = LicenceUsageRequest.FirstVersion.AddRelease(-1);
			Assert("not supported if old build", !LicenceUsageRequest.IsSupportingVersion(licenceDatabase, validation));
			Assert("Notifications should contain outdated version error message", validation.HasNotifications());
			Assert("Notifications should contain outdated version error message", validation.Any(x => x.Message == LicenceUsageRequest.OutdatedVersionValidationMessage));

			validation.Clear();
			build.VersionNumber = LicenceUsageRequest.FirstVersion.AddRelease(30);
			Assert(LicenceUsageRequest.IsSupportingVersion(licenceDatabase, validation));
			Assert(!validation.HasNotifications());
		}

		public void TestSendSystemMessage()
		{
			DebugOnlyOutgoingSystemMessage.Initialize();
			ObjectFactory.Substitute<IOutgoingSystemMessage>(new DebugOnlyOutgoingSystemMessage());
			EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "16.10.20.0");
			var build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(16, 10, 20, 0);
			var licDatabase = BillingTestHelper.CreateLicence(Factory, "AAA").Database;
			licDatabase.LD_HL_CurrentRunningVersion = build.PK;
			var request = new LicenceUsageRequest(licDatabase);
			request.DateFrom = new ZDateTime(2016, 10, 1);
			request.DateTo = new ZDateTime(2016, 10, 30);
			request.Send();

			string expectedXml =
@"<LicenceUsageRequest>
  <DateFrom>2016/10/01</DateFrom>
  <DateTo>2016/10/30</DateTo>
  <RequestedBy>E</RequestedBy>
</LicenceUsageRequest>";

			var actualXml = DebugOnlyOutgoingSystemMessage.XmlMessageBody;
			AssertEquals(expectedXml, actualXml);

			var receivedInfo = new LicenceUsageRequestInfo(actualXml);
			AssertEquals(new DateTime(2016, 10, 1), receivedInfo.DateFrom);
			AssertEquals(new DateTime(2016, 10, 30), receivedInfo.DateTo);
			AssertEquals(Env.CurrentUser.Initials.Trim(), receivedInfo.RequestedBy);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new LicenceUsageRequest(null);
		}
	}
}

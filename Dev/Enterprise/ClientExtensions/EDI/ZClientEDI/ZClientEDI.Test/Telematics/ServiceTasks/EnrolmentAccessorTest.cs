using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Telematics.ServiceTasks;
using Enterprise.MasterFiles.Business;

namespace ZClientEDI.Test.Telematics.ServiceTasks
{
	class EnrolmentAccessorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			enrollmentAccessor = new EnrolmentAccessor();
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 41243;
			clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");
			clientDevice = Factory.New<ClientDeviceHeader>();
			clientDevice.CDH_Identifier = "1";
			orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
		}

		ClientCompany clientCompany;
		ClientDeviceHeader clientDevice;
		OrgCusCode orgCusCode;
		IEnrolmentAccessor enrollmentAccessor;

		public void TestNoDevices()
		{
			// Arrange
			var time = new DateTimeOffset(2020, 5, 1, 1, 1, 1, 1, TimeSpan.Zero);
			var enrollmentPeriodStart = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
			var enrollmentPeriodEnd = enrollmentPeriodStart.AddMonths(1);

			// Act
			var result = enrollmentAccessor.GetEnrollments(Factory, enrollmentPeriodStart, enrollmentPeriodEnd);

			// Assert
			AssertEquals(0, result.Count());
		}

		public void TestRetrievesEnrollmentsWithNullEndTime()
		{
			var time = new DateTimeOffset(2020, 10, 10, 10, 10, 10, 10, TimeSpan.Zero);
			var enrollmentPeriodStart = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
			var enrollmentPeriodEnd = enrollmentPeriodStart.AddMonths(1);
			var expectedEnrollments = new[]
			{
				GetRegistration(clientCompany, clientDevice, orgCusCode, "01", "OSOM", new DateTimeOffset(time.Year, 10, 31, 23, 59, 59, 59, TimeSpan.Zero)),
			};
			Factory.Save();

			RetrievesCurrentlyEnrolledDevicesTest(expectedEnrollments, enrollmentPeriodStart, enrollmentPeriodEnd);
		}

		public void TestRetrievesEnrollmentsWithStartPriorToReportingMonth()
		{
			var time = new DateTimeOffset(2020, 10, 10, 10, 10, 10, 10, TimeSpan.Zero);
			var enrollmentPeriodStart = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
			var enrollmentPeriodEnd = enrollmentPeriodStart.AddMonths(1);
			var expectedEnrollments = new[]
			{
				GetRegistration(clientCompany, clientDevice, orgCusCode, "02", "OSOM", enrollmentPeriodStart.AddYears(-1)),
				GetRegistration(clientCompany, clientDevice, orgCusCode, "03", "OSOM", enrollmentPeriodStart.AddMonths(-5)),
			};
			Factory.Save();

			RetrievesCurrentlyEnrolledDevicesTest(expectedEnrollments, enrollmentPeriodStart, enrollmentPeriodEnd);
		}

		public void TestRetrievesEnrollmentsWithStartDuringReportingMonth()
		{
			var time = new DateTimeOffset(2020, 10, 10, 10, 10, 10, 10, TimeSpan.Zero);
			var enrollmentPeriodStart = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
			var enrollmentPeriodEnd = enrollmentPeriodStart.AddMonths(1);
			var expectedEnrollments = new[]
			{
				GetRegistration(clientCompany, clientDevice, orgCusCode, "02", "OSOM", enrollmentPeriodStart.AddDays(5)),
				GetRegistration(clientCompany, clientDevice, orgCusCode, "03", "OSOM", enrollmentPeriodStart.AddDays(10)),
			};
			Factory.Save();

			RetrievesCurrentlyEnrolledDevicesTest(expectedEnrollments, enrollmentPeriodStart, enrollmentPeriodEnd);
		}

		public void TestRetrievesEnrollmentsWithEndDuringReportingMonth()
		{
			var time = new DateTimeOffset(2020, 10, 10, 10, 10, 10, 10, TimeSpan.Zero);
			var enrollmentPeriodStart = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
			var enrollmentPeriodEnd = enrollmentPeriodStart.AddMonths(1);
			var expectedEnrollments = new[]
			{
				GetRegistration(clientCompany, clientDevice, orgCusCode, "01", "OSOM", enrollmentPeriodStart, new DateTimeOffset(time.Year, 10, 31, 23, 59, 59, 59, TimeSpan.Zero)),
				GetRegistration(clientCompany, clientDevice, orgCusCode, "02", "OSOM", enrollmentPeriodStart, new DateTimeOffset(time.Year, 10, 1, 0, 0, 1, TimeSpan.Zero)),
			};
			Factory.Save();

			RetrievesCurrentlyEnrolledDevicesTest(expectedEnrollments, enrollmentPeriodStart, enrollmentPeriodEnd);
		}

		public void TestRetrievesEnrollmentsWithEndAfterReportingMonth()
		{
			var time = new DateTimeOffset(2020, 10, 10, 10, 10, 10, 10, TimeSpan.Zero);
			var enrollmentPeriodStart = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
			var enrollmentPeriodEnd = enrollmentPeriodStart.AddMonths(1);
			var expectedEnrollments = new[]
			{
				GetRegistration(clientCompany, clientDevice, orgCusCode, "01", "OSOM", new DateTimeOffset(time.Year, 10, 31, 23, 59, 59, 59, TimeSpan.Zero), new DateTimeOffset(time.Year, 11, 2, 0, 0, 0, TimeSpan.Zero)),
			};
			Factory.Save();

			RetrievesCurrentlyEnrolledDevicesTest(expectedEnrollments, enrollmentPeriodStart, enrollmentPeriodEnd);
		}

		public void TestRetrievesEnrollmentAtMinimumReportingPeriod()
		{
			var time = new DateTimeOffset(2020, 10, 10, 10, 10, 10, 10, TimeSpan.Zero);
			var enrollmentPeriodStart = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
			var enrollmentPeriodEnd = enrollmentPeriodStart.AddMonths(1);
			var expectedEnrollments = new[]
			{
				GetRegistration(clientCompany, clientDevice, orgCusCode, "01", "OSOM", enrollmentPeriodStart.AddMonths(-2), enrollmentPeriodStart),
			};
			GetRegistration(clientCompany, clientDevice, orgCusCode, "02", "OSOM", enrollmentPeriodStart.AddMonths(-2), enrollmentPeriodStart.AddMilliseconds(-1));
			Factory.Save();

			RetrievesCurrentlyEnrolledDevicesTest(expectedEnrollments, enrollmentPeriodStart, enrollmentPeriodEnd);
		}

		void RetrievesCurrentlyEnrolledDevicesTest(IEnumerable<ClientTelRimRegistration> expectedEnrollments, DateTimeOffset enrollmentPeriodStart, DateTimeOffset enrollmentPeriodEnd)
		{
			// Arrange
			// Act
			var result = enrollmentAccessor.GetEnrollments(Factory, enrollmentPeriodStart, enrollmentPeriodEnd).ToArray();

			// Assert
			AssertArrayEqualsByElements(
				expectedEnrollments.Select(e => e.TRR_EnrolmentId).OrderBy(id => id).ToArray(),
				result.Select(r => r.TRR_EnrolmentId).OrderBy(id => id).ToArray());
		}

		public void TestDoesNotRetrieveEnrollmentsOutOfCurrentMonth()
		{
			// Arrange
			var time = new DateTimeOffset(2020, 10, 10, 10, 10, 10, 10, TimeSpan.Zero);
			var enrollmentPeriodStart = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
			var enrollmentPeriodEnd = enrollmentPeriodStart.AddMonths(1);
			var expectedEnrollments = new[]
			{
				GetRegistration(clientCompany, clientDevice, orgCusCode, "01", "OSOM", time.AddMonths(1)),
				GetRegistration(clientCompany, clientDevice, orgCusCode, "02", "OSOM", time.AddMonths(2)),
				GetRegistration(clientCompany, clientDevice, orgCusCode, "03", "OSOM", time.AddMonths(-3), time.AddMonths(-2)),
				GetRegistration(clientCompany, clientDevice, orgCusCode, "04", "OSOM", new DateTimeOffset(time.Year, 9, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(time.Year, 9, 30, 23, 59, 59, 59, TimeSpan.Zero)),
			};
			Factory.Save();

			// Act
			var result = enrollmentAccessor.GetEnrollments(Factory, enrollmentPeriodStart, enrollmentPeriodEnd).ToArray();

			// Assert
			AssertEquals(0, result.Length);
		}

		public void TestDoesNotRetrieveEnrollmentEndedPriorToReportingMonth()
		{
			var time = new DateTimeOffset(2020, 10, 10, 10, 10, 10, 10, TimeSpan.Zero);
			var enrollmentPeriodStart = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
			var enrollmentPeriodEnd = enrollmentPeriodStart.AddMonths(1);

			GetRegistration(clientCompany, clientDevice, orgCusCode, "01", "OSOM", enrollmentPeriodStart.AddMonths(1));
			GetRegistration(clientCompany, clientDevice, orgCusCode, "02", "OSOM", enrollmentPeriodStart.AddMonths(2), enrollmentPeriodEnd.AddMonths(5));
			Factory.Save();

			DoesNotRetrieveEnrollmentsOutOfCurrentMonthTest(enrollmentPeriodStart, enrollmentPeriodEnd);
		}

		public void TestDoesNotRetrieveEnrollmentWithStartAfterReportingMonth()
		{
			var time = new DateTimeOffset(2020, 10, 10, 10, 10, 10, 10, TimeSpan.Zero);
			var enrollmentPeriodStart = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
			var enrollmentPeriodEnd = enrollmentPeriodStart.AddMonths(1);

			GetRegistration(clientCompany, clientDevice, orgCusCode, "03", "OSOM", time.AddMonths(-3), time.AddMonths(-2));
			Factory.Save();

			DoesNotRetrieveEnrollmentsOutOfCurrentMonthTest(enrollmentPeriodStart, enrollmentPeriodEnd);
		}

		void DoesNotRetrieveEnrollmentsOutOfCurrentMonthTest(DateTimeOffset enrollmentPeriodStart, DateTimeOffset enrollmentPeriodEnd)
		{
			// Arrange
			// Act
			var result = enrollmentAccessor.GetEnrollments(Factory, enrollmentPeriodStart, enrollmentPeriodEnd).ToArray();

			// Assert
			AssertEquals(0, result.Length);
		}

		public void TestWrongParamsCall()
		{
			// Arrange
			// Act
			// Assert
			AssertExceptionThrown(typeof(ArgumentNullException), () => new EnrolmentAccessor().GetEnrollments(null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));
		}

		ClientTelRimRegistration GetRegistration(ClientCompany company, ClientDeviceHeader device, OrgCusCode code, string enrolmentId, string scheme, DateTimeOffset startTime, DateTimeOffset? endTime = null)
		{
			var clientTelRimRegistration = Factory.New<ClientTelRimRegistration>();
			clientTelRimRegistration.TRR_LCC_ClientCompany = company.PK;
			clientTelRimRegistration.TRR_CDH_ClientDeviceHeader = device.PK;
			clientTelRimRegistration.TRR_OK_OrgCusCode = code.PK;
			clientTelRimRegistration.TRR_EnrolmentId = enrolmentId;
			clientTelRimRegistration.TRR_EnrolmentScheme = scheme;
			clientTelRimRegistration.TRR_StartTime = startTime;
			clientTelRimRegistration.TRR_InstallationDateTimeOffset = startTime;
			if (endTime != null)
			{
				clientTelRimRegistration.TRR_EndTime = (DateTimeOffset)endTime;
			}
			return clientTelRimRegistration;
		}
	}
}

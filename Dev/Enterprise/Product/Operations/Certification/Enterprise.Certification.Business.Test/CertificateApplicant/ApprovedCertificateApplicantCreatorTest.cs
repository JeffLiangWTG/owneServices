using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Certification.Business.Testing
{
	sealed class ApprovedCertificateApplicantCreatorTest : TestCaseWithFactory
	{
		public void TestGetCannotCreateDuplicateUserMessage()
		{
			CertificateApplicant existingApplicant = Factory.New<CertificateApplicant>();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			existingApplicant.Logs.AddNew(AutoEvents.EditedARecord, "Approved by The Big Boss (BB)");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			string actualMessage = ApprovedCertificateApplicantCreator.GetCannotCreateDuplicateUserMessage("John Smith", "jsmith@cargowise.com", existingApplicant);
			const string expected = "The anonymous user (John Smith - jsmith@cargowise.com) you are trying to approve already exists in the database. Previously approved by BB.";
			AssertEquals(expected, actualMessage);
		}

		public void TestCreateAndSaveNewApplicant_ExistingApplicant()
		{
			CertificateApplicant existingApplicant = Factory.New<CertificateApplicant>();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			existingApplicant.Logs.AddNew(AutoEvents.EditedARecord, "Approved by The Big Boss (BB)");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			existingApplicant.HA_EmailAddress = "newuser@cargowise.com";
			TestNotificationHandler handler = new TestNotificationHandler();
			CertificateApplicant newApplicant = new ApprovedCertificateApplicantCreator().CreateAndSaveNewApplicant(Factory, PopulatedQueryString, handler);
			AssertNull("Applicant already exists, should not create a new one", newApplicant);
			AssertEquals(1, handler.Infos.Count);
			AssertEquals("User Already Exists", handler.Infos[0].Key);
			AssertEquals("The anonymous user (John Smith - newuser@cargowise.com) you are trying to approve already exists in the database. Previously approved by BB.", handler.Infos[0].Value);
		}

		public void TestCreateAndSaveNewApplicant_UniqueIndexFailure()
		{
			CertificateApplicant existingApplicant = new BusinessObjectFactory().NewWithValidTestData<CertificateApplicant>();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			existingApplicant.Logs.AddNew(AutoEvents.EditedARecord, "Approved by The Big Boss (BB)");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			existingApplicant.HA_EmailAddress = "newuser@cargowise.com";

			Factory.Saving += delegate(BusinessObjectFactory factory)
			{
				existingApplicant.Factory.Save();
			};
			TestNotificationHandler handler = new TestNotificationHandler();
			CertificateApplicant newApplicant = new ApprovedCertificateApplicantCreator().CreateAndSaveNewApplicant(Factory, PopulatedQueryString, handler);
			AssertNull("Applicant already exists, should not create a new one", newApplicant);
			AssertEquals(1, handler.Infos.Count);
			AssertEquals("User Already Exists", handler.Infos[0].Key);
			AssertEquals("The anonymous user (John Smith - newuser@cargowise.com) you are trying to approve already exists in the database. Previously approved by BB.", handler.Infos[0].Value);
		}

		public void TestCreateAndSaveNewApplicant_NotAllFieldsPopulated()
		{
			TestNotificationHandler handler = new TestNotificationHandler();
			SecureQueryString queryString = new SecureQueryString();
			queryString.Add(ApprovedCertificateApplicantCreator.FieldNames.ApprovingStaffCode, "XXX");
			queryString.Add(ApprovedCertificateApplicantCreator.FieldNames.Email, "newuser@cargowise.com");
			queryString.Add(ApprovedCertificateApplicantCreator.FieldNames.FullName, "Jordan Smith");
			queryString.Add(ApprovedCertificateApplicantCreator.FieldNames.Gender, "M");
			CertificateApplicant newApplicant = new ApprovedCertificateApplicantCreator().CreateAndSaveNewApplicant(Factory, queryString, handler);
			AssertEquals(0, handler.Infos.Count);
			Assert("Should be created and saved in the database", newApplicant.IsInDatabase);
			AssertEquals("Jordan Smith", newApplicant.HA_FullName);
			AssertEquals("newuser@cargowise.com", newApplicant.HA_EmailAddress);
			AssertEquals("XXX", newApplicant.ApprovedBy);
		}

		public void TestCreateAndSaveNewApplicant()
		{
			CertificateApplicant existingApplicant = new BusinessObjectFactory().NewWithValidTestData<CertificateApplicant>();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			existingApplicant.Logs.AddNew(AutoEvents.EditedARecord, "Approved by The Big Boss (BB)");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			existingApplicant.HA_EmailAddress = "newuser@cargowise.com";

			TestNotificationHandler handler = new TestNotificationHandler();
			CertificateApplicant newApplicant = new ApprovedCertificateApplicantCreator().CreateAndSaveNewApplicant(Factory, PopulatedQueryString, handler);
			AssertEquals(0, handler.Infos.Count);
			Assert("Should be created and saved in the database", newApplicant.IsInDatabase);
			AssertEquals("Mr.", newApplicant.HA_Title);
			AssertEquals("John Smith", newApplicant.HA_FullName);
			AssertEquals("Sr.", newApplicant.HA_NameSuffix);
			AssertEquals(ZDateTime.BrettsBirthday, newApplicant.HA_Birthdate);
			AssertEquals("M", newApplicant.HA_Gender);
			AssertEquals("Unit 3", newApplicant.HA_UserAddress1);
			AssertEquals("73a O'Riordan Street", newApplicant.HA_UserAddress2);
			AssertEquals("Alexandria", newApplicant.HA_City);
			AssertEquals("NSW", newApplicant.HA_State);
			AssertEquals("2015", newApplicant.HA_Postcode);
			AssertEquals("AU", newApplicant.HA_RN_NKCountry);
			AssertEquals("newuser@cargowise.com", newApplicant.HA_EmailAddress);
			AssertEquals("0423030777", newApplicant.HA_MobilePhone);
			AssertEquals("02 95578211", newApplicant.HA_HomePhone);
			AssertEquals("02 95578212", newApplicant.HA_FaxNum);
			AssertEquals("02 95578213", newApplicant.HA_WorkPhone);
			AssertEquals("225", newApplicant.HA_WorkExtension);
			AssertEquals("N329392", newApplicant.HA_Passport);
			AssertEquals("S09212", newApplicant.HA_DriversLicenseNumber);
			AssertEquals("ID", newApplicant.HA_RN_NKNationalityCodeISO);
			AssertEquals("XXX", newApplicant.ApprovedBy);
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XXX";
			staff.GS_FullName = "The Second Boss";
			Factory.Save();
		}

		SecureQueryString PopulatedQueryString
		{
			get
			{
				if (populatedQueryString == null)
				{
					populatedQueryString = new SecureQueryString("v3HVcf8upElAJV39hMARUlXJOjNBSoJdmMMzt3vqtZY2Z5brpY23EqHLn4Yp9ptZMPJDgt14E2HZHCVkuupE2JHOPPEtTTijSO63H7blIZg0vftGLN+S4Ap4VBrw9rW30zwY8scuOAI59J3O3I0VZFSsJgc4mEDtlSUSW22wmrI1t64qwSwKyW7Xn1Rb5Ewvo8pv2+J+bQxdIE//syWq9zYYLcfC6yZ8kqDXAZRbh2mDQesERSgs8d9W9Xwhl2l/mawEwu5KilNT61cEIWHTkwLV7CJu9B5uC+6UqovEoLRbMmgOtOpm3A9ZJqZJqf4yNHYNGnhkKSkybGiwY79ZyMybYUcgdFTuvJcDKv4Jebv3eDytPaEieRnQcHdG2GG8Ka/1VXasoFVb8MpNIGsXTFwIVClThVc64IaTP+9vs9egCF1B0OHLTAE5yCHhKUK1tZbnMxzMv3zdnVUP5+FN0/bNH/pYbdpfrR3RiQP4sh9tyEVrogGSJlnC5A7BmrFqVxs9u7NnmcMeSlUWvx5UJcYc/NUvO5Qsh8icdFmwDe2decLfntxQbaBq369KKx2mNI1qEGevrQsWGBMPIWeMp8a/FlIZzQVfbcff5702H0gzW7YGJ8u5gt3He16XO3BJkD25QTpC0l3EJRzrVYI+nZiUL5+r6QunCqB45G9ls9aE8mVilIpYhg==");
					// The reverse of CertificateApplicantTest.TestAnonymousUserRegistrationApprovalEmail					
					// Title = "Mr.";
					// Full Name = "John Smith";
					// Name Suffix = "Sr.";
					// Birthdate = ZDateTime.BrettsBirthday;
					// Gender = "M";
					// User Address 1 = "Unit 3";
					// User Address 2 = "73a O'Riordan Street";
					// City = "Alexandria";
					// State = "NSW";
					// Postcode = "2015";
					// Country Code = "AU";
					// Email Address = "newuser@cargowise.com";
					// Mobile Phone = "0423030777";
					// Home Phone = "02 95578211";
					// Fax Num = "02 95578212";
					// Work Phone = "02 95578213";
					// Work Extension = "225";
					// Passport = "N329392";
					// Drivers License Number = "S09212";
					// Nationality = "ID";
				}
				return populatedQueryString;
			}
		}

		SecureQueryString populatedQueryString;

		class TestNotificationHandler : INotificationHandler
		{
			#region INotificationHandler Members

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				Errors.Add(new KeyValuePair<string, string>(caption, message));
			}

			public void ReportInformation(string message, string caption)
			{
				Infos.Add(new KeyValuePair<string, string>(caption, message));
			}

			#endregion

			public List<KeyValuePair<string, string>> Errors = new List<KeyValuePair<string, string>>();
			public List<KeyValuePair<string, string>> Infos = new List<KeyValuePair<string, string>>();
		}
	}
}

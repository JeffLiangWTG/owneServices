using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.JobApplication;
using Moq;
using static Enterprise.ZClientWebCargoWiseEDI.Test.JobApplicationControllerTestCase;

namespace Enterprise.ZClientWebCargoWiseEDI.Test
{
	class JobApplicationControllerTestCase : TestCaseWithFactory
	{
		public void TestCreateJobApplicationExistingPersonNoApplicantApplicant()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			var person = Factory.New<GlbPerson>();
			person.PER_EmailAddress = "shire.baggins@example.com";
			person.PER_FullName = "dodo maggins";
			person.PER_MobilePhone = "+64 3-234 5678";
			person.PER_RN_NKCountry = "AU";
			Factory.Save();
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-111 1111", country_code = "NZ" };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Application creation should have succeeded", "Successfully created application.", result);
			var applicationQuery = new ZQuery(HRJobApplicationSchema.HP_HV, jobCampaign.PK);
			var newApplication = Factory.LoadTop1<HRJobApplication>(applicationQuery);
			AssertNotNull(newApplication);
			var reloadedApplicant = Factory.Load<HRJobApplicant>(newApplication.HP_HA);
			AssertNotNull(reloadedApplicant);
			AssertEquals("Precondition", "shire.baggins@example.com", reloadedApplicant.HA_EmailAddress);
			AssertEquals("Should not update full name", "dodo maggins", reloadedApplicant.HA_FullName);
			AssertEquals("Should not update mobile phone", "+64 3-234 5678", reloadedApplicant.HA_MobilePhone);
			AssertEquals("Should not update country", "AU", reloadedApplicant.HA_RN_NKCountry);
		}

		public void TestCreateJobApplicationExistingApplicant()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "dodo maggins";
			applicant.HA_MobilePhone = "+64 3-234 5678";
			applicant.HA_RN_NKCountry = "AU";
			Factory.Save();
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-111 1111", country_code = "NZ" };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Application creation should have succeeded", "Successfully created application.", result);
			var applicationQuery = new ZQuery(HRJobApplicationSchema.HP_HV, jobCampaign.PK);
			applicationQuery.AddToFilter(HRJobApplicationSchema.HP_HA, applicant.PK);
			var newApplication = Factory.LoadTop1<HRJobApplication>(applicationQuery);
			AssertNotNull(newApplication);
			var reloadedApplicant = Factory.Load<HRJobApplicant>(newApplication.HP_HA);
			AssertNotNull(reloadedApplicant);
			AssertEquals("Precondition", "shire.baggins@example.com", reloadedApplicant.HA_EmailAddress);
			AssertEquals("Should not update full name", "dodo maggins", reloadedApplicant.HA_FullName);
			AssertEquals("Should not update mobile phone", "+64 3-234 5678", reloadedApplicant.HA_MobilePhone);
			AssertEquals("Should not update country", "AU", reloadedApplicant.HA_RN_NKCountry);
		}

		public void TestCreateJobApplicationNewApplicant()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			Factory.Save();
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "NZ" };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Application creation should have succeeded", "Successfully created application.", result);
			var applicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)"shire.baggins@example.com");
			AssertNotNull(applicant);
			AssertEquals("Precondition", "shire.baggins@example.com", applicant.HA_EmailAddress);
			AssertEquals("Should have set full name", "shrodo dragons", applicant.HA_FullName);
			AssertEquals("Should have set mobile phone", "+64 3-234 5678", applicant.HA_MobilePhone);
			AssertEquals("Should have set country", "NZ", applicant.HA_RN_NKCountry);
			AssertEquals("New applicant should have a new application", 1, applicant.Applications.Count);
			var newApplication = applicant.Applications[0];
			AssertNotNull(newApplication);
			AssertEquals("Should have foreign key to the campaign", jobCampaign.PK, newApplication.HP_HV);
		}

		public void TestCreateJobApplicationShouldNotValidate()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			Factory.Save();
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "badMobile", country_code = "NZ" };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Application creation should have succeeded", "Successfully created application.", result);
			var applicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)"shire.baggins@example.com");
			AssertNotNull(applicant);
			AssertEquals("Precondition", "shire.baggins@example.com", applicant.HA_EmailAddress);
			AssertEquals("Should have set full name", "shrodo dragons", applicant.HA_FullName);
			AssertEquals("Should have set mobile phone", "badMobile", applicant.HA_MobilePhone);
			AssertEquals("Should have set country", "NZ", applicant.HA_RN_NKCountry);
			AssertEquals("New applicant should have a new application", 1, applicant.Applications.Count);
			var newApplication = applicant.Applications[0];
			AssertNotNull(newApplication);
			AssertEquals("Should have foreign key to the campaign", jobCampaign.PK, newApplication.HP_HV);
		}

		public void TestCreateJobApplication_ReferringSource_Web()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			Factory.Save();

			var requestData = new JobApplicationCreationRequestData
			{
				campaign_pk = jobCampaign.PK.ToGuid(),
				email_address = "shire.baggins@example.com",
				full_name = "shrodo dragons",
				country_code = "NZ",
				referring_source = "WEB",
			};

			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);

			var applicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)"shire.baggins@example.com");
			AssertNotNull(applicant);

			var newApplication = applicant.Applications[0];

			AssertNotNull(newApplication);
			AssertEquals("Should set the source type", "WEB", newApplication.HP_SourceType);
		}

		public void TestCreateJobApplication_Address()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			Factory.Save();

			var requestData = new JobApplicationCreationRequestData
			{
				campaign_pk = jobCampaign.PK.ToGuid(),
				email_address = "shire.baggins@example.com",
				full_name = "shrodo dragons",
				country_code = "AU",
				state_code = "NSW",
				city = "Mount Druitt",
				address1 = "123 Fake St",
				address2 = "Unit 4",
			};

			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);

			var applicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)"shire.baggins@example.com");
			CombineAssertions("Address fields should be set", () =>
			{
				AssertEquals(nameof(HRJobApplicant.HA_UserAddress1), "123 Fake St", applicant.HA_UserAddress1);
				AssertEquals(nameof(HRJobApplicant.HA_UserAddress2), "Unit 4", applicant.HA_UserAddress2);
				AssertEquals(nameof(HRJobApplicant.HA_City), "Mount Druitt", applicant.HA_City);
				AssertEquals(nameof(HRJobApplicant.HA_State), "NSW", applicant.HA_State);
				AssertEquals(nameof(HRJobApplicant.HA_RN_NKCountry), "AU", applicant.HA_RN_NKCountry);
			});
		}

		public void TestCreateJobApplication_PartialAddress()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			Factory.Save();

			var requestData = new JobApplicationCreationRequestData
			{
				campaign_pk = jobCampaign.PK.ToGuid(),
				email_address = "shire.baggins@example.com",
				full_name = "shrodo dragons",
				country_code = "AU",
				state_code = "NSW",
				city = "Mount Druitt",
			};

			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);

			var applicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)"shire.baggins@example.com");
			CombineAssertions("Address fields should be set", () =>
			{
				AssertEquals(nameof(HRJobApplicant.HA_UserAddress1), string.Empty, applicant.HA_UserAddress1);
				AssertEquals(nameof(HRJobApplicant.HA_UserAddress2), string.Empty, applicant.HA_UserAddress2);
				AssertEquals(nameof(HRJobApplicant.HA_City), "Mount Druitt", applicant.HA_City);
				AssertEquals(nameof(HRJobApplicant.HA_State), "NSW", applicant.HA_State);
				AssertEquals(nameof(HRJobApplicant.HA_RN_NKCountry), "AU", applicant.HA_RN_NKCountry);
			});
		}

		public void TestCreateJobApplication_DOB()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			Factory.Save();

			var requestData = new JobApplicationCreationRequestData
			{
				campaign_pk = jobCampaign.PK.ToGuid(),
				email_address = "shire.baggins@example.com",
				full_name = "shrodo dragons",
				country_code = "AU",
				date_of_birth = new DateTime(2000, 6, 1)
			};

			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);

			var applicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)"shire.baggins@example.com");
			AssertEquals(new DateTime(2000, 6, 1), applicant.HA_Birthdate);
		}

		public void TestCreateJobApplication_Nationality()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			Factory.Save();

			var requestData = new JobApplicationCreationRequestData
			{
				campaign_pk = jobCampaign.PK.ToGuid(),
				email_address = "shire.baggins@example.com",
				full_name = "shrodo dragons",
				country_code = "AU",
				nationality_iso_code = "AU",
			};

			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);

			var applicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)"shire.baggins@example.com");
			AssertEquals("AU", applicant.HA_RN_NKNationalityCodeISO);
		}

		public void TestCreateJobApplication_Gender()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			Factory.Save();

			var requestData = new JobApplicationCreationRequestData
			{
				campaign_pk = jobCampaign.PK.ToGuid(),
				email_address = "shire.baggins@example.com",
				full_name = "shrodo dragons",
				country_code = "AU",
				gender = "M",
			};

			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);

			var applicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)"shire.baggins@example.com");
			AssertEquals("M", applicant.HA_Gender);
		}

		public void TestCreateJobApplication_WorkPermitStatus()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			Factory.Save();

			var requestData = new JobApplicationCreationRequestData
			{
				campaign_pk = jobCampaign.PK.ToGuid(),
				email_address = "shire.baggins@example.com",
				full_name = "shrodo dragons",
				country_code = "AU",
				work_permit_status = "RES",
			};

			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);

			var applicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)"shire.baggins@example.com");
			AssertEquals("RES", applicant.HA_WorkPermitStatus);
		}

		public void TestCreateJobApplication_Availability()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			Factory.Save();

			var requestData = new JobApplicationCreationRequestData
			{
				campaign_pk = jobCampaign.PK.ToGuid(),
				email_address = "shire.baggins@example.com",
				full_name = "shrodo dragons",
				country_code = "AU",
				availability = "CAS",
			};

			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);

			var applicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)"shire.baggins@example.com");
			AssertEquals("CAS", applicant.HA_Availability);
		}

		public void TestCreateJobApplicationShouldCreateIfThereAreExistingApplicationsForOtherApplicants()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "dodo maggins";
			applicant.HA_MobilePhone = "+64 3-234 5678";
			applicant.HA_RN_NKCountry = "AU";
			var applicant2 = Factory.New<HRJobApplicant>();
			applicant2.HA_EmailAddress = "thejet@mariners.com";
			applicant2.HA_FullName = "frodo";
			var application = applicant2.Applications.AddNew();
			application.HP_HV = jobCampaign.PK;
			AssertEquals("Precondition", applicant2.PK, application.HP_HA);
			Factory.Save();
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-111 1111", country_code = "NZ" };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Application creation should have succeeded", "Successfully created application.", result);
			var reloadedApplicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)"shire.baggins@example.com");
			AssertNotNull(reloadedApplicant);
			AssertEquals("Precondition", "shire.baggins@example.com", reloadedApplicant.HA_EmailAddress);
			AssertEquals("Should not have overwritten full name", "dodo maggins", reloadedApplicant.HA_FullName);
			AssertEquals("Should not have overwritten mobile phone", "+64 3-234 5678", reloadedApplicant.HA_MobilePhone);
			AssertEquals("Should not have overwritten country", "AU", reloadedApplicant.HA_RN_NKCountry);
			AssertEquals("New applicant should have a new application", 1, reloadedApplicant.Applications.Count);
			var newApplication = reloadedApplicant.Applications[0];
			AssertNotNull(newApplication);
			AssertEquals("Should have foreign key to the campaign", jobCampaign.PK, newApplication.HP_HV);
		}

		public void TestCreateJobApplicationEmptyPropertiesShouldNotOverwriteExistingApplicant()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "dodo maggins";
			applicant.HA_MobilePhone = "+64 3-234 5678";
			applicant.HA_RN_NKCountry = "AU";
			Factory.Save();
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = string.Empty, country_code = string.Empty };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Application creation should have succeeded", "Successfully created application.", result);
			var reloadedApplicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)"shire.baggins@example.com");
			AssertNotNull(reloadedApplicant);
			AssertEquals("Precondition", "shire.baggins@example.com", reloadedApplicant.HA_EmailAddress);
			AssertEquals("Should not have overwritten full name", "dodo maggins", reloadedApplicant.HA_FullName);
			AssertEquals("Should not have overwritten mobile phone", "+64 3-234 5678", reloadedApplicant.HA_MobilePhone);
			AssertEquals("Should not have overwritten country", "AU", reloadedApplicant.HA_RN_NKCountry);
			AssertEquals("New applicant should have a new application", 1, reloadedApplicant.Applications.Count);
			var newApplication = reloadedApplicant.Applications[0];
			AssertNotNull(newApplication);
			AssertEquals("Should have foreign key to the campaign", jobCampaign.PK, newApplication.HP_HV);
		}

		#region Bad Request
		public void TestCreateJobApplicationNullArguments()
		{
			var requestData = new JobApplicationCreationRequestData();
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Should return null arguments messages", FormattableString.Invariant($"The {nameof(JobApplicationCreationRequestData.campaign_pk)} must be entered.\r\nPlease enter an {nameof(JobApplicationCreationRequestData.email_address)}."), result);
		}

		public void TestCreateJobApplicationShouldNotCreateIfThereIsAnExistingApplication()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "dodo maggins";
			applicant.HA_MobilePhone = "+64 3-111 1111";
			applicant.HA_RN_NKCountry = "AU";
			var application = applicant.Applications.AddNew();
			application.HP_HV = jobCampaign.PK;
			AssertEquals("Precondition", applicant.PK, application.HP_HA);
			Factory.Save();
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "NZ" };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals("Precondition", HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("Precondition", "application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Cannot create application for email where one already exists for the campaign", "A Job Applicant with the email shire.baggins@example.com has already applied for this Job Campaign.", result);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestCreateJobApplicationInvalidCampaign()
		{
			SetupDefaultDocumentsAndCampaign();
			var requestData = new JobApplicationCreationRequestData { campaign_pk = Guid.NewGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "NZ" };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Should return invalid campaign", FormattableString.Invariant($"The {nameof(JobApplicationCreationRequestData.campaign_pk)} in the request does not correspond to any existing Job Campaigns."), result);
			AssertEquals("No applicants should have been created", false, Factory.ExistsInDatabase(HRJobApplicantSchema.Constants.TableName, new ZQuery(HRJobApplicantSchema.HA_EmailAddress, (ZString)"shire.baggins@example.com")));
		}

		public void TestCreateJobApplicationMaxLengthFieldsValidation()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			Factory.Save();
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = CreateStringOfSpecifiedLength(GlbPersonSchema.PER_FullName.MaxLength + 1), mobile_phone = CreateStringOfSpecifiedLength(GlbPersonSchema.PER_MobilePhone.MaxLength + 1), country_code = CreateStringOfSpecifiedLength(GlbPersonSchema.PER_RN_NKCountry.MaxLength + 1) };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Should return exceeded max length message", FormattableString.Invariant($"full_name exceeded its maximum length of {GlbPersonSchema.PER_FullName.MaxLength}.\r\nmobile_phone exceeded its maximum length of {GlbPersonSchema.PER_MobilePhone.MaxLength}.\r\ncountry_code exceeded its maximum length of {GlbPersonSchema.PER_RN_NKCountry.MaxLength}."), result);
		}

		#endregion
		#region Documents
		#region Bad Request
		public void TestCreateJobApplicationMandatoryDocumentValidation()
		{
			var appDocTypes = new OnlineApplicationDocTypeCollection();
			RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes);
			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType.RT_DocType = "RSU";
			refDocType.RT_Desc = "Resume/CV";
			var refDocType2 = Factory.NewWithValidTestData<RefDocType>();
			refDocType2.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType2.RT_DocType = "CVR";
			refDocType2.RT_Desc = "Cover Letter";
			var refDocType3 = Factory.NewWithValidTestData<RefDocType>();
			refDocType3.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType3.RT_DocType = "ACA";
			refDocType3.RT_Desc = "Academic Transcript";
			var hrDocType = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
			hrDocType.RT_PK = refDocType.PK;
			hrDocType.IsCompulsory = true;
			var hrDocType2 = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
			hrDocType2.RT_PK = refDocType2.PK;
			hrDocType2.IsCompulsory = true;
			var hrDocType3 = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
			hrDocType3.RT_PK = refDocType3.PK;
			hrDocType3.IsCompulsory = true;
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "frodo";
			Factory.Save();
			AssertEquals("Precondition", 0, applicant.DocManagerInfo.Files.Count);
			var bytesResume = ResumeForTest;
			var hashKey = ComputeHashKey(bytesResume);
			var documentData = new JobApplicationCreationDocument { data_type = "docx", document_content = Convert.ToBase64String(bytesResume), document_type = "CVR", file_name = "Don Antonio Cover Letter", document_content_sha256_hash = hashKey };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData } };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Missing mandatory documents", "The document type RSU - Resume/CV is mandatory. A document of this type must be attached.\r\nThe document type ACA - Academic Transcript is mandatory. A document of this type must be attached.", result);
			AssertEquals(0, applicant.DocManagerInfo.Files.Count);
		}

		public void TestCreateJobApplicationShouldNotAddDocumentsWhichAreNotAllowedInRegistry()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "frodo";
			Factory.Save();
			AssertEquals("Precondition", 0, applicant.DocManagerInfo.Files.Count);
			var bytesResume = ResumeForTest;
			var stringResume = Convert.ToBase64String(bytesResume);
			var hashKey = ComputeHashKey(bytesResume);
			var documentData1 = new JobApplicationCreationDocument { data_type = "docx", document_content = stringResume, document_type = "RSU", file_name = "Don Antonio Resume", document_content_sha256_hash = hashKey };
			var documentData2 = new JobApplicationCreationDocument { data_type = "docx", document_content = stringResume, document_type = "CVR", file_name = "Don Antonio Cover Letter", document_content_sha256_hash = hashKey };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData1, documentData2 } };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Invalid document types", "RSU is not a permitted Online Application Document Type.\r\nCVR is not a permitted Online Application Document Type.", result);
			AssertEquals(0, applicant.DocManagerInfo.Files.Count);
		}

		public void TestCreateJobApplicationMaxLengthDocumentFieldsValidation()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "frodo";
			Factory.Save();
			AssertEquals("Precondition", 0, applicant.DocManagerInfo.Files.Count);
			var bytesResume = ResumeForTest;
			var stringResume = Convert.ToBase64String(bytesResume);
			var tooBigDataType = CreateStringOfSpecifiedLength(StorageDocsSchema.SC_DataType.MaxLength + 1);
			var tooBigDocType = CreateStringOfSpecifiedLength(StorageDocsSchema.SC_DocType.MaxLength + 1);
			var tooBigFileName = CreateStringOfSpecifiedLength(StorageDocsSchema.SC_FileName.MaxLength + 1);
			var hashKey = ComputeHashKey(bytesResume);
			var documentData1 = new JobApplicationCreationDocument { data_type = tooBigDataType, document_content = stringResume, document_type = tooBigDocType, file_name = tooBigFileName, document_content_sha256_hash = hashKey };
			var documentData2 = new JobApplicationCreationDocument { data_type = tooBigDataType, document_content = stringResume, document_type = tooBigDocType, file_name = tooBigFileName, document_content_sha256_hash = hashKey };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData1, documentData2 } };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Invalid document types", FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.file_name)}[0] exceeded its maximum length of {StorageDocsSchema.SC_FileName.MaxLength}.\r\n") + FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.data_type)}[0] exceeded its maximum length of {StorageDocsSchema.SC_DataType.MaxLength}.\r\n") + FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.file_name)}[1] exceeded its maximum length of {StorageDocsSchema.SC_FileName.MaxLength}.\r\n") + FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.data_type)}[1] exceeded its maximum length of {StorageDocsSchema.SC_DataType.MaxLength}.\r\n") + FormattableString.Invariant($"{tooBigDocType} is not a permitted Online Application Document Type.\r\n") + FormattableString.Invariant($"{tooBigDocType} is not a permitted Online Application Document Type."), result);
			AssertEquals(0, applicant.DocManagerInfo.Files.Count);
		}

		public void TestCreateJobApplicationMaxDocumentSizeValidation()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "frodo";
			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType.RT_DocType = "RSU";
			var refDocType2 = Factory.NewWithValidTestData<RefDocType>();
			refDocType2.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType2.RT_DocType = "CVR";
			var hrDocType = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
			hrDocType.RT_PK = refDocType.PK;
			var hrDocType2 = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
			hrDocType2.RT_PK = refDocType2.PK;
			Factory.Save();
			AssertEquals("Precondition", 0, applicant.DocManagerInfo.Files.Count);
			SystemDataRegistry.Instance.eDocsMaximumFilesize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var bytesResume = resourceRetriever.GetBytes(@"DocumentScanning.Business.2MB.dat");
			var stringResume = Convert.ToBase64String(bytesResume);
			var hashKey = ComputeHashKey(bytesResume);
			var documentData1 = new JobApplicationCreationDocument { data_type = "dat", document_content = stringResume, document_type = "RSU", file_name = "Don Antonio Resume", document_content_sha256_hash = hashKey };
			var documentData2 = new JobApplicationCreationDocument { data_type = "dat", document_content = stringResume, document_type = "CVR", file_name = "Don Antonio Cover Letter", document_content_sha256_hash = hashKey };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData1, documentData2 } };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Exceeded max document content length", $"{nameof(JobApplicationCreationDocument.document_content)}[0] was too large. Please ensure documents are smaller than 1MB.\r\n{nameof(JobApplicationCreationDocument.document_content)}[1] was too large. Please ensure documents are smaller than 1MB.", result);
			AssertEquals(0, applicant.DocManagerInfo.Files.Count);
		}

		public void TestCreateJobApplicationInvalidFileName()
		{
			var appDocTypes = new OnlineApplicationDocTypeCollection();
			RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes);
			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType.RT_DocType = "RSU";
			var hrDocType = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
			hrDocType.RT_PK = refDocType.PK;
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "frodo";
			Factory.Save();
			var bytesResume = ResumeForTest;
			var hashKey = ComputeHashKey(bytesResume);
			var documentData = new JobApplicationCreationDocument { data_type = "docx", document_content = Convert.ToBase64String(bytesResume), document_type = "RSU", file_name = string.Empty, document_content_sha256_hash = hashKey };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData } };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Invalid File Name", FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.file_name)}[0] is empty. It must be entered."), result);
		}

		public void TestCreateJobApplicationDocumentShouldNotHaveEmptyBytes()
		{
			var appDocTypes = new OnlineApplicationDocTypeCollection();
			RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes);
			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType.RT_DocType = "RSU";
			var hrDocType = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
			hrDocType.RT_PK = refDocType.PK;
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "frodo";
			Factory.Save();
			var emptyBytes = Array.Empty<byte>();
			var emptyBytesString = Convert.ToBase64String(emptyBytes);
			AssertEquals("Precondition", string.Empty, emptyBytesString);
			var hashKey = ComputeHashKey(emptyBytes);
			var documentData = new JobApplicationCreationDocument { data_type = "docx", document_content = emptyBytesString, document_type = "RSU", file_name = "Don Antonio resume", document_content_sha256_hash = hashKey };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData } };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Should not have empty bytes", FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.document_content)}[0] is empty. It must be entered."), result);
		}

		public void TestCreateJobApplicationCompulsoryDocumentShouldNotBeNull()
		{
			var appDocTypes = new OnlineApplicationDocTypeCollection();
			RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes);
			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType.RT_DocType = "RSU";
			refDocType.RT_Desc = "Resume/CV";
			var refDocType2 = Factory.NewWithValidTestData<RefDocType>();
			refDocType2.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType2.RT_DocType = "CVR";
			var hrDocType = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
			hrDocType.RT_PK = refDocType.PK;
			hrDocType.IsCompulsory = true;
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "frodo";
			Factory.Save();
			AssertEquals("Precondition", 0, applicant.DocManagerInfo.Files.Count);
			var parsingQueueLength = Factory.Load<HRJobApplicationParsingQueue>(new ZQuery()).Length;
			AssertEquals("Precondition", 0, parsingQueueLength);
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU" };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Mismatch in number of document arguments", "The document type RSU - Resume/CV is mandatory. A document of this type must be attached.", result);
		}

		public void TestCreateJobApplicationMultipleDocumentArrayShouldHaveAllDocumentFieldArraysSameLength()
		{
			var appDocTypes = new OnlineApplicationDocTypeCollection();
			RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes);
			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType.RT_DocType = "RSU";
			var refDocType2 = Factory.NewWithValidTestData<RefDocType>();
			refDocType2.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType2.RT_DocType = "CVR";
			var hrDocType = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
			hrDocType.RT_PK = refDocType.PK;
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "frodo";
			Factory.Save();
			AssertEquals("Precondition", 0, applicant.DocManagerInfo.Files.Count);
			var parsingQueueLength = Factory.Load<HRJobApplicationParsingQueue>(new ZQuery()).Length;
			AssertEquals("Precondition", 0, parsingQueueLength);
			var bytesResume = ResumeForTest;
			var stringResume = Convert.ToBase64String(bytesResume);
			var documentData1 = new JobApplicationCreationDocument { data_type = "docx", document_content = stringResume, file_name = "Don Antonio Resume", };
			var documentData2 = new JobApplicationCreationDocument { data_type = "docx", document_content = stringResume, document_type = "CVR", file_name = "Don Antonio Cover Letter", };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData1, documentData2 } };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Missing arguments", "document_content_sha256_hash[0] is empty. It must be entered.\r\ndocument_content_sha256_hash[1] is empty. It must be entered.\r\n is not a permitted Online Application Document Type.\r\nCVR is not a permitted Online Application Document Type.", result);
		}

		public void TestCreateJobApplicationDocumentCheckSumValidation()
		{
			var appDocTypes = new OnlineApplicationDocTypeCollection();
			RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes);
			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType.RT_DocType = "RSU";
			var hrDocType = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
			hrDocType.RT_PK = refDocType.PK;
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "frodo";
			Factory.Save();
			var bytesResume = ResumeForTest;
			var documentData = new JobApplicationCreationDocument { data_type = "docx", document_content = Convert.ToBase64String(bytesResume), document_type = "RSU", file_name = "Don Antonio Resume", document_content_sha256_hash = string.Empty };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData } };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Checksum incorrect", FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.document_content_sha256_hash)}[0] is empty. It must be entered."), result);
			AssertEquals(0, applicant.DocManagerInfo.Files.Count);
			documentData.document_content_sha256_hash = "randomString";
			requestData.documents = new[] { documentData };
			response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Checksum incorrect", FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.document_content)}[0]'s SHA-256 Hash is invalid."), result);
			AssertEquals(0, applicant.DocManagerInfo.Files.Count);
		}

		#endregion
		public void TestCreateJobApplicationDocument()
		{
			var appDocTypes = new OnlineApplicationDocTypeCollection();
			RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes);
			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType.RT_DocType = "RSU";
			var hrDocType = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
			hrDocType.RT_PK = refDocType.PK;
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "frodo";
			Factory.Save();
			AssertEquals("Precondition", 0, applicant.DocManagerInfo.Files.Count);
			var parsingQueueLength = Factory.Load<HRJobApplicationParsingQueue>(new ZQuery()).Length;
			AssertEquals("Precondition", 0, parsingQueueLength);
			var bytesResume = ResumeForTest;
			var hashKey = ComputeHashKey(bytesResume);
			var documentData = new JobApplicationCreationDocument { data_type = "docx", document_content = Convert.ToBase64String(bytesResume), document_type = "RSU", file_name = "Don Antonio resume", document_content_sha256_hash = hashKey };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData } };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("File should have successfully been parsed", "Successfully created application.", result);
			var resumeAsZBlob = new ZBlob(bytesResume);
			var reloadedApplicant = new BusinessObjectFactory().Load<HRJobApplicant>(applicant.PK);
			AssertEquals("File should have been added to applicant", 1, reloadedApplicant.DocManagerInfo.Files.Count);
			var applicantFile = reloadedApplicant.DocManagerInfo.Files[0];
			AssertEquals("DocType should have been parsed from request", "RSU", applicantFile.DocType);
			AssertEquals("File name should be parsed from request", "Don Antonio resume.docx", applicantFile.FileName);
			AssertEquals("Resume contents should stay unchanged", resumeAsZBlob, applicantFile.ImageData);
			var application = Factory.LoadTop1<HRJobApplication>(new ZQuery(HRJobApplicationSchema.HP_HA, applicant.PK));
			AssertEquals("File should have been added to application", 1, application.DocManagerInfo.Files.Count);
			var applicationFile = application.DocManagerInfo.Files[0];
			AssertEquals("DocType should have been parsed from request", "RSU", applicationFile.DocType);
			AssertEquals("File name should be parsed from request", "Don Antonio resume.docx", applicationFile.FileName);
			AssertEquals("Resume contents should stay unchanged", resumeAsZBlob, applicationFile.ImageData);
			var parsingQueue = Factory.Load<HRJobApplicationParsingQueue>(new ZQuery(HRJobApplicationParsingQueueSchema.HPQ_HP, application.PK));
			AssertEquals("Should have added the application to the parsing queue", 1, parsingQueue.Length);
			AssertEquals("Storage Doc Reference should match eDoc.", application.DocManagerInfo.AllEDocs[0].UniqueKey, parsingQueue[0].HPQ_StorageDocReference);
		}

		public void TestCreateJobApplicationMultipleDocuments()
		{
			var appDocTypes = new OnlineApplicationDocTypeCollection();
			RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes);
			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType.RT_DocType = "RSU";
			refDocType.RT_Desc = "Resume";
			var refDocType2 = Factory.NewWithValidTestData<RefDocType>();
			refDocType2.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType2.RT_DocType = "CVR";
			refDocType2.RT_Desc = "Cover Letter";
			var hrDocType = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
			hrDocType.RT_PK = refDocType.PK;
			var hrDocType2 = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
			hrDocType2.RT_PK = refDocType2.PK;
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "frodo";
			Factory.Save();
			AssertEquals("Precondition", 0, applicant.DocManagerInfo.Files.Count);
			var parsingQueueLength = Factory.Load<HRJobApplicationParsingQueue>(new ZQuery()).Length;
			AssertEquals("Precondition", 0, parsingQueueLength);
			var bytesResume = ResumeForTest;
			var stringResume = Convert.ToBase64String(bytesResume);
			var hashKey = ComputeHashKey(bytesResume);
			var documentData1 = new JobApplicationCreationDocument { data_type = "docx", document_content = stringResume, document_type = "RSU", file_name = "Don Antonio Resume", document_content_sha256_hash = hashKey };
			var documentData2 = new JobApplicationCreationDocument { data_type = "docx", document_content = stringResume, document_type = "CVR", file_name = "Don Antonio Cover Letter", document_content_sha256_hash = hashKey };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData1, documentData2 } };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Application creation should have succeeded", "Successfully created application.", result);
			var application = Factory.LoadTop1<HRJobApplication>(new ZQuery(HRJobApplicationSchema.HP_HA, applicant.PK));
			AssertNotNull(application);
			var reloadedApplicant = new BusinessObjectFactory().Load<HRJobApplicant>(applicant.PK);
			AssertEquals("File should have been added to applicant", 2, reloadedApplicant.DocManagerInfo.AllEDocs.Count);
			var resumeAsZBlob = new ZBlob(bytesResume);
			var files = new[] { reloadedApplicant.DocManagerInfo.AllEDocs[0], reloadedApplicant.DocManagerInfo.AllEDocs[1] };
			var resumeFile = files.FirstOrDefault(x => x.FileName == "Don Antonio Resume.docx" && x.DocType == "RSU");
			var coverLetterFile = files.FirstOrDefault(x => x.FileName == "Don Antonio Cover Letter.docx" && x.DocType == "CVR");
			AssertNotNull(resumeFile);
			AssertEquals("Resume contents should stay unchanged", resumeAsZBlob, resumeFile.ImageData);
			AssertNotNull(coverLetterFile);
			AssertEquals("Resume contents should stay unchanged", resumeAsZBlob, coverLetterFile.ImageData);
			AssertEquals("File should have been added to application", 2, application.DocManagerInfo.AllEDocs.Count);
			files = new[] { application.DocManagerInfo.AllEDocs[0], application.DocManagerInfo.AllEDocs[1] };
			resumeFile = files.FirstOrDefault(x => x.FileName == "Don Antonio Resume.docx" && x.DocType == "RSU");
			coverLetterFile = files.FirstOrDefault(x => x.FileName == "Don Antonio Cover Letter.docx" && x.DocType == "CVR");
			AssertNotNull(resumeFile);
			AssertEquals("Resume contents should stay unchanged", resumeAsZBlob, resumeFile.ImageData);
			AssertNotNull(coverLetterFile);
			AssertEquals("Resume contents should stay unchanged", resumeAsZBlob, coverLetterFile.ImageData);
			var parsingQueue = Factory.Load<HRJobApplicationParsingQueue>(new ZQuery(HRJobApplicationParsingQueueSchema.HPQ_HP, application.PK));
			AssertEquals("Should have added the application to the parsing queue", 2, parsingQueue.Length);
			var actualKeys = parsingQueue.Select(item => item.HPQ_StorageDocReference).ToArray();
			var expectedKeys = new[] { application.DocManagerInfo.AllEDocs[0].UniqueKey, application.DocManagerInfo.AllEDocs[1].UniqueKey };
			AssertContainsExactElementsInAnyOrder(expectedKeys, actualKeys);
		}

		public void TestCreateJobApplicationDuplicateDocuments()
		{
			var appDocTypes = new OnlineApplicationDocTypeCollection();
			RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes);
			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			refDocType.RT_DocType = "RSU";
			refDocType.RT_Desc = "Resume";
			var hrDocType = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
			hrDocType.RT_PK = refDocType.PK;
			var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@example.com";
			applicant.HA_FullName = "frodo";
			Factory.Save();
			AssertEquals("Precondition", 0, applicant.DocManagerInfo.Files.Count);
			var parsingQueueLength = Factory.Load<HRJobApplicationParsingQueue>(new ZQuery()).Length;
			AssertEquals("Precondition", 0, parsingQueueLength);
			var bytesResume = ResumeForTest;
			var stringResume = Convert.ToBase64String(bytesResume);
			var hashKey = ComputeHashKey(bytesResume);
			var documentData = new JobApplicationCreationDocument { data_type = "docx", document_content = stringResume, document_type = "RSU", file_name = "Don Antonio Resume", document_content_sha256_hash = hashKey };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData, documentData } };
			var response = controller.CreateJobApplication(requestData);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
			var result = response.Content.ReadAsAsync<string>().Result;
			AssertEquals("Application creation should have succeeded", "Successfully created application.", result);
			var application = Factory.LoadTop1<HRJobApplication>(new ZQuery(HRJobApplicationSchema.HP_HA, applicant.PK));
			AssertNotNull(application);
			var reloadedApplicant = new BusinessObjectFactory().Load<HRJobApplicant>(applicant.PK);
			AssertEquals("File should have been added to applicant", 1, reloadedApplicant.DocManagerInfo.AllEDocs.Count);
		}

		internal static string ComputeHashKey(byte[] bytesResume)
		{
			using (var sha256Hash = SHA256.Create())
			{
				return Convert.ToBase64String(sha256Hash.ComputeHash(bytesResume));
			}
		}

		#endregion
		#region Implementation
		internal static byte[] ResumeForTest
		{
			get
			{
				if (resumeForTest == null)
				{
					var resourceRetriever = new EmbeddedResourceRetriever(typeof(JobApplicationControllerTestCase).Assembly);
					resumeForTest = resourceRetriever.GetBytes(@"ZClientWebEDI.Test.TestFiles.Don Antonio resume.docx");
				}
				return resumeForTest;
			}
		}
		static byte[] resumeForTest;

		string CreateStringOfSpecifiedLength(int length)
		{
			return string.Empty.PadRight(length, 'a');
		}

		internal static HRRecruitmentJobCampaign SetupDefaultDocumentsAndCampaign(BusinessObjectFactory factory)
		{
			var appDocTypes = new OnlineApplicationDocTypeCollection();
			RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes);
			return factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
		}

		HRRecruitmentJobCampaign SetupDefaultDocumentsAndCampaign()
		{
			return SetupDefaultDocumentsAndCampaign(Factory);
		}

		JobApplicationController controller;
		protected override void SetUp()
		{
			base.SetUp();
			controller = GetJobApplicationController("10.61.165.176");
		}

		internal static JobApplicationController GetJobApplicationController(string ip)
		{
			var mockRequest = new Mock<HttpWorkerRequest>();
			mockRequest.Setup(o => o.GetRemoteAddress()).Returns(ip);
			mockRequest.Setup(o => o.GetRawUrl()).Returns("/api/JobApplication/Mocked");
			HttpContext.Current = new HttpContext(mockRequest.Object);
			var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/myaccount/api/JobApplication/CreateJobApplication");
			request.Properties.Add("MS_HttpConfiguration", new HttpConfiguration());
			return new JobApplicationController { Request = request };
		}

		public class JobApplicationControllerForExceptionTest : JobApplicationController
		{
			protected override HRJobApplicant CreateOrUpdateApplicant(HRJobApplicant applicant, JobApplicationCreationRequestData applicationCreationData)
			{
				throw new Exception("Error which should be caught");
			}

			protected override IEnumerable<ZGuid> ParseDocuments(HRJobApplicant applicant, HRJobApplication application, JobApplicationCreationRequestData applicationCreationData)
			{
				foreach (var document in applicationCreationData.documents)
				{
					var byteContents = Convert.FromBase64String(document.document_content);
					var fileName = document.file_name;
					var dataType = document.data_type.ToUpperInvariant();
					var documentType = document.document_type;
					applicant.DocManagerInfo.AddFileOrDocument(byteContents, FormattableString.Invariant($"{fileName}.{dataType}"), documentType, true);
					application.DocManagerInfo.AddFileOrDocument(byteContents, FormattableString.Invariant($"{fileName}.{dataType}"), documentType, true);
				}

				throw new Exception("Error which should be caught");
			}
		}
		#endregion
	}

	class JobApplicationControllerExceptionTestCase : TestCaseWithFactory
	{
		public void TestCreatingApplicantShouldReturnBadRequest()
		{
			// arrange
			var appDocTypes = new OnlineApplicationDocTypeCollection();

			using (RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes))
			{
				var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();

				var applicant = Factory.New<HRJobApplicant>();
				applicant.HA_EmailAddress = "shire.baggins@example.com";
				applicant.HA_FullName = "frodo";
				Factory.Save();

				var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU" };
				var exceptionController = GetJobApplicationControllerForExceptionTest("10.61.165.176");
				ErrorReporter.Clear();

				// act
				var response = exceptionController.CreateJobApplication(requestData);

				// assert
				AssertEquals(HttpStatusCode.InternalServerError, response.StatusCode);
				AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);

				var result = response.Content.ReadAsAsync<string>().Result;
				AssertEquals("Exception should have been caught and reported in response", "Error which should be caught", result);
				AssertEquals(typeof(Exception), ErrorReporter.LastExceptionReported.GetType());
				AssertEquals("JobApplicationController.CreateJobApplication exception", ErrorReporter.LastKeyReported);
				AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();

				var newEDocs = applicant.DocManagerInfo.AllEDocs.Cast<IeDoc>().Where(d => !d.ParentMain.IsInDatabase).ToArray();
				AssertEquals(newEDocs.Length, 0);

				AssertEquals(Factory.Load<HRJobApplication>(new ZQuery()).Length, 0);
			}
		}

		public void TestCreateJobApplicationExistingApplicantExceptionWhenParsingDocumentShouldReturnBadRequest()
		{
			// arrange
			var appDocTypes = new OnlineApplicationDocTypeCollection();

			using (RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes))
			{
				var refDocType = Factory.NewWithValidTestData<RefDocType>();
				refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
				refDocType.RT_DocType = "RSU";

				var hrDocType = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
				hrDocType.RT_PK = refDocType.PK;

				var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
				var applicant = Factory.New<HRJobApplicant>();
				applicant.HA_EmailAddress = "shire.baggins@example.com";
				applicant.HA_FullName = "Frodo Maggins";
				applicant.HA_MobilePhone = "+64 3-234 5679";
				applicant.HA_RN_NKCountry = "NZ";
				Factory.Save();

				var bytesResume = ResumeForTest;
				var stringResume = Convert.ToBase64String(bytesResume);
				var hashKey = ComputeHashKey(bytesResume);
				var documentData = new JobApplicationCreationDocument { data_type = "docx", document_content = stringResume, document_type = "RSU", file_name = "Don Antonio Resume", document_content_sha256_hash = hashKey };
				var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData } };
				var exceptionController = GetJobApplicationControllerForExceptionTest("10.61.165.176");
				ErrorReporter.Clear();

				// act
				var response = exceptionController.CreateJobApplication(requestData);

				// assert
				AssertEquals(HttpStatusCode.InternalServerError, response.StatusCode);
				AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);

				var result = response.Content.ReadAsAsync<string>().Result;
				AssertEquals("Exception should have been caught and reported in response", "Error which should be caught", result);
				AssertEquals(typeof(Exception), ErrorReporter.LastExceptionReported.GetType());
				AssertEquals("JobApplicationController.CreateJobApplication exception", ErrorReporter.LastKeyReported);
				AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();

				var reloadedApplicant = new BusinessObjectFactory().Load<HRJobApplicant>(applicant.PK);
				AssertEquals("Should not have been overwritten", "Frodo Maggins", reloadedApplicant.HA_FullName);
				AssertEquals("Should not have been overwritten", "+64 3-234 5679", reloadedApplicant.HA_MobilePhone);
				AssertEquals("Should not have been overwritten", "NZ", reloadedApplicant.HA_RN_NKCountry);
				AssertEquals("Should not have any eDocs", 0, reloadedApplicant.DocManagerInfo.AllEDocs.Count);

				var newEDocs = applicant.DocManagerInfo.AllEDocs.Cast<IeDoc>().Where(d => !d.ParentMain.IsInDatabase).ToArray();
				AssertEquals(newEDocs.Length, 0);
				var newEDocs2 = reloadedApplicant.DocManagerInfo.AllEDocs.Cast<IeDoc>().Where(d => !d.ParentMain.IsInDatabase).ToArray();
				AssertEquals(newEDocs2.Length, 0);

				AssertEquals(Factory.Load<HRJobApplication>(new ZQuery()).Length, 0);
			}
		}

		public void TestCreateJobApplicationNewApplicantExceptionWhenParsingDocumentShouldReturnBadRequest()
		{
			// arrange
			var appDocTypes = new OnlineApplicationDocTypeCollection();

			using (RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes))
			{
				var refDocType = Factory.NewWithValidTestData<RefDocType>();
				refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
				refDocType.RT_DocType = "RSU";

				var hrDocType = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
				hrDocType.RT_PK = refDocType.PK;

				var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
				Factory.Save();

				var bytesResume = ResumeForTest;
				var stringResume = Convert.ToBase64String(bytesResume);
				var hashKey = ComputeHashKey(bytesResume);
				var documentData = new JobApplicationCreationDocument { data_type = "docx", document_content = stringResume, document_type = "RSU", file_name = "Don Antonio Resume", document_content_sha256_hash = hashKey };
				var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData } };
				var exceptionController = GetJobApplicationControllerForExceptionTest("10.61.165.176");
				ErrorReporter.Clear();

				// act
				var response = exceptionController.CreateJobApplication(requestData);

				// assert
				AssertEquals(HttpStatusCode.InternalServerError, response.StatusCode);
				AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);

				var result = response.Content.ReadAsAsync<string>().Result;
				AssertEquals("Exception should have been caught and reported in response", "Error which should be caught", result);

				var reloadedApplicant = new BusinessObjectFactory().LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)"shire.baggins@example.com");
				AssertNull(reloadedApplicant);
				AssertEquals(typeof(Exception), ErrorReporter.LastExceptionReported.GetType());
				AssertEquals("JobApplicationController.CreateJobApplication exception", ErrorReporter.LastKeyReported);
				AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestCreateJobApplicationDocumentFails()
		{
			// arrange
			var appDocTypes = new OnlineApplicationDocTypeCollection();
			using (RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes))
			{
				var refDocType = Factory.NewWithValidTestData<RefDocType>();
				refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
				refDocType.RT_DocType = "RSU";

				var hrDocType = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.AddNew();
				hrDocType.RT_PK = refDocType.PK;

				var jobCampaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
				var applicant = Factory.New<HRJobApplicant>();
				applicant.HA_EmailAddress = "shire.baggins@example.com";
				applicant.HA_FullName = "frodo";
				Factory.Save();

				AssertEquals("Precondition", 0, applicant.DocManagerInfo.Files.Count);

				var parsingQueueLength = Factory.Load<HRJobApplicationParsingQueue>(new ZQuery()).Length;

				AssertEquals("Precondition", 0, parsingQueueLength);

				var bytesResume = ResumeForTest;
				var hashKey = ComputeHashKey(bytesResume);
				var documentData = new JobApplicationCreationDocument { data_type = "docx", document_content = "something not base64 so this throws an exception", document_type = "RSU", file_name = "Don Antonio resume", document_content_sha256_hash = hashKey };
				var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@example.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData } };

				var controller = GetJobApplicationController("10.61.165.176");

				// act
				var response = controller.CreateJobApplication(requestData);

				// assert
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertEquals("application/json", response.Content.Headers.ContentType.MediaType);
				AssertEquals(Factory.Load<HRJobApplication>(new ZQuery()).Length, 0);
			}

			ErrorReporter.Clear();
		}

		static JobApplicationControllerForExceptionTest GetJobApplicationControllerForExceptionTest(string ip)
		{
			var mockRequest = new Mock<HttpWorkerRequest>();
			_ = mockRequest.Setup(o => o.GetRemoteAddress()).Returns(ip);
			_ = mockRequest.Setup(o => o.GetRawUrl()).Returns("/api/JobApplication/Mocked");

			HttpContext.Current = new HttpContext(mockRequest.Object);

			var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/myaccount/api/JobApplication/CreateJobApplication");
			request.Properties.Add("MS_HttpConfiguration", new HttpConfiguration());

			return new JobApplicationControllerForExceptionTest { Request = request };
		}
	}
}

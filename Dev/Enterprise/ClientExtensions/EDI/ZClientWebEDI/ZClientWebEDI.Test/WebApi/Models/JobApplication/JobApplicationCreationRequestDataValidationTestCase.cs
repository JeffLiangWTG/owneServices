using System;
using System.Security.Cryptography;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.JobApplication.Test
{
	class JobApplicationCreationRequestDataValidationTestCase : TestCaseWithFactory
	{
		public void TestCreateJobApplicationNullArguments()
		{
			var requestData = new JobApplicationCreationRequestData();
			var validator = new JobApplicationCreationRequestDataValidation(Factory, requestData);
			validator.CheckMandatoryFieldsForNull();
			AssertEquals("Should return null arguments messages", FormattableString.Invariant($"The {nameof(JobApplicationCreationRequestData.campaign_pk)} must be entered.\r\nPlease enter an {nameof(JobApplicationCreationRequestData.email_address)}."), validator.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
		}

		public void TestCreateJobApplicationShouldNotCreateIfThereIsAnExistingApplication()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@hobbitses.com";
			applicant.HA_FullName = "dodo maggins";
			applicant.HA_MobilePhone = "+64 3-111 1111";
			applicant.HA_RN_NKCountry = "AU";
			var application = applicant.Applications.AddNew();
			application.HP_HV = jobCampaign.PK;
			AssertEquals("Precondition", applicant.PK, application.HP_HA);
			Factory.Save();
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@hobbitses.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "NZ" };
			var validator = new JobApplicationCreationRequestDataValidation(Factory, requestData);
			var loadedCampaign = Factory.Load<HRRecruitmentJobCampaign>(requestData.campaign_pk);
			var loadedApplicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)requestData.email_address);
			validator.ValidateAuthenticationData(loadedCampaign, loadedApplicant);
			AssertEquals("Cannot create application for email where one already exists for the campaign", "A Job Applicant with the email shire.baggins@hobbitses.com has already applied for this Job Campaign.", validator.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
		}

		public void TestCreateJobApplicationInvalidCampaign()
		{
			SetupDefaultDocumentsAndCampaign();
			var requestData = new JobApplicationCreationRequestData { campaign_pk = Guid.NewGuid(), email_address = "shire.baggins@hobbitses.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "NZ" };
			var validator = new JobApplicationCreationRequestDataValidation(Factory, requestData);
			var loadedCampaign = Factory.Load<HRRecruitmentJobCampaign>(requestData.campaign_pk);
			var loadedApplicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)requestData.email_address);
			validator.ValidateAuthenticationData(loadedCampaign, loadedApplicant);
			AssertEquals("Should return invalid campaign", FormattableString.Invariant($"The {nameof(JobApplicationCreationRequestData.campaign_pk)} in the request does not correspond to any existing Job Campaigns."), validator.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
		}

		public void TestCreateJobApplicationMaxLengthFieldsValidation()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			Factory.Save();
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@hobbitses.com", full_name = CreateStringOfSpecifiedLength(GlbPersonSchema.PER_FullName.MaxLength + 1), mobile_phone = CreateStringOfSpecifiedLength(GlbPersonSchema.PER_MobilePhone.MaxLength + 1), country_code = CreateStringOfSpecifiedLength(GlbPersonSchema.PER_RN_NKCountry.MaxLength + 1) };
			var validator = new JobApplicationCreationRequestDataValidation(Factory, requestData);
			var loadedCampaign = Factory.Load<HRRecruitmentJobCampaign>(requestData.campaign_pk);
			var loadedApplicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)requestData.email_address);
			validator.ValidateAuthenticationData(loadedCampaign, loadedApplicant);
			AssertEquals("Should return exceeded max length message", FormattableString.Invariant($"full_name exceeded its maximum length of {GlbPersonSchema.PER_FullName.MaxLength}.\r\nmobile_phone exceeded its maximum length of {GlbPersonSchema.PER_MobilePhone.MaxLength}.\r\ncountry_code exceeded its maximum length of {GlbPersonSchema.PER_RN_NKCountry.MaxLength}."), validator.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
		}

		#region Documents
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
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@hobbitses.com";
			Factory.Save();
			AssertEquals("Precondition", 0, applicant.DocManagerInfo.Files.Count);
			var bytesResume = ResumeForTest;
			var sha256Hash = SHA256.Create();
			var documentData = new JobApplicationCreationDocument { data_type = "docx", document_content = Convert.ToBase64String(bytesResume), document_type = "CVR", file_name = "Don Antonio Cover Letter", document_content_sha256_hash = Convert.ToBase64String(sha256Hash.ComputeHash(bytesResume)) };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@hobbitses.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData } };
			var validator = new JobApplicationCreationRequestDataValidation(Factory, requestData);
			var loadedCampaign = Factory.Load<HRRecruitmentJobCampaign>(requestData.campaign_pk);
			var loadedApplicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)requestData.email_address);
			validator.ValidateAuthenticationData(loadedCampaign, loadedApplicant);
			AssertEquals("Missing mandatory documents", "The document type RSU - Resume/CV is mandatory. A document of this type must be attached.\r\nThe document type ACA - Academic Transcript is mandatory. A document of this type must be attached.", validator.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
		}

		public void TestCreateJobApplicationShouldNotAddDocumentsWhichAreNotAllowedInRegistry()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@hobbitses.com";
			Factory.Save();
			AssertEquals("Precondition", 0, applicant.DocManagerInfo.Files.Count);
			var bytesResume = ResumeForTest;
			var stringResume = Convert.ToBase64String(bytesResume);
			var sha256Hash = SHA256.Create();
			var hashKey = Convert.ToBase64String(sha256Hash.ComputeHash(bytesResume));
			var documentData1 = new JobApplicationCreationDocument { data_type = "docx", document_content = stringResume, document_type = "RSU", file_name = "Don Antonio Resume", document_content_sha256_hash = hashKey };
			var documentData2 = new JobApplicationCreationDocument { data_type = "docx", document_content = stringResume, document_type = "CVR", file_name = "Don Antonio Cover Letter", document_content_sha256_hash = hashKey };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@hobbitses.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData1, documentData2 } };
			var validator = new JobApplicationCreationRequestDataValidation(Factory, requestData);
			var loadedCampaign = Factory.Load<HRRecruitmentJobCampaign>(requestData.campaign_pk);
			var loadedApplicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)requestData.email_address);
			validator.ValidateAuthenticationData(loadedCampaign, loadedApplicant);
			AssertEquals("Invalid document types", "RSU is not a permitted Online Application Document Type.\r\nCVR is not a permitted Online Application Document Type.", validator.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
		}

		public void TestCreateJobApplicationMaxLengthDocumentFieldsValidation()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@hobbitses.com";
			Factory.Save();
			AssertEquals("Precondition", 0, applicant.DocManagerInfo.Files.Count);
			var bytesResume = ResumeForTest;
			var stringResume = Convert.ToBase64String(bytesResume);
			var tooBigDataType = CreateStringOfSpecifiedLength(StorageDocsSchema.SC_DataType.MaxLength + 1);
			var tooBigDocType = CreateStringOfSpecifiedLength(StorageDocsSchema.SC_DocType.MaxLength + 1);
			var tooBigFileName = CreateStringOfSpecifiedLength(StorageDocsSchema.SC_FileName.MaxLength + 1);
			var sha256Hash = SHA256.Create();
			var hashKey = Convert.ToBase64String(sha256Hash.ComputeHash(bytesResume));
			var documentData1 = new JobApplicationCreationDocument { data_type = tooBigDataType, document_content = stringResume, document_type = tooBigDocType, file_name = tooBigFileName, document_content_sha256_hash = hashKey };
			var documentData2 = new JobApplicationCreationDocument { data_type = tooBigDataType, document_content = stringResume, document_type = tooBigDocType, file_name = tooBigFileName, document_content_sha256_hash = hashKey };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@hobbitses.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData1, documentData2 } };
			var validator = new JobApplicationCreationRequestDataValidation(Factory, requestData);
			var loadedCampaign = Factory.Load<HRRecruitmentJobCampaign>(requestData.campaign_pk);
			var loadedApplicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)requestData.email_address);
			validator.ValidateAuthenticationData(loadedCampaign, loadedApplicant);
			AssertEquals("Invalid document types", FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.file_name)}[0] exceeded its maximum length of {StorageDocsSchema.SC_FileName.MaxLength}.\r\n") + FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.data_type)}[0] exceeded its maximum length of {StorageDocsSchema.SC_DataType.MaxLength}.\r\n") + FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.file_name)}[1] exceeded its maximum length of {StorageDocsSchema.SC_FileName.MaxLength}.\r\n") + FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.data_type)}[1] exceeded its maximum length of {StorageDocsSchema.SC_DataType.MaxLength}.\r\n") + FormattableString.Invariant($"{tooBigDocType} is not a permitted Online Application Document Type.\r\n") + FormattableString.Invariant($"{tooBigDocType} is not a permitted Online Application Document Type."), validator.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
		}

		public void TestCreateJobApplicationMaxDocumentSizeValidation()
		{
			var jobCampaign = SetupDefaultDocumentsAndCampaign();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@hobbitses.com";
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
			var sha256Hash = SHA256.Create();
			var hashKey = Convert.ToBase64String(sha256Hash.ComputeHash(bytesResume));
			var documentData1 = new JobApplicationCreationDocument { data_type = "dat", document_content = stringResume, document_type = "RSU", file_name = "Don Antonio Resume", document_content_sha256_hash = hashKey };
			var documentData2 = new JobApplicationCreationDocument { data_type = "dat", document_content = stringResume, document_type = "CVR", file_name = "Don Antonio Cover Letter", document_content_sha256_hash = hashKey };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@hobbitses.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData1, documentData2 } };
			var validator = new JobApplicationCreationRequestDataValidation(Factory, requestData);
			var loadedCampaign = Factory.Load<HRRecruitmentJobCampaign>(requestData.campaign_pk);
			var loadedApplicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)requestData.email_address);
			validator.ValidateAuthenticationData(loadedCampaign, loadedApplicant);
			AssertEquals("Exceeded max document content length", $"{nameof(JobApplicationCreationDocument.document_content)}[0] was too large. Please ensure documents are smaller than 1MB.\r\n{nameof(JobApplicationCreationDocument.document_content)}[1] was too large. Please ensure documents are smaller than 1MB.", validator.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
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
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@hobbitses.com";
			Factory.Save();
			var bytesResume = ResumeForTest;
			var sha256Hash = SHA256.Create();
			var hashKey = Convert.ToBase64String(sha256Hash.ComputeHash(bytesResume));
			var documentData = new JobApplicationCreationDocument { data_type = "docx", document_content = Convert.ToBase64String(bytesResume), document_type = "RSU", file_name = string.Empty, document_content_sha256_hash = hashKey };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@hobbitses.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData } };
			var validator = new JobApplicationCreationRequestDataValidation(Factory, requestData);
			var loadedCampaign = Factory.Load<HRRecruitmentJobCampaign>(requestData.campaign_pk);
			var loadedApplicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)requestData.email_address);
			validator.ValidateAuthenticationData(loadedCampaign, loadedApplicant);
			AssertEquals("Invalid File Name", FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.file_name)}[0] is empty. It must be entered."), validator.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
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
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@hobbitses.com";
			Factory.Save();
			var emptyBytes = Array.Empty<byte>();
			var emptyBytesString = Convert.ToBase64String(emptyBytes);
			AssertEquals("Precondition", string.Empty, emptyBytesString);
			var sha256Hash = SHA256.Create();
			var hashKey = Convert.ToBase64String(sha256Hash.ComputeHash(emptyBytes));
			var documentData = new JobApplicationCreationDocument { data_type = "docx", document_content = emptyBytesString, document_type = "RSU", file_name = "Don Antonio resume", document_content_sha256_hash = hashKey };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@hobbitses.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData } };
			var validator = new JobApplicationCreationRequestDataValidation(Factory, requestData);
			var loadedCampaign = Factory.Load<HRRecruitmentJobCampaign>(requestData.campaign_pk);
			var loadedApplicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)requestData.email_address);
			validator.ValidateAuthenticationData(loadedCampaign, loadedApplicant);
			AssertEquals("Should not have empty bytes", FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.document_content)}[0] is empty. It must be entered."), validator.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
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
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@hobbitses.com";
			Factory.Save();
			AssertEquals("Precondition", 0, applicant.DocManagerInfo.Files.Count);
			var parsingQueueLength = Factory.Load<HRJobApplicationParsingQueue>(new ZQuery()).Length;
			AssertEquals("Precondition", 0, parsingQueueLength);
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@hobbitses.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU" };
			var validator = new JobApplicationCreationRequestDataValidation(Factory, requestData);
			var loadedCampaign = Factory.Load<HRRecruitmentJobCampaign>(requestData.campaign_pk);
			var loadedApplicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)requestData.email_address);
			validator.ValidateAuthenticationData(loadedCampaign, loadedApplicant);
			AssertEquals("Mismatch in number of document arguments", "The document type RSU - Resume/CV is mandatory. A document of this type must be attached.", validator.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
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
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_EmailAddress = "shire.baggins@hobbitses.com";
			Factory.Save();
			var bytesResume = ResumeForTest;
			var documentData = new JobApplicationCreationDocument { data_type = "docx", document_content = Convert.ToBase64String(bytesResume), document_type = "RSU", file_name = "Don Antonio Resume", document_content_sha256_hash = string.Empty };
			var requestData = new JobApplicationCreationRequestData { campaign_pk = jobCampaign.PK.ToGuid(), email_address = "shire.baggins@hobbitses.com", full_name = "shrodo dragons", mobile_phone = "+64 3-234 5678", country_code = "AU", documents = new[] { documentData } };
			var validator = new JobApplicationCreationRequestDataValidation(Factory, requestData);
			var loadedCampaign = Factory.Load<HRRecruitmentJobCampaign>(requestData.campaign_pk);
			var loadedApplicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)requestData.email_address);
			validator.ValidateAuthenticationData(loadedCampaign, loadedApplicant);
			AssertEquals("Checksum incorrect", FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.document_content_sha256_hash)}[0] is empty. It must be entered."), validator.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
			AssertEquals(0, applicant.DocManagerInfo.Files.Count);
			documentData.document_content_sha256_hash = "randomString";
			requestData.documents = new[] { documentData };
			validator = new JobApplicationCreationRequestDataValidation(Factory, requestData);
			validator.ValidateAuthenticationData(loadedCampaign, loadedApplicant);
			AssertEquals("Checksum incorrect", FormattableString.Invariant($"{nameof(JobApplicationCreationDocument.document_content)}[0]'s SHA-256 Hash is invalid."), validator.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
			AssertEquals(0, applicant.DocManagerInfo.Files.Count);
		}

		#endregion
		#region Implementation
		byte[] ResumeForTest
		{
			get
			{
				if (resumeForTest == null)
				{
					var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
					resumeForTest = resourceRetriever.GetBytes(@"ZClientWebEDI.Test.TestFiles.Don Antonio resume.docx");
				}
				return resumeForTest;
			}
		}
		byte[] resumeForTest;
		HRRecruitmentJobCampaign SetupDefaultDocumentsAndCampaign()
		{
			var appDocTypes = new OnlineApplicationDocTypeCollection();
			RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, appDocTypes);
			return Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
		}

		string CreateStringOfSpecifiedLength(int length)
		{
			return string.Empty.PadRight(length, 'a');
		}
		#endregion
	}
}

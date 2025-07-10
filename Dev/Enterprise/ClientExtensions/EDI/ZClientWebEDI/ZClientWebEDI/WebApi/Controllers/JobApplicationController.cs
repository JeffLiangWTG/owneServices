using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.JobApplication;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/JobApplication")]
	public class JobApplicationController : BusinessObjectController
	{
		[HttpPost]
		[Route("CreateJobApplication")]
		public HttpResponseMessage CreateJobApplication([FromBody] JobApplicationCreationRequestData applicationCreationData)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var requestDataValidation = new JobApplicationCreationRequestDataValidation(Factory, applicationCreationData);

				requestDataValidation.CheckMandatoryFieldsForNull();
				if (!requestDataValidation.ErrorMessageBuilder.IsEmpty)
				{
					return Request.CreateResponse(HttpStatusCode.BadRequest, requestDataValidation.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
				}

				var campaign = Factory.Load<HRRecruitmentJobCampaign>(applicationCreationData.campaign_pk);
				var applicant = Factory.LoadFromUniqueKey<HRJobApplicant>(HRJobApplicantSchema.HA_EmailAddress, (ZString)applicationCreationData.email_address);

				requestDataValidation.ValidateAuthenticationData(campaign, applicant);

				if (!requestDataValidation.ErrorMessageBuilder.IsEmpty)
				{
					return Request.CreateResponse(HttpStatusCode.BadRequest, requestDataValidation.ErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
				}

				HRJobApplication application = null;

				try
				{
					applicant = CreateOrUpdateApplicant(applicant, applicationCreationData);
					application = CreateJobApplication(applicant, applicationCreationData);

					if (applicationCreationData.documents != null)
					{
						var parsedDocumentPKs = ParseDocuments(applicant, application, applicationCreationData);

						AddJobApplicationParsingQueue(application, parsedDocumentPKs);
					}

					applicant.SuspendValidation();
					application.SuspendValidation();
					applicant.Person?.SuspendValidation();
					Factory.Save();
					applicant.DocManagerInfo.Save();
					application.DocManagerInfo.Save();
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					ErrorReporter.ReportOnce("JobApplicationController.CreateJobApplication exception", e.Message, e);

					try
					{
						RevertBusinessObjectsOnError(applicant, application);
					}
					catch (Exception e2) when (!e2.IsCriticalException())
					{
						ErrorReporter.ReportOnce("JobApplicationController.CreateJobApplicationRevert exception", e2.Message, e2);
						return Request.CreateResponse(HttpStatusCode.InternalServerError, $"OriginalException: {e.Message}{System.Environment.NewLine}SecondaryException: {e2.Message}");
					}

					return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
				}

				return Request.CreateResponse(HttpStatusCode.OK, "Successfully created application.");
			}
		}

		protected virtual HRJobApplicant CreateOrUpdateApplicant(HRJobApplicant applicant, JobApplicationCreationRequestData applicantDetails)
		{
			if (applicant == null)
			{
				applicant = Factory.New<HRJobApplicant>();

				if (!string.IsNullOrEmpty(applicantDetails.email_address))
				{
					var existingPerson = Factory.LoadTop1<GlbPerson>(new ZQuery(GlbPersonSchema.PER_EmailAddress, applicantDetails.email_address));
					if (existingPerson != null)
					{
						applicant.HA_PER = existingPerson.PK;
					}
				}

				applicant.HA_EmailAddress = applicantDetails.email_address;
			}

			var mapping = new (string, Action<string>)[]
			{
				(applicantDetails.full_name, name => applicant.HA_FullName = name),
				(applicantDetails.mobile_phone, mobile_phone => applicant.HA_MobilePhone = mobile_phone),
				(applicantDetails.country_code, country_code => applicant.HA_RN_NKCountry = country_code),
				(applicantDetails.address1, address => applicant.HA_UserAddress1 = address),
				(applicantDetails.address2, address => applicant.HA_UserAddress2 = address),
				(applicantDetails.city, city => applicant.HA_City = city),
				(applicantDetails.state_code, state_code => applicant.HA_State =  state_code),
				(applicantDetails.nationality_iso_code, nationality => applicant.HA_RN_NKNationalityCodeISO = nationality),
				(applicantDetails.gender, gender => applicant.HA_Gender = gender),
				(applicantDetails.work_permit_status, status =>  applicant.HA_WorkPermitStatus = status),
				(applicantDetails.availability, availability => applicant.HA_Availability = availability),
			};

			if (!applicant.Person.IsInDatabase)
			{
				foreach (var (val, setter) in mapping)
				{
					if (!string.IsNullOrEmpty(val))
					{
						setter(val);
					}
				}

				if (applicantDetails.date_of_birth != default)
				{
					applicant.HA_Birthdate = new ZDate(applicantDetails.date_of_birth);
				}
			}

			return applicant;
		}

		protected HRJobApplication CreateJobApplication(HRJobApplicant applicant, JobApplicationCreationRequestData applicantDetails)
		{
			var application = applicant.Applications.AddNew();
			application.HP_HV = applicantDetails.campaign_pk;
			application.HP_SourceType = applicantDetails.referring_source;

			return application;
		}

		#region Documents

		protected virtual IEnumerable<ZGuid> ParseDocuments(HRJobApplicant applicant, HRJobApplication application, JobApplicationCreationRequestData applicationCreationData)
		{
			var parsedDocumentPKs = new List<ZGuid>();

			foreach (var document in applicationCreationData.documents)
			{
				var byteContents = Convert.FromBase64String(document.document_content);
				var fileName = document.file_name;
				var dataType = document.data_type.ToUpperInvariant();
				var documentType = document.document_type;

				applicant.DocManagerInfo.AddFileOrDocument(byteContents, FormattableString.Invariant($"{fileName}.{dataType}"), documentType, true);
				var addedDoc = application.DocManagerInfo.AddFileOrDocument(byteContents, FormattableString.Invariant($"{fileName}.{dataType}"), documentType, true);
				parsedDocumentPKs.Add(addedDoc.UniqueKey);
			}

			return parsedDocumentPKs;
		}

		protected void AddJobApplicationParsingQueue(HRJobApplication application, IEnumerable<ZGuid> storageDocReferences)
		{
			foreach (var reference in storageDocReferences)
			{
				var query = new ZQuery(HRJobApplicationParsingQueueSchema.HPQ_HP, application.PK);
				query.AddToFilter(HRJobApplicationParsingQueueSchema.HPQ_StorageDocReference, reference);

				if (!Factory.Exists(typeof(HRJobApplicationParsingQueue), query))
				{
					var queue = Factory.New<HRJobApplicationParsingQueue>();
					queue.HPQ_HP = application.PK;
					queue.HPQ_StorageDocReference = reference;
				}
			}
		}

		void RevertBusinessObjectsOnError(HRJobApplicant applicant, HRJobApplication application)
		{
			var shouldSaveEDocsFactory = false;

			if (applicant != null)
			{
				if (applicant.IsInDatabase)
				{
					var newEDocs = applicant.DocManagerInfo.AllEDocs.Cast<IeDoc>().Where(d => !d.ParentMain.IsInDatabase).ToArray();

					if (newEDocs.Any())
					{
						shouldSaveEDocsFactory = true;
					}

					newEDocs.ForEach(x => x.ParentMain.Delete());
					applicant.Reload();
				}
				else
				{
					applicant.Delete();
				}
			}

			if (application != null)
			{
				application.Delete();
			}

			Factory.Save();

			if (shouldSaveEDocsFactory)
			{
				applicant.DocManagerInfo.Save();
			}
		}

		#endregion
	}
}

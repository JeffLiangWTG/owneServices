using System;
using System.Linq;
using System.Security.Cryptography;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.JobApplication
{
	public class JobApplicationCreationRequestDataValidation
	{
		public JobApplicationCreationRequestDataValidation(BusinessObjectFactory factory, JobApplicationCreationRequestData applicationCreationData)
		{
			Factory = factory;
			ApplicationCreationData = applicationCreationData;
		}

		readonly BusinessObjectFactory Factory;
		readonly JobApplicationCreationRequestData ApplicationCreationData;

		public ZStringBuilder ErrorMessageBuilder { get; } = new ZStringBuilder();

		public void CheckMandatoryFieldsForNull()
		{
			if (ApplicationCreationData.campaign_pk == Guid.Empty)
			{
				ErrorMessageBuilder.Append(Res.GetString("dbce202e-a401-4e41-81c8-2b955e1c269e", "The {0} must be entered.", nameof(JobApplicationCreationRequestData.campaign_pk)));
			}

			if (ApplicationCreationData.email_address.IsNullOrEmpty())
			{
				ErrorMessageBuilder.Append(Res.GetString("b680b180-b77b-4bb7-a504-8465936f8449", "Please enter an {0}.", nameof(JobApplicationCreationRequestData.email_address)));
			}
		}

		public void ValidateAuthenticationData(HRRecruitmentJobCampaign campaign, HRJobApplicant applicant)
		{
			if (campaign == null)
			{
				ErrorMessageBuilder.Append(Res.GetString("84d8137a-95c2-4343-aee6-19dc08d08c86", "The {0} in the request does not correspond to any existing Job Campaigns.", nameof(JobApplicationCreationRequestData.campaign_pk)));
				return;
			}

			if (HasExistingApplication(campaign, applicant))
			{
				ErrorMessageBuilder.Append(Res.GetString("49bfe554-99e9-4054-b2a7-0b6f00ffaaa0", "A Job Applicant with the email {0} has already applied for this Job Campaign.", ApplicationCreationData.email_address));
				return;
			}

			CheckApplicantFields();
			CheckDocumentFields();
		}

		bool HasExistingApplication(HRRecruitmentJobCampaign campaign, HRJobApplicant existingApplicant)
		{
			if (existingApplicant == null)
			{
				return false;
			}

			var applicationExistsQuery = new ZQuery(HRJobApplicationSchema.HP_HV, campaign.PK);
			applicationExistsQuery.AddToFilter(HRJobApplicationSchema.HP_HA, existingApplicant.PK);

			return Factory.ExistsInDatabase(HRJobApplicationSchema.Constants.TableName, applicationExistsQuery);
		}

		#region Applicant

		void CheckApplicantFields()
		{
			AppendMaxLengthErrorIfRequired(nameof(ApplicationCreationData.email_address), ApplicationCreationData.email_address, HRJobApplicantSchema.HA_EmailAddress.MaxLength);
			AppendMaxLengthErrorIfRequired(nameof(ApplicationCreationData.full_name), ApplicationCreationData.full_name, GlbPersonSchema.PER_FullName.MaxLength);
			AppendMaxLengthErrorIfRequired(nameof(ApplicationCreationData.mobile_phone), ApplicationCreationData.mobile_phone, GlbPersonSchema.PER_MobilePhone.MaxLength);
			AppendMaxLengthErrorIfRequired(nameof(ApplicationCreationData.country_code), ApplicationCreationData.country_code, GlbPersonSchema.PER_RN_NKCountry.MaxLength);
		}

		#endregion

		#region Documents

		void CheckDocumentFields()
		{
			var permittedDocTypes = RecruiterDataRegistry.Instance.OnlineApplicationDocTypes.Value.Cast<OnlineApplicationDocType>().ToArray();
			var compulsoryDocTypes = permittedDocTypes.Where(dt => dt.IsCompulsory);

			if (ApplicationCreationData.documents != null)
			{
				using (var sha256Hash = SHA256.Create())
				{
					var maximumAllowedBytesForDocument = SystemDataRegistry.Instance.eDocsMaximumFilesize.Value * 1024 * 1024;
					for (var i = 0; i < ApplicationCreationData.documents.Length; i++)
					{
						if (ApplicationCreationData.documents[i].file_name.IsNullOrEmpty())
						{
							ErrorMessageBuilder.Append(GetMustBeEnteredMessage(nameof(JobApplicationCreationDocument.file_name), i));
						}
						else
						{
							AppendMaxLengthErrorIfRequired(nameof(JobApplicationCreationDocument.file_name) + FormattableString.Invariant($"[{i}]"), ApplicationCreationData.documents[i].file_name, StorageDocsSchema.SC_FileName.MaxLength);
						}

						if (ApplicationCreationData.documents[i].data_type.IsNullOrEmpty())
						{
							ErrorMessageBuilder.Append(GetMustBeEnteredMessage(nameof(JobApplicationCreationDocument.data_type), i));
						}
						else
						{
							AppendMaxLengthErrorIfRequired(nameof(JobApplicationCreationDocument.data_type) + FormattableString.Invariant($"[{i}]"), ApplicationCreationData.documents[i].data_type, StorageDocsSchema.SC_DataType.MaxLength);
						}

						if (ApplicationCreationData.documents[i].document_content.IsNullOrEmpty())
						{
							ErrorMessageBuilder.Append(GetMustBeEnteredMessage(nameof(JobApplicationCreationDocument.document_content), i));
						}
						else
						{
							byte[] documentContentBytes = null;
							try
							{
								documentContentBytes = Convert.FromBase64String(ApplicationCreationData.documents[i].document_content);
							}
							catch (FormatException ex)
							{
								ErrorMessageBuilder.Append(Res.GetString("b2d02f6f-1e10-43d6-a25c-b7fc168951fe", "{0}[{1}] has invalid base64 data. Error={2}", nameof(JobApplicationCreationDocument.document_content), i, ex.Message));
								continue;
							}

							if (maximumAllowedBytesForDocument < documentContentBytes.Length)
							{
								ErrorMessageBuilder.Append(Res.GetString("77eb85f9-55a5-4a14-b6bd-013ef1b14cfc", "{0}[{1}] was too large. Please ensure documents are smaller than {2}MB.", nameof(JobApplicationCreationDocument.document_content), i, SystemDataRegistry.Instance.eDocsMaximumFilesize.Value));
							}

							if (ApplicationCreationData.documents[i].document_content_sha256_hash.IsNullOrEmpty())
							{
								ErrorMessageBuilder.Append(GetMustBeEnteredMessage(nameof(JobApplicationCreationDocument.document_content_sha256_hash), i));
							}
							else if (!sha256Hash.ComputeHash(documentContentBytes).SequenceEqual(Convert.FromBase64String(ApplicationCreationData.documents[i].document_content_sha256_hash)))
							{
								ErrorMessageBuilder.Append(Res.GetString("0bad158d-bd28-414b-a006-d5287a3b3f62", "{0}[{1}]'s SHA-256 Hash is invalid.", nameof(JobApplicationCreationDocument.document_content), i));
							}
						}
					}
				}

				ApplicationCreationData.documents.Where(doc => permittedDocTypes.All(x => !x.DocType.RT_DocType.EqualsIgnoringCase(doc.document_type))).ForEach(docType => ErrorMessageBuilder.Append(Res.GetString("0b6fcebd-f5f1-4e2c-9332-643b1e3e2db4", "{0} is not a permitted Online Application Document Type.", docType.document_type)));
			}

			compulsoryDocTypes.Where(docType => ApplicationCreationData.documents == null || !ApplicationCreationData.documents.Any(x => docType.DocType.RT_DocType.EqualsIgnoringCase(x.document_type)))
				.ForEach(docType => ErrorMessageBuilder.Append(Res.GetString("07d232b8-721e-42e1-a8b9-55789961ff70", "The document type {0} - {1} is mandatory. A document of this type must be attached.", docType.DocType.RT_DocType, docType.DocTypeDescription)));
		}

		#endregion

		static string GetMustBeEnteredMessage(string fieldName, int index)
		{
			return Res.GetString("c44a0847-f8b1-47fc-9766-809c293b258b", "{0}[{1}] is empty. It must be entered.", fieldName, index);
		}

		void AppendMaxLengthErrorIfRequired(string fieldName, string field, int maxLength)
		{
			if (field != null && field.Length > maxLength)
			{
				ErrorMessageBuilder.Append(Res.GetString("edc2b4a3-8f55-402f-9b90-72f5e19dbfa9", "{0} exceeded its maximum length of {1}.", fieldName, maxLength));
			}
		}
	}
}

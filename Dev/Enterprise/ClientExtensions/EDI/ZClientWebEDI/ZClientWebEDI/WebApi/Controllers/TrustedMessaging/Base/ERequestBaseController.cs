using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Xml;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Controllers.TrustedMessaging.Base;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public abstract class ERequestBaseController<TRequest> : TrustedControllerWithLicenceDatabase<TRequest> where TRequest : IERequestInfo
	{
		public ERequestBaseController() : base()
		{
		}

		public ERequestBaseController(NLogWrapper logger) : base(logger)
		{
		}

		protected void GetAutoLoginUrlCore(ITrustedContext<AutoLoginResponse> context, TRequest requestInfo)
		{
			if (!context.Success)
			{
				return;
			}

			var database = GetLicenceDatabase(context, requestInfo);

			if (context.Messages?.Messages.Count > 0)
			{
				return;
			}

			if (database == null || !database.LD_IsActive || !database.LD_AllowAutoLogin)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, "Auto login to MyAccount is disabled on this system.");
				return;
			}

			var userAccount = EdiCustomerUserAccount.GetUserAccountForWebAccess(Factory, database, requestInfo.UserId);

			if (UserAgreementRequestValidationHelper.CanBypassUserAccountCollectionAgreement(requestInfo.Product))
			{
				if (userAccount == null)
				{
					var requestDataValidation = new UserAccountCreationRequestDataValidation(requestInfo);
					requestDataValidation.Validate();

					if (requestDataValidation.ErrorMessageBuilder.Messages.Count > 0)
					{
						context.Messages = requestDataValidation.ErrorMessageBuilder;
						return;
					}

					userAccount = EdiCustomerUserAccount.CreateNewUserAccount(Factory, database, requestInfo.UserId, requestInfo.FullName, requestInfo.Email, requestInfo.UserCountry);
				}
			}
			else
			{
				if (userAccount == null)
				{
					context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, "Access to eRequest Portal is not granted.");
					return;
				}
				else
				{
					var myAccountAgreement = EdiUserAgreement.GetCurrentAgreement(Factory, null, EdiUserAgreementTypes.Codes.UserAccountCollection, requestInfo.UserCountry)?.Agreement;
					if (myAccountAgreement != null && !userAccount.HasAcknowledgedUserAgreement(myAccountAgreement))
					{
						context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_AgreementNotAcknowledged, ErrorCodes.Descriptions.Authorization_AgreementNotAcknowledged);
						return;
					}
				}
			}

			var importer = new WebRequestContactImporter(Factory, database);
			var contact = importer.ImportFromTrustedUserInfo(requestInfo).Item2;
			importer.SaveIfNeeded();

			if (contact == null && userAccount.EUA_IsEmailVerificationRequired)
			{
				var router = new MyAccountLoginRouter(null, userAccount);
				var redirectUrl = router.GetRoutingUrl();
				context.ResponseInfo = new AutoLoginResponse(redirectUrl);
				return;
			}

			if (contact == null || (userAccount.EUA_ContactRelationshipStatus != ContactRelationshipStatusList.Codes.DistinctEmailRequired && !contact.OC_Email.IsEmpty && !contact.OC_WebAccessEnabled))
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, "Access to eRequest Portal is not granted.");
				return;
			}

			var autoLoginUrl = BuildAutoLoginUrl(requestInfo, database, userAccount, contact);
			if (autoLoginUrl != null)
			{
				context.ResponseInfo = new AutoLoginResponse(autoLoginUrl);
				return;
			}
			else
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, "Landing page ID is invalid.");
				return;
			}
		}

		Uri BuildAutoLoginUrl(IERequestInfo userInfo, LicenceDatabase database, EdiCustomerUserAccount userAccount, MasterFiles.Business.OrgContact contact)
		{
			var helper = new GlowPortalUrlHelper(new TokenizedAccessControl());

			if (userInfo.LandingPageId == UserPortal.UserPortalLauncher.eRequestNewLandingPageId)
			{
				var product = string.Equals(userInfo.Product, ProductTypes.Codes.CargoWiseOne, StringComparison.OrdinalIgnoreCase) ? ProductTypes.Codes.Enterprise : userInfo.Product;
				string licenceCode = !string.IsNullOrEmpty(userInfo.LicenceCode) ? userInfo.LicenceCode : GetLicenceCode(database, contact.Header);
				return helper.GetNewRequestUrl(contact.PK.ToGuid(), userAccount.PK.ToGuid(), licenceCode, product, userInfo.Module, userInfo.SubModule, userInfo.Criticality, userInfo.ReferenceId);
			}
			else if (userInfo.LandingPageId == UserPortal.UserPortalLauncher.eRequestPortalLandingPageId)
			{
				return helper.GetRequestPortalUrl(contact.PK.ToGuid(), userAccount.PK.ToGuid());
			}
			else if (userInfo.LandingPageId == UserPortal.UserPortalLauncher.eRequestEditLandingPageId)
			{
				ZGuid requestPk = GlowPortalUrlHelper.IncidentRequestPkFromNumber(userInfo.IncidentNumber);
				if (!requestPk.IsEmpty)
				{
					return helper.GetEditRequestUrl(contact.PK.ToGuid(), userAccount.PK.ToGuid(), requestPk.ToGuid());
				}
				else
				{
					helper.GetRequestPortalUrl(contact.PK.ToGuid(), userAccount.PK.ToGuid());
				}
			}

			return null;
		}

		string GetLicenceCode(LicenceDatabase database, MasterFiles.Business.OrgHeader org)
		{
			if (ProductTypes.IsEnterpriseFamily(database.LD_Product))
			{
				var company = database.LicHeadersForAllCompanies.Cast<LicenceHeader>().FirstOrDefault(l => l.Company.Header.PK == org.PK)?.Company;
				if (company != null)
				{
					return FormattableString.Invariant($"{database.EnterpriseCode}{company.LC_CompanyCode}{database.LD_ServerCode}");
				}
				return database.LicenceCodeForSystemMessage;
			}
			else
			{
				return FormattableString.Invariant($"{database.EnterpriseID}.{database.DatabaseId}");
			}
		}

		protected void UploadCore(ITrustedContext<bool> context, IERequestInfo info)
		{
			if (!context.Success)
			{
				return;
			}

			if (string.IsNullOrEmpty(info.ERequestDocument))
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_MissingRequiredField, "ERequestDocument is required.");
				return;
			}

			try
			{
				using (var reader = new StringReader(info.ERequestDocument))
				{
					var doc = (ERequestDocument)ZXmlSerializer.New(typeof(ERequestDocument)).Deserialize(reader);
					if (ZGuid.TryParse(doc.ReferenceId, out var refId) && refId.IsValid)
					{
						var attachments = doc.Attachments.OfType<ERequestDocumentAttachment>().Where(x => !x.FileName.IsEmpty && !x.Data.IsNullOrEmpty());
						if (attachments.Any())
						{
							foreach (var att in attachments)
							{
								var docQueue = Factory.New<EdiERequestDocumentQueue>();
								docQueue.EDQ_INC_ReferenceID = refId;
								docQueue.EDQ_FileName = att.FileName;
								docQueue.EDQ_IsPublished = att.IsPublished;
								docQueue.EDQ_Data = att.Data;
							}

							Factory.Save();
							context.ResponseInfo = true;
							return;
						}
						else
						{
							context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, "ERequestDocument contains no elements.");
							return;
						}
					}
					else
					{
						context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, "Invalid ERequestDocument referenceId.");
						return;
					}
				}
			}
			catch (ZSaveException ex)
			{
				ErrorReporter.ReportOnce("ERequestController.Upload(): Upload ERequestDocument failed", ex);
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Server_Error, "Internal Server Error.");
				return;
			}
			catch (InvalidOperationException)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, "Invalid ERequestDocument XML format.");
				return;
			}
		}
	}
}

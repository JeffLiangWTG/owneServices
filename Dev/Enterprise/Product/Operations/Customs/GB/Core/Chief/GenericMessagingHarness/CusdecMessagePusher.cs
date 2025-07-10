using System;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.GB.Chief.CspPuller;
using Enterprise.Customs.GB.Registry;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Chief
{
	public abstract class CusdecMessagePusher
	{
		public CusdecMessagePusher(EDIInterchange interchangeToUpload)
		{
			Argument.NotNull(interchangeToUpload, "interchangeToUpload");
			if (interchangeToUpload.EI_From.IsEmpty)
			{
				throw new Exception("Cannot send this interchange as it has no recipient.  EI_FROM should be the badge code of the organisation on whose behalf we're sending. This is a programming error on the part of the creator of the interchange.");
			}
			if (interchangeToUpload.ContainedMessages == null || interchangeToUpload.ContainedMessages.Count == 0)
			{
				throw new Exception("CusDec uploader must operate at the message-within-interchange level.  This interchange contained no message.");
			}
			if (interchangeToUpload.ContainedMessages.Count > 1)
			{
				throw new Exception("CusDec uploader must upload a single message per interchange.  This interchange contained this many messages: " + interchangeToUpload.ContainedMessages.Count.ToString());
			}
			this.ediMessageToUpload = GetPayloadFromInterchangeAndSetTrainingFlag(interchangeToUpload); //.ContainedMessages[0].EM_MessageText;
			this.Credentials = GetCompanyCredentialsForThisInterchange(interchangeToUpload);
			var cred = GetCredentialForThisInterchange(interchangeToUpload);
			if (cred != null)
			{
				this.Credential = cred;
			}
		}

		/// <summary>
		/// Returns that actual string we want to send as the payload.  Could be the whole interchange, or just a message, depending on recipient. 
		/// </summary>
		public abstract string GetPayloadFromInterchangeAndSetTrainingFlag(EDIInterchange interchangeToUpload);

		public string UploadEdifactToCspReturningInterchangeString()
		{
			CredentialsRequireRefreshing = false;
			IGbCspUploaderInterface cspConnection = GetConnectionForPusher();   // Set per CSP
			string cspEndpointUrl = GetEndpointUrlToWhichPusherConnects(); // Set per CSP
			if (string.IsNullOrEmpty(cspEndpointUrl))
			{
				throw new UriFormatException("The uploader/downloader Service Tasks cannot run because no URL has been set-up in the registry.");
			}

			try
			{
				new Uri(cspEndpointUrl);  // This will blow up if the URL in the registry is not value.
				cspConnection.Url = cspEndpointUrl;
			}
			catch (UriFormatException)
			{
				throw new UriFormatException("The uploader/downloader Service Tasks cannot run because an invalid URL has been set-up in the registry. [URL] = [" + cspEndpointUrl + "]");
			}

			cspConnection.CredentialsSetting = this.Credential;
			string result = null;
			try
			{
				// NB for CNS the company code sent in the soap envelope is irrelevant because they glean it from the interchange's header. 
				var userAgent = Chief.GenericMessagingHarness.CredentialsAndBadgeChecker.GetUserAgentForSoapRequests(this.Credential.BadgeCode);
				cspConnection.UserAgent = userAgent;
				UserAgentForTesting = userAgent;
				result = cspConnection.processEDIMessage(this.ediMessageToUpload, this.Credential.Company, !isTrainingMode);
				this.Status = CheckForKnownErrorMessages(result, cspConnection);
				CredentialsRequireRefreshing = CspMessagePuller.MaybeClearLoginFailureCount(this.Credential, this.Credentials);
			}
			catch (System.Net.WebException ex)
			{
				this.Status = ErrorCodes.Failure;
				LastErrorMessageToLog = HttpErrorHelper.HandleWebExceptionAndPrepareLogMessage(cspConnection, ex, this.Credential, ediMessageToUpload);
				CredentialsRequireRefreshing = CspMessagePuller.MaybeIncrementLoginFailureCount(ex, this.Credential, this.Credentials);
				if (ex.IsCriticalException())
				{
					throw;
				}
			}
#if NET48
			catch (System.Web.Services.Protocols.SoapException se)
			{
				// Soap exception - e.g. "SOAP CHIEF UNAVAILABLE"
				this.Status = ErrorCodes.Failure;
				LastErrorMessageToLog = HttpErrorHelper.HandleWebExceptionAndPrepareLogMessage(cspConnection, se, this.Credential, ediMessageToUpload);
				CredentialsRequireRefreshing = CspMessagePuller.MaybeIncrementLoginFailureCount(se, this.Credential, this.Credentials);
				if (se.Detail != null)
				{
					LastErrorMessageToLog += " " + se.Detail.InnerText;
				}
				if (se.IsCriticalException())
				{
					throw;
				}
			}
#endif
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				this.Status = ErrorCodes.Failure;
				LastErrorMessageToLog = "Method processEDIMessage() generated an exception: " + ex.Message;
				CredentialsRequireRefreshing = CspMessagePuller.MaybeIncrementLoginFailureCount(ex, this.Credential, this.Credentials);
				if (ex.IsCriticalException())
				{
					throw;
				}
			}
			return result;
		}

		public string UserAgentForTesting
		{ get; private set; }

		public abstract IGbCspUploaderInterface GetConnectionForPusher();

		public abstract string GetEndpointUrlToWhichPusherConnects();

		public enum ErrorCodes
		{
			Failure,
			Success
		}

		public string LastErrorMessageToLog { get; set; }

		public ErrorCodes Status { get; set; }

		public CredentialsSetting Credential { get; set; }

		public CredentialsSettingCollection Credentials { get; set; }

		public bool CredentialsRequireRefreshing { get; private set; }

		protected string ediMessageToUpload;

		CredentialsSettingCollection GetCompanyCredentialsForThisInterchange(EDIInterchange interchange)
		{
			if (interchange != null && interchange.ContainedMessages[0] != null && interchange.ContainedMessages[0].Branch != null && interchange.ContainedMessages[0].Branch.Company != null)
			{
				var credentials = CredentialsSetting.GetAllCredentials(interchange.ContainedMessages[0].Branch.Company.PK) ?? throw new ArgumentException("Credentials for company " + interchange.ContainedMessages[0].Branch.Company.CompanyName + " were not found, cannot upload.  Ensure you supply the credentials in the registry");
				return credentials;
			}
			return null;
		}

		public CredentialsSetting GetCredentialForThisInterchange(EDIInterchange interchange)
		{
			ZString badge = interchange.EI_To;
			return CredentialsSetting.GetCredentialsForBadge(badge, this.Credentials);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		ErrorCodes CheckForKnownErrorMessages(string textResponseFromCsp, IGbCspUploaderInterface cspConnection)
		{
			switch (textResponseFromCsp.ToLower())  // Why are we placing the text of the error where the edifact payload should be!?
			{
				case "invalid message format":
				case "must be your own company":
					this.LastErrorMessageToLog = string.Format("The CSP reported an error. Please contact them for assistance. If the error is about bad credentials then please contact the CSP's helpdesk before the {0} helpdesk, because bad credentials are not a fault in the {0} software. ", BrandingFactory.Instance.ProductName) + HttpErrorHelper.HandleWebExceptionAndPrepareLogMessage(cspConnection, null, this.Credential, textResponseFromCsp);
					return ErrorCodes.Failure;
				default:
					return ErrorCodes.Success;
			}
		}

		protected bool isTrainingMode;
	}
}

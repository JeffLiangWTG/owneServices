using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Chief.CspPuller
{
	/// <summary>
	/// Destin8 Webservice handler, pulls down queued EdiPrintMessages from Destin8, saves them, acknowledges them.
	/// </summary>
	public abstract class CspMessagePuller
	{
		public enum ErrorCodes
		{
			Success,
			Failure
		}

		public CredentialsSetting Credential
		{
			get;
			set;
		}

		public CredentialsSettingCollection CompanyCredentials { get; set; }

		public string LastErrorMessageToLog
		{
			get;
			private set;
		}

		/// <summary>
		/// Flag showing overall success or not.
		/// </summary>
		public ErrorCodes StatusOfDownload
		{
			get;
			private set;
		}

		public string AcknowledgeMessageBatchBackToCsp(int batchIdToAcknowledge)
		{
			if (batchIdToAcknowledge == 0)
			{
				return "";  // don't do anything
			}

			LastErrorMessageToLog = null;
			ICspResultOfAcknowledgement iCspAcknowledgementResultError;
			ICspPrintsMailBoxProvider cspConnectionToPrintsMailbox = GetConnectionAndSetupUrlAndCredentials(); // ObjectFactory.Get<IDestin8WebService>(); // Returns real proxy for live or the Mocked one during tests.
			string result = null;

			try
			{
				iCspAcknowledgementResultError = cspConnectionToPrintsMailbox.acknowledgeEdifactPrints(Credential.Company, Credential.Printer, batchIdToAcknowledge.ToString());
				if (iCspAcknowledgementResultError != null && iCspAcknowledgementResultError.messageCode != 0)  // Destin8 return NULL for success, and for error they will return a code-desc pair (an EdiAcknowledgeVO); however during our test with the dummy proxy object we cannot return null (or we get an error).  So in our test we return an empty EdiAcknowledgeVO object.
				{   // Had an error in acknowledging
					result = iCspAcknowledgementResultError.messageText;
					StatusOfDownload = ErrorCodes.Failure;
				}
				else
				{   // either we got back NULL (from destin8) or we got back an empty EdiAcknowledgeVO object (from our test) (out dummy webserice don't like returning null)
					StatusOfDownload = ErrorCodes.Success;
				}
			}
#if NET48
			catch (System.Web.Services.Protocols.SoapException se)
			{
				StatusOfDownload = ErrorCodes.Failure;
				LastErrorMessageToLog = "Could not acknowledge messages back to CSP. Suffered SoapException: " + se.Message;
			}
#endif
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				StatusOfDownload = ErrorCodes.Failure;
				LastErrorMessageToLog = "Could not acknowledge messages back to CSP. Suffered Exception: " + ex.Message;
			}

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public CspPrintMessagesList PollCspForWaitingPrintMessagesAndReturnStrings(ref bool credentialsRequireRefreshing)
		{
			StatusOfDownload = ErrorCodes.Failure;
			ICspPrintsMailBoxProvider cspConnectionToPrintsMailbox = GetConnectionAndSetupUrlAndCredentials(); //ObjectFactory.Get<IDestin8WebService>(); // Returns real proxy for live or the Mocked one during tests.

			if (string.IsNullOrEmpty(this.Credential.Printer))
			{
				LastErrorMessageToLog = "No printer code has been configured. Please set one up in the registry.";
				return null;
			}

			ICspDownloadResult iCspDownloadResultOfPoll = null;
			LastErrorMessageToLog = null;

			try
			{
				iCspDownloadResultOfPoll = cspConnectionToPrintsMailbox.getAvailableEdifactPrints(this.Credential.Company, this.Credential.Printer);
				StatusOfDownload = ErrorCodes.Success;
				credentialsRequireRefreshing = MaybeClearLoginFailureCount(Credential, CompanyCredentials);
			}
#if NET48
			catch (System.Web.Services.Protocols.SoapException se)
			{
				credentialsRequireRefreshing = MaybeIncrementLoginFailureCount(se, Credential, CompanyCredentials);
				if (se.IsCriticalException())
				{
					throw;
				}
				else
				{
					StatusOfDownload = ErrorCodes.Failure;
					LastErrorMessageToLog = "Could not download from CSP. Suffered SoapException: " + se.Message;
					return new CspPrintMessagesList(0);
				}
			}
#endif
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				credentialsRequireRefreshing = MaybeIncrementLoginFailureCount(ex, Credential, CompanyCredentials);
				if (ex.IsCriticalException())
				{
					throw;
				}
				else
				{
					StatusOfDownload = ErrorCodes.Failure;
					LastErrorMessageToLog = HttpErrorHelper.HandleWebExceptionAndPrepareLogMessage(cspConnectionToPrintsMailbox, ex, this.Credential);
					return new CspPrintMessagesList(0);
				}
			}

			if (iCspDownloadResultOfPoll == null || iCspDownloadResultOfPoll.MessagesArray == null || iCspDownloadResultOfPoll.MessagesArray.Length == 0)
			{   // nothing waiting for us
				return new CspPrintMessagesList(0);
			}

			CspPrintMessagesList messagesArray = new CspPrintMessagesList(iCspDownloadResultOfPoll.batchId);
			if (string.IsNullOrEmpty(iCspDownloadResultOfPoll.errorText))
			{
				foreach (string printMessage in iCspDownloadResultOfPoll.MessagesArray)
				{
					messagesArray.AddMessage(printMessage);
				}
			}
			else
			{
				messagesArray.Error = new CspPrintMessagesList.ErrorStruct();
				messagesArray.Error.ErrorText = iCspDownloadResultOfPoll.errorText;
			}
			return messagesArray;
		}

		public static bool MaybeIncrementLoginFailureCount(Exception ex, CredentialsSetting credential, CredentialsSettingCollection companyCredentials)
		{
			if (credential != null && ex.Message.Contains("401"))  // login failure
			{
				credential.WebServiceFailureCount++;
				return UpdateLoginFailureCount(credential, companyCredentials);
			}
			return false;
		}

		public static bool MaybeClearLoginFailureCount(CredentialsSetting credential, CredentialsSettingCollection companyCredentials)
		{
			if (credential != null && credential.WebServiceFailureCount > 0)
			{
				credential.WebServiceFailureCount = 0;
				return UpdateLoginFailureCount(credential, companyCredentials);
			}
			return false;
		}

		static bool UpdateLoginFailureCount(CredentialsSetting badgeCredential, CredentialsSettingCollection companyCredentials)
		{
			if (badgeCredential != null && companyCredentials != null)
			{
				using (GBCustomsDataRegistry.Instance.Credentials.DataType.SuspendValidation())
				{
					var companyPk = GlbCompany.CurrentCompany.PK.ToGuid();
					GBCustomsDataRegistry.Instance.Credentials.SetValue(companyPk, Guid.Empty, Guid.Empty, companyCredentials);
				}
				return true;
			}
			return false;
		}

		public abstract ICspPrintsMailBoxProvider GetConnectionAndSetupUrlAndCredentials();
	}
}

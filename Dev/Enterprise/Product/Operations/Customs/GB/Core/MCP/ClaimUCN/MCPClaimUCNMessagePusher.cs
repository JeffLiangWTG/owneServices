using System;
using System.Linq;
using System.Web.Services.Protocols;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.MCP.ClaimUCN
{
	public class MCPClaimUCNMessagePusher
	{
		public MCPClaimUCNMessagePusher(EDIInterchange interchangeToUpload)
		{
			Argument.NotNull(interchangeToUpload, nameof(interchangeToUpload));
			if (interchangeToUpload.EI_From.IsEmpty)
			{
				throw new Exception("Cannot send this interchange as it has no recipient.  EI_FROM should be the badge code of the organisation on whose behalf we're sending. This is a programming error on the part of the creator of the interchange.");
			}
			if (interchangeToUpload.ContainedMessages == null || interchangeToUpload.ContainedMessages.Count == 0)
			{
				throw new Exception("Claim UCN uploader must operate at the message-within-interchange level.  This interchange contained no message.");
			}
			if (interchangeToUpload.ContainedMessages.Count > 1)
			{
				throw new Exception($"Claim UCN uploader must upload a single message per interchange.  This interchange contained {interchangeToUpload.ContainedMessages.Count} messages");
			}
			ediMessageToUpload = interchangeToUpload.ContainedMessages[0].EM_MessageText;
			var cred = GetCredentialForThisInterchange(interchangeToUpload);
			if (cred != null)
			{
				Credential = cred;
			}
		}

		public IMCPClaimUCNUploader GetConnectionForPusher()
		{
			return ObjectFactory.Get<IMCPClaimUCNWebService>(); // Returns real proxy for live or the Mocked one during tests.
		}

		public string GetEndpointUrlToWhichPusherConnects()
		{
			return GBCustomsDataRegistry.Instance.McpIslWebserviceUrl;
		}

		public string AckISLReports()
		{
			var islConnection = PrepConnection();
			string result = null;
			try
			{
				result = islConnection.ackISLReports(ediMessageToUpload);
				if (string.IsNullOrEmpty(result))
				{
					Status = ErrorCodes.Success;
				}
				else
				{
					Status = ErrorCodes.Failure;
				}
			}
			catch (System.Net.WebException ex)
			{
				Status = ErrorCodes.Failure;
				LastErrorMessageToLog = HttpErrorHelper.HandleWebExceptionAndPrepareLogMessage(islConnection, ex, Credential, ediMessageToUpload);
				if (ex.IsCriticalException())
				{
					throw;
				}
			}
			catch (SoapException se)
			{
				Status = ErrorCodes.Failure;
				LastErrorMessageToLog = HttpErrorHelper.HandleWebExceptionAndPrepareLogMessage(islConnection, se, Credential, ediMessageToUpload);
				if (se.Detail != null)
				{
					LastErrorMessageToLog += " " + se.Detail.InnerText;
				}
				if (se.IsCriticalException())
				{
					throw;
				}
			}
			catch (Exception ex)
			{
				Status = ErrorCodes.Failure;
				LastErrorMessageToLog = "Method ackISLReports() generated an exception: " + ex.Message;
				if (ex.IsCriticalException())
				{
					throw;
				}
			}
			return result;
		}

		public BatchesAvailable GetISLReports()
		{
			var islConnection = PrepConnection();
			BatchesAvailable result = null;
			try
			{
				result = islConnection.getISLReports(Credential.McpIslCompanyCode, Credential.McpIslDevice);
				if (result?.batchDetails != null)
				{
					if (result.batchDetails.Length > 0)
					{
						if (!result.batchDetails[0].messages.IsNullOrEmpty())
						{
							Status = ErrorCodes.Success;
						}
						else
						{
							Status = ErrorCodes.NothingToDownload;
						}
					}
				}
			}
			catch (System.Net.WebException ex)
			{
				Status = ErrorCodes.Failure;
				LastErrorMessageToLog = HttpErrorHelper.HandleWebExceptionAndPrepareLogMessage(islConnection, ex, Credential, ediMessageToUpload);
				if (ex.IsCriticalException())
				{
					throw;
				}
			}
			catch (SoapException se)
			{
				Status = ErrorCodes.Failure;
				LastErrorMessageToLog = HttpErrorHelper.HandleWebExceptionAndPrepareLogMessage(islConnection, se, Credential, ediMessageToUpload);
				if (se.Detail != null)
				{
					LastErrorMessageToLog += " " + se.Detail.InnerText;
				}
				if (se.IsCriticalException())
				{
					throw;
				}
			}
			catch (Exception ex)
			{
				Status = ErrorCodes.Failure;
				LastErrorMessageToLog = "Method getISLReports() generated an exception: " + ex.Message;
				if (ex.IsCriticalException())
				{
					throw;
				}
			}
			return result;
		}

		public string SendISLMessage()
		{
			var islConnection = PrepConnection();
			string result = null;
			try
			{
				result = islConnection.sendISLMessageSync(ediMessageToUpload, Credential.McpIslCompanyCode);
				Status = CheckForKnownErrorMessages(result, islConnection);
			}
			catch (System.Net.WebException ex)
			{
				Status = ErrorCodes.Failure;
				LastErrorMessageToLog = HttpErrorHelper.HandleWebExceptionAndPrepareLogMessage(islConnection, ex, Credential, ediMessageToUpload);
				if (ex.IsCriticalException())
				{
					throw;
				}
			}
			catch (SoapException se)
			{
				Status = ErrorCodes.Failure;
				LastErrorMessageToLog = HttpErrorHelper.HandleWebExceptionAndPrepareLogMessage(islConnection, se, Credential, ediMessageToUpload);
				if (se.Detail != null)
				{
					LastErrorMessageToLog += " " + se.Detail.InnerText;
				}
				if (se.IsCriticalException())
				{
					throw;
				}
			}
			catch (Exception ex)
			{
				Status = ErrorCodes.Failure;
				LastErrorMessageToLog = "Method sendISLMessageSync() generated an exception: " + ex.Message;
				if (ex.IsCriticalException())
				{
					throw;
				}
			}
			return result;
		}

		public enum ErrorCodes
		{
			Failure,
			BusinessError,
			NothingToDownload,
			Success
		}

		public string LastErrorMessageToLog { get; set; }

		public ErrorCodes Status { get; set; }

		public McpIslCredentialsSetting Credential { get; set; }

		public McpIslCredentialsSetting GetCredentialForThisInterchange(EDIInterchange interchange)
		{
			McpIslCredentialsSetting credential = null;
			if (interchange.ContainedMessages.Count == 1)
			{
				var message = interchange.ContainedMessages[0];
				var badge = interchange.EI_From;
				if (badge.IsEmpty)
				{
					badge = message.EM_MessageOwner;
				}
				if (badge.IsEmpty)
				{
					badge = (message.EM_LinkedObject as JobDeclaration)?.JE_CustomsProfile ?? ZString.Empty;
				}
				using (DisposableEnvironment.ForBranch(message.EM_GB.ToGuid()))
				{
					credential = GBCustomsDataRegistry.Instance.McpIslWebServiceCredentialsSet.GetValueWithoutFallback(Guid.Empty, message.EM_GB.ToGuid(), Guid.Empty)
						.Cast<McpIslCredentialsSetting>().FirstOrDefault(x => x.McpIslCompanyCode == badge);
				}
			}
			return credential;
		}

		protected string ediMessageToUpload;

		IMCPClaimUCNUploader PrepConnection()
		{
			var islConnection = GetConnectionForPusher();
			var cspEndpointUrl = GetEndpointUrlToWhichPusherConnects();
			if (string.IsNullOrEmpty(cspEndpointUrl))
			{
				throw new UriFormatException("The uploader/downloader Service Tasks cannot run because no URL has been set-up in the registry.");
			}

			try
			{
				new Uri(cspEndpointUrl);
				islConnection.Url = cspEndpointUrl;
			}
			catch (UriFormatException)
			{
				throw new UriFormatException("The uploader/downloader Service Tasks cannot run because an invalid URL has been set-up in the registry. [URL] = [" + cspEndpointUrl + "]");
			}

			islConnection.CredentialsSetting = Credential;
			return islConnection;
		}

		ErrorCodes CheckForKnownErrorMessages(ZString textResponse, IMCPClaimUCNUploader islConnection)
		{
			string responseCode = textResponse.SubstringSafe(1, 4);

			if (responseCode.Length == 4)
			{
				switch (responseCode)
				{
					case Constants.ResponseCode.Success: // "!0000}", "!0000~[ucn]}"
						return ErrorCodes.Success;
					case Constants.ResponseCode.UnitIdTooLong:
						LastErrorMessageToLog = $"Unit id too long: " + textResponse;
						return ErrorCodes.BusinessError;
					case Constants.ResponseCode.InvalidUnitId:
						LastErrorMessageToLog = $"Invalid unit id: " + textResponse;
						return ErrorCodes.BusinessError;
					case Constants.ResponseCode.AlreadyNominated:
						LastErrorMessageToLog = $"Container is already nominated: " + textResponse;
						return ErrorCodes.BusinessError;
					default:
						LastErrorMessageToLog = $"Message rejected: " + textResponse;
						return ErrorCodes.BusinessError;
				}
			}
			else
			{
				LastErrorMessageToLog = $"The CSP reported an error. Please contact them for assistance. If the error is about bad credentials then please contact the CSP's helpdesk before the {BrandingFactory.Instance.ProductName} helpdesk. " + HttpErrorHelper.HandleWebExceptionAndPrepareLogMessage(islConnection, null, Credential, textResponse);
				return ErrorCodes.Failure;
			}
		}
	}
}

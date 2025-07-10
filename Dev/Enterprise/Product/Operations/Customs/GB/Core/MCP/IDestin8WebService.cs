using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.MCP.CusDec.destin8WebService
{
	public interface IDestin8WebService : Chief.IGbCspUploaderInterface, ICspPrintsMailBoxProvider
	{
	}

	public partial class ChiefEDIPortClient : IDestin8WebService
	{
		CredentialsSetting credentialsSetting;
		public CredentialsSetting CredentialsSetting
		{
			get { return credentialsSetting; }
			set
			{
				credentialsSetting = value;
				Credentials = credentialsSetting.GetCredential(null, null);
			}
		}

		string url = "http://ediuat.destin8.co.uk";
		public string Url
		{
			get => url;
			set
			{
				this.Endpoint.Address = new System.ServiceModel.EndpointAddress(value);
				this.url = value;
			}
		}

		ICredentials credentials;
		public ICredentials Credentials
		{
			get => credentials;
			set
			{
				var credentials = value as NetworkCredential;
				this.ClientCredentials.UserName.UserName = credentials.UserName;
				this.ClientCredentials.UserName.Password = credentials.Password;
				this.credentials = value;
			}
		}

		string userAgent = Chief.GenericMessagingHarness.CredentialsAndBadgeChecker.GetUserAgentForSoapRequests(string.Empty);
		public string UserAgent
		{
			get => userAgent;
			set
			{
				var existing = this.Endpoint.EndpointBehaviors.OfType<UserAgentEndpointBehavior>().FirstOrDefault();
				if (existing != null)
				{
					this.Endpoint.EndpointBehaviors.Remove(existing);
				}
				this.Endpoint.EndpointBehaviors.Add(new UserAgentEndpointBehavior(value));
				userAgent = value;
			}
		}

		static partial void ConfigureEndpoint(ServiceEndpoint serviceEndpoint, ClientCredentials clientCredentials)
		{
			serviceEndpoint.Address = new System.ServiceModel.EndpointAddress("http://ediuat.destin8.co.uk");
			serviceEndpoint.EndpointBehaviors.Add(new UserAgentEndpointBehavior(Chief.GenericMessagingHarness.CredentialsAndBadgeChecker.GetUserAgentForSoapRequests(string.Empty)));
		}

		ICspResultOfAcknowledgement ICspPrintsMailBoxProvider.acknowledgeEdifactPrints(string companyCode, string printer, string batchId)
		{
			UserAgent = Chief.GenericMessagingHarness.CredentialsAndBadgeChecker.GetUserAgentForSoapRequests(companyCode);
			return acknowledgeEdifactPrintsAsync(companyCode, printer, batchId).GetAwaiter().GetResult().Response;
		}

		ICspDownloadResult ICspPrintsMailBoxProvider.getAvailableEdifactPrints(string company, string printer)
		{
			UserAgent = Chief.GenericMessagingHarness.CredentialsAndBadgeChecker.GetUserAgentForSoapRequests(company);
			return getAvailableEdifactPrintsAsync(company, printer).GetAwaiter().GetResult().result;
		}

		ICspDownloadResult ICspPrintsMailBoxProvider.checkCdsCredentials(string company, string printer)
		{
			UserAgent = Chief.GenericMessagingHarness.CredentialsAndBadgeChecker.GetUserAgentForSoapRequests(company);
			return new CdsCredentialChecker().checkCdsCredentials(Credentials, printer, GBCustomsDataRegistry.Instance.McpCdsCheckCredentialsUrl, UserAgent);
		}

		public string processEDIMessage(string ediMessageToUpload, string companyCode, bool operationalFlag)
		{
			UserAgent = Chief.GenericMessagingHarness.CredentialsAndBadgeChecker.GetUserAgentForSoapRequests(companyCode);
			return processEDIMessageAsync(ediMessageToUpload, companyCode, operationalFlag).GetAwaiter().GetResult().Response;
		}
	}

	public partial class EDIAcknowledgeVO : ICspResultOfAcknowledgement
	{
		// all members match
	}

	public partial class EDIMessageBatch : ICspDownloadResult
	{
		string[] ICspDownloadResult.MessagesArray
		{
			get
			{
				List<string> list = new List<string>();
				if (this.batch != null)
				{
					foreach (PrintMessage oneMessage in this.batch.messages)
					{
						list.Add(oneMessage.message);
					}
					return list.ToArray();
				}
				return null;
			}
		}

		int ICspDownloadResult.batchId
		{
			get
			{
				if (this.batch != null)
				{
					return this.batch.batchID;
				}
				return 0;
			}
			set
			{
				if (this.batch != null)
				{
					this.batch.batchID = value;
				}
			}
		}

		string ICspDownloadResult.errorText
		{
			get { return this.errorText; }
		}
	}
}

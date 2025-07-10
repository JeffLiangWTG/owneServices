using System;
using System.ComponentModel;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.MCP.ClaimUCN
{
	public interface IMCPClaimUCNWebService : IMCPClaimUCNUploader
	{
	}

	public partial class SLInterfaceMCPPortClient : IMCPClaimUCNWebService
	{
		McpIslCredentialsSetting credentialsSetting;
		public McpIslCredentialsSetting CredentialsSetting
		{
			get { return credentialsSetting; }
			set
			{
				credentialsSetting = value;
				ClientCredentials.UserName.UserName = credentialsSetting.McpIslUsername;
				ClientCredentials.UserName.Password = credentialsSetting.McpIslPassword;
			}
		}

		Uri uri;
		[DefaultValue("")]
		[SettingsBindable(true)]
		public string Url
		{
			get
			{
				if (!(uri == null))
				{
					return uri.ToString();
				}
				return string.Empty;
			}
			set
			{
				uri = new Uri(value);
				Endpoint.Address = new System.ServiceModel.EndpointAddress(uri);
			}
		}
	}
}

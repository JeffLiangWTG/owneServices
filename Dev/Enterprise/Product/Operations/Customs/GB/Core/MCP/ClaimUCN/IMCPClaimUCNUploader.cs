using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.MCP.ClaimUCN
{
	public interface IMCPClaimUCNUploader : IUrlProvider
	{
		McpIslCredentialsSetting CredentialsSetting { get; set; }

		string sendISLMessageSync(string ediMessageToUpload, string companyCode);

		BatchesAvailable getISLReports(string companyCode, string device);

		string ackISLReports(string messageToUpdate);
	}

	public interface IUrlProvider
	{
		string Url { get; set; }
	}
}

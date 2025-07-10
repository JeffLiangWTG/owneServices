using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.Chief
{
	public interface IGbCspUploaderInterface : IUrlProvider
	{
		CredentialsSetting CredentialsSetting { get; set; }
		string processEDIMessage(string ediMessageToUpload, string companyCode, bool operationalFlag);
	}
}

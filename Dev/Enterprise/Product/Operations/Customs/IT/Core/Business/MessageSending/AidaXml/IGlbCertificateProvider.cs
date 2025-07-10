namespace Enterprise.Customs.IT.Business;

public interface IGlbCertificateProvider
{
	IGlbMauExternalPassword GetMauCertificatePassword(string userId);

	ICryptokiGlbExternalPassword GetCryptokiCertificate();

	IAutomaticSignatureExternalPassword AutomaticSignaturePassword { get; }

	bool HasValidAutomaticSignaturePassword { get; }
}

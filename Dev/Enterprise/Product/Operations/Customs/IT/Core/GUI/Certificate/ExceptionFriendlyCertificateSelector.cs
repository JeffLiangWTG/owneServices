using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI.Certificates;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.GUI;

sealed class ExceptionFriendlyCertificateSelector : TokenCertificateSelector
{
	ExceptionFriendlyCertificateSelector(string chipset) : base(chipset)
	{
	}

	public new static ExceptionFriendlyCertificateSelector New(string chipset)
	{
		return new ExceptionFriendlyCertificateSelector(chipset);
	}

	protected override ICryptokiCertificateProvider GetNewCryptokiCertificateProvider(string certificateSource)
	{
		var cryptokiCertificateProvider = base.GetNewCryptokiCertificateProvider(certificateSource);
		return new ExceptionHandlerCryptokiCertificateProviderDecorator(cryptokiCertificateProvider);
	}
}

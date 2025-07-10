using CargoWise.Common;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public sealed class IrildesRequestContext : IIrildesRequestContext
{
	public IrildesRequestContext(NctsHeader header, IGlbCertificateProvider glbCertificateProvider)
	{
		nctsHeader = Argument.NotNull(header, nameof(header));
		this.glbCertificateProvider = Argument.NotNull(glbCertificateProvider, nameof(glbCertificateProvider));
	}

	IGlbMauExternalPassword IIrildesRequestContext.MauCertificate
		=> mauPassword ??= glbCertificateProvider.GetMauCertificatePassword(nctsHeader.BH_CustomsProfile);

	NctsHeader IIrildesRequestContext.NctsHeader => nctsHeader;
	readonly NctsHeader nctsHeader;

	readonly IGlbCertificateProvider glbCertificateProvider;
	IGlbMauExternalPassword mauPassword;
}

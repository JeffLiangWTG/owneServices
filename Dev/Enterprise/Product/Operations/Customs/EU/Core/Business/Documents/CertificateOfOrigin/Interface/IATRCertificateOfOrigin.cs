using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public interface IATRCertificateOfOrigin : ICertificateOfOrigin
	{
		IATRCertificateDeclaration Declaration { get; }
		IATRBoxItems ATRBoxItemBuilder { get; }
		IATRCertificateItem TotalATRCertificateItem { get; }
		ZBool ShouldAddTotalCertificateItem { get; }
		ZString ARTNumberCaption { get; }
		ZString ARTEuropeanUnionCaption { get; }
	}
}

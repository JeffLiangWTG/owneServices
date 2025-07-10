using CargoWise.Types;

namespace Enterprise.Integration.Certification
{
	public interface IOrgContactCertificateApplicantCreator
	{
		ICertificateApplicant LoadOrCreateFromContact(ZGuid contactPk);
		ICertificateApplicant LoadFromContact(ZGuid contactPk);
	}
}
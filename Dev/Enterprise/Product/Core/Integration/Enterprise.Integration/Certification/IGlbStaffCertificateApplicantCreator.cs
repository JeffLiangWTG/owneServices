using CargoWise.Types;

namespace Enterprise.Integration.Certification
{
	public interface IGlbStaffCertificateApplicantCreator
	{
		ICertificateApplicant LoadOrCreateFromStaff(ZGuid contactPk);
		ICertificateApplicant LoadFromStaff(ZGuid contactPk);
	}
}
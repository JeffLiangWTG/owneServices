using Enterprise.Recruiter.Business;

namespace Enterprise.Certification.Business
{
	public class CertificateApplicantValidation : HRJobApplicantValidation
	{
		public CertificateApplicantValidation(CertificateApplicant parent)
			: base(parent)
		{
		}

		protected new CertificateApplicant Parent => (CertificateApplicant)base.Parent;
	}
}

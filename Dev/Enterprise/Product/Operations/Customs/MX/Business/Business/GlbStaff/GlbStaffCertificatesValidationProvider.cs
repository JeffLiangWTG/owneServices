using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Business
{
	public sealed class GlbStaffCertificatesValidationProvider : ICertificatesValidationProvider
	{
		public MasterFiles.Business.GenRegCertAccredMaintListValidation GetValidation(GenRegCertAccredMaintList parent) => new GenRegCertAccredMaintListValidation(parent);
	}
}

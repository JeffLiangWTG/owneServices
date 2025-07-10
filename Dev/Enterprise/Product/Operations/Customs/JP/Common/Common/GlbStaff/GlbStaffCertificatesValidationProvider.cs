using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Common
{
	public sealed class GlbStaffCertificatesValidationProvider : ICertificatesValidationProvider
	{
		public MasterFiles.Business.GenRegCertAccredMaintListValidation GetValidation(GenRegCertAccredMaintList parent) => new GenRegCertAccredMaintListValidation(parent);
	}
}

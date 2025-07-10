using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Business.Testing
{
	sealed class GlbStaffCertificatesValidationProviderTest : TestCaseWithFactory
	{
		public void TestGetValidation()
		{
			AssertType<GenRegCertAccredMaintListValidation>(new GlbStaffCertificatesValidationProvider().GetValidation(Factory.New<GenRegCertAccredMaintList>()));
		}
	}
}

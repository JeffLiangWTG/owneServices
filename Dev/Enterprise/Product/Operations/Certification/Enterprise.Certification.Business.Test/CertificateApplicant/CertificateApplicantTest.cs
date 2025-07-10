using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Certification.Business.Testing
{
	[TestedType(typeof(CertificateApplicant))]
	sealed class CertificateApplicantTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			AssertNoExceptionThrown(Factory.Save);
		}

		public override void TestFetchForLoad()
		{
			AssertNoExceptionThrown(Factory.Save);
		}
	}
}

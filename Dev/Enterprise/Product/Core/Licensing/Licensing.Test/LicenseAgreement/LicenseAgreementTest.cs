using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Licensing.Test
{
	[TestedType(typeof(LicenseAgreement))]
	class LicenseAgreementTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<LicenseAgreement>();
		}
	}
}

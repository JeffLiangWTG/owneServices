using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACTariffHeader))]
	sealed class CACTariffHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var tariffHeader = Factory.New<CACTariffHeader>();
			AssertEquals("Rates", typeof(CACRateCollection), tariffHeader.Rates.GetType());

			//Delete
			var child = tariffHeader.Rates.AddNew();
			tariffHeader.Delete();
			AssertEquals("CACRateHeader ChildObject deleted", true, child.IsDeleted);
		}

		public void TestLoader()
		{
			const string tariffCode = "4901";
			var tariffHeader = Factory.New<CACTariffHeader>();
			tariffHeader.ZF_TariffCode = tariffCode;
			tariffHeader.ZF_AuthEffectiveDate = ZDateTime.Today.AddDays(-1);
			tariffHeader.ZF_AuthExpiryDate = ZDateTime.Today.AddDays(+1);
			tariffHeader.ZF_Inactive = true;

			tariffHeader = Factory.New<CACTariffHeader>();
			tariffHeader.ZF_TariffCode = tariffCode;
			tariffHeader.ZF_AuthEffectiveDate = ZDateTime.Today.AddDays(-2);
			tariffHeader.ZF_AuthExpiryDate = ZDateTime.Today.AddDays(+1);

			tariffHeader = Factory.New<CACTariffHeader>();
			tariffHeader.ZF_TariffCode = tariffCode;
			tariffHeader.ZF_AuthEffectiveDate = ZDateTime.Today.AddDays(-1);
			tariffHeader.ZF_AuthExpiryDate = ZDateTime.Today.AddDays(+1);

			AssertEquals("CACTariffHeader", tariffHeader, CACTariffHeader.Load(Factory, ZDateTime.Today, tariffCode));
		}
	}
}

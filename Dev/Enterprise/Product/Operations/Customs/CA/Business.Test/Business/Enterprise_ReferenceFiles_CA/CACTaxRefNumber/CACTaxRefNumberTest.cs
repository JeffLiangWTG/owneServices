using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACTaxRefNumber))]
	sealed class CACTaxRefNumberTest : EnterpriseBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var header = Factory.New<CACTaxRefNumHeader>();
			var refNumber = header.RefNumbers.AddNew();
			AssertEquals("Header", header, refNumber.Header);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<CACClassHeader>();
			header.ZA_AreaCode = "A A";
			return header.RefNumbers.AddNew().RefNumbers.AddNew();
		}
	}
}

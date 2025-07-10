using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class IdentificationMeansCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestCY_CodeList()
		{
			var identificationMeansCode = Factory.CreateIdentificationMeansCode();
			var codeList = identificationMeansCode.Lookups.CY_CodeList;
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(codeList, NUnit.Framework.Is.TypeOf<IdentificationMeansList>(), "IdentificationMeansList");
				NUnit.Framework.Assert.That(codeList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<IdentificationMeansList>()), "Cached CY_CodeList");
			});
		}
	}
}

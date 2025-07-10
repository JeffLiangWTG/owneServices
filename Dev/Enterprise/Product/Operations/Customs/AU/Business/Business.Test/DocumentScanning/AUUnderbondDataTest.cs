using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUUnderbondDataTest : TestCaseWithFactory
	{
		public void TestGetEDocsViaUniversalXmlSupport()
		{
			AssertType<AUUnderbondDataEDocsViaUniversalXmlSupport>(new AUUnderbondData().GetEDocsViaUniversalXmlSupport());
		}
	}
}

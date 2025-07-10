using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class RequestedDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestStatusList()
		{
			var requestedDocument = Factory.New<RequestedDocument>();
			var list = requestedDocument.Lookups.StatusList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("RCV, PRP, CAN, OPE"), "Codes");
				NUnit.Framework.Assert.That(list, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<RequestedDocumentStatusList>()), "Cached");
			});
		}
	}
}

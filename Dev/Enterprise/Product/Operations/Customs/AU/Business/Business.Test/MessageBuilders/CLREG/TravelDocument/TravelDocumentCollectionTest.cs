using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(TravelDocumentCollection))]
	sealed class TravelDocumentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var wrapper = new OrgHeaderWrapper(Factory.NewWithValidTestData<OrgHeader>());
			return new TravelDocumentCollection(wrapper.CLREGInfoProvider);
		}
	}
}

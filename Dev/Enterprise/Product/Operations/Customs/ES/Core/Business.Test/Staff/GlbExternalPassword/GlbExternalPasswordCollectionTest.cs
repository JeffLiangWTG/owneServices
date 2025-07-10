using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordCollection))]
	class GlbExternalPasswordCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
			=> new GlbExternalPasswordCollection(Factory.NewWithValidTestData<GlbStaff>());
	}
}

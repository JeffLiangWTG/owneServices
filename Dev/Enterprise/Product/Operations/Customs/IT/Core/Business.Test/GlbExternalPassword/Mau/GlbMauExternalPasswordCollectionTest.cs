using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(GlbMauExternalPasswordCollection))]
sealed class GlbMauExternalPasswordCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var company = Factory.New<GlbCompany>();
		return new GlbMauExternalPasswordCollection(company);
	}
}

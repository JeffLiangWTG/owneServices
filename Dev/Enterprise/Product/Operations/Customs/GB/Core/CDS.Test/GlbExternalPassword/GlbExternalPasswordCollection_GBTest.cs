using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(GlbExternalPasswordCollection_GB))]
	public class GlbExternalPasswordCollection_GBTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbExternalPasswordCollection_GB(Factory.NewWithValidTestData<GlbCompany>());
		}
	}
}

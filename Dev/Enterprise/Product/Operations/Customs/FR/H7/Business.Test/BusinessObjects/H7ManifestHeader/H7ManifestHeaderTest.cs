using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.FR.H7.Business.Testing
{
	[TestedType(typeof(H7ManifestHeader))]
	sealed class H7ManifestHeaderTest : EU.H7.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestLookupsType()
		{
			var header = GetNewBusinessObject() as H7ManifestHeader;
			AssertType<H7ManifestHeaderLookups>(header.Lookups);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<H7ManifestHeader>();
	}
}

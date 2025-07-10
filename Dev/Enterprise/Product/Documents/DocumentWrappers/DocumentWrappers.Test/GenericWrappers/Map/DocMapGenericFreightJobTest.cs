using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentWrappers.GenericWrappers.Map;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class DocMapGenericFreightJobTest : TestCaseWithFactory
	{
		public void TestStaticConstructor()
		{
			SchemaWrapper wrapper = DocMapGenericFreightJob.New(null, Factory);
			AssertNotNull("Static New() should return a SchemaWrapper", wrapper);
			AssertEquals("wrapper.TypeOfWrapperToMap", typeof(FreightWrapper), wrapper.TypeOfWrapperToMap);
		}
	}
}

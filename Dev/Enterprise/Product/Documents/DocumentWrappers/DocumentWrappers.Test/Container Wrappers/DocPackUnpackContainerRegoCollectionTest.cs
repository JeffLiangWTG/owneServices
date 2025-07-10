using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Container_Wrappers
{
	[TestedType(typeof(DocPackUnpackContainerRegoCollection))]
	internal class DocPackUnpackContainerRegoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocPackUnpackContainerRegoCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var container = Factory.New<CFSContainer>();
			return DocPackUnpackContainerRego.New(container, Factory);
		}

		protected override DocPackUnpackContainerRegoCollection GetCollectionToTest()
		{
			return new DocPackUnpackContainerRegoCollection(Factory);
		}
	}
}

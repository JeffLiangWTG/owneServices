using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocIMOBodyCollection))]
	sealed class DocIMOBodyCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocIMOBodyCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocIMOBody("", "", "", "", "");
		}

		protected override DocIMOBodyCollection GetCollectionToTest()
		{
			return new DocIMOBodyCollection(Factory);
		}
	}
}

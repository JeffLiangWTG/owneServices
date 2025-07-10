using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocBaseConsolCollection))]
	sealed class DocBaseConsolCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocBaseConsolCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonConsol consol = CommonConsol.New(Factory);
			return DocBaseConsol.New(consol, Factory);
		}

		protected override DocBaseConsolCollection GetCollectionToTest()
		{
			return new DocBaseConsolCollection(Factory);
		}
	}
}

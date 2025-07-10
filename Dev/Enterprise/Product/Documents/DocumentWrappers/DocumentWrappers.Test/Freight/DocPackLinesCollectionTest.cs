using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocPackLinesCollection))]
	sealed class DocPackLinesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocPackLinesCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var packLine = Factory.New<PackLine>();
			return DocPackLines.New(packLine, Factory);
		}

		protected override DocPackLinesCollection GetCollectionToTest()
		{
			return new DocPackLinesCollection(Factory);
		}
	}
}

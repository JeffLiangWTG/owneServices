using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocSalesTradeLanesCollection))]
	public class DocSalesTradeLanesCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocSalesTradeLanesCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocSalesTradeLane.New(new ViewDocTradeLane(), Factory);
		}

		protected override DocSalesTradeLanesCollection GetCollectionToTest()
		{
			return new DocSalesTradeLanesCollection(Factory);
		}
	}
}

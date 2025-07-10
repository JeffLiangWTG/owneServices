using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocBillOfLadingBodySectionCollection))]
	sealed class DocBillOfLadingBodySectionCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocBillOfLadingBodySectionCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocBillOfLadingBodySection(Factory);
		}

		protected override DocBillOfLadingBodySectionCollection GetCollectionToTest()
		{
			return new DocBillOfLadingBodySectionCollection(Factory);
		}
	}
}

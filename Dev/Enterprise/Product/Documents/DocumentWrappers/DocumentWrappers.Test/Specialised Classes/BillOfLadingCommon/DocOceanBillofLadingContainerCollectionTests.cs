using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocFormedPagesContainerCollection))]
	sealed class DocOceanBillofLadingContainerCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocFormedPagesContainerCollection>
	{
		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var container = Factory.NewWithValidTestData<CommonContainer>();
			DocContainer docContainer = DocContainer.New(container, Factory);
			return new DocFormedPagesContainer(docContainer);
		}

		protected override DocFormedPagesContainerCollection GetCollectionToTest()
		{
			return new DocFormedPagesContainerCollection(Factory);
		}

		#endregion
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	[TestedType(typeof(DocOneOffContainerCollection))]
	sealed class DocOneOffContainerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocOneOffContainerCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var rateOneOffContainers = Factory.New<RateOneOffContainers>();
			return DocOneOffContainer.New(rateOneOffContainers, Factory);
		}

		protected override DocOneOffContainerCollection GetCollectionToTest()
		{
			return new DocOneOffContainerCollection(Factory);
		}
	}
}

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(Export5ASItemWrapperCollection))]
	class Export5ASItemWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<Export5ASItemWrapperCollection>
	{
		protected override Export5ASItemWrapperCollection GetCollectionToTest() => new Export5ASItemWrapperCollection(Enumerable.Empty<Export5ASItem>(), Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection() => new Export5ASItemWrapper(new Export5ASItem(), Factory);
	}
}

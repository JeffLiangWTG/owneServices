using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.AFR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.DocumentWrappers.Testing
{
	[TestedType(typeof(DocJPAFRContainerCollection))]
	sealed class DocJPAFRContainerCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocJPAFRContainerCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var testBill = DocJPAFRBills.New(Factory.New<JPAFRBills>(), Factory);
			var jpAFRContainer = Factory.New<JPAFRContainer>();
			return DocJPAFRContainer.New(jpAFRContainer, testBill, 1, Factory);
		}

		protected override DocJPAFRContainerCollection GetCollectionToTest()
		{
			return new DocJPAFRContainerCollection(Factory);
		}
	}
}

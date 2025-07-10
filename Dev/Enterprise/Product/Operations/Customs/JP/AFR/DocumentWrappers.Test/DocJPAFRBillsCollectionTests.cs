using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.AFR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.DocumentWrappers.Testing
{
	[TestedType(typeof(DocJPAFRBillsCollection))]
	sealed class DocJPAFRBillsCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocJPAFRBillsCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var jpAFRBills = Factory.New<JPAFRBills>();
			return DocJPAFRBills.New(jpAFRBills, Factory);
		}

		protected override DocJPAFRBillsCollection GetCollectionToTest()
		{
			return new DocJPAFRBillsCollection(Factory);
		}
	}
}

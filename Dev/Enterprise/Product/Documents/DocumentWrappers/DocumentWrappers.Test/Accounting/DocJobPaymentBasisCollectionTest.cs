using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocJobPaymentBasisCollection))]
	sealed class DocJobPaymentBasisCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocJobPaymentBasisCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var bo = Factory.NewWithValidTestData<JobPaymentBasis>();
			return DocJobPaymentBasis.New(bo, Factory);
		}

		protected override DocJobPaymentBasisCollection GetCollectionToTest()
		{
			return new DocJobPaymentBasisCollection(Factory);
		}
	}
}

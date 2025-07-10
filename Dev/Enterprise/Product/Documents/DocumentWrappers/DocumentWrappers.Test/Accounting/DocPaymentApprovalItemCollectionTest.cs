using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers
{
	[TestedType(typeof(DocPaymentApprovalItemCollection))]
	sealed class DocPaymentApprovalItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocPaymentApprovalItemCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocPaymentApprovalItem.New(Factory.New<PaymentApprovalItem>(), Factory);
		}

		protected override DocPaymentApprovalItemCollection GetCollectionToTest()
		{
			return new DocPaymentApprovalItemCollection(Factory);
		}
	}
}

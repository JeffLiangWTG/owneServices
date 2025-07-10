using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using NUnit.Framework;
using static Enterprise.DocumentWrappers.DocPaymentApproval;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(FlattenedLineCollection))]
	sealed class DocPaymentApprovalFlattenedLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FlattenedLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return FlattenedLine.New(DocPaymentApprovalItem.New(Factory.New<PaymentApprovalItem>(), Factory), Factory);
		}

		protected override FlattenedLineCollection GetCollectionToTest()
		{
			return new FlattenedLineCollection(new DocPaymentApprovalItemCollection(Factory));
		}
	}
}

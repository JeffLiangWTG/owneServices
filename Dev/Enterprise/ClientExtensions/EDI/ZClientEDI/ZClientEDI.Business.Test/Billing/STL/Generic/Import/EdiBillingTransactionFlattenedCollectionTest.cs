using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiBillingTransactionFlattenedCollection))]
	public class EdiBillingTransactionFlattenedCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EdiBillingTransactionFlattenedCollection>
	{
		protected override EdiBillingTransactionFlattenedCollection GetCollectionToTest()
		{
			return new EdiBillingTransactionFlattenedCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EdiBillingTransactionFlattened(Factory);
		}
	}
}

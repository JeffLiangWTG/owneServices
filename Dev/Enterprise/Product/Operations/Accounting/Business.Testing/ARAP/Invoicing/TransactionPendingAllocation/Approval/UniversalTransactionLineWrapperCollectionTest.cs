using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(UniversalTransactionLineWrapperCollection))]
	public class UniversalTransactionLineWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UniversalTransactionLineWrapperCollection>
	{
		public void TestAllowNewAndRemove()
		{
			var collection = GetCollectionToTest();
			Assert("AllowNew", !collection.AllowNew);
			Assert("AllowRemove", !collection.AllowRemove);
		}

		protected override UniversalTransactionLineWrapperCollection GetCollectionToTest()
		{
			return new UniversalTransactionLineWrapperCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new UniversalTransactionLineWrapper(null, null, null);
		}
	}
}

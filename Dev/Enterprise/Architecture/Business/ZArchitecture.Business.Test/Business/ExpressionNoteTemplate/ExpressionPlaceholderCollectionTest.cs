using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ExpressionPlaceholderCollection))]
	sealed class ExpressionPlaceholderCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExpressionPlaceholderCollection>
	{
		public void TestAddRemoveDisallowed()
		{
			var collection = new ExpressionPlaceholderCollection();
			Assert(!collection.AllowNew);
			Assert(!collection.AllowRemove);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ExpressionPlaceholder(0);
		}

		protected override ExpressionPlaceholderCollection GetCollectionToTest()
		{
			return new ExpressionPlaceholderCollection();
		}
	}
}

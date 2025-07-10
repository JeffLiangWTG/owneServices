using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(SIMADumpingNumberCollection))]
	sealed class SIMADumpingNumberCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SIMADumpingNumberCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, GetCollectionToTest().AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(false, GetCollectionToTest().AllowRemove);
		}

		#region Implementation

		protected override SIMADumpingNumberCollection GetCollectionToTest()
		{
			return new SIMADumpingNumberCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var collection = new SIMADumpingNumberCollection(Factory);
			return collection.AddNew();
		}

		#endregion
	}
}

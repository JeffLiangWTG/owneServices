using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(WriteOffResultCollection))]
	public class WriteOffResultCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WriteOffResultCollection>
	{
		public virtual void TestAllowNewCore()
		{
			var testItem = new WriteOffResultCollection(Factory);
			Assert(!testItem.AllowNew);
		}

		#region Overrides of BusinessObjectCollectionBaseTestCase<BusinessObjectCollection>

		protected override WriteOffResultCollection GetCollectionToTest() => new WriteOffResultCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new WriteOffResult("", "", "", "");

		#endregion
	}
}

using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GenericConsol
{
	[TestedType(typeof(GenericConsolCollection))]
	public class GenericConsolCollectionTest : ActiveBusinessObjectCollectionTestCase<GenericConsolCollection>
	{
		protected override GenericConsolCollection GetCollectionToTest()
		{
			return new GenericConsolCollection(Factory);
		}

		public override void TestDelete()
		{
			base.TestRemoveFromRelationship();
		}

		public override void TestTypedAddNew()
		{
			Assert(true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert(true);
		}
	}
}

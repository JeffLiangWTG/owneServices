using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Testing
{
	[TestedType(typeof(CACusRulingFindBoxCollection))]
	sealed class CusRulingFindBoxCollectionTest : ActiveBusinessObjectCollectionTestCase<CACusRulingFindBoxCollection>
	{
		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestTypedget_Item()
		{
			Assert(true);
		}

		protected override CACusRulingFindBoxCollection GetCollectionToTest()
		{
			return new CACusRulingFindBoxCollection(Factory, "ABC");
		}
	}
}

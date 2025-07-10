using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class SortOrderAsBindableBooleanItemTest : IBindableBooleanItemTest
	{
		protected override IBindableBooleanItem GetNewBusinessObject()
		{
			return new SortOrder("DummyName", "DummyList");
		}

		public override void TestText()
		{
			AssertEquals("DummyName", TestBizObj.Text);
		}

		protected override void AssertBoolValueMatchesConcreteFieldValue(ZBool value)
		{
			AssertEquals(value, ((SortOrder)TestBizObj).Selected);
		}
	}
}

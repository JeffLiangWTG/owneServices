using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public abstract class IBindableBooleanItemTest : TestCaseWithFactory
	{
		protected IBindableBooleanItem TestBizObj;

		protected abstract IBindableBooleanItem GetNewBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			TestBizObj = GetNewBusinessObject();
		}

		public virtual void TestBoolValue()
		{
			TestBizObj.BoolValue = true;
			Assert(TestBizObj.BoolValue);
			Assert((ZBool)(TestBizObj.BoolValueInfo.Value));
			AssertBoolValueMatchesConcreteFieldValue(true);
			TestBizObj.BoolValue = false;
			Assert(!TestBizObj.BoolValue);
			Assert(!(ZBool)(TestBizObj.BoolValueInfo.Value));
			AssertBoolValueMatchesConcreteFieldValue(false);
		}

		protected virtual void AssertBoolValueMatchesConcreteFieldValue(ZBool value)
		{
			Assert(true);
		}

		public abstract void TestText();
	}
}

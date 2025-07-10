using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class GroupByAsBindableBooleanItemTest : IBindableBooleanItemTest
	{
		protected override IBindableBooleanItem GetNewBusinessObject()
		{
			return new GroupBy("DummyName", "DummyList");
		}

		public override void TestText()
		{
			((GroupBy)TestBizObj).DisplayNameLocalizedData = new ResourceStringData("key", "虚拟名称");
			AssertEquals("DummyName", TestBizObj.Text);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				AssertEquals("虚拟名称", TestBizObj.Text);
			}
		}

		protected override void AssertBoolValueMatchesConcreteFieldValue(ZBool value)
		{
			AssertEquals(value, ((GroupBy)TestBizObj).Selected);
		}
	}
}

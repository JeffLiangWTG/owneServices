using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class OptionalTemplateSheetAsBindableBooleanItemTest : IBindableBooleanItemTest
	{
		protected override IBindableBooleanItem GetNewBusinessObject()
		{
			return new OptionalTemplateSheet(Factory, "DummyName");
		}

		public override void TestText()
		{
			AssertEquals("DummyName", TestBizObj.Text);
		}

		protected override void AssertBoolValueMatchesConcreteFieldValue(ZBool value)
		{
			AssertEquals(value, ((OptionalTemplateSheet)TestBizObj).Selected);
		}
	}
}

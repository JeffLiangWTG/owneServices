using CargoWise.Customs.IL.MessageDefinitions.Common;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class CodeTypeWrapperTest : Customs.Business.Testing.DataProviderTestCase<CodeTypeWrapper>
	{
		public void TestNewOrNull()
		{
			CombineAssertions("Null expected for empty values", () =>
			{
				AssertNull("When value is empty space", CodeTypeWrapper.NewOrNull(" "));
				AssertNull("When value is empty", CodeTypeWrapper.NewOrNull(""));
			});

			AssertNotNull("When value is not empty", CodeTypeWrapper.NewOrNull("1"));
		}

		public void TestValue()
		{
			var wrapper = CreateWrapper();
			AssertEquals("VAL", wrapper.Value);
		}

		protected override CodeTypeWrapper GetProvider() => CodeTypeWrapper.NewOrNull("VAL");

		ICodeType CreateWrapper() => GetProvider();
	}
}

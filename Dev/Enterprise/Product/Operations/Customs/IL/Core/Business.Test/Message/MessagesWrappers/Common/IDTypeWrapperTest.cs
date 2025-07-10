using CargoWise.Customs.IL.MessageDefinitions.Common;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class IDTypeWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDTypeWrapper>
	{
		public void TestNewOrNull()
		{
			CombineAssertions("Null expected for empty values", () =>
			{
				AssertNull("When value is empty space", IDTypeWrapper.NewOrNull(" "));
				AssertNull("When value is empty", IDTypeWrapper.NewOrNull(""));
				AssertNull("When value is empty and schemeId is not empty", IDTypeWrapper.NewOrNull("", "1"));
			});

			AssertNotNull("When value is not empty", IDTypeWrapper.NewOrNull("1"));
		}

		public void TestValue()
		{
			var wrapper = CreateWrapper();
			AssertEquals("VAL", wrapper.Value);
		}

		public void TestSchemeID()
		{
			var wrapper = CreateWrapper();
			AssertNull("SchemeID is null", wrapper.SchemeID);

			wrapper = IDTypeWrapper.NewOrNull("VAL", "1");
			AssertEquals("SchemeID is 1", "1", wrapper.SchemeID);
		}

		protected override IDTypeWrapper GetProvider() => IDTypeWrapper.NewOrNull("VAL");

		IIDType CreateWrapper() => GetProvider();
	}
}

using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class IsPrefarenceDocumentIndTypeWrapperTest : Customs.Business.Testing.DataProviderTestCase<IsPrefarenceDocumentIndTypeWrapper>
	{
		public void TestNew()
		{
			AssertNotNull("When value is false", IsPrefarenceDocumentIndTypeWrapper.New(false));
			AssertNotNull("When value is true", IsPrefarenceDocumentIndTypeWrapper.New(true));
		}

		public void TestValue()
		{
			var wrapper = CreateWrapper();
			AssertEquals(false, wrapper.Value);
			testValue = true;
			wrapper = CreateWrapper();
			AssertEquals(true, wrapper.Value);
		}

		protected override IsPrefarenceDocumentIndTypeWrapper GetProvider() => IsPrefarenceDocumentIndTypeWrapper.New(testValue);

		IIsPrefarenceDocumentIndType CreateWrapper() => GetProvider();

		bool testValue;
	}
}

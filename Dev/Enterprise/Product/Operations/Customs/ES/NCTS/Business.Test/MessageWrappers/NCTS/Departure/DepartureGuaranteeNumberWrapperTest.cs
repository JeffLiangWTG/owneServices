using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class DepartureGuaranteeNumberWrapperTest : Customs.Business.Testing.DataProviderTestCase<DepartureGuaranteeNumberWrapper>
	{
		public void TestType()
		{
			AssertEquals("Expected filled Type", Type, wrapper.Type);
		}

		public void TestAccessCode()
		{
			AssertEquals("Expected filled AccessCode", Code, wrapper.AccessCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new DepartureGuaranteeNumberWrapper(Type, Code);
		}
		DepartureGuaranteeNumberWrapper wrapper;

		protected override DepartureGuaranteeNumberWrapper GetProvider() => wrapper;

		const string Type = "Type";
		const string Code = "Code";
	}
}

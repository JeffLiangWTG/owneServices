using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LPOUSCommonContainerIndicatorWrapperTest : WrapperHelperTest<T2LPOUSCommonContainerIndicatorWrapper>
	{
		public void TestIsContainerised()
		{
			var wrapper = new T2LPOUSCommonContainerIndicatorWrapper(true);
			AssertEquals("Expected filled IsContainerised", true, wrapper.IsContainerised);
		}

		protected override T2LPOUSCommonContainerIndicatorWrapper GetProvider() => new T2LPOUSCommonContainerIndicatorWrapper(true);
	}
}

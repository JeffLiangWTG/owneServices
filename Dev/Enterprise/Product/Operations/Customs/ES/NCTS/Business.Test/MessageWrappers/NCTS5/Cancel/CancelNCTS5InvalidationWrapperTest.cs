using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class CancelNCTS5InvalidationWrapperTest : WrapperHelperTest<CancelNCTS5InvalidationWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if ReasonForCancellation is empty string", typeof(ArgumentException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be empty string (\"\").", "reasonForCancellation"), () => GetWrapper(ZString.Empty));
		}

		public void TestIsInitiatedByCustoms()
		{
			AssertEquals("Expected filled IsInitiatedByCustoms with fixed value false", ZBool.False, wrapper.IsInitiatedByCustoms);
		}

		public void TestJustification()
		{
			AssertEquals("Expected filled Justification with value provide to the wrapper", "ReasonForCancellation", wrapper.Justification);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = GetWrapper("ReasonForCancellation");
		}

		CancelNCTS5InvalidationWrapper wrapper;

		CancelNCTS5InvalidationWrapper GetWrapper(ZString reasonForCancellation) => new CancelNCTS5InvalidationWrapper(reasonForCancellation);

		protected override CancelNCTS5InvalidationWrapper GetProvider() => wrapper;
	}
}

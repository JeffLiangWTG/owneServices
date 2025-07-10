using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class ObligationGuaranteeWrapperTest : DataProviderTestCase<ObligationGuaranteeWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new ObligationGuaranteeWrapper(null, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals(1, wrapper.SequenceNumeric);
	}

	public void TestSecurityDetailsCode()
	{
		guarantee.PW_BondType = "0";
		AssertEquals("Security Details", "0", wrapper.SecurityDetailsCode);
	}

	public void TestGuaranteeReferences()
	{
		CombineAssertions(() =>
		{
			var guaranteeReference = wrapper.GuaranteeReferences.Single();
			AssertType<GuaranteeReferenceWrapper>("Type", guaranteeReference);
			AssertEquals("SequenceNumeric", 1, guaranteeReference.SequenceNumeric);
		});
	}

	protected override ObligationGuaranteeWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();

		guarantee = Factory.New<GuaranteeForDeclaration>();
		wrapper = new ObligationGuaranteeWrapper(guarantee, 1);
	}

	GuaranteeForDeclaration guarantee;
	ObligationGuaranteeWrapper wrapper;
}

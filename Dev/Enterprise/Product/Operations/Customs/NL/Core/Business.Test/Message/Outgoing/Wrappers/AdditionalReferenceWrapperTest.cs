using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class AdditionalReferenceWrapperTest : DataProviderTestCase<AdditionalReferenceWrapper>
{
	public void TestConstructor()
	{
		CusReference testCusReference = null;
		AssertExceptionThrown<ArgumentNullException>(() => new AdditionalReferenceWrapper(testCusReference, 1));

		CusSupportingInfo testCusSupportingInfo = null;
		AssertExceptionThrown<ArgumentNullException>(() => new AdditionalReferenceWrapper(testCusSupportingInfo, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, GetProvider().SequenceNumeric);
	}

	public void TestId()
	{
		addInfo.CSI_ReferenceNumber = "INF-123";
		cusReference.CFR_Reference = "SupplyChainActorReference";

		CombineAssertions(() =>
		{
			AssertEquals("Additional Info", "INF-123", GetProvider().Id);
			AssertEquals("Supply Chain Actor Reference", "SupplyChainActorReference", new AdditionalReferenceWrapper(cusReference, 1).Id);
		});
	}

	public void TestCode()
	{
		addInfo.CSI_Code = "INF";
		cusReference.CFR_Code = "FW";

		CombineAssertions(() =>
		{
			AssertEquals("Additional Info", "INF", GetProvider().Code);
			AssertEquals("Supply Chain Actor Reference", "FW", new AdditionalReferenceWrapper(cusReference, 1).Code);
		});
	}

	public void TestCCQualifierCode()
	{
		CombineAssertions(() =>
		{
			AssertNull("Additional Info", GetProvider().CCQualifierCode);
			AssertNull("Supply Chain Actor Reference", new AdditionalReferenceWrapper(cusReference, 1).CCQualifierCode);
		});
	}

	protected override AdditionalReferenceWrapper GetProvider() => new AdditionalReferenceWrapper(addInfo, 1);

	protected override void SetUp()
	{
		base.SetUp();

		addInfo = Factory.New<AdditionalInfo>();
		cusReference = Factory.New<CusReference>();
	}
	AdditionalInfo addInfo;
	CusReference cusReference;
}

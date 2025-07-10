using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class AdditionalInformationWrapperTest : DataProviderTestCase<AdditionalInformationWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new AdditionalInformationWrapper(null, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, wrapperAddInfo.SequenceNumeric);
		AssertEquals("Additional Information Reason for Invalidation", 1, wrapperAddInfoReasonForInvalidation.SequenceNumeric);
	}

	public void TestStatementCode() => CombineAssertions(() =>
	{
		addInfo.CSI_Code = "INF";
		AssertEquals("Additional Info", "INF", wrapperAddInfo.StatementCode);
		AssertNull("Additional Information Reason for Invalidation", wrapperAddInfoReasonForInvalidation.StatementCode);
	});

	public void TestStatementDescription() => CombineAssertions(() =>
	{
		addInfo.CSI_Description = "AdditionalDescription";
		AssertEquals("Additional Info", "AdditionalDescription", wrapperAddInfo.StatementDescription);
		AssertEquals("Additional Information Reason for Invalidation", "Reason For Invalidation", wrapperAddInfoReasonForInvalidation.StatementDescription);
	});

	public void TestCCQualifierCode()
	{
		AssertNull("Additional Info", wrapperAddInfo.CCQualifierCode);
		AssertNull("Additional Information Reason for Invalidation", wrapperAddInfoReasonForInvalidation.CCQualifierCode);
	}

	public void TestStatementTypeCode()
	{
		AssertEquals("Additional Information", "CUS", wrapperAddInfo.StatementTypeCode);
		AssertEquals("Additional Information Reason for Invalidation", "CUS", wrapperAddInfoReasonForInvalidation.StatementTypeCode);
	}

	public void TestPointers()
	{
		AssertEquals(wrapperAddInfo.Pointers, Array.Empty<IPointer>());
	}

	protected override AdditionalInformationWrapper GetProvider() => wrapperAddInfo;

	protected override void SetUp()
	{
		base.SetUp();

		addInfo = Factory.New<AdditionalInfo>();
		wrapperAddInfo = new AdditionalInformationWrapper(addInfo, 1);
		wrapperAddInfoReasonForInvalidation = new AdditionalInformationWrapper("Reason For Invalidation");
	}
	AdditionalInfo addInfo;
	AdditionalInformationWrapper wrapperAddInfo;
	AdditionalInformationWrapper wrapperAddInfoReasonForInvalidation;
}

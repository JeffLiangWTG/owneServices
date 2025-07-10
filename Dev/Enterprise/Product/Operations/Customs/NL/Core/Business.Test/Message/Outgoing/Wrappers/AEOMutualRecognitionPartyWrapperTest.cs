using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class AEOMutualRecognitionPartyWrapperTest : DataProviderTestCase<AEOMutualRecognitionPartyWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new AEOMutualRecognitionPartyWrapper(null, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, wrapper.SequenceNumeric);
	}

	public void TestId()
	{
		AssertEquals("REFJI1", wrapper.Id);
	}

	public void TestRoleCode()
	{
		AssertEquals("FW", wrapper.RoleCode);
	}

	protected override AEOMutualRecognitionPartyWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		var cusReference = Factory.New<CusReference>();
		cusReference.CFR_Code = "FW";
		cusReference.CFR_Reference = "REFJI1";
		wrapper = new AEOMutualRecognitionPartyWrapper(cusReference, 1);
	}
	AEOMutualRecognitionPartyWrapper wrapper;
}

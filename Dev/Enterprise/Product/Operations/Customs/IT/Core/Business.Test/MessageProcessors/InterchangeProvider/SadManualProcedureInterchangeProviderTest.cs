using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SadManualProcedureInterchangeProviderTest : SadInterchangeProviderTest
{
	protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new SadManualProcedureInterchangeProvider(collection);

	protected override ZString ExpectedProcessedMessageStatusCode => EDIMessageStatusList.Codes.Manual;

	protected override ZString ExpectedQueuedInterchangeStatusCode => EDIInterchangeStatusList.Codes.Manual;

	public void TestSetAdditionalInterchangeValuesForTransmit()
	{
		var interchange = Factory.New<EDIInterchange>();
		var interchangeProviderForTest = new SadManualProcedureInterchangeProviderForTest(new NonDependentEDIMessageCollection(Factory));
		AssertEquals("PRE-CONDITION: expected empty", ZGuid.Empty, interchange.EI_SessionGUID);

		interchangeProviderForTest.SetAdditionalInterchangeValuesForTransmitExposed(interchange);
		AssertNotEquals("POST-CONDITION: expected filled", ZGuid.Empty, interchange.EI_SessionGUID);
	}
}

class SadManualProcedureInterchangeProviderForTest : SadManualProcedureInterchangeProvider
{
	public SadManualProcedureInterchangeProviderForTest(NonDependentEDIMessageCollection messages) : base(messages)
	{
	}

	public void SetAdditionalInterchangeValuesForTransmitExposed(EDIInterchange interchange) => SetAdditionalInterchangeValuesForTransmit(interchange);
}

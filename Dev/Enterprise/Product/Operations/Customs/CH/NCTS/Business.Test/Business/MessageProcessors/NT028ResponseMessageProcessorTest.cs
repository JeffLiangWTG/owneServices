using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NTx28ResponseMessageProcessor))]
sealed class NT028ResponseMessageProcessorTest : NTx28ResponseMessageProcessorTest
{
	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarDepartureResponse;

	protected override string GetResponseMessage(string correlationIdentifier, string decision) => TestingData.GetNT028(correlationId: correlationIdentifier, decision: decision);
}

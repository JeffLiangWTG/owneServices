using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NTx28ResponseMessageProcessor))]
sealed class NT528ResponseMessageProcessorTest : NTx28ResponseMessageProcessorTest
{
	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationResponse;

	protected override string GetResponseMessage(string correlationIdentifier, string decision) => TestingData.GetNT528(correlationId: correlationIdentifier, decision: decision);
}

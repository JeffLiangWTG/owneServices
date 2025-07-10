using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NTx04ResponseMessageProcessor))]
sealed class NT504ResponseMessageProcessorTest : NTx04ResponseMessageProcessorTest
{
	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationAmendmentResponse;

	protected override string GetMessage(bool initiatedByCustoms, string mrn, string mrnVersion, string correlationId = null, string decision = null, ZDateTime? decisionDateAndTime = null, ZDate? activationDeadline = null)
		=> TestingData.GetNT504(initiatedByCustoms: initiatedByCustoms, correlationId: correlationId, decision: decision, mrn: mrn, mrnVersion: mrnVersion, decisionDateAndTime: decisionDateAndTime, activationDeadline: activationDeadline);
}

using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT021ResponseMessageProcessor))]
sealed class NT021ArrivalResponseMessageProcessorTest : NT021ResponseMessageProcessorTest
{
	protected override string TestedMovementType => Common.EU.NctsMoveHeaderType.Codes.Arrival;
}

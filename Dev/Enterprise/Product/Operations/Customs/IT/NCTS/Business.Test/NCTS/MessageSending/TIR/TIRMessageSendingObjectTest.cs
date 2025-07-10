using System;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(TIRMessageSendingObject))]
sealed class TIRMessageSendingObjectTest : NctsHeaderDepartureSADMessageSendingObjectTest<TIRMessageSendingObject>
{
	protected override Type ExpectedMessageHeaderType => typeof(TIRHeaderWrapper);

	protected override Type ExpectedMessageLineType => typeof(TIRLineWrapper);

	protected override TIRMessageSendingObject GetMessageSendingObject(NctsHeader nctsHeader) => new TIRMessageSendingObject(nctsHeader);
}

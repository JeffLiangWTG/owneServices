using System;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(TransitMessageSendingObject))]
sealed class TransitMessageSendingObjectTest : NctsHeaderDepartureSADMessageSendingObjectTest<TransitMessageSendingObject>
{
	protected override Type ExpectedMessageHeaderType => typeof(TransitHeaderWrapper);

	protected override Type ExpectedMessageLineType => typeof(TransitLineWrapper);

	protected override TransitMessageSendingObject GetMessageSendingObject(NctsHeader nctsHeader) => new TransitMessageSendingObject(nctsHeader);
}

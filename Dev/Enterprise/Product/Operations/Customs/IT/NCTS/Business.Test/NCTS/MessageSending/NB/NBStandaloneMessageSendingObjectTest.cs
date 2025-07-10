using Enterprise.Customs.IT.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NBStandaloneMessageSendingObject))]
sealed class NBStandaloneMessageSendingObjectTest : NctsHeaderDepartureMessageSendingObjectTest<NBStandaloneMessageSendingObject>
{
	protected override string ExpectedSubType => SADConstants.MessageSubTypes.NB;

	protected override NBStandaloneMessageSendingObject GetMessageSendingObject(NctsHeader nctsHeader) => new NBStandaloneMessageSendingObject(nctsHeader);
}

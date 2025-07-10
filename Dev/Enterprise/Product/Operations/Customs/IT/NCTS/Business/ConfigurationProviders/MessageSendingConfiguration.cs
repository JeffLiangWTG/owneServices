using System.Linq;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class MessageSendingConfiguration : EU.NCTS.Business.MessageSendingConfiguration
{
	protected override EU.NCTS.Business.NctsHeaderMessageSendingObjectParent GetNewNctsHeaderMessageSendingObjectParentCore(EU.NCTS.Business.NctsHeader header)
	{
		var itHeader = (NctsHeader)header;
		return new NctsHeaderMessageSendingObjectParent(itHeader);
	}

	public override CodeDescriptionPairList MessageTypeList(EU.NCTS.Business.NctsHeader header)
	{
		return GetMessageTypeList((NctsHeader)header);
	}

	protected override bool ShowJustificationCore(EU.NCTS.Business.NctsHeader header) => false;

	#region MessageType List

	CodeDescriptionPairList GetMessageTypeList(NctsHeader nctsHeader)
	{
		var result = new CodeDescriptionPairList();
		var messageTypeList = new EDIMessageTypeList();

		new NctsPhase5SendingMessageAuthorizer(nctsHeader)
			.GetAllowedSendingMessageTypes()
			.ToList()
			.ForEach(x => result.AddPair(x, messageTypeList.GetDescriptionFromCode(x)));

		return result;
	}

	#endregion
}

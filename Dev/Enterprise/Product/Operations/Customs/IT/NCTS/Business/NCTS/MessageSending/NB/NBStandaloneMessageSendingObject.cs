using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NBStandaloneMessageSendingObject : NctsHeaderDepartureMessageSendingObject
{
	public NBStandaloneMessageSendingObject(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	protected override IEnumerable<ISadCustomsMessage> GetMessageObjects()
	{
		foreach (var nbMessage in GetNBMessages())
		{
			yield return new NBMessage(nbMessage, this);
		}
	}

	protected override ZString GetMessageSubType() => SADConstants.MessageSubTypes.NB;

	#region Implementation

	IEnumerable<INBMessageSendingObject> GetNBMessages()
	{
		var i = 1;
		foreach (var goodsItem in DepartureMovement.GoodsItems.GetItemsWithSendableGroupedPreviousDocuments())
		{
			yield return new NBStandaloneMessageWrapper(goodsItem, i++);
		}
	}

	#endregion
}

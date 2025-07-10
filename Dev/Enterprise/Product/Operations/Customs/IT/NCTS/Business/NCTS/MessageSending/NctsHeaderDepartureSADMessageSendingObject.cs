using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public abstract class NctsHeaderDepartureSADMessageSendingObject : NctsHeaderDepartureMessageSendingObject, IETMessageSendingObject
{
	protected NctsHeaderDepartureSADMessageSendingObject(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	public IETHeader MessageHeader => GetMessageHeader(NctsHeader);

	protected abstract IETHeader GetMessageHeader(NctsHeader nctsHeader);

	public IEnumerable<IETLine> MessageLines => DepartureMovement
		.GoodsItems
		.Cast<NctsDepartureCargoDesc>()
		.Select(goodsItem => GetMessageLine(goodsItem));

	protected abstract IETLine GetMessageLine(NctsDepartureCargoDesc goodsItem);

	public IEnumerable<INBMessageSendingObject> NBMessages => nbMessages ?? (nbMessages = GetNBMessages());
	IEnumerable<INBMessageSendingObject> nbMessages;

	protected override IEnumerable<ISadCustomsMessage> GetMessageObjects()
	{
		yield return new ETMessage(this);
	}

	protected override ZString GetMessageSubType() => SADConstants.MessageSubTypes.ET;

	protected override ZString GetCombinedCustomsMessageSubType()
	{
		var combinedCustomsMessageSubType = base.GetCombinedCustomsMessageSubType();
		if (NBMessages.Any())
		{
			combinedCustomsMessageSubType += FormattableString.Invariant($" + {SADConstants.MessageSubTypes.NB}");
		}
		return combinedCustomsMessageSubType;
	}

	#region Implementation

	IEnumerable<INBMessageSendingObject> GetNBMessages()
	{
		foreach (var goodsItem in DepartureMovement.GoodsItems.GetItemsWithSendableGroupedPreviousDocuments())
		{
			yield return new NBMessageWrapperWithinOriginalDeclaration(goodsItem);
		}
	}

	#endregion
}

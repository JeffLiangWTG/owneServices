using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class NBNEStandaloneMessageSendingObject : SADMessageSendingObject, ISadOutgoingCustomsMessageGeneratorValuesProvider
{
	public NBNEStandaloneMessageSendingObject(CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
		: base(header, jobDeclarationMessageSendingObjectParent, false)
	{
	}

	#region ISadOutgoingCustomsMessageGeneratorValuesProvider

	IEnumerable<ISadCustomsMessage> ISadOutgoingCustomsMessageGeneratorValuesProvider.GetCustomsMessageObjects()
	{
		var nbStandaloneMessageSendingObject = new NBStandaloneMessageSendingObject(Header, JobDeclarationMessageSendingObjectParent);

		var result = new List<SadCustomsMessage>();
		foreach (var nBMessageSendingObject in nbStandaloneMessageSendingObject.NbMessageSendingObjects)
		{
			result.Add(new NBMessage(nBMessageSendingObject, this));
		}

		return result;
	}

	ICustomsMessageFountainProvider ISadOutgoingCustomsMessageGeneratorValuesProvider.FountainProvider => fountainProvider ?? (fountainProvider = new JobDeclarationFountainProvider(Declaration));
	ICustomsMessageFountainProvider fountainProvider;

	#endregion

	protected override ZString GetMessageSubType() => SADConstants.MessageSubTypes.NBE;

	protected override ZString GetCombinedCustomsMessageSubType()
	{
		return FormattableString.Invariant($"{SADConstants.MessageSubTypes.NB} + NE");
	}
}

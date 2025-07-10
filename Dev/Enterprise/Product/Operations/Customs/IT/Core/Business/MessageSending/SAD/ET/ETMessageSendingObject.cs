using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class ETMessageSendingObject : SADMessageSendingObject, IETMessageSendingObject, ISadOutgoingCustomsMessageGeneratorValuesProvider
{
	public ETMessageSendingObject(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent) : base(entryHeader, jobDeclarationMessageSendingObjectParent, false)
	{
	}

	public ETMessageSendingObject(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent, bool isForDeterminingMessageChangedStatus)
		: base(entryHeader, jobDeclarationMessageSendingObjectParent, isForDeterminingMessageChangedStatus)
	{
	}

	public IETHeader MessageHeader => new ETHeaderWrapper(Header);

	public IEnumerable<IETLine> MessageLines
	{
		get
		{
			foreach (CusEntryLine entryLine in Header.MergedLines)
			{
				yield return new ETLineWrapper(entryLine);
			}
		}
	}

	protected override ZString GetMessageSubType() => SADConstants.MessageSubTypes.ET;

	#region ISadOutgoingCustomsMessageGeneratorValuesProvider

	IEnumerable<ISadCustomsMessage> ISadOutgoingCustomsMessageGeneratorValuesProvider.GetCustomsMessageObjects()
	{
		yield return new ETMessage(this);
	}

	ICustomsMessageFountainProvider ISadOutgoingCustomsMessageGeneratorValuesProvider.FountainProvider => FountainProvider;

	#endregion
}

using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class IMMessageSendingObject : SADMessageSendingObject, IIMMessageSendingObject, ISadOutgoingCustomsMessageGeneratorValuesProvider
{
	public IMMessageSendingObject(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
		: base(entryHeader, jobDeclarationMessageSendingObjectParent, false)
	{
	}

	public IMMessageSendingObject(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent, bool isForDeterminingMessageChangedStatus)
		: base(entryHeader, jobDeclarationMessageSendingObjectParent, isForDeterminingMessageChangedStatus)
	{
	}

	public IIMHeader MessageHeader => IMHeaderWrapperFactory.GetIMHeaderWrapper(Header);

	public IEnumerable<IIMLine> MessageLines
	{
		get
		{
			foreach (CusEntryLine entryLine in Header.MergedLines)
			{
				yield return IMLineWrapperFactory.GetIMLineWrapper(entryLine);
			}
		}
	}

	#region JobDeclarationMessageSendingObject Overrides

	protected override ZString GetMessageSubType() => SADConstants.MessageSubTypes.IM;

	#endregion

	#region ISadOutgoingCustomsMessageGeneratorValuesProvider

	IEnumerable<ISadCustomsMessage> ISadOutgoingCustomsMessageGeneratorValuesProvider.GetCustomsMessageObjects()
	{
		yield return new IMMessage(this);
	}

	ICustomsMessageFountainProvider ISadOutgoingCustomsMessageGeneratorValuesProvider.FountainProvider => FountainProvider;

	#endregion
}

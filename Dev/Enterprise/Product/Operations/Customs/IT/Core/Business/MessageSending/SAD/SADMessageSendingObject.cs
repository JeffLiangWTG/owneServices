using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public abstract class SADMessageSendingObject : JobDeclarationMessageSendingObject, ISadMessageSendingObject
{
	protected SADMessageSendingObject(CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent, bool isForDeterminingMessageChangedStatus)
		: base(header, jobDeclarationMessageSendingObjectParent)
	{
		Argument.GreaterThan(header.MergedLines.Count, 0, nameof(header.MergedLines));
		IsForDeterminingMessageChangedStatus = isForDeterminingMessageChangedStatus;
	}

	protected bool IsForDeterminingMessageChangedStatus { get; }

	public IEnumerable<INBMessageSendingObject> NBMessages
	{
		get
		{
			return IsForDeterminingMessageChangedStatus
				? GetNBMessagesForDeterminingMessageChangedStatus()
				: GetNBMessages();
		}
	}

	protected override ZString GetCombinedCustomsMessageSubType()
	{
		var result = base.GetCombinedCustomsMessageSubType();
		if (MergedLinesWithGroupedPreviousDocuments.Any())
		{
			result += FormattableString.Invariant($" + {SADConstants.MessageSubTypes.NB}");
		}
		return result;
	}

	protected ICustomsMessageFountainProvider FountainProvider => fountainProvider ?? (fountainProvider = new JobDeclarationFountainProvider(Declaration));
	ICustomsMessageFountainProvider fountainProvider;

	#region ISadMessageSendingObject

	ZBool ISadMessageSendingObject.FallbackProcedure => CustomsMessageSendingMode == CustomsMessageSendingModeList.Codes.FallbackProcedure;

	ZString ISadMessageSendingObject.DeclarantTaxNumber => CustomsCredentialHelper.GetAccountFromInternalCode(Declaration.JE_CustomsProfile)?.DeclarantTaxNumber ?? ZString.Empty;

	#endregion

	#region Implementation

	protected IEnumerable<CusEntryLine> MergedLinesWithGroupedPreviousDocuments => Header.MergedLines.Cast<CusEntryLine>().Where(line => line.GroupedPreviousDocuments.Any());

	IEnumerable<INBMessageSendingObject> GetNBMessages()
	{
		foreach (CusEntryLine entryLine in MergedLinesWithGroupedPreviousDocuments)
		{
			yield return new NBMessageWrapperWithinOriginalDeclaration(entryLine);
		}
	}

	IEnumerable<INBMessageSendingObject> GetNBMessagesForDeterminingMessageChangedStatus()
	{
		foreach (CusEntryLine entryLine in MergedLinesWithGroupedPreviousDocuments.Where(x => x.ZG_NBStatus == EntryLineCustomsStatusList.Codes.Approved || x.ZG_NBStatus == EntryLineCustomsStatusList.Codes.Sent))
		{
			yield return new NBMessageWrapperWithinOriginalDeclaration(entryLine);
		}
	}

	#endregion
}

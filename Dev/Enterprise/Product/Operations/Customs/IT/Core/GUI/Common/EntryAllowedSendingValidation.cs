using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.GUI;

public class EntryAllowedSendingValidation
{
	public EntryAllowedSendingValidation(IEnumerable<IEntryMessageSendingObjectInfo> sendingObjectsInfo)
	{
		this.sendingObjectsInfo = Argument.NotNull(sendingObjectsInfo, nameof(sendingObjectsInfo));
	}

	readonly IEnumerable<IEntryMessageSendingObjectInfo> sendingObjectsInfo;

	public bool CheckAndWarn()
	{
		var nonSendableSendingObjects = sendingObjectsInfo.Where(x => !x.EntryStatusAllowsSending);
		return !nonSendableSendingObjects.Any() || WarnUserAndAskConfirmation(nonSendableSendingObjects);
	}

	#region Implementation

	bool WarnUserAndAskConfirmation(IEnumerable<IEntryMessageSendingObjectInfo> nonSendableSendingObjects)
	{
		var warningMessage = GetWarningMessage(nonSendableSendingObjects);
		var caption = Res.GetString("B9E1B342-9CA3-4F51-804D-7A85F9FC123E", "Warning - Entry Status Screening");
		var confirmationString = Res.GetString("BE735DEA-4D90-4361-BCB1-59E0DCA8FB7F", "I AM AWARE THAT RESENDING COULD RESULT IN A DUPLICATED DECLARATION");
		return Globals.Message.ShowConfirmation(warningMessage, caption, confirmationString, MessageBoxIcon.Warning) == DialogResult.OK;
	}

	string GetWarningMessage(IEnumerable<IEntryMessageSendingObjectInfo> nonSendableSendingObjects)
	{
		var warningMessageBuilder = new ZStringBuilder();
		warningMessageBuilder.Append(Res.GetString("DD75B3D1-F260-4F92-BD37-763A11AD845C", @"The following entries were already sent and are already registered or waiting for messages from Customs.
Resending these entries could result in duplicated declarations.

Entries:"));

		foreach (var sendingObject in nonSendableSendingObjects)
		{
			var effectiveStatus = GetEffectiveStatus(sendingObject);
			warningMessageBuilder.Append(FormattableString.Invariant($"{sendingObject.EntryReference}: {effectiveStatus}"));
		}
		return warningMessageBuilder.ToStringWithNewLineBetweenAppends();
	}

	ZString GetEffectiveStatus(IEntryMessageSendingObjectInfo sendingObject)
	{
		var entryCustomsStatus = sendingObject.EntryCustomsStatus;
		return entryCustomsStatus.IsEmpty
			? sendingObject.EntryMessageStatus
			: entryCustomsStatus;
	}

	#endregion
}

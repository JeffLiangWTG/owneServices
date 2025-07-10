using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class HighHouseContPivotNoManager
	{
		public HighHouseContPivotNoManager(CusEntryHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		readonly CusEntryHeader header;

		public void AssignLineNumbers()
		{
			var maxNumber = header.HighHouseContPivotNo;
			var currentNumber = maxNumber + 1;

			foreach (PackingGroup pack in header.PackingGroups)
			{
				if (pack.CR_HouseContainerNumber == 0 || pack.CR_HouseContainerNumber > maxNumber)
				{
					pack.CR_HouseContainerNumber = currentNumber++;
				}
			}
		}

		void ReAssignHouseContainerNumberIfNeeded(ZShort maxLineNumber, EDIMessage lodgeMessage)
		{
			if (maxLineNumber > 0 && lodgeMessage != null)
			{
				var packGroups = header.PackingGroups.Cast<PackingGroup>();
				var packingGroupsBeforeLodgeMessage = packGroups.Where(x => x.CR_SystemCreateTimeUtc < lodgeMessage.EM_SystemCreateTimeUtc && x.CR_HouseContainerNumber == 0).OrderBy(x => x.CR_SystemCreateTimeUtc);
				var reAssignedHouseContainerNumber = packGroups.MaxOrDefault(c => c.CR_HouseContainerNumber) + 1;
				foreach (PackingGroup pack in packingGroupsBeforeLodgeMessage)
				{
					pack.CR_HouseContainerNumber = reAssignedHouseContainerNumber++;
				}
			}
		}

		public void ReCalculateIfNeeded()
		{
			if (!header.EntryNumber.IsEmpty)
			{
				var lastClearLogTimeUtc = GetLastClearLogTimeUtc();
				if (!lastClearLogTimeUtc.IsEmpty)
				{
					var maxLineNumberInfo = GetMaxLineNumberFromMessages(lastClearLogTimeUtc);
					UpdateValueIfNew(maxLineNumberInfo.maxLineNumber, true);
					ReAssignHouseContainerNumberIfNeeded(maxLineNumberInfo.maxLineNumber, maxLineNumberInfo.lodgeMessage);
				}
			}
		}

		ZDateTime GetLastClearLogTimeUtc()
		{
			var logs = header.Logs
				.Find(c => c.SL_SE_NKEvent == Events.StatusChangeCode
						&& (c.SL_Reference == CustomsEntryStatus.ClearFormalLodge.Code || c.SL_Reference == CustomsEntryStatus.ClearAmendment.Code))
				.OrderByDescending(c => c.SL_PostedTimeUtc);

			return logs.FirstOrDefault()?.SL_PostedTimeUtc ?? ZDateTime.Empty;
		}

		(ZShort maxLineNumber, EDIMessage lodgeMessage) GetMaxLineNumberFromMessages(ZDateTime lastClearLogTimeUtc)
		{
			var messages = header.Messages
				.Where(c => c.EM_MessageType == CMRMessage.CMRMessageTypes.IMD && c.EM_SystemCreateTimeUtc <= lastClearLogTimeUtc)
				.OrderByDescending(c => c.EM_SystemCreateTimeUtc);

			var hasClearMessage = false;

			foreach (var message in messages)
			{
				switch (message.EM_MessageSubType)
				{
					case CMRMessage.MessageSubTypes.Original:
					case CMRMessage.MessageSubTypes.Amendment:
					case CMRMessage.MessageSubTypes.Change:
					case CMRMessage.MessageSubTypes.Request:
					case CMRMessage.MessageSubTypes.ReplaceHeader:
					case CMRMessage.MessageSubTypes.Withdraw:
						{
							if (message.IsTransmitMessage && hasClearMessage)
							{
								var maxLineNumber = (message as CMRIMDMessage)?.MaxLineNumber ?? ZShort.Zero;
								if (maxLineNumber > 0)
								{
									return (maxLineNumber, message);
								}
							}

							break;
						}

					case CMRMessage.ManifestResponseSubTypes.Clear:
					case CMRMessage.CMRMessageTypes.IMD:
						{
							if (!message.IsTransmitMessage)
							{
								hasClearMessage = true;
							}

							break;
						}
				}
			}

			return (ZShort.Zero, null);
		}

		public void Update()
		{
			var maxContPivotNo = header.PackingGroups.Cast<PackingGroup>().MaxOrDefault(c => c.CR_HouseContainerNumber);
			UpdateValueIfNew(maxContPivotNo, false);
		}

		void UpdateValueIfNew(ZShort newValue, bool isRecalculation)
		{
			bool changed = false;
			var oldValue = header.HighHouseContPivotNo;

			var action = isRecalculation ? "recalculated" : "updated";
			var logText = $"{nameof(header.HighHouseContPivotNo)} {action} from {oldValue} to {newValue}";

			var maxValue = Math.Max(oldValue, newValue);
			if (oldValue != maxValue)
			{
				header.AddInfo.ZA_HighHouseContPivotNo_Hidden = maxValue;
				changed = true;

				if (isRecalculation)
				{
					logText = $"{logText}. Status:{header.CH_Status} EntryStatus:{header.CH_EntryStatus}";
				}
			}

			if (!isRecalculation || changed)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				header.Logs.AddNew(Events.EditedARecord, logText);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}
	}
}

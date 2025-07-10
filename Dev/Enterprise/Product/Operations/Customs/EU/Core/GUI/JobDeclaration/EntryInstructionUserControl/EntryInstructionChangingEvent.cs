using System;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI
{
	public class EntryInstructionChangingEvent : EventArgs
	{
		public EntryInstructionChangingEvent(CusEntryInstruction oldEntryInstruction, CusEntryInstruction newEntryInstruction)
		{
			NewEntryInstruction = newEntryInstruction;
			OldEntryInstruction = oldEntryInstruction;
		}

		public CusEntryInstruction NewEntryInstruction { get; }
		public CusEntryInstruction OldEntryInstruction { get; }
	}
}

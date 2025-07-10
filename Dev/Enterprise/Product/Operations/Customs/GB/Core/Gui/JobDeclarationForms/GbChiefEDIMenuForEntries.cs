using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.GUI
{
	public class GbChiefEDIMenuForEntries : ChiefEDIMenu
	{
		public GbChiefEDIMenuForEntries()
			: base(showToggleTrainingFlag: false)
		{ }

		protected override void SendToChiefViaCsp(Customs.Business.CusdecMessageFunction howToSend)
		{
			if (Entry == null)
			{
				Globals.Message.Show("Please select one row first", "Select Row", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.DialogResult.OK);
			}
			else
			{
				base.Declaration = Entry.Declaration;
				foreach (CusEntryHeader e in Declaration.ActiveEntryHeaders)
				{
					e.DoNotSendMessageForThisEntryBecauseSendingForIndividualEntries = e != Entry;
				}

				base.SendToChiefViaCsp(howToSend);  // Send message

				// Restore all flags in case user presses main declaration-level option before closing form
				foreach (CusEntryHeader e in Declaration.ActiveEntryHeaders)
				{
					e.DoNotSendMessageForThisEntryBecauseSendingForIndividualEntries = false;
				}
			}
		}

		protected override bool HasEligibleEntryToCancel
		{
			get
			{
				return Entry != null && !Entry.DoNotSendMessageForThisEntryBecauseSendingForIndividualEntries && !Entry.EntryNumber.IsEmpty;
			}
		}

		CusEntryHeader entry;
		public CusEntryHeader Entry
		{
			get { return entry; }
			set
			{
				entry = value;
				this.Declaration = entry.Declaration;
			}
		}
	}
}

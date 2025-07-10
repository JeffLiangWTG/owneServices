using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CNJobDeclarationMessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
	{
		public CNJobDeclarationMessageSendingObjectValidation(CNJobDeclarationMessageSendingObject parent)
			: base(parent)
		{ }

		CusEntryHeader EntryHeader => fEntryHeader ?? (fEntryHeader = Parent.Header as CusEntryHeader);
		CusEntryHeader fEntryHeader;

		protected override void CheckEntryStatusIsWesternEuropean()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1098:DoNotInitializeStringFieldsWithResGetString", Justification = "Baseline")]
		protected override void CheckShouldSend()
		{
			base.CheckShouldSend();
			if (Parent.ShouldSend && !EntryHeader.MergedLines.Any())
			{
				Parent.ShouldSendInfo.AddError(Res.GetString("5282602C-1EFA-4F48-BFBC-C8C55CDDD069", "This entry has no lines. Please check if there are any Invoice Lines linked to the corresponding Entry Instruction."));
			}
		}
	}
}

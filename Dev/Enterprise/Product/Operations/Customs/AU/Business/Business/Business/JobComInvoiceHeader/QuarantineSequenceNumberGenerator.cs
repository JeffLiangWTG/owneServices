using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineSequenceNumberGenerator : ShortSequenceNumberGenerator
	{
		public QuarantineSequenceNumberGenerator(JobComInvoiceHeader header)
			: base(header, (x) => { return !(x as JobComInvoiceLine).QuarantineExDocLine.HasQuarantineLineBeenAccepted(); })
		{
			this.header = header;
		}

		protected JobComInvoiceHeader header;

		protected override int SequenceStartingNumberCore => HighestLineNoSent < short.MaxValue ? (HighestLineNoSent + 1) : 0;

		protected override void RecalculateWhenAboutToBeDetachedOrDeletedCore(IShortSequenceNumberLine lineToBeDeleted)
		{
			if (!IsSuspended && !(lineToBeDeleted as JobComInvoiceLine).QuarantineExDocLine.HasQuarantineLineBeenAccepted())
			{
				base.RecalculateWhenAboutToBeDetachedOrDeletedCore(lineToBeDeleted);
			}
		}

		protected override void RecalculateWhenAddedCore(IShortSequenceNumberLine lineToAdd)
		{
			if (!IsSuspended)
			{
				using (GetLineNumberSuspender())
				{
					lineToAdd.SequenceNumber = EnsureValidSequenceNumber(HighestLineNo + 1);
				}
			}
		}

		protected override void RecalculateWhenRenumberedCore(IShortSequenceNumberLine lineBeingRenumbered, ZShort oldValue)
		{
			//The user shouldn't be able to change the line number, but the base functionality should definitely not occur.
		}

		int HighestLineNo => Math.Max(HighestLineNoSent, HighestLineNoExisting);
		int HighestLineNoSent => header?.QuarantineExDocHeader?.QH_QuarantineMessageMaxLine ?? 0;
		int HighestLineNoExisting => Lines.Any() ? Lines.Max(x => x.SequenceNumber) : 0;
	}
}

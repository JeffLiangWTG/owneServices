using CargoWise.Types;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EntryHeaderAmendmentAndEntryLineDeletionTest : Customs.Business.Testing.EntryHeaderAmendmentAndEntryLineDeletionTest
	{
		protected override ZString AmendmentClearedCode => CustomsEntryStatus.ClearAmendment.Code;

		protected override ZString AmendmentPendingCode => CustomsEntryStatus.AwaitingAmendment.Code;
	}
}

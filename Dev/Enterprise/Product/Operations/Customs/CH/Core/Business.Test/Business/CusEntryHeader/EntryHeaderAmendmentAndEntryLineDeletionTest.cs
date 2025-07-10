using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class EntryHeaderAmendmentAndEntryLineDeletionTest : Customs.Business.Testing.EntryHeaderAmendmentAndEntryLineDeletionTest
{
	protected override ZString AmendmentClearedCode => CHLogicalStatusList.Codes.Accepted;
	protected override ZString AmendmentPendingCode => CHLogicalStatusList.Codes.Sent;
}

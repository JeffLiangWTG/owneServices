using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.GUI
{
	public class AmendmentSnapshotManagementMenuItemsCreator : Customs.GUI.AmendmentSnapshotManagementMenuItemsCreator
	{
		public AmendmentSnapshotManagementMenuItemsCreator(Customs.GUI.EDIMenu menu) : base(menu)
		{
		}

		protected override bool ShouldRevertToLastClearedMenuItemForTypicalEntryHeaderCore(Customs.Business.CusEntryHeader entryHeader)
		{
			var frEntryHeader = (CusEntryHeader)entryHeader;
			return frEntryHeader.CanBeRevertedToLastBAE;
		}
	}
}

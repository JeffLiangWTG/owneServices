using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class EntryUniqueTransactionIdentifierGridContextMenuItemComponentTest : UniqueTransactionIdentifierGridContextMenuItemComponentAbstractTest
{
	protected override GridContextMenuItemComponent<ITEDIMessage> GetNewContextMenuItemComponent(ZGrid grid)
		=> new EntryUniqueTransactionIdentifierGridContextMenuItemComponent(grid);
}

using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.GUI.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class NctsUniqueTransactionIdentifierGridContextMenuItemComponentTest : UniqueTransactionIdentifierGridContextMenuItemComponentAbstractTest
{
	protected override GridContextMenuItemComponent<ITEDIMessage> GetNewContextMenuItemComponent(ZGrid grid)
		=> new NctsUniqueTransactionIdentifierGridContextMenuItemComponent(grid);
}

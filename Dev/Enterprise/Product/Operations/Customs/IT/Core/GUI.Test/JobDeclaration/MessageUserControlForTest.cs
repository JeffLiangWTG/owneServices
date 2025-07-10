using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class MessageUserControlForTest : MessageUserControl
{
	public ZBool DynamicLayoutAppliedExposed => base.DynamicLayoutApplied;

	public IPanelLayoutProvider GetNewEntryDetailsPanelLayoutExposed() => base.GetNewEntryDetailsPanelLayout();
}

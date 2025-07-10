using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[SuppressFormDesignerAnalysis]
	class ZNoteDescriptionDropEdit : ZGridDropEdit
	{
		protected override ZDropButton NewDropButton() => new ZNoteDescriptionDropButton();

		protected override int CodeBoxWidth => IsDropButtonVisible ? base.CodeBoxWidth : DropButton.Width;
	}
}

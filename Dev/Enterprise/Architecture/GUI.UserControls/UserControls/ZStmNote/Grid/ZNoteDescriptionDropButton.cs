using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[SuppressFormDesignerAnalysis]
	internal class ZNoteDescriptionDropButton : ZGridDropButton
	{
		protected override void ShowDropDown(bool lButtonDown)
		{
			if (IsVisible) // don't allow drop down if DropEdit is masquerading as a textbox ;)
			{
				base.ShowDropDown(lButtonDown);
			}
		}
	}
}

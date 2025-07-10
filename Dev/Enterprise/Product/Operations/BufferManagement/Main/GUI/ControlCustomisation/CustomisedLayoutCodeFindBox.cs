using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class CustomisedLayoutCodeFindBox : ZCodeFindBox
	{
		public CustomisedLayoutCodeFindBox()
		{
			ReadOnly = false;
			ShowDescriptionBox = false;
			AutoSize = false;
			ShouldResize = false;
			PopupButtonReadonlyCanBeDifferent = true;
		}
	}
}

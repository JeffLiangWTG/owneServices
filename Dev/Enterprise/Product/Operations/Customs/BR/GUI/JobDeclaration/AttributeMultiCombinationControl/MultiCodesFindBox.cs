using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.BR.GUI
{
	public class MultiCodesFindBox : ZGridFindBox
	{
		protected override IFindBoxPopup GetNewPopupForm()
		{
			return new MultiCodesSelectForm(List as CodeDescriptionPairList);
		}
	}
}

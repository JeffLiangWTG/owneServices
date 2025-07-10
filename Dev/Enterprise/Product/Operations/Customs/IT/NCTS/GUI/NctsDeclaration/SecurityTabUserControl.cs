using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.GUI;

public partial class SecurityTabUserControl : EU.NCTS.GUI.SecurityTabUserControl
{
	public SecurityTabUserControl()
	{
		InitializeComponent();
	}

	protected override PlaceOfUnloadingControlType GetPlaceOfUnloadingControlType(NctsHeader header) => PlaceOfUnloadingControlType.CodeFindBox;
}

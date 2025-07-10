namespace Enterprise.Customs.GB.GUI;

public class CustomsOfficesUserControl : EU.GUI.CustomsOfficesUserControl
{
	public CustomsOfficesUserControl() : base()
	{
		CustomsOfficeFindBox.CaptionResourceString = null;
	}

	protected override bool IsOverrideLabelByFriendlyNameCore => false;
}

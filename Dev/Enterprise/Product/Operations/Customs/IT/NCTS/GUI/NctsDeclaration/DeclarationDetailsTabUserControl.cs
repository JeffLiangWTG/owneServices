namespace Enterprise.Customs.IT.NCTS.GUI;

public partial class DeclarationDetailsTabUserControl : EU.NCTS.GUI.DeclarationDetailsTabUserControl
{
	public DeclarationDetailsTabUserControl()
	{
		InitializeComponent();
		SetSameCaptionResourceStringForControlResultDateLimit();
		SetSpecificControlsVisibility();
	}

	#region Implementation

	void SetSameCaptionResourceStringForControlResultDateLimit()
	{
		ControlResultDateLimitForNormalDeclarationDateEdit.CaptionResourceString = ControlResultDateLimitDateEdit.CaptionResourceString;
	}

	void SetSpecificControlsVisibility()
	{
		AuthorisedLocationCodeTextBox.Visible = false;
	}

	#endregion
}

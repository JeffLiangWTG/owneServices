using Enterprise.Customs.IT.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.GUI;

public partial class DeclarationStatusTabUserControl : EU.NCTS.GUI.DeclarationStatusTabUserControl
{
	public DeclarationStatusTabUserControl()
	{
		InitializeComponent();
		SetReadOnlyControls();
	}

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);
		if (dataSource is NctsHeader nctsHeader)
		{
			ConfigureCustomsGroupBoxVisibility(nctsHeader);
		}
	}

	#region Implementation

	void SetReadOnlyControls()
	{
		ArrivalDateEdit.ReadOnly = AcceptanceDateEdit.ReadOnly = RegistrationDateEdit.ReadOnly = ReleaseDateEdit.ReadOnly = true;
	}

	void ConfigureCustomsGroupBoxVisibility(NctsHeader nctsHeader)
	{
		CustomsGroupBox.Visible = nctsHeader.IsDepartureMovement;
	}

	#endregion
}

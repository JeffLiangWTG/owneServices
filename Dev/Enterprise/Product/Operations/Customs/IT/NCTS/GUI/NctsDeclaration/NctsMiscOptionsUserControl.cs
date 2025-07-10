using Enterprise.Customs.IT.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.GUI;

public partial class NctsMiscOptionsUserControl : EU.NCTS.GUI.MiscOptionsUserControl
{
	public NctsMiscOptionsUserControl()
	{
		InitializeComponent();
	}

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);
		if (dataSource is NctsHeader header)
		{
			SetDepartureMovementRelatedFieldVisibility(header.IsDepartureMovement);
		}
	}

	#region Implementation

	void SetDepartureMovementRelatedFieldVisibility(bool isDepartureMovement)
	{
		UseElectronicFolderCheckBox.Visible = isDepartureMovement;
		DeferralGroupBox.Visible = isDepartureMovement;
		WarehouseGroupBox.Visible = isDepartureMovement;
		ParticipantDropEdit.Visible = isDepartureMovement;
	}

	#endregion
}

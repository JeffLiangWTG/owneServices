using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI;

public partial class AEJobDeclarationUserControl : BaseCustomsDeclarationUserControl
{
	public AEJobDeclarationUserControl()
	{
		InitializeComponent();
	}

	protected override void HandleDeclarationControlVisibilityChangedCore()
	{
		base.HandleDeclarationControlVisibilityChangedCore();
		JE_ContainerModeBoundDropDownEdit.Visible = false;
	}

	protected override void SetupForDeclaration(BaseJobDeclaration declaration)
	{
		base.SetupForDeclaration(declaration);
		SetShipmentDetailsLayoutPanelAllowOverlaps();
	}

	void SetShipmentDetailsLayoutPanelAllowOverlaps()
	{
		if (ShipmentDetailsLayoutPanel.FindSingleOrDefault<Control>("ColumnSeparator0") is { } columnSeparator0)
		{
			foreach (var controlName in ControlsToAllowOverlap)
			{
				if (ShipmentDetailsLayoutPanel.FindSingleOrDefault<Control>(controlName) is { } control)
				{
					control.AllowOverlap(columnSeparator0);
				}
			}
		}
	}

	List<string> ControlsToAllowOverlap => new List<string>
	{
		nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.HouseBillParcelPostTextBox),
		nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsOriginUserControl),
		nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsFinalDestinationUserControl),
		nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.GoodsDescriptionTextBox),
		nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.OwnersReferenceTextBox),
		nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsIncoTermsUserControl),
		nameof(Customs.GUI.ShipmentDetailsControlBag.Instance.ShipmentDetailsScreeningUserControl),
	};

	public new Business.JobDeclaration JobDeclaration
	{
		get { return (Business.JobDeclaration)base.JobDeclaration; }
		set { base.JobDeclaration = value; }
	}
}

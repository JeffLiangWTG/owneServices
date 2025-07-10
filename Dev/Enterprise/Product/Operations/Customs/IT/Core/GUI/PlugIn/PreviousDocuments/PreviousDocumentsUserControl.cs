using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.GUI.Common;

namespace Enterprise.Customs.IT.GUI;

public partial class PreviousDocumentsUserControl : EU.GUI.PlugIn.PreviousDocumentsUserControl, IPreviousDocumentsForm
{
	public PreviousDocumentsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
		Helper.InitializeTariffCodeFindBox(TariffCodeFindBox);
	}

	protected override void InitializeGridLayoutCore()
	{
		using (PreviousDocumentsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			base.InitializeGridLayoutCore();
			Helper.InitializeGridLayout(PreviousDocumentsGrid);
		}
	}

	protected override void ChangeControlsVisibility()
	{
		base.ChangeControlsVisibility();
		ToggleUCC6DependantControlsVisibility();
	}

	#region IPreviousDocumentsForm Members

	ZString IPreviousDocumentsForm.GetUniversalTariffType()
	{
		var type = ZString.Empty;
		if (CurrentDataItem is JobDeclaration declaration)
		{
			type = declaration.JE_MessageType;
		}
		else if (CurrentDataItem is OrgSupplierPart)
		{
			var parent = TopLevelControl.Controls.Find("EUOrgSupplierPartFormCustomsControl", true).FirstOrDefault() as OrgSupplierPartFormCustomsControl;
			if (parent != null)
			{
				type = parent.GetTariffType();
			}
		}
		return type;
	}

	ZDateTime IPreviousDocumentsForm.GetEffectiveDate() => CurrentDataItem is JobDeclaration declaration ? declaration.DateOfValuation : ZDateTime.Today;

	#endregion

	#region Implementation

	PreviousDocumentsFormInitializer Helper => helper ?? (helper = new PreviousDocumentsFormInitializer(this));
	PreviousDocumentsFormInitializer helper;

	void ToggleUCC6DependantControlsVisibility()
	{
		if (CurrentDataItem is JobDeclaration declaration)
		{
			var isUCC6 = declaration.Configuration.IsUCC6(declaration);
			PackageQuantityTextBox.Visible = !isUCC6;
			PackageQuantityUCC6CalcDropEdit.Visible = isUCC6;
		}
	}

	#endregion
}

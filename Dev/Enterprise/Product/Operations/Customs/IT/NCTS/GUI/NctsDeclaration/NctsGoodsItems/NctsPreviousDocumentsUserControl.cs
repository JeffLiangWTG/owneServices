using CargoWise.Types;
using Enterprise.Customs.IT.GUI.Common;

namespace Enterprise.Customs.IT.NCTS.GUI;

public partial class NctsPreviousDocumentsUserControl : EU.NCTS.GUI.PreviousDocumentsUserControl, IPreviousDocumentsForm
{
	public NctsPreviousDocumentsUserControl()
	{
		InitializeComponent();
		Helper.InitializeTariffCodeFindBox(TariffCodeFindBox);
	}

	protected override void InitializeGridLayout()
	{
		base.InitializeGridLayout();
		Helper.InitializeGridLayout(PreviousDocumentsGrid);
	}

	#region IPreviousDocumentsForm Members

	ZString IPreviousDocumentsForm.GetUniversalTariffType() => Universal.Constants.TariffTypes.Import;

	ZDateTime IPreviousDocumentsForm.GetEffectiveDate() => ZDateTime.Today;

	#endregion

	#region Implementation

	PreviousDocumentsFormInitializer Helper => helper ?? (helper = new PreviousDocumentsFormInitializer(this));
	PreviousDocumentsFormInitializer helper;

	#endregion
}

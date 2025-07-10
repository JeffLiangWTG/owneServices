using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class CusEntryLineExtendedInformationsUserControl : ZUserControl
{
	public CusEntryLineExtendedInformationsUserControl()
	{
		InitializeComponent();

		if (!DesignModeFinder.IsDesigning)
		{
			AddDynamicLayoutUserControl();
		}
	}

	#region Dynamic Layout

	void AddDynamicLayoutUserControl()
	{
		DynamicQuantitiesPanel.UpdateLayout(QuantitiesDetailsPanelLayout);
	}

	IPanelLayoutProvider QuantitiesDetailsPanelLayout => quantitiesDetailsPanelLayout ?? (quantitiesDetailsPanelLayout = GetNewCusEntryLineExtendedInformationQuantitiesLayout());
	IPanelLayoutProvider quantitiesDetailsPanelLayout;

	protected virtual IPanelLayoutProvider GetNewCusEntryLineExtendedInformationQuantitiesLayout() => new CusEntryLineExtendedInformationQuantitiesLayout();

	#endregion
}

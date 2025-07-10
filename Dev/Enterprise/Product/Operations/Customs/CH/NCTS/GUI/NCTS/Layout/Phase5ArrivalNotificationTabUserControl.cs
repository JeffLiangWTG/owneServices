using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public partial class Phase5ArrivalNotificationTabUserControl : EU.NCTS.GUI.Phase5ArrivalNotificationTabUserControl
{
	public Phase5ArrivalNotificationTabUserControl()
	{
		InitializeComponent();
	}

	const string IsVisibleForBindingString = nameof(ZGroupBox.IsVisibleForBinding);

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);

		MovementReferenceNumbersGroupBox.DataBindings.RemoveBinding(IsVisibleForBindingString);

		if (dataSource != null)
		{
			MovementReferenceNumbersGroupBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, "ArrivalMovementHeader.MultipleMRNIndicator", false, DataSourceUpdateMode.Never));
		}
	}
}

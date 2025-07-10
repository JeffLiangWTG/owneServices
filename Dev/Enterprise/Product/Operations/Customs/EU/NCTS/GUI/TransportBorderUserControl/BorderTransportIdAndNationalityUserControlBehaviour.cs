using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	sealed class BorderTransportIdAndNationalityUserControlBehaviour : ControlBehaviour<BorderTransportIdAndNationalityUserControl, NctsDepartureMovementHeader>
	{
		protected override void UpdateBehaviourCore(BorderTransportIdAndNationalityUserControl control, NctsDepartureMovementHeader movementHeader)
		{
			control.BorderTransportIdTextBox.CharacterCasing = movementHeader?.RequireTransportUpperCaseID ?? false
				? CharacterCasing.Upper
				: CharacterCasing.Normal;

			var isSeaExportTransportMode = movementHeader?.IsSeaExportTransportMode ?? false;
			control.BorderTransportIdTextBox.Visible = !isSeaExportTransportMode;
			control.BorderTransportIdTextBox.UpdateCaption();

			control.VesselCodeFindBox.Visible = isSeaExportTransportMode;
			control.VesselCodeFindBox.UpdateCaption();
		}
	}
}

using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI.JSInterop;

namespace CargoWise.Windows.UI;

partial class KForm
{
#if DEBUG
	public ControlInformationOverlayComponent Overlay { get; init; }

	internal Control FindControlByWinzorControlId(string winzorControlId)
	{
		return winzorControlId == WinzorControlId ? this : FindDescendantByWinzorControlId(winzorControlId);
	}
#endif

	public bool EnableTranslationFeedbackHighlight { get; set; }
}

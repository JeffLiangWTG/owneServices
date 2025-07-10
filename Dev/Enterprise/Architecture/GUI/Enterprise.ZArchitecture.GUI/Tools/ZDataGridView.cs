using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI;

internal class ZDataGridView : DataGridView
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
	public virtual ContextMenu ExtensionMenu { get; set; }
}

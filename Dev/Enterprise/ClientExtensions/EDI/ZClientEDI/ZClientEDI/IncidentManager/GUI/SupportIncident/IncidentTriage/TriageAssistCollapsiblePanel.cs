using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class TriageAssistCollapsiblePanel : ZCollapsiblePanel
	{
		protected override bool IsHorizontal => base.IsHorizontal || Dock == DockStyle.Fill;
	}
}

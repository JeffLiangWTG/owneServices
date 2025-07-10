using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class TriageAssistGrid : ZGrid
	{
		protected override void NotifyColumnEditStart()
		{
			// we don't want editing to set has changes at all.
		}
	}
}

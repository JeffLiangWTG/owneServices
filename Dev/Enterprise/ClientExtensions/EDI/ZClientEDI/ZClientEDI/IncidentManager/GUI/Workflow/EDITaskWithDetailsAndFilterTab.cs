
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class EDITaskWithDetailsAndFilterTab : TaskWithDetailsAndFilterTab
	{
		protected override TaskWithDetailsAndFilterControl GetNewFilterControl()
		{
			return new EDITaskWithDetailsAndFilterControl();
		}
	}
}

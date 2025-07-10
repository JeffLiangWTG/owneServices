using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public class JobRelatedDiagramsUserControlProvider : IJobRelatedDiagramsUserControlProvider
	{
		public IJobRelatedDiagramsUserControl GetUserControl()
		{
			return new JobRelatedDiagramsUserControl();
		}
	}
}

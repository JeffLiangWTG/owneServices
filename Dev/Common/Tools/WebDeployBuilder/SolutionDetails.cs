
namespace WebDeployBuilder
{
	public enum SolutionType
	{
		Legacy,
		Published
	}

	/// <summary>
	/// Details of a solution to be deployed to clients
	///		SourceFolder - where the solution is located
	///		DestinationName - name of the target folder on deployment machine
	/// </summary>
	public struct SolutionDetails
	{
		public string SourceFolder;
		public string DestinationName;
		public List<CopyTask> CopyTasks;
		public SolutionType SolutionType;
	
		public SolutionDetails(string sourceFolder, string destinationName, List<CopyTask> copyTasks, SolutionType solutionType)
		{
			SourceFolder = sourceFolder;
			DestinationName = destinationName;
			CopyTasks = copyTasks;
			SolutionType = solutionType;
		}
	}
}

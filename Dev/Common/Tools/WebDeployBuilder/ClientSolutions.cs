using System.Collections;

namespace WebDeployBuilder
{
	/// <summary>
	/// Collection of solutions that will be distributed as a client-specific 
	/// web deployment package
	/// </summary>
	public class ClientSolutions
	{
		public string ClientName;
		protected ArrayList solutions;

		public ClientSolutions(string clientName)
		{
			this.ClientName = clientName;
			solutions = new ArrayList();
		}

		public void AddSolution(SolutionDetails solution)
		{
			solutions.Add(solution);
		}

		public SolutionDetails[] Solutions
		{
			get
			{
				SolutionDetails[] result = new SolutionDetails[solutions.Count];
				for (int i = 0; i < solutions.Count; i++)
				{
					result[i] = (SolutionDetails)solutions[i]!;
				}
				return result;
			}
		}
	}
}

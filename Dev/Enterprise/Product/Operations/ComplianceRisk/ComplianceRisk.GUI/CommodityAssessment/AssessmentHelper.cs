using System;
using System.Threading.Tasks;
using Enterprise.ComplianceRisk.Business;

namespace Enterprise.ComplianceRisk.GUI
{
	public class AssessmentHelper : IAssessmentHelper
	{
		public AssessmentHelper(Func<Task> initializeAssessment)
		{
			InitializeAssessmentFunc = initializeAssessment;
		}

		public Func<Task> InitializeAssessmentFunc { get; }

		public async Task InitializeAssessment()
		{
			if (InitializeAssessmentFunc != null)
			{
				await InitializeAssessmentFunc.Invoke();
			}
		}
	}
}

using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobCriticalValidation : CriticalValidation<Job>
	{
		public JobCriticalValidation(Job job) : base(job)
		{
		}

		protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			foreach (var result in base.OnSavingOnlyCriticalChecks())
			{
				yield return result;
			}

			yield return CheckBranchNotEmpty();
			yield return CheckDepartmentNotEmpty();
		}

		CriticalValidationResult CheckBranchNotEmpty()
		{
			if (Parent.JH_GB.IsEmpty)
			{
				return new CriticalValidationResult(CriticalValidationErrorType.BranchOfJobShouldNotBeNull,
					CriticalValidationMessageTemplate.BranchOfJobShouldNotBeNull,
					Parent.GetJobCreationWithEmptyBranchMessage());
			}
			return null;
		}

		CriticalValidationResult CheckDepartmentNotEmpty()
		{
			if (Parent.JH_GE.IsEmpty)
			{
				return new CriticalValidationResult(CriticalValidationErrorType.DepartmentOfJobShouldNotBeNull,
					CriticalValidationMessageTemplate.DepartmentOfJobShouldNotBeNull,
					Parent.GetJobCreationWithEmptyDepartmentMessage());
			}
			return null;
		}
	}
}

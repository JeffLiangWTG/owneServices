using System;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IJCSUserContextProvider : IDisposable
	{
		void SetUserContext(ZGuid companyPK, ZGuid branchPK);

		ZGuid BranchPK { get; }

		ZGuid CompanyPK { get; }
	}

	public class JCSUserContextProvider : IDisposable, IJCSUserContextProvider
	{
		public void SetUserContext(ZGuid companyPK, ZGuid branchPK)
		{
			// if current branch is not same as the branchPK parameter, then change UserContext
			if (branchPK != BranchPK)
			{
				//Dispose the current one first
				DisposeCurrentContext();
				CompanyPK = companyPK;
				BranchPK = branchPK;
				//Create the new context for the new branch
				CurrentUserContext = DisposableEnvironment.ForBranch(branchPK.ToGuid());
			}
		}

		public void Dispose()
		{
			DisposeCurrentContext();
		}

		void DisposeCurrentContext()
		{
			CurrentUserContext?.Dispose();
		}

		public ZGuid BranchPK { get; private set; }

		public ZGuid CompanyPK { get; private set; }

		IDisposable CurrentUserContext { get; set; }
	}
}

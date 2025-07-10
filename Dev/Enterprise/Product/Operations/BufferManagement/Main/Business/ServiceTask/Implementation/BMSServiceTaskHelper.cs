using System;
using System.Linq;
using CargoWise.Application;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.BufferManagement.Business
{
	/// <summary>
	/// The purpose of this class is to facilitate the transition of all service tasks
	/// to "CanRunInAnyBranch = true"
	/// For many PAVE service tasks we don't care which branch is being used,
	/// So we are picking a random branch.
	/// </summary>
	public class BMSServiceTaskHelper : IBMSServiceTaskHelper
	{
		#region Fallback Branch

		public IDisposable GetTemporaryEnvironmentForServiceTaskBranch()
		{
			return Environment.DisposableEnvironment.ForBranch(DefaultServiceTaskBranchPK);
		}

		Guid DefaultServiceTaskBranchPK => defaultServiceTaskBranchPK ?? (defaultServiceTaskBranchPK = GetDefaultServiceTaskBranchPK()).Value;
		Guid? defaultServiceTaskBranchPK;

		public static Guid GetDefaultServiceTaskBranchPK()
		{
			var branchFound = ObjectFactory
				.Get<IServiceManagerQuerier>()
				.TryGetServiceTaskBranchPK(TransferRuleRunnerServiceTask.Code, out var branchPK);

			if (branchFound)
			{
				return branchPK;
			}

			// We really do not care which branch is being used in this service task
			// So we just get any.
			return GlbCompany.CurrentCompany.ActiveBranches.Cast<GlbBranch>().Select(b => b.PK).FirstOrDefault().ToGuid();
		}

		#endregion
	}
}

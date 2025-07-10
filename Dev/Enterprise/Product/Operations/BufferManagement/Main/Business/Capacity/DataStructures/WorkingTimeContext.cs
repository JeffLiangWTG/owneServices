using System;
using CargoWise.CalendarArithmetic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class WorkingTimeContext
	{
		#region Create

		public static WorkingTimeContext Create(BusinessObjectFactory factory)
		{
			return new WorkingTimeContext(GlbBranch.GetCurrentBranch(factory), GlbDepartment.GetCurrentDepartment(factory), null, null);
		}

		public static WorkingTimeContext Create(IBranchDepartmentProvider branchDepartmentProvider, GlbStaff resource = null, BusinessObjectFactory factory = null)
		{
			factory = factory ?? resource?.Factory ?? (branchDepartmentProvider as IFactoryProvider)?.Factory;

			if (factory == null)
			{
				throw new InvalidOperationException("No factory provided to initialise the WorkingTimeContext with. We need a resource, provider, or explicit factory supplied.");
			}

			var branch = branchDepartmentProvider.GetBranch(factory);
			var department = branchDepartmentProvider.GetDepartment(factory);

			ReportBranchAndOrDepartmentErrors(branch, department, branchDepartmentProvider);

			return new WorkingTimeContext((GlbBranch)branch ?? GlbBranch.GetCurrentBranch(factory), (GlbDepartment)department ?? GlbDepartment.GetCurrentDepartment(factory), resource, branchDepartmentProvider);
		}

		public static WorkingTimeContext CreateWithOverride(IBranchDepartmentProvider branchDepartmentProvider, IGlbBranch branchOverride, IGlbDepartment departmentOverride)
		{
			var factory = (branchDepartmentProvider as IFactoryProvider)?.Factory
				?? throw new InvalidOperationException("No factory provided to initialise the WorkingTimeContext with. We need a resource, provider, or explicit factory supplied.");

			var branch = branchOverride ?? branchDepartmentProvider.GetBranch(factory);
			var department = departmentOverride ?? branchDepartmentProvider.GetDepartment(factory);

			ReportBranchAndOrDepartmentErrors(branch, department, branchDepartmentProvider);

			return new WorkingTimeContext((GlbBranch)branch ?? GlbBranch.GetCurrentBranch(factory), (GlbDepartment)department ?? GlbDepartment.GetCurrentDepartment(factory), null, branchDepartmentProvider);
		}

		public static WorkingTimeContext CreateForServiceTask(IBranchDepartmentProvider branchDepartmentProvider)
		{
			var factory = ((IFactoryProvider)branchDepartmentProvider).Factory;
			var branch = branchDepartmentProvider.GetBranch(factory);
			var department = branchDepartmentProvider.GetDepartment(factory);
			if (branch == null || department == null)
			{
				return null;
			}
			else
			{
				return new WorkingTimeContext((GlbBranch)branch, (GlbDepartment)department, null, branchDepartmentProvider);
			}
		}

		#endregion

		WorkingTimeContext(GlbBranch branch, GlbDepartment department, GlbStaff resource, IBranchDepartmentProvider provider)
		{
			if (branch == null || department == null)
			{
				throw new WorkingTimeNotAvailableException(branch, department, provider);
			}

			Branch = branch;
			Department = department;
			Resource = resource;
		}

		public GlbBranch Branch { get; }
		public GlbDepartment Department { get; }
		public GlbStaff Resource { get; }

		public ZDateTime GetCurrentLocalTime(BusinessObjectFactory factory, bool checkHomeBranchAndDepartment = false)
		{
			return ToLocalTime(ZDateTime.UtcNow, factory, checkHomeBranchAndDepartment);
		}

		public ZDateTime ToLocalTime(ZDateTime dateTime, BusinessObjectFactory factory, bool checkHomeBranchAndDepartment = false)
		{
			GlbBranch branch = null;

			if (checkHomeBranchAndDepartment)
			{
				branch = Resource?.HomeBranch;
			}

			if (branch == null)
			{
				branch = factory == Branch.Factory
					? Branch
					: factory.Load<GlbBranch>(Branch.PK);
			}

			return dateTime.ToLocalBranchTime(branch);
		}

		internal ZoneMultiplierProvider ZoneMultiplierProvider
		{
			get { return zoneMultiplierProvider ?? (zoneMultiplierProvider = new ZoneMultiplierProvider()); }
		}

		ZoneMultiplierProvider zoneMultiplierProvider;

		public IWorkTimeArithmetic GetWorkTimeArithmetic(BusinessObjectFactory factory, bool checkHomeBranchAndDepartment = false)
		{
			var homeDeptPK = checkHomeBranchAndDepartment ? Resource?.HomeDepartment?.PK : null;
			var homeBranchPK = checkHomeBranchAndDepartment ? Resource?.HomeBranch?.PK : null;

			var deptPK = homeDeptPK ?? Department?.PK ?? GlbDepartment.GetCurrentDepartment(factory).PK;
			var branchPK = homeBranchPK ?? Branch?.PK ?? GlbBranch.GetCurrentBranch(factory).PK;
			var resourcePK = Resource?.PK ?? ZGuid.Empty;

			return CapacityCalculationWorkTimeArithmetic.GetInstance(factory, deptPK, branchPK, resourcePK);
		}

		static void ReportBranchAndOrDepartmentErrors(IGlbBranch branch, IGlbDepartment department, IBranchDepartmentProvider provider)
		{
			if (branch == null && department == null)
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"WorkingTimeContext was initialised with a provider that provided a null branch and a null department. Provider type: [{provider.GetType().Name}]."));
			}
			else if (branch == null)
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"WorkingTimeContext was initialised with a provider that provided a null branch. Provider type: [{provider.GetType().Name}]."));
			}
			else if (department == null)
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"WorkingTimeContext was initialised with a provider that provided a null department. Provider type: [{provider.GetType().Name}]."));
			}
		}

		public override bool Equals(object obj)
		{
			return obj is WorkingTimeContext ctx &&
			ctx.Resource?.PK == this.Resource?.PK &&
			ctx.Branch.PK == this.Branch.PK &&
			ctx.Department.PK == this.Department.PK;
		}

		public override int GetHashCode()
		{
			return Resource?.GetHashCode() ?? 0 ^ Branch.PK.GetHashCode() ^ Department.PK.GetHashCode();
		}
	}
}

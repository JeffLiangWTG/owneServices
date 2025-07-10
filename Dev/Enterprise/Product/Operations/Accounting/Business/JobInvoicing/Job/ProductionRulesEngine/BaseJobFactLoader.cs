using System;
using System.Collections.Generic;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public abstract class BaseJobFactLoader
	{
		protected BaseJobFactLoader(RulesContextType rulesContextType)
		{
			RulesContextType = rulesContextType;
		}

		protected RulesContextType RulesContextType { get; }
		protected EnvironmentFact EnvironmentFact { get; private set; }
		protected IOrganisationWithMainAddressFact LocalClientFact { get; private set; }
		protected StaffFact SalesRepFact { get; private set; }
		protected JobBranchDepartmentFact JobBranchDepartmentFact { get; private set; }

		public IEnumerable<IInputFact> GetFacts(IJobInvoicingPlugIn parentPlugin, ICompany loginCompany, IBranch loginBranch, IDepartment loginDepartment)
		{
			var factAccumulator = new FactAccumulator();
			GenerateCommonJobFacts(factAccumulator, parentPlugin, loginCompany, loginBranch, loginDepartment);
			return GetFactsCore(parentPlugin, factAccumulator);
		}

		protected abstract IEnumerable<IInputFact> GetFactsCore(IJobInvoicingPlugIn parentPlugin, FactAccumulator factAccumulator);

		void GenerateCommonJobFacts(FactAccumulator factAccumulator, IJobInvoicingPlugIn parentPlugin, ICompany loginCompany, IBranch loginBranch, IDepartment loginDepartment)
		{
			var invoiceSupporter = parentPlugin.InvoicingSupporter;
			var branchFact = factAccumulator.CreateUniqueBranchFact(loginBranch);
			var departmentFact = factAccumulator.CreateUniqueDepartmentFact(loginDepartment);
			EnvironmentFact = new EnvironmentFact(loginCompany, branchFact, departmentFact);
			LocalClientFact = CreateOrNull(invoiceSupporter.Job.LocalCharges, factAccumulator.CreateUniqueOrganisationFact);
			SalesRepFact = CreateOrNull(invoiceSupporter.Job.RepSales, factAccumulator.CreateUniqueStaffFact);

			if (RulesContextType == RulesContextType.JobBillingTaxBranchDefaulting)
			{
				var jobBranchFact = CreateOrNull(invoiceSupporter.Job.Branch, factAccumulator.CreateUniqueBranchFact);
				var jobDepartmentFact = CreateOrNull(invoiceSupporter.Job.Department, factAccumulator.CreateUniqueDepartmentFact);

				JobBranchDepartmentFact = new JobBranchDepartmentFact(jobBranchFact, jobDepartmentFact);
			}
		}

		protected TFact CreateOrNull<TFact, TBizo>(TBizo bizo, Func<TBizo, TFact> accumulate)
		{
			return bizo == null ? default : accumulate(bizo);
		}
	}
}

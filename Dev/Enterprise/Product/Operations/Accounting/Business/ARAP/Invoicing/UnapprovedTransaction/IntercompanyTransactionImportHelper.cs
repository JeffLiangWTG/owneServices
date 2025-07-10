using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class IntercompanyTransactionImportHelper
	{
		[SuppressMessage("Microsoft.Performance", "CA1815: Override equals and operator equals on value types", Justification = "It is not used for comparison.")]
		public struct GetAPBranchResult
		{
			public bool IsIntercompanyInvoice
			{
				get { return isIntercompanyInvoice; }
				set
				{
					isIntercompanyInvoice = value;
					if (!IsIntercompanyInvoice && BranchForIntercompanyImport != null)
					{
						BranchForIntercompanyImport = null;
					}
				}
			}
			bool isIntercompanyInvoice;

			public GlbBranch BranchForIntercompanyImport
			{
				get { return branchForIntercompanyImport; }
				set
				{
					branchForIntercompanyImport = value;
					if (!IsIntercompanyInvoice && BranchForIntercompanyImport != null)
					{
						IsIntercompanyInvoice = true;
					}
				}
			}
			GlbBranch branchForIntercompanyImport;
		}

		public static GetAPBranchResult GetAPBranchForAutoImport(InvoicingBase transaction, Func<GlbCompany, bool> isTransactionValidToImportInCompany)
		{
			Argument.NotNull(isTransactionValidToImportInCompany, nameof(isTransactionValidToImportInCompany));

			return GetAPBranch(transaction.Factory, transaction, null, true, isTransactionValidToImportInCompany);
		}

		static GetAPBranchResult GetAPBranch(BusinessObjectFactory factory, IIntercompanyBranchDepartmentDeciderSource transaction, InvoicingBase apInvoicingBase, bool isAutoImport,
			Func<GlbCompany, bool> isTransactionValidToImportInCompany)
		{
			Func<Tuple<bool, GlbBranch>> getJobRelatedBranch = () =>
			{
				GlbBranch branch = null;
				var isApplicable = false;
				if (apInvoicingBase != null && apInvoicingBase.Lines.Count > 0 && apInvoicingBase.IsJobRelated && apInvoicingBase.LinesHaveSameBranch
					&& apInvoicingBase.Lines.Cast<InvoicingLineBase>().All(x => x.Job != null && x.Job.JH_GB.IsValid))
				{
					branch = apInvoicingBase.Lines[0].Branch;
					isApplicable = true;
				}

				return Tuple.Create(isApplicable, branch);
			};

			return GetAPBranch(factory, transaction, apInvoicingBase != null, isAutoImport, isTransactionValidToImportInCompany, getJobRelatedBranch);
		}

		static GetAPBranchResult GetAPBranch(BusinessObjectFactory factory, IIntercompanyBranchDepartmentDeciderSource transaction, bool isAPInvoiceCreated, bool isAutoImport,
				Func<GlbCompany, bool> isTransactionValidToImportInCompany,
				Func<Tuple<bool, GlbBranch>> getJobRelatedBranch)
		{
			Argument.NotNull(isTransactionValidToImportInCompany, nameof(isTransactionValidToImportInCompany));
			Argument.NotNull(transaction, nameof(transaction));
			Argument.NotNull(getJobRelatedBranch, nameof(getJobRelatedBranch));

			var result = new GetAPBranchResult();

			var branchFilter = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, transaction.AH_OH);
			branchFilter.AddToFilter(new ZQuery(GlbBranchSchema.GB_IsActive, true));
			var foundBranches = factory.Load<GlbBranch>(branchFilter);

			var foundCompanies = foundBranches.GroupBy(x => x.GB_GC).Select(x => x.First().Company).ToHashSet();

			var companyFilter = new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, transaction.AH_OH);
			companyFilter.AddToFilter(new ZQuery(GlbCompanySchema.GC_IsActive, true));
			var companiesByOrgProxy = factory.Load<GlbCompany>(companyFilter);
			foundCompanies.UnionWith(companiesByOrgProxy);

			result.IsIntercompanyInvoice = foundCompanies.Any(x => isTransactionValidToImportInCompany(x)); //we need to calculate IsIntercompanyInvoice always even when foundCompanies.Count() != 1 as IsIntercompanyInvoice triggers error message.

			if (result.IsIntercompanyInvoice && foundCompanies.Count == 1)
			{
				if (foundBranches.Length == 1)
				{
					result.BranchForIntercompanyImport = foundBranches.First();
				}
				else
				{
					var foundCompany = foundCompanies.First();
					IJobCostingPlugIn consol = null;
					Tuple<bool, GlbBranch> jobRelatedBranchResult;
					if (transaction.IsConsolInvoice && (consol = transaction.Consol) != null)
					{
						result.BranchForIntercompanyImport = ConsolInvoiceBranchDepartmentCalculator.GetBranch(consol, foundCompany.PK, factory);
					}
					else if (isAutoImport && !isAPInvoiceCreated && transaction.IsJobRelated)
					{
						result.BranchForIntercompanyImport = foundCompany.FirstActiveBranch;
					}
					else if ((jobRelatedBranchResult = getJobRelatedBranch()).Item1)
					{
						result.BranchForIntercompanyImport = jobRelatedBranchResult.Item2;
					}
				}
			}

			return result;
		}

		static Tuple<ZGuid, string> GetAPDepartmentWhenLogIntoAPCompany(BusinessObjectFactory factory, IIntercompanyBranchDepartmentDeciderSource transaction, InvoicingBase apInvoicingBase)
		{
			Func<Tuple<bool, Tuple<ZGuid, string>>> getJobRelatedDepartmentPK = () =>
			{
				var departmentPK = ZGuid.Empty;
				var error = "";
				var isApplicable = false;

				if (apInvoicingBase != null && apInvoicingBase.Lines.Count > 0 && apInvoicingBase.IsJobRelated)
				{
					isApplicable = true;

					var lineWithJob = apInvoicingBase.Lines.Cast<InvoicingLineBase>().Where(x => x.Job != null && x.Job.JH_GE.IsValid).OrderBy(x => x.AL_Sequence).FirstOrDefault();

					if (lineWithJob != null)
					{
						departmentPK = lineWithJob.AL_GE;
					}
					else
					{
						apInvoicingBase.AddRowError(GetErrorMessageWhenTransactionLinesHaveInvalidDepartment());
					}
				}

				return Tuple.Create(isApplicable, Tuple.Create(departmentPK, error));
			};

			return GetAPDepartmentWhenLogIntoAPCompany(factory, transaction, getJobRelatedDepartmentPK,
				consol => Res.GetString("7E18ADBA-0ECB-4415-8D13-62FF5C7328BD", "Intercompany Transaction cannot be imported as Department cannot be set with reference to the Consolidation '{0}'.", consol.JK_UniqueConsignRef));
		}

		static Tuple<ZGuid, string> GetAPDepartmentWhenLogIntoAPCompany(BusinessObjectFactory factory, IIntercompanyBranchDepartmentDeciderSource transaction,
				Func<Tuple<bool, Tuple<ZGuid, string>>> getJobRelatedDepartmentPK,
				Func<IJobCostingPlugIn, string> getConsolDepartmentNotFoundError)
		{
			Argument.NotNull(getJobRelatedDepartmentPK, nameof(getJobRelatedDepartmentPK));

			var departmentPK = ZGuid.Empty;
			var error = "";
			IJobCostingPlugIn consol = null;
			Tuple<bool, Tuple<ZGuid, string>> jobRelatedDepartmentResult;

			if (transaction.IsConsolInvoice && (consol = transaction.Consol) != null)
			{
				departmentPK = ConsolInvoiceBranchDepartmentCalculator.GetDepartment(consol, factory);
				if (!departmentPK.IsValid)
				{
					error = getConsolDepartmentNotFoundError(consol);
				}
			}
			else if ((jobRelatedDepartmentResult = getJobRelatedDepartmentPK()).Item1)
			{
				departmentPK = jobRelatedDepartmentResult.Item2.Item1;
				error = jobRelatedDepartmentResult.Item2.Item2;
			}
			else
			{
				departmentPK = transaction.AH_GE;
			}

			return Tuple.Create(departmentPK, error);
		}

		public static GlbDepartment GetDepartmentToLogIntoAPCompany(GlbBranch branch, GlbDepartment department)
		{
			Argument.NotNull(branch, nameof(branch));
			Argument.NotNull(department, nameof(department));

			GlbDepartment departmentForLogin = null;

			var isDepartmentValid = branch.IsDepartmentAllowed(department.PK);
			if (isDepartmentValid)
			{
				departmentForLogin = department;
			}
			else
			{
				var firstDepartment = branch.AllowedDepartments.OrderBy(x => x.DepartmentCode).FirstOrDefault();
				departmentForLogin = firstDepartment?.Department;
			}

			return departmentForLogin ?? department;
		}

		public static void SetBranchAndDepartmentOnAPInvoice(InvoicingBase transaction, InvoicingBase apInvoicingBase, bool isAutoImport)
		{
			Argument.NotNull(apInvoicingBase, nameof(apInvoicingBase));
			Argument.NotNull(apInvoicingBase, nameof(apInvoicingBase));

			var departmentResult = GetAPDepartmentWhenLogIntoAPCompany(transaction.Factory, transaction, apInvoicingBase);
			var departmentPK = departmentResult.Item1;
			var departmentError = departmentResult.Item2;
			apInvoicingBase.AH_GE = departmentPK;
			if (!string.IsNullOrEmpty(departmentError))
			{
				apInvoicingBase.AddRowError(departmentError);
			}

			var isJobRelated = transaction.IsJobRelated && !(transaction.IsConsolInvoice && transaction.Consol != null);

			if ((isAutoImport && isJobRelated) || !isAutoImport)
			{
				var branch = GetAPBranch(transaction.Factory, transaction, apInvoicingBase, isAutoImport, x => true).BranchForIntercompanyImport;
				if (branch != null)
				{
					if (!apInvoicingBase.IsAmendingCreditNote)
					{
						if ((!isAutoImport && branch.GB_GC == GlbCompany.CurrentCompany.PK) || isAutoImport)
						{
							apInvoicingBase.AH_GB = branch.PK;
						}
						else
						{
							apInvoicingBase.AH_GB = ZGuid.Empty;
							return;
						}
					}
				}
				else
				{
					if (isAutoImport)
					{
						apInvoicingBase.AddRowError(Res.GetString("c5f732e3-c8eb-4029-90cf-3a82800a9e47", "Intercompany Transaction cannot be imported as Transaction Branch or Company cannot be set with reference to the invoice debtor organization proxy."));
					}
					else
					{
						apInvoicingBase.AH_GB = ZGuid.Empty;
					}
					return;
				}
			}

			foreach (var line in apInvoicingBase.Lines.Cast<InvoicingLineBase>().Where(x => !x.AL_JH.IsValid))
			{
				line.AL_GB = apInvoicingBase.AH_GB;
			}
		}

		public static void SetBranchAndDepartmentOnUXMLImport(TransactionPendingAllocation transaction, IntercompanyBranchDepartmentDeciderSourceWrapper sourceWrapper)
		{
			var departmentResult = GetAPDepartmentWhenLogIntoAPCompany(transaction.Factory, sourceWrapper,
				() => Tuple.Create(sourceWrapper.IsJobRelated && !sourceWrapper.JobDepartmentPK.IsEmpty, Tuple.Create(sourceWrapper.JobDepartmentPK, "")),
				consol => Res.GetString("BC9D3841-B766-439F-8750-FDD5D21B4A79", "Intercompany Transaction Department is used because Department cannot be set with reference to the Consolidation '{0}'.", consol.JK_UniqueConsignRef));
			var departmentPK = departmentResult.Item1;
			var departmentError = departmentResult.Item2;
			if (!string.IsNullOrEmpty(departmentError))
			{
				transaction.AddRowWarning(departmentError);
			}
			else if (departmentPK.IsValid && transaction.AH_GE != departmentPK)
			{
				transaction.AH_GE = departmentPK;
			}

			var branch = GetAPBranch(transaction.Factory, sourceWrapper, false, false, x => true,
				() => Tuple.Create(sourceWrapper.IsJobRelated && sourceWrapper.JobBranch != null, sourceWrapper.JobBranch)).BranchForIntercompanyImport;
			if (branch == null || branch.GB_GC != GlbCompany.CurrentCompany.PK)
			{
				transaction.AddRowWarning(Res.GetString("13DEC1D9-990E-4B11-825C-239342E05EBB", "Transaction Branch is set to the message branch because Transaction Branch cannot be set with reference to the invoice debtor organization proxy."));
			}
			else if (transaction.AH_GB != branch.PK)
			{
				transaction.AH_GB = branch.PK;
			}
		}

		internal interface IIntercompanyBranchDepartmentDeciderSource
		{
			bool IsJobRelated { get; }

			bool IsConsolInvoice { get; }

			IJobCostingPlugIn Consol { get; }

			ZGuid AH_GE { get; }

			ZGuid AH_OH { get; }
		}

		public class IntercompanyBranchDepartmentDeciderSourceWrapper : IIntercompanyBranchDepartmentDeciderSource
		{
			public ZGuid AH_GE { get; set; }

			public ZGuid AH_OH { get; set; }

			public bool IsConsolInvoice { get; set; }

			public IJobCostingPlugIn Consol { get; set; }

			public bool IsJobRelated { get; set; }

			public GlbBranch JobBranch { get; set; }

			public ZGuid JobDepartmentPK { get; set; }
		}

		public static string GetErrorMessageWhenTransactionLinesHaveInvalidDepartment()
		{
			return Res.GetString("412FD1F7-E7C6-40F0-8BC2-FF8DE931EC86", "Intercompany Transaction cannot be imported as some Transaction line jobs have invalid department.");
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBaseBulkChargeImporter : InvoiceBulkOperation
	{
		public InvoicingBaseBulkChargeImporter(InvoicingBase parentInvoice)
			: base(parentInvoice.Factory)
		{
			Argument.NotNull(parentInvoice, "ParentInvoice");
			this.ParentInvoice = parentInvoice;
		}

		#region Public Members

		public void Import()
		{
			ObjectFactory
				.Get<IInvoicingBaseLineImporter>()
				.ImportLinesFromChargeCollection(ParentInvoice, GetAllChargesSelectedForImport());
		}

		internal Charge[] Charges;
		internal Charge[] ChargesFilteredByViewingPermission;
		internal Dictionary<ZGuid, IEnumerable<ZGuid>> ParentChildrenJobPKDictionary;

		static int ChargeLoadingBatchSize =>
#if DEBUG
		Globals.IsTest ? 5 :
#endif
		100;

		static int JobLoadingBatchSize =>
#if DEBUG
		Globals.IsTest ? 5 :
#endif
		500;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		public void LoadJobsCollection()
		{
			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<FunctionalitySuspender>.GetSuspender(Factory))
			using (SQLInjectionDetector.Disable())
			{
				// Clear previous state
				foreach (InvoicingBaseBulkChargeImporterDependentJob job in Jobs)
				{
					job.ClearCharges();
				}

				Jobs.RemoveAll();

				var topLevelJobQuery = new ZQuery(Jobs.CompleteFilter);
				topLevelJobQuery.AddToFilter(Filters.GetQuery());

				var jobPKs = new DynamicBusinessObjectCollection(this.Factory);

				try
				{
					jobPKs.Load($"SELECT {JobHeaderSchema.Constants.PK} FROM {JobHeaderSchema.Constants.SqlSchemaName}.{JobHeaderSchema.Constants.TableName} {topLevelJobQuery.GetAsWhereClause(false)}", topLevelJobQuery.Params);
				}
				catch (System.Data.Common.DbException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.RanOutOfInternalResources) //8623
				{
					Globals.Message.ShowWarning(Res.GetString("72A1BCCE-4C0C-48E9-B907-703071FCAA6F", "Your query is too complicated, please simplify your search conditions."));
				}

				var pks = from ZGuid pk in jobPKs.GetFieldValues(JobHeaderSchema.PK) select pk;
				if (pks.Any())
				{
					// Pre-load jobs to have access to their AdditionalJobsToShowChargesFor
					var topLevelJobs = Factory.Load<InvoicingBaseBulkChargeImporterDependentJob>(new ZQuery(JobHeaderSchema.PK, pks.ToArray()));
					var additionalJobPKsToShowCharges = new List<ZGuid>();
					foreach (InvoicingBaseBulkChargeImporterDependentJob job in topLevelJobs)
					{
						job.ClearCharges();
						job.Master = this;
						additionalJobPKsToShowCharges.AddRange(job.AdditionalJobsToShowChargesFor);
					}

					ParentChildrenJobPKDictionary = new Dictionary<ZGuid, IEnumerable<ZGuid>>();
					var relatedPKs = new List<ZGuid>();
					foreach (var partialPKs in AccountingUtils.ChunksOf(pks.Select(c => c), JobLoadingBatchSize))
					{
						var fetchQuery = new ZQuery(JobHeaderSchema.JH_JH_ParentJob, partialPKs.ToArray());
						fetchQuery.AddToFilter(JobHeaderSchema.JH_GC, ParentInvoice.AH_GC);
						Factory.AddFetchHint(JobHeaderSchema.Instance, fetchQuery);

						var relatedJobQuery = new ZQuery(JobHeaderSchema.JH_JH_ParentJob, partialPKs.ToArray());
						var relatedJobPKs = new DynamicBusinessObjectCollection(Factory);
						relatedJobPKs.Load($"SELECT {JobHeaderSchema.Constants.PK}, {JobHeaderSchema.Constants.JH_JH_ParentJob} FROM {JobHeaderSchema.Constants.SqlSchemaName}.{JobHeaderSchema.Constants.TableName} {relatedJobQuery.GetAsWhereClause(false)}", relatedJobQuery.Params);
						relatedPKs.AddRange(from ZGuid pk in relatedJobPKs.GetFieldValues(JobHeaderSchema.PK) select pk);

						foreach (ZGuid parentPK in (from ZGuid pk in relatedJobPKs.GetFieldValues(JobHeaderSchema.JH_JH_ParentJob) select pk).Distinct())
						{
							ParentChildrenJobPKDictionary.Add(parentPK,
								(from DynamicBusinessObject pair in relatedJobPKs where ((ZGuid)pair[JobHeaderSchema.JH_JH_ParentJob]) == parentPK select ((ZGuid)pair[JobHeaderSchema.PK])));
						}
					}

					var allPKs = pks.Union(relatedPKs).Union(additionalJobPKsToShowCharges).Distinct();
					var tmpCharges = new List<Charge>();

					foreach (var partialPKs in AccountingUtils.ChunksOf(allPKs, ChargeLoadingBatchSize))
					{
						Factory.AddFetchHint(JobChargeSchema.Instance, new ZQuery(JobChargeSchema.JR_JH, partialPKs.ToArray()));
						Factory.AddFetchHint(JobExRateSchema.Instance, new ZQuery(JobExRateSchema.JF_JH, partialPKs.ToArray()));
						var chargesFilter = new ZQuery(JobChargeSchema.JR_JH, partialPKs.ToArray());
						chargesFilter.AddToFilter(Filters.GetChildQuery());
						tmpCharges.AddRange(Factory.Load<Charge>(chargesFilter));
					}

					Charges = tmpCharges.ToArray();
					ChargesFilteredByViewingPermission = Charges.Where(x => IsAllowedtoViewChargeOutsideLoginPermission(x)).ToArray();
					if (Charges.Length == ChargesFilteredByViewingPermission.Length)
					{
						Jobs.AddRange(topLevelJobs);
					}
					else
					{
						Jobs.AddRange(topLevelJobs.Where(x => x.Charges.Count > 0));
					}

					foreach (InvoicingBaseBulkChargeImporterDependentJob job in Jobs)
					{
						job.InitializeParentFromGenericJobWithoutSettingDefaults();
					}
				}
				Jobs.Master.UpdateSelectedLocalTotal();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Combining Cache key, not related to GUI")]
		bool AllowedToLogin(GlbBranch branch, GlbDepartment department)
		{
			return Factory.GetCachedValue("Login BRN:" + branch.GB_Code + " DEP:" + department.GE_Code, delegate
			{
				var security = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid());
				return security.Login.IsAllowed;
			});
		}

		bool IsAllowedtoViewChargeOutsideLoginPermission(Charge charge)
		{
			var result = true;
			if (charge != null && !Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed && charge.Branch != null && charge.Department != null)
			{
				if (charge.Branch != GlbBranch.CurrentBranch || charge.Department != GlbDepartment.CurrentDepartment)
				{
					result = AllowedToLogin(charge.Branch, charge.Department);
				}
			}

			return result;
		}

		public bool IsChargeHidingApplied()
		{
			if (Charges != null && ChargesFilteredByViewingPermission != null)
			{
				return Charges.Length > ChargesFilteredByViewingPermission.Length;
			}
			return false;
		}

		#endregion

		#region Functionality Suspender for Import

		internal class FunctionalitySuspender : ServiceContainerSuspenderHelper.FunctionalitySuspenderService
		{
		}

		#endregion

		#region GUI Bindable Members

		#region Filters

		public new InvoicingBaseBulkChargeImporterFilters Filters
		{
			get { return (InvoicingBaseBulkChargeImporterFilters)FiltersCore; }
		}

		protected override InvoiceBulkOperationFilters GetNewFilters()
		{
			InvoicingBaseBulkChargeImporterFilters result = new InvoicingBaseBulkChargeImporterFilters(ParentInvoice);
			result.ChargePKsToExclude.AddRange(GetChargePKsToExclude());
			return result;
		}

		ZGuid[] GetChargePKsToExclude()
		{
			List<ZGuid> result = new List<ZGuid>();

			foreach (InvoicingLineBase line in ParentInvoice.Lines)
			{
				if (line.OriginalJobCharge != null)
				{
					result.Add(line.OriginalJobCharge.PK);
				}
			}

			return result.ToArray();
		}

		#endregion

		#region Jobs

		InvoicingBaseBulkChargeImporterDependentJobCollection fJobs;
		public InvoicingBaseBulkChargeImporterDependentJobCollection Jobs
		{
			get { return fJobs ?? (fJobs = new InvoicingBaseBulkChargeImporterDependentJobCollection(this, Factory)); }
		}

		#endregion

		#region Selected Total

		[ReadOnly(true)]
		public ZDecimal SelectedLocalTotal { get; private set; }

		public ZPropertyInfo SelectedLocalTotalInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedLocalTotal)); }
		}

		public IDisposable GetUpdateSelectedLocalTotalSuspender(bool runUpdateOnDispose = true)
		{
			return new UpdateSelectedLocalTotalSuspender(this, runUpdateOnDispose);
		}

		public bool IsGetUpdateSelectedLocalTotalSuspended
		{
			get { return UpdateSelectedLocalTotalSuspenderCount > 0; }
		}

		class UpdateSelectedLocalTotalSuspender : IDisposable
		{
			internal UpdateSelectedLocalTotalSuspender(InvoicingBaseBulkChargeImporter parent, bool runUpdateOnDispose)
			{
				this.parent = parent;
				this.runUpdateOnDispose = runUpdateOnDispose;
				parent.UpdateSelectedLocalTotalSuspenderCount++;
			}

			InvoicingBaseBulkChargeImporter parent;
			readonly bool runUpdateOnDispose;

			void IDisposable.Dispose()
			{
				parent.UpdateSelectedLocalTotalSuspenderCount--;
				if (parent.UpdateSelectedLocalTotalSuspenderCount == 0 && runUpdateOnDispose)
				{
					parent.UpdateSelectedLocalTotal();
				}

				parent = null;
			}
		}

		int UpdateSelectedLocalTotalSuspenderCount;

		public void UpdateSelectedLocalTotal()
		{
			if (!IsGetUpdateSelectedLocalTotalSuspended)
			{
				ZDecimal result = 0m;
				foreach (Charge charge in GetAllChargesSelectedForImport())
				{
					result += charge.JR_LocalCostAmt;
				}

				SelectedLocalTotal = result;
				SelectedLocalTotalInfo.RefreshBinding();
				SelectedInvoiceCurrencyTotalInfo.RefreshBinding();
#if DEBUG
				if (Globals.IsTest)
				{
					UpdateSelectedLocalTotalExecutionTimes++;
				}
#endif
			}
		}

#if DEBUG
		internal int UpdateSelectedLocalTotalExecutionTimes;
#endif

		public ZDecimal SelectedInvoiceCurrencyTotal
		{
			get
			{
				ZDecimal result = 0m;
				if (ParentInvoice.AH_PostedToEFT)
				{
					foreach (InvoicingBaseBulkChargeImporterDependentJob job in Jobs)
					{
						if (job.IsSelectedForImport)
						{
							result += job.SelectedChargesInvoiceCurrencyAmount;
						}
					}
				}
				else
				{
					result = Env.CurrentCompany.ExchangeRate.LocalToForeign(SelectedLocalTotal, ParentInvoice.AH_ExchangeRate, ParentInvoice.AH_RX_NKTransactionCurrency);
				}
				return result;
			}
		}

		public ZPropertyInfo SelectedInvoiceCurrencyTotalInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedInvoiceCurrencyTotal)); }
		}

		#endregion

		public InvoicingBase ParentInvoice { get; private set; }

		#endregion

		Charge[] GetAllChargesSelectedForImport()
		{
			return Jobs.Cast<InvoicingBaseBulkChargeImporterDependentJob>()
				.SelectMany(job => job.GetChargesSelectedForImport())
				.Distinct()
				.ToArray();
		}
	}
}

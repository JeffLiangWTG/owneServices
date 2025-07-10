using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobProfitLoss : NonPersistentBusinessObject, IObsoleteValidation, IJobProfitLoss, IProfitLossTotals
	{
		public JobProfitLoss(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public FunctionalitySuspender RefreshBindingSuspender => refreshBindingSuspender ?? (refreshBindingSuspender = new FunctionalitySuspender());
		FunctionalitySuspender refreshBindingSuspender;

		public new void RefreshBinding()
		{
			if (!RefreshBindingSuspender.IsSuspended)
			{
				base.RefreshBinding();
			}
		}

		#region Job PKs

		public void SetConsol(IJobCostingPlugIn plugin)
		{
			this.Plugin = plugin;
			ResetCache();
		}

		internal IJobCostingPlugIn Plugin;

		public void SetJobPKs(ZGuid[] jobPKs)
		{
			ManualJobPKs = jobPKs;
			ResetCache();
		}

		public void SetParent(IJobHeaderParent parent)
		{
			this.Parent = parent;
		}

		public IJobHeaderParent Parent { get; private set; }

		public ZGuid[] JobPKs
		{
			get { return ManualJobPKs ?? (System.Array.Empty<ZGuid>()); }
		}

		internal ZGuid[] ManualJobPKs;

		#endregion

		#region Properties

		public ZInt Decimals
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.Decimals; }
		}

		#region Summary Values (Recognized + Not Recognized)

		#region Total Accrual

		ZDecimal totalAccrual;
		public ZDecimal TotalAccrual
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalAccrual;
			}
		}

		ZDecimal totalAccrualExcludingDSB;
		public ZDecimal TotalAccrualExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalAccrualExcludingDSB;
			}
		}

		public ZPropertyInfo TotalAccrualInfo
		{
			get { return GetZPropertyInfo(nameof(TotalAccrual)); }
		}

		#endregion

		#region Total WIP

		ZDecimal totalWIP;
		public ZDecimal TotalWIP
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalWIP;
			}
		}

		ZDecimal totalWIPExcludingDSB;
		public ZDecimal TotalWIPExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalWIPExcludingDSB;
			}
		}

		public ZPropertyInfo TotalWIPInfo
		{
			get { return GetZPropertyInfo(nameof(TotalWIP)); }
		}

		#endregion

		#region Total Cost

		ZDecimal totalCost;
		public ZDecimal TotalCost
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalCost;
			}
		}

		ZDecimal totalCostExcludingDSB;
		public ZDecimal TotalCostExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalCostExcludingDSB;
			}
		}

		public ZPropertyInfo TotalCostInfo
		{
			get { return GetZPropertyInfo(nameof(TotalCost)); }
		}

		#endregion

		#region Total Revenue

		ZDecimal totalRevenue;
		public ZDecimal TotalRevenue
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalRevenue;
			}
		}

		ZDecimal totalRevenueExcludingDSB;
		public ZDecimal TotalRevenueExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalRevenueExcludingDSB;
			}
		}

		public ZPropertyInfo TotalRevenueInfo
		{
			get { return GetZPropertyInfo(nameof(TotalRevenue)); }
		}

		#endregion

		#region Total Profit / Loss

		ZDecimal totalLineAmount;
		public ZDecimal TotalLineAmount
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalLineAmount;
			}
		}

		ZDecimal totalLineAmountExcludingDSB;
		public ZDecimal TotalLineAmountExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalLineAmountExcludingDSB;
			}
		}

		public ZPropertyInfo TotalLineAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalLineAmount)); }
		}

		#endregion

		#region Margin Profit/Rev

		public ZString MarginProfitRev
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value)
				{
					return FormatPercents(TotalRevenue + TotalWIP == 0 ? 0 : TotalLineAmount / (TotalRevenue + TotalWIP));
				}
				else
				{
					return FormatPercents(TotalRevenueExcludingDSB + TotalWIPExcludingDSB == 0 ? 0 : TotalLineAmountExcludingDSB / (TotalRevenueExcludingDSB + TotalWIPExcludingDSB));
				}
			}
		}

		public ZPropertyInfo MarginProfitRevInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitRev)); }
		}

		#endregion

		#region Margin Profit/Cost

		public ZString MarginProfitCost
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value)
				{
					return FormatPercents(TotalCost + TotalAccrual == 0 ? 0 : -TotalLineAmount / (TotalCost + TotalAccrual));
				}
				else
				{
					return FormatPercents(TotalCostExcludingDSB + TotalAccrualExcludingDSB == 0 ? 0 : -TotalLineAmountExcludingDSB / (TotalCostExcludingDSB + TotalAccrualExcludingDSB));
				}
			}
		}

		public ZPropertyInfo MarginProfitCostInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitCost)); }
		}

		#endregion

		#region TaxExpense

		ZDecimal totalTaxExpenseRevenue;
		public ZDecimal TotalTaxExpenseRevenue
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalTaxExpenseRevenue;
			}
		}

		public ZPropertyInfo TotalTaxExpenseRevenueInfo
		{
			get { return GetZPropertyInfo(nameof(TotalTaxExpenseRevenue)); }
		}

		ZDecimal totalTaxExpenseCost;
		public ZDecimal TotalTaxExpenseCost
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalTaxExpenseCost;
			}
		}

		public ZPropertyInfo TotalTaxExpenseCostInfo
		{
			get { return GetZPropertyInfo(nameof(TotalTaxExpenseCost)); }
		}

		public bool IsTaxExpenseSupported => GlbCompany.CurrentCompany.IsEnabledForTaxFrameworkConfiguration(Factory);

		#endregion

		#endregion

		#region Recognized Values

		#region Total Accrual

		ZDecimal totalAccrualRecognized;
		public ZDecimal TotalAccrualRecognized
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalAccrualRecognized;
			}
		}

		ZDecimal totalAccrualRecognizedExcludingDSB;
		public ZDecimal TotalAccrualRecognizedExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalAccrualRecognizedExcludingDSB;
			}
		}

		public ZPropertyInfo TotalAccrualRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalAccrualRecognized)); }
		}

		#endregion

		#region Total WIP

		ZDecimal totalWIPRecognized;
		public ZDecimal TotalWIPRecognized
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalWIPRecognized;
			}
		}

		ZDecimal totalWIPRecognizedExcludingDSB;
		public ZDecimal TotalWIPRecognizedExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalWIPRecognizedExcludingDSB;
			}
		}

		public ZPropertyInfo TotalWIPRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalWIPRecognized)); }
		}

		#endregion

		#region Total Cost

		ZDecimal totalCostRecognized;
		public ZDecimal TotalCostRecognized
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalCostRecognized;
			}
		}

		ZDecimal totalCostRecognizedExcludingDSB;
		public ZDecimal TotalCostRecognizedExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalCostRecognizedExcludingDSB;
			}
		}

		public ZPropertyInfo TotalCostRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalCostRecognized)); }
		}

		#endregion

		#region Total Revenue

		ZDecimal totalRevenueRecognized;
		public ZDecimal TotalRevenueRecognized
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}

				return totalRevenueRecognized;
			}
		}

		ZDecimal totalRevenueRecognizedExcludingDSB;
		public ZDecimal TotalRevenueRecognizedExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalRevenueRecognizedExcludingDSB;
			}
		}

		public ZPropertyInfo TotalRevenueRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalRevenueRecognized)); }
		}

		#endregion

		#region Total Profit / Loss

		ZDecimal totalLineAmountRecognized;
		public ZDecimal TotalLineAmountRecognized
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalLineAmountRecognized;
			}
		}

		ZDecimal totalLineAmountRecognizedExcludingDSB;
		public ZDecimal TotalLineAmountRecognizedExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalLineAmountRecognizedExcludingDSB;
			}
		}

		public ZPropertyInfo TotalLineAmountRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalLineAmountRecognized)); }
		}

		#endregion

		#region Margin Profit/Rev

		public ZString MarginProfitRevRecognized
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value)
				{
					return FormatPercents(TotalRevenueRecognized + TotalWIPRecognized == 0 ? 0 : TotalLineAmountRecognized / (TotalRevenueRecognized + TotalWIPRecognized));
				}
				else
				{
					return FormatPercents(TotalRevenueRecognizedExcludingDSB + TotalWIPRecognizedExcludingDSB == 0 ? 0 : TotalLineAmountRecognizedExcludingDSB / (TotalRevenueRecognizedExcludingDSB + TotalWIPRecognizedExcludingDSB));
				}
			}
		}

		public ZPropertyInfo MarginProfitRevRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitRev)); }
		}

		#endregion

		#region Margin Profit/Cost

		public ZString MarginProfitCostRecognized
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value)
				{
					return FormatPercents(TotalCostRecognized + TotalAccrualRecognized == 0 ? 0 : -TotalLineAmountRecognized / (TotalCostRecognized + TotalAccrualRecognized));
				}
				else
				{
					return FormatPercents(TotalCostRecognizedExcludingDSB + TotalAccrualRecognizedExcludingDSB == 0 ? 0 : -TotalLineAmountRecognizedExcludingDSB / (TotalCostRecognizedExcludingDSB + TotalAccrualRecognizedExcludingDSB));
				}
			}
		}

		public ZPropertyInfo MarginProfitCostRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitCostRecognized)); }
		}

		#endregion

		#endregion

		#region Not Recognized Values

		#region Total Accrual

		ZDecimal totalAccrualNotRecognized;
		public ZDecimal TotalAccrualNotRecognized
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalAccrualNotRecognized;
			}
		}

		ZDecimal totalAccrualNotRecognizedExcludingDSB;
		public ZDecimal TotalAccrualNotRecognizedExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalAccrualNotRecognizedExcludingDSB;
			}
		}

		public ZPropertyInfo TotalAccrualNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalAccrualNotRecognized)); }
		}

		#endregion

		#region Total WIP

		ZDecimal totalWIPNotRecognized;
		public ZDecimal TotalWIPNotRecognized
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalWIPNotRecognized;
			}
		}

		ZDecimal totalWIPNotRecognizedExcludingDSB;
		public ZDecimal TotalWIPNotRecognizedExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalWIPNotRecognizedExcludingDSB;
			}
		}

		public ZPropertyInfo TotalWIPNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalWIPNotRecognized)); }
		}

		#endregion

		#region Total Cost

		ZDecimal totalCostNotRecognized;
		public ZDecimal TotalCostNotRecognized
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalCostNotRecognized;
			}
		}

		ZDecimal totalCostNotRecognizedExcludingDSB;
		public ZDecimal TotalCostNotRecognizedExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalCostNotRecognizedExcludingDSB;
			}
		}

		public ZPropertyInfo TotalCostNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalCostNotRecognized)); }
		}

		#endregion

		#region Total Revenue

		ZDecimal totalRevenueNotRecognized;
		public ZDecimal TotalRevenueNotRecognized
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalRevenueNotRecognized;
			}
		}

		ZDecimal totalRevenueNotRecognizedExcludingDSB;
		public ZDecimal TotalRevenueNotRecognizedExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalRevenueNotRecognizedExcludingDSB;
			}
		}

		public ZPropertyInfo TotalRevenueNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalRevenueNotRecognized)); }
		}

		#endregion

		#region Total Profit / Loss

		ZDecimal totalLineAmountNotRecognized;
		public ZDecimal TotalLineAmountNotRecognized
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalLineAmountNotRecognized;
			}
		}

		ZDecimal totalLineAmountNotRecognizedExcludingDSB;
		public ZDecimal TotalLineAmountNotRecognizedExcludingDSB
		{
			get
			{
				if (!IsJobProfitLossAmountsCalculated)
				{
					CalculateJobProfitLossAmounts();
				}
				return totalLineAmountNotRecognizedExcludingDSB;
			}
		}

		public ZPropertyInfo TotalLineAmountNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalLineAmountNotRecognized)); }
		}

		#endregion

		#region Margin Profit/Rev

		public ZString MarginProfitRevNotRecognized
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value)
				{
					return FormatPercents(TotalRevenueNotRecognized + TotalWIPNotRecognized == 0 ? 0 : TotalLineAmountNotRecognized / (TotalRevenueNotRecognized + TotalWIPNotRecognized));
				}
				else
				{
					return FormatPercents(TotalRevenueNotRecognizedExcludingDSB + TotalWIPNotRecognizedExcludingDSB == 0 ? 0 : TotalLineAmountNotRecognizedExcludingDSB / (TotalRevenueNotRecognizedExcludingDSB + TotalWIPNotRecognizedExcludingDSB));
				}
			}
		}

		public ZPropertyInfo MarginProfitRevNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitRevNotRecognized)); }
		}

		#endregion

		#region Margin Profit/Cost

		public ZString MarginProfitCostNotRecognized
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value)
				{
					return FormatPercents(TotalCostNotRecognized + TotalAccrualNotRecognized == 0 ? 0 : -TotalLineAmountNotRecognized / (TotalCostNotRecognized + TotalAccrualNotRecognized));
				}
				else
				{
					return FormatPercents(TotalCostNotRecognizedExcludingDSB + TotalAccrualNotRecognizedExcludingDSB == 0 ? 0 : -TotalLineAmountNotRecognizedExcludingDSB / (TotalCostNotRecognizedExcludingDSB + TotalAccrualNotRecognizedExcludingDSB));
				}
			}
		}

		public ZPropertyInfo MarginProfitCostNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitCostNotRecognized)); }
		}

		#endregion

		#endregion

		#endregion

		#region Filter

		public IProfitLossFilterProvider Filter
		{
			get
			{
				if (fFilter == null)
				{
					fFilter = new ProfitLossFilterProvider(this);
				}
				return fFilter;
			}
		}

		ProfitLossFilterProvider fFilter;

		#endregion

		#region Details Collection

		[BusinessObjectTestExclude]
		public ProfitLossCollectionBase ProfitLossDetails
		{
			get
			{
				if (fProfitLossDetails == null)
				{
					if (ManualJobPKs != null && ManualJobPKs.Length > 0)
					{
						fProfitLossDetails = new ProfitLossCollection(this, ManualJobPKs);
					}
					else if (Plugin != null)
					{
						fProfitLossDetails = new ProfitLossCollection(this, Plugin as IJobHeaderParent);
					}

					if (fProfitLossDetails != null)
					{
						fProfitLossDetails.Load();
					}
				}

				return fProfitLossDetails;
			}
		}

		ProfitLossCollectionBase fProfitLossDetails;

		#endregion

		#region Details Filtered Collection

		[BusinessObjectTestExclude]
		public ProfitLossDetailCollectionBaseView ProfitLossFilteredDetails
		{
			get
			{
				if (fProfitLossFilteredDetails == null && ProfitLossDetails != null)
				{
					fProfitLossFilteredDetails = new ProfitLossDetailCollectionView(ProfitLossDetails, Plugin);
				}
				return fProfitLossFilteredDetails;
			}
		}

		public ProfitLossDetailCollectionBaseView fProfitLossFilteredDetails;

		#endregion

		#region Summary Collection

		[BusinessObjectTestExclude]
		public ProfitLossSummaryCollectionBase ProfitLossSummaryDetails
		{
			get
			{
				if (fProfitLossSummaryDetails == null)
				{
					if (ManualJobPKs != null && ManualJobPKs.Length > 0)
					{
						fProfitLossSummaryDetails = new ProfitLossSummaryCollection(this, ManualJobPKs);
					}
					else if (Plugin != null)
					{
						fProfitLossSummaryDetails = new ProfitLossSummaryCollection(this, Plugin);
					}

					if (fProfitLossSummaryDetails != null)
					{
						fProfitLossSummaryDetails.Load();
						IsJobProfitLossAmountsCalculated = false;
					}
				}
				return fProfitLossSummaryDetails;
			}
		}

		ProfitLossSummaryCollectionBase fProfitLossSummaryDetails;

		#endregion

		#region Summary Filtered Collection

		[BusinessObjectTestExclude]
		public ProfitLossSummaryCollectionBaseView ProfitLossSummaryFilteredDetails
		{
			get
			{
				if (fProfitLossSummaryFilteredDetails == null && ProfitLossSummaryDetails != null)
				{
					fProfitLossSummaryFilteredDetails = new ProfitLossSummaryCollectionView(ProfitLossSummaryDetails, Plugin);
				}
				return fProfitLossSummaryFilteredDetails;
			}
		}

		ProfitLossSummaryCollectionBaseView fProfitLossSummaryFilteredDetails;

		#endregion

		#region Global Job Costing Collection

		[BusinessObjectTestExclude]
		public ProfitLossCollectionBase GlobalJobCostingProfitLoss
		{
			get
			{
				if (fGlobalJobCostingProfitLoss == null)
				{
					if (Parent != null)
					{
						fGlobalJobCostingProfitLoss = new GlobalProfitLossCollection(this, Parent);
					}

					if (fGlobalJobCostingProfitLoss != null)
					{
						fGlobalJobCostingProfitLoss.Load();
					}
				}

				return fGlobalJobCostingProfitLoss;
			}
		}

		ProfitLossCollectionBase fGlobalJobCostingProfitLoss;

		#endregion

		#region Implementation

		public void ResetCache()
		{
			fProfitLossDetails = null;
			fProfitLossSummaryDetails = null;
			fProfitLossFilteredDetails = null;
			fProfitLossSummaryFilteredDetails = null;
		}

		ZString FormatPercents(ZDecimal num)
		{
			return num.ToString("P", Env.CurrentCompany.Country.Culture);
		}

		public enum JobProfitLossType
		{
			Details,
			Summary,
			Global
		}

		public void ReloadCollectionsAndRelatedProperties(JobProfitLossType jobProfitLossType)
		{
			if (jobProfitLossType == JobProfitLossType.Global && fGlobalJobCostingProfitLoss != null)
			{
				fGlobalJobCostingProfitLoss.Load();
			}
			else
			{
				if (jobProfitLossType == JobProfitLossType.Details && fProfitLossDetails != null)
				{
					fProfitLossDetails.Load();
					ProfitLossFilteredDetails.FilterProfitLossCollection();
				}
				if (fProfitLossSummaryDetails != null)
				{
					fProfitLossSummaryDetails.Load();
					ProfitLossSummaryFilteredDetails.FilterProfitLossSummaryCollection();
					IsJobProfitLossAmountsCalculated = false;
				}
			}
			RefreshBinding();
		}

		bool IsJobProfitLossAmountsCalculated;

		void CalculateJobProfitLossAmounts()
		{
			ClearTotalAmount();

			foreach (ProfitLossSummaryDetailView detail in ProfitLossSummaryFilteredDetails)
			{
				if (!Job.GetChargeTypeInformation(detail.ChargeCode, (Job)detail.Job).IsDisbursement)
				{
					totalAccrualExcludingDSB += detail.ZZ_Calc_Accrual;
					totalWIPExcludingDSB += detail.ZZ_Calc_WIP;
					totalCostExcludingDSB += detail.ZZ_Calc_Cost;
					totalRevenueExcludingDSB += detail.ZZ_Calc_Revenue;
					totalLineAmountExcludingDSB += detail.ZZ_Calc_LineAmount;

					totalAccrualRecognizedExcludingDSB += detail.ZZ_Calc_Accrual_Recognized;
					totalWIPRecognizedExcludingDSB += detail.ZZ_Calc_WIP_Recognized;
					totalCostRecognizedExcludingDSB += detail.ZZ_Calc_Cost_Recognized;
					totalRevenueRecognizedExcludingDSB += detail.ZZ_Calc_Revenue_Recognized;
					totalLineAmountRecognizedExcludingDSB += detail.ZZ_Calc_LineAmount_Recognized;

					totalAccrualNotRecognizedExcludingDSB += detail.ZZ_Calc_Accrual_NotRecognized;
					totalWIPNotRecognizedExcludingDSB += detail.ZZ_Calc_WIP_NotRecognized;
					totalCostNotRecognizedExcludingDSB += detail.ZZ_Calc_Cost_NotRecognized;
					totalRevenueNotRecognizedExcludingDSB += detail.ZZ_Calc_Revenue_NotRecognized;
					totalLineAmountNotRecognizedExcludingDSB += detail.ZZ_Calc_LineAmount_NotRecognized;
				}

				totalAccrual += detail.ZZ_Calc_Accrual;
				totalWIP += detail.ZZ_Calc_WIP;
				totalCost += detail.ZZ_Calc_Cost;
				totalRevenue += detail.ZZ_Calc_Revenue;
				totalLineAmount += detail.ZZ_Calc_LineAmount;

				totalAccrualRecognized += detail.ZZ_Calc_Accrual_Recognized;
				totalWIPRecognized += detail.ZZ_Calc_WIP_Recognized;
				totalCostRecognized += detail.ZZ_Calc_Cost_Recognized;
				totalRevenueRecognized += detail.ZZ_Calc_Revenue_Recognized;
				totalLineAmountRecognized += detail.ZZ_Calc_LineAmount_Recognized;

				totalAccrualNotRecognized += detail.ZZ_Calc_Accrual_NotRecognized;
				totalWIPNotRecognized += detail.ZZ_Calc_WIP_NotRecognized;
				totalCostNotRecognized += detail.ZZ_Calc_Cost_NotRecognized;
				totalRevenueNotRecognized += detail.ZZ_Calc_Revenue_NotRecognized;
				totalLineAmountNotRecognized += detail.ZZ_Calc_LineAmount_NotRecognized;

				totalTaxExpenseRevenue += detail.ZZ_Calc_TaxExpenseRevenue;

				totalTaxExpenseCost += detail.ZZ_Calc_TaxExpenseCost;
			}

			IsJobProfitLossAmountsCalculated = true;
		}

		void ClearTotalAmount()
		{
			totalAccrual = ZDecimal.Zero;
			totalAccrualExcludingDSB = ZDecimal.Zero;

			totalWIP = ZDecimal.Zero;
			totalWIPExcludingDSB = ZDecimal.Zero;

			totalCost = ZDecimal.Zero;
			totalCostExcludingDSB = ZDecimal.Zero;

			totalRevenue = ZDecimal.Zero;
			totalRevenueExcludingDSB = ZDecimal.Zero;

			totalLineAmount = ZDecimal.Zero;
			totalLineAmountExcludingDSB = ZDecimal.Zero;

			#region Recognized

			totalAccrualRecognized = ZDecimal.Zero;
			totalAccrualRecognizedExcludingDSB = ZDecimal.Zero;

			totalWIPRecognized = ZDecimal.Zero;
			totalWIPRecognizedExcludingDSB = ZDecimal.Zero;

			totalCostRecognized = ZDecimal.Zero;
			totalCostRecognizedExcludingDSB = ZDecimal.Zero;

			totalRevenueRecognized = ZDecimal.Zero;
			totalRevenueRecognizedExcludingDSB = ZDecimal.Zero;

			totalLineAmountRecognized = ZDecimal.Zero;
			totalLineAmountRecognizedExcludingDSB = ZDecimal.Zero;

			#endregion 

			#region NotRecognized

			totalAccrualNotRecognized = ZDecimal.Zero;
			totalAccrualNotRecognizedExcludingDSB = ZDecimal.Zero;

			totalWIPNotRecognized = ZDecimal.Zero;
			totalWIPNotRecognizedExcludingDSB = ZDecimal.Zero;

			totalCostNotRecognized = ZDecimal.Zero;
			totalCostNotRecognizedExcludingDSB = ZDecimal.Zero;

			totalRevenueNotRecognized = ZDecimal.Zero;
			totalRevenueNotRecognizedExcludingDSB = ZDecimal.Zero;

			totalLineAmountNotRecognized = ZDecimal.Zero;
			totalLineAmountNotRecognizedExcludingDSB = ZDecimal.Zero;

			#endregion

			totalTaxExpenseRevenue = ZDecimal.Zero;

			totalTaxExpenseCost = ZDecimal.Zero;
		}

		#endregion
	}
}

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	public class ProfitLossSummaryDetailView : NonPersistentBusinessObject
	{
		#region Schema

		public abstract class Schema
		{
			public const string Decimals = "Decimals";
			public const string ZZ_Calc_AC = "ZZ_Calc_AC";
			public const string ZZ_Calc_JH = "ZZ_Calc_JH";
			public const string ZZ_Calc_GB = "ZZ_Calc_GB";
			public const string ZZ_Calc_GE = "ZZ_Calc_GE";
			public const string ZZ_Calc_JobLocalReferenceNum = "ZZ_Calc_JobLocalReferenceNum";
			public const string ZZ_Calc_ChargeCodeDescription = "ZZ_Calc_ChargeCodeDescription";
			public const string ZZ_Calc_Revenue = "ZZ_Calc_Revenue";
			public const string ZZ_Calc_WIP = "ZZ_Calc_WIP";
			public const string ZZ_Calc_Cost = "ZZ_Calc_Cost";
			public const string ZZ_Calc_Accrual = "ZZ_Calc_Accrual";
			public const string ZZ_Calc_LineAmount = "ZZ_Calc_LineAmount";
			public const string ZZ_Calc_MarginPercentage = "ZZ_Calc_MarginPercentage";
			public const string ZZ_Calc_Revenue_Recognized = "ZZ_Calc_Revenue_Recognized";
			public const string ZZ_Calc_WIP_Recognized = "ZZ_Calc_WIP_Recognized";
			public const string ZZ_Calc_Cost_Recognized = "ZZ_Calc_Cost_Recognized";
			public const string ZZ_Calc_Accrual_Recognized = "ZZ_Calc_Accrual_Recognized";
			public const string ZZ_Calc_LineAmount_Recognized = "ZZ_Calc_LineAmount_Recognized";
			public const string ZZ_Calc_MarginPercentage_Recognized = "ZZ_Calc_MarginPercentage_Recognized";
			public const string ZZ_Calc_WIP_NotRecognized = "ZZ_Calc_WIP_NotRecognized";
			public const string ZZ_Calc_Cost_NotRecognized = "ZZ_Calc_Cost_NotRecognized";
			public const string ZZ_Calc_Accrual_NotRecognized = "ZZ_Calc_Accrual_NotRecognized";
			public const string ZZ_Calc_LineAmount_NotRecognized = "ZZ_Calc_LineAmount_NotRecognized";
			public const string ZZ_Calc_MarginPercentage_NotRecognized = "ZZ_Calc_MarginPercentage_NotRecognized";
			public const string ZZ_Calc_TaxExpenseRevenue = "ZZ_Calc_TaxExpenseRevenue";
			public const string ZZ_Calc_TaxExpenseCost = "ZZ_Calc_TaxExpenseCost";
		}

		#endregion

		#region Constructor

		public ProfitLossSummaryDetailView(ProfitLossSummaryDetail profitLossSummaryDetail)
			: base(profitLossSummaryDetail.Factory)
		{
			this.profitLossSummaryDetail = profitLossSummaryDetail;
		}

		#endregion

		public ProfitLossSummaryDetail ProfitLossSummaryDetail { get { return profitLossSummaryDetail; } }
		readonly ProfitLossSummaryDetail profitLossSummaryDetail;

		#region Properties

		#region Decimals

		public ZInt Decimals
		{
			get
			{
				if (fDecimals.IsEmpty)
				{
					fDecimals = profitLossSummaryDetail.Decimals;
				}
				return fDecimals;
			}
			set
			{
				SetNonPersistentPropertyValue(DecimalsInfo, ref fDecimals, value);
			}
		}

		ZInt fDecimals;

		public ZPropertyInfo DecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(Decimals)); }
		}

		#endregion

		#region ZZ_Calc_AC

		[RelatedBusinessObject("ChargeCode")]
		[List("ChargeCodes")]
		public ZGuid ZZ_Calc_AC
		{
			get
			{
				if (fZZ_Calc_AC.IsEmpty)
				{
					fZZ_Calc_AC = profitLossSummaryDetail.ZZ_Calc_AC;
				}
				return fZZ_Calc_AC;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_ACInfo, ref fZZ_Calc_AC, value);
			}
		}

		ZGuid fZZ_Calc_AC;
		public ZPropertyInfo ZZ_Calc_ACInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_AC)); }
		}

		public AccChargeCode ChargeCode
		{
			get { return (AccChargeCode)Factory.Load(typeof(AccChargeCode), ZZ_Calc_AC); }
		}

		#endregion

		#region ZZ_Calc_JH

		[RelatedBusinessObject("Job")]
		[List("Jobs")]
		public ZGuid ZZ_Calc_JH
		{
			get
			{
				if (fZZ_Calc_JH.IsEmpty)
				{
					fZZ_Calc_JH = profitLossSummaryDetail.ZZ_Calc_JH;
				}
				return fZZ_Calc_JH;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_JHInfo, ref fZZ_Calc_JH, value);
			}
		}

		ZGuid fZZ_Calc_JH;
		public ZPropertyInfo ZZ_Calc_JHInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_JH)); }
		}

		public JobHeader Job
		{
			get { return (JobHeader)Factory.Load(typeof(JobHeader), ZZ_Calc_JH); }
		}

		#endregion

		#region ZZ_Calc_GB

		[RelatedBusinessObject("Branch")]
		[List("Branches")]
		public ZGuid ZZ_Calc_GB
		{
			get
			{
				if (fZZ_Calc_GB.IsEmpty)
				{
					fZZ_Calc_GB = profitLossSummaryDetail.ZZ_Calc_GB;
				}
				return fZZ_Calc_GB;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_GBInfo, ref fZZ_Calc_GB, value);
			}
		}

		ZGuid fZZ_Calc_GB;
		public ZPropertyInfo ZZ_Calc_GBInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_GB)); }
		}

		public GlbBranch Branch
		{
			get { return (GlbBranch)Factory.Load(typeof(GlbBranch), ZZ_Calc_GB); }
		}

		#endregion

		#region ZZ_Calc_GE

		[RelatedBusinessObject("Department")]
		[List("Departments")]
		public ZGuid ZZ_Calc_GE
		{
			get
			{
				if (fZZ_Calc_GE.IsEmpty)
				{
					fZZ_Calc_GE = profitLossSummaryDetail.ZZ_Calc_GE;
				}
				return fZZ_Calc_GE;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_GEInfo, ref fZZ_Calc_GE, value);
			}
		}

		ZGuid fZZ_Calc_GE;
		public ZPropertyInfo ZZ_Calc_GEInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_GE)); }
		}

		public GlbDepartment Department
		{
			get { return (GlbDepartment)Factory.Load(typeof(GlbDepartment), ZZ_Calc_GE); }
		}

		#endregion

		#region ZZ_Calc_JobLocalReferenceNum

		public ZString ZZ_Calc_JobLocalReferenceNum
		{
			get
			{
				if (fZZ_Calc_JobLocalReferenceNum.IsEmpty)
				{
					fZZ_Calc_JobLocalReferenceNum = profitLossSummaryDetail.ZZ_Calc_JobLocalReferenceNum;
				}
				return fZZ_Calc_JobLocalReferenceNum;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_JobLocalReferenceNumInfo, ref fZZ_Calc_JobLocalReferenceNum, value);
			}
		}

		ZString fZZ_Calc_JobLocalReferenceNum;
		public ZPropertyInfo ZZ_Calc_JobLocalReferenceNumInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_JobLocalReferenceNum)); }
		}

		#endregion

		#region ZZ_Calc_ChargeCodeDescription

		public ZString ZZ_Calc_ChargeCodeDescription
		{
			get
			{
				if (fZZ_Calc_ChargeCodeDescription.IsEmpty)
				{
					fZZ_Calc_ChargeCodeDescription = profitLossSummaryDetail.ZZ_Calc_ChargeCodeDescription;
				}
				return fZZ_Calc_ChargeCodeDescription;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_ChargeCodeDescriptionInfo, ref fZZ_Calc_ChargeCodeDescription, value);
			}
		}

		ZString fZZ_Calc_ChargeCodeDescription;
		public ZPropertyInfo ZZ_Calc_ChargeCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_ChargeCodeDescription)); }
		}

		#endregion

		#region Summary Values (Recognized + Not Recognized)

		#region ZZ_Calc_Revenue

		public ZDecimal ZZ_Calc_Revenue
		{
			get
			{
				if (fZZ_Calc_Revenue.IsEmpty)
				{
					fZZ_Calc_Revenue = profitLossSummaryDetail.ZZ_Calc_Revenue;
				}
				return fZZ_Calc_Revenue;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_RevenueInfo, ref fZZ_Calc_Revenue, value);
			}
		}

		ZDecimal fZZ_Calc_Revenue;
		public ZPropertyInfo ZZ_Calc_RevenueInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Revenue)); }
		}

		#endregion

		#region ZZ_Calc_WIP

		public ZDecimal ZZ_Calc_WIP
		{
			get
			{
				if (fZZ_Calc_WIP.IsEmpty)
				{
					fZZ_Calc_WIP = profitLossSummaryDetail.ZZ_Calc_WIP;
				}
				return fZZ_Calc_WIP;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_WIPInfo, ref fZZ_Calc_WIP, value);
			}
		}

		ZDecimal fZZ_Calc_WIP;
		public ZPropertyInfo ZZ_Calc_WIPInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_WIP)); }
		}

		#endregion

		#region ZZ_Calc_Cost

		public ZDecimal ZZ_Calc_Cost
		{
			get
			{
				if (fZZ_Calc_Cost.IsEmpty)
				{
					fZZ_Calc_Cost = profitLossSummaryDetail.ZZ_Calc_Cost;
				}
				return fZZ_Calc_Cost;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_CostInfo, ref fZZ_Calc_Cost, value);
			}
		}

		ZDecimal fZZ_Calc_Cost;
		public ZPropertyInfo ZZ_Calc_CostInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Cost)); }
		}

		#endregion

		#region ZZ_Calc_Accrual

		public ZDecimal ZZ_Calc_Accrual
		{
			get
			{
				if (fZZ_Calc_Accrual.IsEmpty)
				{
					fZZ_Calc_Accrual = profitLossSummaryDetail.ZZ_Calc_Accrual;
				}
				return fZZ_Calc_Accrual;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_AccrualInfo, ref fZZ_Calc_Accrual, value);
			}
		}

		ZDecimal fZZ_Calc_Accrual;
		public ZPropertyInfo ZZ_Calc_AccrualInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Accrual)); }
		}

		#endregion

		#region ZZ_Calc_LineAmount

		public ZDecimal ZZ_Calc_LineAmount
		{
			get
			{
				if (fZZ_Calc_LineAmount.IsEmpty)
				{
					fZZ_Calc_LineAmount = profitLossSummaryDetail.ZZ_Calc_LineAmount;
				}
				return fZZ_Calc_LineAmount;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_LineAmountInfo, ref fZZ_Calc_LineAmount, value);
			}
		}

		ZDecimal fZZ_Calc_LineAmount;
		public ZPropertyInfo ZZ_Calc_LineAmountInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_LineAmount)); }
		}

		#endregion

		#region ZZ_Calc_MarginPercentage

		public ZDecimal ZZ_Calc_MarginPercentage
		{
			get
			{
				if (fZZ_Calc_MarginPercentage.IsEmpty)
				{
					fZZ_Calc_MarginPercentage = profitLossSummaryDetail.ZZ_Calc_MarginPercentage;
				}
				return fZZ_Calc_MarginPercentage;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_MarginPercentageInfo, ref fZZ_Calc_MarginPercentage, value);
			}
		}

		ZDecimal fZZ_Calc_MarginPercentage;
		public ZPropertyInfo ZZ_Calc_MarginPercentageInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_MarginPercentage)); }
		}

		#endregion

		#endregion

		#region Recognized Values

		#region ZZ_Calc_Revenue_Recognized

		public ZDecimal ZZ_Calc_Revenue_Recognized
		{
			get
			{
				if (fZZ_Calc_Revenue_Recognized.IsEmpty)
				{
					fZZ_Calc_Revenue_Recognized = profitLossSummaryDetail.ZZ_Calc_Revenue_Recognized;
				}
				return fZZ_Calc_Revenue_Recognized;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_Revenue_RecognizedInfo, ref fZZ_Calc_Revenue_Recognized, value);
			}
		}

		ZDecimal fZZ_Calc_Revenue_Recognized;
		public ZPropertyInfo ZZ_Calc_Revenue_RecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Revenue_Recognized)); }
		}

		#endregion

		#region ZZ_Calc_WIP_Recognized

		public ZDecimal ZZ_Calc_WIP_Recognized
		{
			get
			{
				if (fZZ_Calc_WIP_Recognized.IsEmpty)
				{
					fZZ_Calc_WIP_Recognized = profitLossSummaryDetail.ZZ_Calc_WIP_Recognized;
				}
				return fZZ_Calc_WIP_Recognized;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_WIP_RecognizedInfo, ref fZZ_Calc_WIP_Recognized, value);
			}
		}

		ZDecimal fZZ_Calc_WIP_Recognized;
		public ZPropertyInfo ZZ_Calc_WIP_RecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_WIP_Recognized)); }
		}

		#endregion

		#region ZZ_Calc_Cost_Recognized

		public ZDecimal ZZ_Calc_Cost_Recognized
		{
			get
			{
				if (fZZ_Calc_Cost_Recognized.IsEmpty)
				{
					fZZ_Calc_Cost_Recognized = profitLossSummaryDetail.ZZ_Calc_Cost_Recognized;
				}
				return fZZ_Calc_Cost_Recognized;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_Cost_RecognizedInfo, ref fZZ_Calc_Cost_Recognized, value);
			}
		}

		ZDecimal fZZ_Calc_Cost_Recognized;
		public ZPropertyInfo ZZ_Calc_Cost_RecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Cost_Recognized)); }
		}

		#endregion

		#region ZZ_Calc_Accrual_Recognized

		public ZDecimal ZZ_Calc_Accrual_Recognized
		{
			get
			{
				if (fZZ_Calc_Accrual_Recognized.IsEmpty)
				{
					fZZ_Calc_Accrual_Recognized = profitLossSummaryDetail.ZZ_Calc_Accrual_Recognized;
				}
				return fZZ_Calc_Accrual_Recognized;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_Accrual_RecognizedInfo, ref fZZ_Calc_Accrual_Recognized, value);
			}
		}

		ZDecimal fZZ_Calc_Accrual_Recognized;
		public ZPropertyInfo ZZ_Calc_Accrual_RecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Accrual_Recognized)); }
		}

		#endregion

		#region ZZ_Calc_LineAmount_Recognized

		public ZDecimal ZZ_Calc_LineAmount_Recognized
		{
			get
			{
				if (fZZ_Calc_LineAmount_Recognized.IsEmpty)
				{
					fZZ_Calc_LineAmount_Recognized = profitLossSummaryDetail.ZZ_Calc_LineAmount_Recognized;
				}
				return fZZ_Calc_LineAmount_Recognized;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_LineAmount_RecognizedInfo, ref fZZ_Calc_LineAmount_Recognized, value);
			}
		}

		ZDecimal fZZ_Calc_LineAmount_Recognized;
		public ZPropertyInfo ZZ_Calc_LineAmount_RecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_LineAmount_Recognized)); }
		}

		#endregion

		#region ZZ_Calc_MarginPercentage_Recognized

		public ZDecimal ZZ_Calc_MarginPercentage_Recognized
		{
			get
			{
				if (fZZ_Calc_MarginPercentage_Recognized.IsEmpty)
				{
					fZZ_Calc_MarginPercentage_Recognized = profitLossSummaryDetail.ZZ_Calc_MarginPercentage_Recognized;
				}
				return fZZ_Calc_MarginPercentage_Recognized;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_MarginPercentage_RecognizedInfo, ref fZZ_Calc_MarginPercentage_Recognized, value);
			}
		}

		ZDecimal fZZ_Calc_MarginPercentage_Recognized;
		public ZPropertyInfo ZZ_Calc_MarginPercentage_RecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_MarginPercentage_Recognized)); }
		}

		#endregion

		#endregion

		#region Not Recognized Values

		#region ZZ_Calc_Revenue_NotRecognized

		public ZDecimal ZZ_Calc_Revenue_NotRecognized
		{
			get
			{
				if (fZZ_Calc_Revenue_NotRecognized.IsEmpty)
				{
					fZZ_Calc_Revenue_NotRecognized = profitLossSummaryDetail.ZZ_Calc_Revenue_NotRecognized;
				}
				return fZZ_Calc_Revenue_NotRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_Revenue_NotRecognizedInfo, ref fZZ_Calc_Revenue_NotRecognized, value);
			}
		}

		ZDecimal fZZ_Calc_Revenue_NotRecognized;
		public ZPropertyInfo ZZ_Calc_Revenue_NotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Revenue_NotRecognized)); }
		}

		#endregion

		#region ZZ_Calc_WIP_NotRecognized

		public ZDecimal ZZ_Calc_WIP_NotRecognized
		{
			get
			{
				if (fZZ_Calc_WIP_NotRecognized.IsEmpty)
				{
					fZZ_Calc_WIP_NotRecognized = profitLossSummaryDetail.ZZ_Calc_WIP_NotRecognized;
				}
				return fZZ_Calc_WIP_NotRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_WIP_NotRecognizedInfo, ref fZZ_Calc_WIP_NotRecognized, value);
			}
		}
		ZDecimal fZZ_Calc_WIP_NotRecognized;

		public ZPropertyInfo ZZ_Calc_WIP_NotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_WIP_NotRecognized)); }
		}

		#endregion

		#region ZZ_Calc_Cost_NotRecognized

		public ZDecimal ZZ_Calc_Cost_NotRecognized
		{
			get
			{
				if (fZZ_Calc_Cost_NotRecognized.IsEmpty)
				{
					fZZ_Calc_Cost_NotRecognized = profitLossSummaryDetail.ZZ_Calc_Cost_NotRecognized;
				}
				return fZZ_Calc_Cost_NotRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_Cost_NotRecognizedInfo, ref fZZ_Calc_Cost_NotRecognized, value);
			}
		}

		ZDecimal fZZ_Calc_Cost_NotRecognized;
		public ZPropertyInfo ZZ_Calc_Cost_NotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Cost_NotRecognized)); }
		}

		#endregion

		#region ZZ_Calc_Accrual_NotRecognized

		public ZDecimal ZZ_Calc_Accrual_NotRecognized
		{
			get
			{
				if (fZZ_Calc_Accrual_NotRecognized.IsEmpty)
				{
					fZZ_Calc_Accrual_NotRecognized = profitLossSummaryDetail.ZZ_Calc_Accrual_NotRecognized;
				}
				return fZZ_Calc_Accrual_NotRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_Accrual_NotRecognizedInfo, ref fZZ_Calc_Accrual_NotRecognized, value);
			}
		}

		ZDecimal fZZ_Calc_Accrual_NotRecognized;
		public ZPropertyInfo ZZ_Calc_Accrual_NotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Accrual_NotRecognized)); }
		}

		#endregion

		#region ZZ_Calc_LineAmount_NotRecognized

		public ZDecimal ZZ_Calc_LineAmount_NotRecognized
		{
			get
			{
				if (fZZ_Calc_LineAmount_NotRecognized.IsEmpty)
				{
					fZZ_Calc_LineAmount_NotRecognized = profitLossSummaryDetail.ZZ_Calc_LineAmount_NotRecognized;
				}
				return fZZ_Calc_LineAmount_NotRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_LineAmount_NotRecognizedInfo, ref fZZ_Calc_LineAmount_NotRecognized, value);
			}
		}
		ZDecimal fZZ_Calc_LineAmount_NotRecognized;

		public ZPropertyInfo ZZ_Calc_LineAmount_NotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_LineAmount_NotRecognized)); }
		}

		#endregion

		#region ZZ_Calc_MarginPercentage_NotRecognized

		public ZDecimal ZZ_Calc_MarginPercentage_NotRecognized
		{
			get
			{
				if (fZZ_Calc_MarginPercentage_NotRecognized.IsEmpty)
				{
					fZZ_Calc_MarginPercentage_NotRecognized = profitLossSummaryDetail.ZZ_Calc_MarginPercentage_NotRecognized;
				}
				return fZZ_Calc_MarginPercentage_NotRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_MarginPercentage_NotRecognizedInfo, ref fZZ_Calc_MarginPercentage_NotRecognized, value);
			}
		}

		ZDecimal fZZ_Calc_MarginPercentage_NotRecognized;
		public ZPropertyInfo ZZ_Calc_MarginPercentage_NotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_MarginPercentage_NotRecognized)); }
		}

		#endregion

		#endregion

		#region Tax Expense Values

		public ZDecimal ZZ_Calc_TaxExpenseRevenue
		{
			get
			{
				if (fZZ_Calc_TaxExpenseRevenue.IsEmpty)
				{
					fZZ_Calc_TaxExpenseRevenue = profitLossSummaryDetail.ZZ_Calc_TaxExpenseRevenue;
				}
				return fZZ_Calc_TaxExpenseRevenue;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_TaxExpenseRevenueInfo, ref fZZ_Calc_TaxExpenseRevenue, value);
			}
		}

		ZDecimal fZZ_Calc_TaxExpenseRevenue;

		public ZPropertyInfo ZZ_Calc_TaxExpenseRevenueInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_TaxExpenseRevenue)); }
		}

		public ZDecimal ZZ_Calc_TaxExpenseCost
		{
			get
			{
				if (fZZ_Calc_TaxExpenseCost.IsEmpty)
				{
					fZZ_Calc_TaxExpenseCost = profitLossSummaryDetail.ZZ_Calc_TaxExpenseCost;
				}
				return fZZ_Calc_TaxExpenseCost;
			}
			set
			{
				SetNonPersistentPropertyValue(ZZ_Calc_TaxExpenseCostInfo, ref fZZ_Calc_TaxExpenseCost, value);
			}
		}

		ZDecimal fZZ_Calc_TaxExpenseCost;

		public ZPropertyInfo ZZ_Calc_TaxExpenseCostInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_TaxExpenseCost)); }
		}

		#endregion

		#endregion

		#region Lookups

		#region Charge Codes

		public AccChargeCodeCollection ChargeCodes
		{
			get
			{
				if (fChargeCodes == null)
				{
					fChargeCodes = new AccChargeCodeCollection(Factory);
				}

				return fChargeCodes;
			}
		}

		AccChargeCodeCollection fChargeCodes;

		#endregion

		#region Branches

		public GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchCollection(Factory);
				}

				return fBranches;
			}
		}

		GlbBranchCollection fBranches;

		#endregion

		#region Departments

		public GlbDepartmentCollection Departments
		{
			get
			{
				if (fDepartments == null)
				{
					fDepartments = new GlbDepartmentCollection(Factory);
				}

				return fDepartments;
			}
		}

		GlbDepartmentCollection fDepartments;

		#endregion

		#region Jobs

		public JobHeaderCollection Jobs
		{
			get
			{
				if (fJobs == null)
				{
					fJobs = new JobHeaderCollection(Factory);
				}

				return fJobs;
			}
		}

		JobHeaderCollection fJobs;

		#endregion

		#endregion
	}
}

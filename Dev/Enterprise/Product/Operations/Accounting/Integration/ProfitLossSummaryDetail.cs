using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	public class ProfitLossSummaryDetail : DynamicBusinessObject, IObsoleteValidation
	{
		public ProfitLossSummaryDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		#region Decimals

		public ZInt Decimals
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.Decimals; }
		}

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
			get { return new ZGuid(Row[ZZ_Calc_ACInfo.Name]); }
		}

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
			get { return new ZGuid(Row[ZZ_Calc_JHInfo.Name]); }
		}

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
			get { return new ZGuid(Row[ZZ_Calc_GBInfo.Name]); }
		}

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
			get { return new ZGuid(Row[ZZ_Calc_GEInfo.Name]); }
		}

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
			get { return new ZString(Row[ZZ_Calc_JobLocalReferenceNumInfo.Name]); }
		}

		public ZPropertyInfo ZZ_Calc_JobLocalReferenceNumInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_JobLocalReferenceNum)); }
		}

		#endregion

		#region ZZ_Calc_ChargeCodeDescription

		public ZString ZZ_Calc_ChargeCodeDescription
		{
			get { return new ZString(Row[ZZ_Calc_ChargeCodeDescriptionInfo.Name]); }
		}

		public ZPropertyInfo ZZ_Calc_ChargeCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_ChargeCodeDescription)); }
		}

		#endregion

		#region Summary Values (Recognized + Not Recognized)

		#region ZZ_Calc_Revenue

		public ZDecimal ZZ_Calc_Revenue
		{
			get { return ZZ_Calc_Revenue_Recognized + ZZ_Calc_Revenue_NotRecognized; }
		}

		public ZPropertyInfo ZZ_Calc_RevenueInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Revenue)); }
		}

		#endregion

		#region ZZ_Calc_WIP

		public ZDecimal ZZ_Calc_WIP
		{
			get { return ZZ_Calc_WIP_Recognized + ZZ_Calc_WIP_NotRecognized; }
		}

		public ZPropertyInfo ZZ_Calc_WIPInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_WIP)); }
		}

		#endregion

		#region ZZ_Calc_Cost

		public ZDecimal ZZ_Calc_Cost
		{
			get { return ZZ_Calc_Cost_Recognized + ZZ_Calc_Cost_NotRecognized; }
		}

		public ZPropertyInfo ZZ_Calc_CostInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Cost)); }
		}

		#endregion

		#region ZZ_Calc_Accrual

		public ZDecimal ZZ_Calc_Accrual
		{
			get { return ZZ_Calc_Accrual_Recognized + ZZ_Calc_Accrual_NotRecognized; }
		}

		public ZPropertyInfo ZZ_Calc_AccrualInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Accrual)); }
		}

		#endregion

		#region ZZ_Calc_LineAmount

		public ZDecimal ZZ_Calc_LineAmount
		{
			get { return ZZ_Calc_LineAmount_Recognized + ZZ_Calc_LineAmount_NotRecognized; }
		}

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
				decimal dividend = ZZ_Calc_LineAmount;
				decimal divisor = ZZ_Calc_Revenue + ZZ_Calc_WIP;
				decimal marginPercentage = 0.00M;

				if (divisor == 0M)
				{
					marginPercentage = dividend;
				}
				else
				{
					marginPercentage = (dividend / divisor) * 100;
				}

				return marginPercentage;
			}
		}

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
			get { return new ZDecimal(Row[ZZ_Calc_Revenue_RecognizedInfo.Name]); }
		}

		public ZPropertyInfo ZZ_Calc_Revenue_RecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Revenue_Recognized)); }
		}

		#endregion

		#region ZZ_Calc_WIP_Recognized

		public ZDecimal ZZ_Calc_WIP_Recognized
		{
			get { return new ZDecimal(Row[ZZ_Calc_WIP_RecognizedInfo.Name]); }
		}

		public ZPropertyInfo ZZ_Calc_WIP_RecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_WIP_Recognized)); }
		}

		#endregion

		#region ZZ_Calc_Cost_Recognized

		public ZDecimal ZZ_Calc_Cost_Recognized
		{
			get { return new ZDecimal(Row[ZZ_Calc_Cost_RecognizedInfo.Name]); }
		}

		public ZPropertyInfo ZZ_Calc_Cost_RecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Cost_Recognized)); }
		}

		#endregion

		#region ZZ_Calc_Accrual_Recognized

		public ZDecimal ZZ_Calc_Accrual_Recognized
		{
			get { return new ZDecimal(Row[ZZ_Calc_Accrual_RecognizedInfo.Name]); }
		}

		public ZPropertyInfo ZZ_Calc_Accrual_RecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Accrual_Recognized)); }
		}

		#endregion

		#region ZZ_Calc_LineAmount_Recognized

		public ZDecimal ZZ_Calc_LineAmount_Recognized
		{
			get { return new ZDecimal(Row[ZZ_Calc_LineAmount_RecognizedInfo.Name]); }
		}

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
				decimal dividend = ZZ_Calc_LineAmount_Recognized;
				decimal divisor = ZZ_Calc_Revenue_Recognized + ZZ_Calc_WIP_Recognized;
				decimal marginPercentage = 0.00M;

				if (divisor == 0M)
				{
					marginPercentage = dividend;
				}
				else
				{
					marginPercentage = (dividend / divisor) * 100;
				}

				return marginPercentage;
			}
		}

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
			get { return new ZDecimal(Row[ZZ_Calc_Revenue_NotRecognizedInfo.Name]); }
		}

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
				if (fZZ_Calc_WIP_NotRecognized == null)
				{
					fZZ_Calc_WIP_NotRecognized = new ZDecimal(Row[ZZ_Calc_WIP_NotRecognizedInfo.Name]) + CFXAmount;
				}
				return fZZ_Calc_WIP_NotRecognized.Value;
			}
		}
		ZDecimal? fZZ_Calc_WIP_NotRecognized;

		public ZPropertyInfo ZZ_Calc_WIP_NotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_WIP_NotRecognized)); }
		}

		#endregion

		#region CFXAmount

		public delegate ZDecimal CFXCalculationHandler(ZGuid jobPK, ZGuid chargeCodePK, ZGuid branchPK, ZGuid departmentPK);

		public CFXCalculationHandler GetCFXAmount { private get; set; }

		ZDecimal CFXAmount
		{
			get
			{
				if (fCFXAmount == null)
				{
					fCFXAmount = GetCFXAmount == null ? ZDecimal.Zero : GetCFXAmount(ZZ_Calc_JH, ZZ_Calc_AC, ZZ_Calc_GB, ZZ_Calc_GE);
				}
				return fCFXAmount.Value;
			}
		}
		ZDecimal? fCFXAmount;

		#endregion

		#region ZZ_Calc_Cost_NotRecognized

		public ZDecimal ZZ_Calc_Cost_NotRecognized
		{
			get { return new ZDecimal(Row[ZZ_Calc_Cost_NotRecognizedInfo.Name]); }
		}

		public ZPropertyInfo ZZ_Calc_Cost_NotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_Cost_NotRecognized)); }
		}

		#endregion

		#region ZZ_Calc_Accrual_NotRecognized

		public ZDecimal ZZ_Calc_Accrual_NotRecognized
		{
			get { return new ZDecimal(Row[ZZ_Calc_Accrual_NotRecognizedInfo.Name]); }
		}

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
				if (fZZ_Calc_LineAmount_NotRecognized == null)
				{
					fZZ_Calc_LineAmount_NotRecognized = new ZDecimal(Row[ZZ_Calc_LineAmount_NotRecognizedInfo.Name]) + CFXAmount;
				}
				return fZZ_Calc_LineAmount_NotRecognized.Value;
			}
		}
		ZDecimal? fZZ_Calc_LineAmount_NotRecognized;

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
				decimal dividend = ZZ_Calc_LineAmount_NotRecognized;
				decimal divisor = ZZ_Calc_Revenue_NotRecognized + ZZ_Calc_WIP_NotRecognized;
				decimal marginPercentage = 0.00M;

				if (divisor == 0M)
				{
					marginPercentage = dividend;
				}
				else
				{
					marginPercentage = (dividend / divisor) * 100;
				}

				return marginPercentage;
			}
		}

		public ZPropertyInfo ZZ_Calc_MarginPercentage_NotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_MarginPercentage_NotRecognized)); }
		}

		#endregion

		#endregion

		#region Tax Expense Values

		public ZDecimal ZZ_Calc_TaxExpenseRevenue
		{
			get { return new ZDecimal(Row[ZZ_Calc_TaxExpenseRevenueInfo.Name]); }
		}

		public ZPropertyInfo ZZ_Calc_TaxExpenseRevenueInfo
		{
			get { return GetZPropertyInfo(nameof(ZZ_Calc_TaxExpenseRevenue)); }
		}

		public ZDecimal ZZ_Calc_TaxExpenseCost
		{
			get { return new ZDecimal(Row[ZZ_Calc_TaxExpenseCostInfo.Name]); }
		}

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

		#region Implementation

		DataRow Row
		{
			get { return ((IBusinessObjectInternals)this).Row; }
		}

		#endregion
	}
}

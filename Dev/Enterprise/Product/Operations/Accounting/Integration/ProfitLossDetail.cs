using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	public class ProfitLossDetail : DynamicBusinessObject, IObsoleteValidation, IProfitLossTotals
	{
		public ProfitLossDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
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

		#region ZY_Calc_SystemCreateTime

		public ZDateTime ZY_Calc_SystemCreateTime
		{
			get { return new ZDateTime(Row[ZY_Calc_SystemCreateTimeInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_SystemCreateTimeInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_SystemCreateTime)); }
		}

		#endregion

		#region ZY_Calc_ReversalDate

		public ZDateTime ZY_Calc_ReversalDate
		{
			get { return new ZDateTime(Row[ZY_Calc_ReversalDateInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_ReversalDateInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_ReversalDate)); }
		}

		#endregion

		#region ZY_Calc_AC

		[RelatedBusinessObject("ChargeCode")]
		[List("ChargeCodes")]
		public ZGuid ZY_Calc_AC
		{
			get { return new ZGuid(Row[ZY_Calc_ACInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_ACInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_AC)); }
		}

		public AccChargeCode ChargeCode
		{
			get { return (AccChargeCode)Factory.Load(typeof(AccChargeCode), ZY_Calc_AC); }
		}

		#endregion

		#region ZY_Calc_JH

		[RelatedBusinessObject("Job")]
		[List("Jobs")]
		public ZGuid ZY_Calc_JH
		{
			get { return new ZGuid(Row[ZY_Calc_JHInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_JHInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_JH)); }
		}

		public JobHeader Job
		{
			get { return (JobHeader)Factory.Load(typeof(JobHeader), ZY_Calc_JH); }
		}

		#endregion

		#region ZY_Calc_GB

		[RelatedBusinessObject("Branch")]
		[List("Branches")]
		public ZGuid ZY_Calc_GB
		{
			get { return new ZGuid(Row[ZY_Calc_GBInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_GBInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_GB)); }
		}

		public GlbBranch Branch
		{
			get { return (GlbBranch)Factory.Load(typeof(GlbBranch), ZY_Calc_GB); }
		}

		#endregion

		#region ZY_Calc_GE

		[RelatedBusinessObject("Department")]
		[List("Departments")]
		public ZGuid ZY_Calc_GE
		{
			get { return new ZGuid(Row[ZY_Calc_GEInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_GEInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_GE)); }
		}

		public GlbDepartment Department
		{
			get { return (GlbDepartment)Factory.Load(typeof(GlbDepartment), ZY_Calc_GE); }
		}

		#endregion

		#region ZY_Calc_GC

		[RelatedBusinessObject("Company")]
		[List("Companies")]
		public ZGuid ZY_Calc_GC
		{
			get { return new ZGuid(Row[ZY_Calc_GCInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_GCInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_GC)); }
		}

		public GlbCompany Company
		{
			get { return Factory.Load<GlbCompany>(ZY_Calc_GC); }
		}

		#endregion

		#region ZY_Calc_ChargeCodeDescription

		public ZString ZY_Calc_ChargeCodeDescription
		{
			get { return new ZString(Row[ZY_Calc_ChargeCodeDescriptionInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_ChargeCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_ChargeCodeDescription)); }
		}

		#endregion

		#region ZY_Calc_InvoiceDate

		public ZDateTime ZY_Calc_InvoiceDate
		{
			get { return new ZDateTime(Row[ZY_Calc_InvoiceDateInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_InvoiceDateInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_InvoiceDate)); }
		}

		#endregion

		#region ZY_Calc_LineAmount

		public ZDecimal ZY_Calc_LineAmount
		{
			get
			{
				if (fZY_Calc_LineAmount == null)
				{
					fZY_Calc_LineAmount = new ZDecimal(Row[ZY_Calc_LineAmountInfo.Name]) + (GetCFXAmount == null ? ZDecimal.Zero : GetCFXAmount(ZY_Calc_JR, ZY_Calc_LineType, ZY_Calc_RecognizedDate));
				}
				return fZY_Calc_LineAmount.Value;
			}
		}
		ZDecimal? fZY_Calc_LineAmount;

		public ZPropertyInfo ZY_Calc_LineAmountInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_LineAmount)); }
		}

		public delegate ZDecimal CFXCalculationHandler(ZGuid chargePK, ZString lineType, ZDateTime recognizedDate);

		public CFXCalculationHandler GetCFXAmount { private get; set; }

		#endregion

		#region ZY_Calc_LineType

		public ZString ZY_Calc_LineType
		{
			get { return new ZString(Row[ZY_Calc_LineTypeInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_LineTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_LineType)); }
		}

		#endregion

		#region ZY_Calc_PostDate

		public ZDateTime ZY_Calc_PostDate
		{
			get { return new ZDateTime(Row[ZY_Calc_PostDateInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_PostDateInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_PostDate)); }
		}

		#endregion

		#region ZY_Calc_FullyPaidDate

		public ZDateTime ZY_Calc_FullyPaidDate
		{
			get { return new ZDateTime(Row[ZY_Calc_FullyPaidDateInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_FullyPaidDateInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_FullyPaidDate)); }
		}

		#endregion

		#region ZY_Calc_JobLocalReferenceNum

		public ZString ZY_Calc_JobLocalReferenceNum
		{
			get { return new ZString(Row[ZY_Calc_JobLocalReferenceNumInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_JobLocalReferenceNumInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_JobLocalReferenceNum)); }
		}

		#endregion

		#region ZY_Calc_TransactionNum

		public ZString ZY_Calc_TransactionNum
		{
			get { return new ZString(Row[ZY_Calc_TransactionNumInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_TransactionNumInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_TransactionNum)); }
		}

		#endregion

		#region ZY_Calc_Ledger

		public ZString ZY_Calc_Ledger
		{
			get { return new ZString(Row[ZY_Calc_LedgerInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_LedgerInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_Ledger)); }
		}

		#endregion

		#region ZY_Calc_TransactionType

		public ZString ZY_Calc_TransactionType
		{
			get { return new ZString(Row[ZY_Calc_TransactionTypeInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_TransactionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_TransactionType)); }
		}

		#endregion

		#region ZY_Calc_OH

		[RelatedBusinessObject("Organisation")]
		[List("Organisations")]
		public ZGuid ZY_Calc_OH
		{
			get { return new ZGuid(Row[ZY_Calc_OHInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_OHInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_OH)); }
		}

		public OrgHeader Organisation
		{
			get { return (OrgHeader)Factory.Load(typeof(OrgHeader), ZY_Calc_OH); }
		}

		#endregion

		#region ZY_Calc_AL

		public ZGuid ZY_Calc_AL
		{
			get { return new ZGuid(Row[ZY_Calc_ALInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_ALInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_AL)); }
		}

		#endregion

		#region ZY_Calc_AH

		public ZGuid ZY_Calc_AH
		{
			get { return new ZGuid(Row[ZY_Calc_AHInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_AHInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_AH)); }
		}

		#endregion

		#region ZY_Calc_AuditBy

		public ZString ZY_Calc_AuditedBy => new ZString(Row[ZY_Calc_AuditedByInfo.Name]);

		public ZPropertyInfo ZY_Calc_AuditedByInfo => GetZPropertyInfo(nameof(ZY_Calc_AuditedBy));

		#endregion

		#region ZY_Calc_JR

		public ZGuid ZY_Calc_JR
		{
			get { return new ZGuid(Row[ZY_Calc_JRInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_JRInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_JR)); }
		}

		#endregion

		#region ZY_Calc_RecognizedDate

		public ZDateTime ZY_Calc_RecognizedDate
		{
			get { return new ZDateTime(Row[ZY_Calc_RecognizedDateInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_RecognizedDateInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_RecognizedDate)); }
		}

		#endregion

		#region ZY_Calc_RecognitionType

		public ZString ZY_Calc_RecognitionType
		{
			get { return new ZString(Row[ZY_Calc_RecognitionTypeInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_RecognitionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_RecognitionType)); }
		}

		#endregion

		#region ZY_Calc_ConsolNum

		public ZString ZY_Calc_ConsolNum
		{
			get { return new ZString(Row[ZY_Calc_ConsolNumInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_ConsolNumInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_ConsolNum)); }
		}

		#endregion

		#region ZY_Calc_LocalCurrency

		public ZString ZY_Calc_LocalCurrency
		{
			get { return new ZString(Row[ZY_Calc_LocalCurrencyInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_LocalCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_LocalCurrency)); }
		}

		#endregion

		#region ZY_Calc_APLine

		public ZGuid ZY_Calc_APLine
		{
			get { return new ZGuid(Row[ZY_Calc_APLineInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_APLineInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_APLine)); }
		}

		#endregion

		#region ZY_Calc_ARLine

		public ZGuid ZY_Calc_ARLine
		{
			get { return new ZGuid(Row[ZY_Calc_ARLineInfo.Name]); }
		}

		public ZPropertyInfo ZY_Calc_ARLineInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_ARLine)); }
		}

		#endregion

		#region Company Totals

		#region TotalAccrual

		public ZDecimal TotalAccrual
		{
			get { return new ZDecimal(Row[TotalAccrualInfo.Name]); }
		}

		public ZPropertyInfo TotalAccrualInfo
		{
			get { return GetZPropertyInfo(nameof(TotalAccrual)); }
		}

		#endregion

		#region TotalWIP

		public ZDecimal TotalWIP
		{
			get { return new ZDecimal(Row[TotalWIPInfo.Name]); }
		}

		public ZPropertyInfo TotalWIPInfo
		{
			get { return GetZPropertyInfo(nameof(TotalWIP)); }
		}

		#endregion

		#region TotalCost

		public ZDecimal TotalCost
		{
			get { return new ZDecimal(Row[TotalCostInfo.Name]); }
		}

		public ZPropertyInfo TotalCostInfo
		{
			get { return GetZPropertyInfo(nameof(TotalCost)); }
		}

		#endregion

		#region TotalRevenue

		public ZDecimal TotalRevenue
		{
			get { return new ZDecimal(Row[TotalRevenueInfo.Name]); }
		}

		public ZPropertyInfo TotalRevenueInfo
		{
			get { return GetZPropertyInfo(nameof(TotalRevenue)); }
		}

		#endregion

		#region Company Total Profit / Loss

		public ZDecimal TotalLineAmount
		{
			get { return new ZDecimal(Row[TotalLineAmountInfo.Name]); }
		}

		public ZPropertyInfo TotalLineAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalLineAmount)); }
		}

		#endregion

		#region Margin Profit/Rev

		public ZString MarginProfitRev
		{
			get { return FormatPercents(TotalRevenue + TotalWIP == 0 ? 0 : TotalLineAmount / (TotalRevenue + TotalWIP)); }
		}

		public ZPropertyInfo MarginProfitRevInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitRev)); }
		}

		#endregion

		#region Margin Profit/Cost

		public ZString MarginProfitCost
		{
			get { return FormatPercents(TotalCost + TotalAccrual == 0 ? 0 : -TotalLineAmount / (TotalCost + TotalAccrual)); }
		}

		public ZPropertyInfo MarginProfitCostInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitCost)); }
		}

		#endregion

		#region TotalRevenueRecognized

		public ZDecimal TotalRevenueRecognized
		{
			get { return new ZDecimal(Row[TotalRevenueRecognizedInfo.Name]); }
		}

		public ZPropertyInfo TotalRevenueRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalRevenueRecognized)); }
		}

		#endregion

		#region TotalWIPRecognized

		public ZDecimal TotalWIPRecognized
		{
			get { return new ZDecimal(Row[TotalWIPRecognizedInfo.Name]); }
		}

		public ZPropertyInfo TotalWIPRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalWIPRecognized)); }
		}

		#endregion

		#region TotalCostRecognized

		public ZDecimal TotalCostRecognized
		{
			get { return new ZDecimal(Row[TotalCostRecognizedInfo.Name]); }
		}

		public ZPropertyInfo TotalCostRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalCostRecognized)); }
		}

		#endregion

		#region TotalAccrualRecognized

		public ZDecimal TotalAccrualRecognized
		{
			get { return new ZDecimal(Row[TotalAccrualRecognizedInfo.Name]); }
		}

		public ZPropertyInfo TotalAccrualRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalAccrualRecognized)); }
		}

		#endregion

		#region Total Profit / Loss Recognized

		public ZDecimal TotalLineAmountRecognized
		{
			get { return TotalAccrualRecognized + TotalCostRecognized + TotalWIPRecognized + TotalRevenueRecognized; }
		}

		public ZPropertyInfo TotalLineAmountRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalLineAmountRecognized)); }
		}

		#endregion

		#region Margin Profit/Rev

		public ZString MarginProfitRevRecognized
		{
			get { return FormatPercents(TotalRevenueRecognized + TotalWIPRecognized == 0 ? 0 : TotalLineAmountRecognized / (TotalRevenueRecognized + TotalWIPRecognized)); }
		}

		public ZPropertyInfo MarginProfitRevRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitRevRecognized)); }
		}

		#endregion

		#region Margin Profit/Cost

		public ZString MarginProfitCostRecognized
		{
			get { return FormatPercents(TotalCostRecognized + TotalAccrualRecognized == 0 ? 0 : -TotalLineAmountRecognized / (TotalCostRecognized + TotalAccrualRecognized)); }
		}

		public ZPropertyInfo MarginProfitCostRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitCostRecognized)); }
		}

		#endregion

		#region TotalRevenueNotRecognized

		public ZDecimal TotalRevenueNotRecognized
		{
			get { return new ZDecimal(Row[TotalRevenueNotRecognizedInfo.Name]); }
		}

		public ZPropertyInfo TotalRevenueNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalRevenueNotRecognized)); }
		}

		#endregion

		#region TotalWIPNotRecognized

		public ZDecimal TotalWIPNotRecognized
		{
			get { return new ZDecimal(Row[TotalWIPNotRecognizedInfo.Name]); }
		}

		public ZPropertyInfo TotalWIPNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalWIPNotRecognized)); }
		}

		#endregion

		#region TotalCostNotRecognized

		public ZDecimal TotalCostNotRecognized
		{
			get { return new ZDecimal(Row[TotalCostNotRecognizedInfo.Name]); }
		}

		public ZPropertyInfo TotalCostNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalCostNotRecognized)); }
		}

		#endregion

		#region TotalAccrualNotRecognized

		public ZDecimal TotalAccrualNotRecognized
		{
			get { return new ZDecimal(Row[TotalAccrualNotRecognizedInfo.Name]); }
		}

		public ZPropertyInfo TotalAccrualNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalAccrualNotRecognized)); }
		}

		#endregion

		#region Total Profit / Loss Not Recognized

		public ZDecimal TotalLineAmountNotRecognized
		{
			get { return TotalAccrualNotRecognized + TotalCostNotRecognized + TotalWIPNotRecognized + TotalRevenueNotRecognized; }
		}

		public ZPropertyInfo TotalLineAmountNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalLineAmountNotRecognized)); }
		}

		#endregion

		#region MarginProfitCostNotRecognized

		public ZString MarginProfitCostNotRecognized
		{
			get { return FormatPercents(TotalCostNotRecognized + TotalAccrualNotRecognized == 0 ? 0 : -TotalLineAmountNotRecognized / (TotalCostNotRecognized + TotalAccrualNotRecognized)); }
		}

		public ZPropertyInfo MarginProfitCostNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitCostNotRecognized)); }
		}

		#endregion

		#region MarginProfitRevNotRecognized

		public ZString MarginProfitRevNotRecognized
		{
			get { return FormatPercents(TotalRevenueNotRecognized + TotalWIPNotRecognized == 0 ? 0 : TotalLineAmountNotRecognized / (TotalRevenueNotRecognized + TotalWIPNotRecognized)); }
		}

		public ZPropertyInfo MarginProfitRevNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitRevNotRecognized)); }
		}

		#endregion

		#region Tax Expense

		public ZDecimal TotalTaxExpenseRevenue
		{
			get { return new ZDecimal(Row[TotalTaxExpenseRevenueInfo.Name]); }
		}

		public ZPropertyInfo TotalTaxExpenseRevenueInfo
		{
			get { return GetZPropertyInfo(nameof(TotalTaxExpenseRevenue)); }
		}

		public ZDecimal TotalTaxExpenseCost
		{
			get { return new ZDecimal(Row[TotalTaxExpenseCostInfo.Name]); }
		}

		public ZPropertyInfo TotalTaxExpenseCostInfo
		{
			get { return GetZPropertyInfo(nameof(TotalTaxExpenseCost)); }
		}

		#endregion

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
			get { return fBranches ?? (fBranches = new GlbBranchCollection(Factory)); }
		}
		GlbBranchCollection fBranches;

		#endregion

		#region Departments

		public GlbDepartmentCollection Departments
		{
			get { return fDepartments ?? (fDepartments = new GlbDepartmentCollection(Factory)); }
		}
		GlbDepartmentCollection fDepartments;

		#endregion

		#region Organisations

		public OrganisationsFindBoxCollection Organisations
		{
			get { return fOrganisations ?? (fOrganisations = new OrganisationsFindBoxCollection(Factory)); }
		}
		OrganisationsFindBoxCollection fOrganisations;

		#endregion

		#region Jobs

		public JobHeaderCollection Jobs
		{
			get { return fJobs ?? (fJobs = new JobHeaderCollection(Factory)); }
		}
		JobHeaderCollection fJobs;

		#endregion

		#region Companies

		public GlbCompanyCollection Companies
		{
			get { return fCompanies ?? (fCompanies = new GlbCompanyCollection(Factory)); }
		}
		GlbCompanyCollection fCompanies;

		#endregion

		#endregion

		#region Implementation

		DataRow Row
		{
			get { return ((IBusinessObjectInternals)this).Row; }
		}

		ZString FormatPercents(ZDecimal num)
		{
			return num.ToString("P", Env.CurrentCompany.Country.Culture);
		}

		#endregion
	}
}

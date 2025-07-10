using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	public class ProfitLossDetailView : NonPersistentBusinessObject
	{
		#region Schema

		public abstract class Schema
		{
			public const string Decimals = "Decimals";
			public const string ZY_Calc_SystemCreateTime = "ZY_Calc_SystemCreateTime";
			public const string ZY_Calc_ReversalDate = "ZY_Calc_ReversalDate";
			public const string ZY_Calc_AC = "ZY_Calc_AC";
			public const string ZY_Calc_JH = "ZY_Calc_JH";
			public const string ZY_Calc_GB = "ZY_Calc_GB";
			public const string ZY_Calc_GE = "ZY_Calc_GE";
			public const string ZY_Calc_GC = "ZY_Calc_GC";
			public const string ZY_Calc_ChargeCodeDescription = "ZY_Calc_ChargeCodeDescription";
			public const string ZY_Calc_InvoiceDate = "ZY_Calc_InvoiceDate";
			public const string ZY_Calc_LineAmount = "ZY_Calc_LineAmount";
			public const string ZY_Calc_LineType = "ZY_Calc_LineType";
			public const string ZY_Calc_PostDate = "ZY_Calc_PostDate";
			public const string ZY_Calc_FullyPaidDate = "ZY_Calc_FullyPaidDate";
			public const string ZY_Calc_JobLocalReferenceNum = "ZY_Calc_JobLocalReferenceNum";
			public const string ZY_Calc_TransactionNum = "ZY_Calc_TransactionNum";
			public const string ZY_Calc_TransactionType = "ZY_Calc_TransactionType";
			public const string ZY_Calc_OH = "ZY_Calc_OH";
			public const string ZY_Calc_AL = "ZY_Calc_AL";
			public const string ZY_Calc_AH = "ZY_Calc_AH";
			public const string ZY_Calc_JR = "ZY_Calc_JR";
			public const string ZY_Calc_RecognizedDate = "ZY_Calc_RecognizedDate";
			public const string ZY_Calc_RecognitionType = "ZY_Calc_RecognitionType";
			public const string ZY_Calc_ConsolNum = "ZY_Calc_ConsolNum";
			public const string ZY_Calc_LocalCurrency = "ZY_Calc_LocalCurrency";
			public const string TotalAccrual = "TotalAccrual";
			public const string TotalWIP = "TotalWIP";
			public const string TotalCost = "TotalCost";
			public const string TotalRevenue = "TotalRevenue";
			public const string TotalLineAmount = "TotalLineAmount";
			public const string MarginProfitRev = "MarginProfitRev";
			public const string MarginProfitCost = "MarginProfitCost";
			public const string TotalRevenueRecognized = "TotalRevenueRecognized";
			public const string TotalWIPRecognized = "TotalWIPRecognized";
			public const string TotalCostRecognized = "TotalCostRecognized";
			public const string TotalAccrualRecognized = "TotalAccrualRecognized";
			public const string TotalLineAmountRecognized = "TotalLineAmountRecognized";
			public const string MarginProfitRevRecognized = "MarginProfitRevRecognized";
			public const string MarginProfitCostRecognized = "MarginProfitCostRecognized";
			public const string TotalRevenueNotRecognized = "TotalRevenueNotRecognized";
			public const string TotalWIPNotRecognized = "TotalWIPNotRecognized";
			public const string TotalCostNotRecognized = "TotalCostNotRecognized";
			public const string TotalAccrualNotRecognized = "TotalAccrualNotRecognized";
			public const string TotalLineAmountNotRecognized = "TotalLineAmountNotRecognized";
			public const string MarginProfitCostNotRecognized = "MarginProfitCostNotRecognized";
			public const string MarginProfitRevNotRecognized = "MarginProfitRevNotRecognized";
		}

		#endregion

		public ProfitLossDetailView(ProfitLossDetail profitLossDetail)
			: base(profitLossDetail.Factory)
		{
			this.profitLossDetail = profitLossDetail;
		}

		public ProfitLossDetail ProfitLossDetail { get { return profitLossDetail; } }
		readonly ProfitLossDetail profitLossDetail;

		#region Properties

		#region Decimals

		public ZInt Decimals
		{
			get
			{
				if (fDecimals.IsEmpty)
				{
					fDecimals = profitLossDetail.Decimals;
				}
				return fDecimals;
			}
			set
			{
				SetNonPersistentPropertyValue(DecimalsInfo, ref fDecimals, value);
			}
		}

		public ZInt fDecimals;
		public ZPropertyInfo DecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(Decimals)); }
		}

		#endregion

		#region ZY_Calc_SystemCreateTime

		public ZDateTime ZY_Calc_SystemCreateTime
		{
			get
			{
				if (fZY_Calc_SystemCreateTime.IsEmpty)
				{
					fZY_Calc_SystemCreateTime = profitLossDetail.ZY_Calc_SystemCreateTime;
				}
				return fZY_Calc_SystemCreateTime;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_SystemCreateTimeInfo, ref fZY_Calc_SystemCreateTime, value);
			}
		}

		ZDateTime fZY_Calc_SystemCreateTime;

		public ZPropertyInfo ZY_Calc_SystemCreateTimeInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_SystemCreateTime)); }
		}

		#endregion

		#region ZY_Calc_ReversalDate

		public ZDateTime ZY_Calc_ReversalDate
		{
			get
			{
				if (fZY_Calc_ReversalDate.IsEmpty)
				{
					fZY_Calc_ReversalDate = profitLossDetail.ZY_Calc_ReversalDate;
				}
				return fZY_Calc_ReversalDate;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_ReversalDateInfo, ref fZY_Calc_ReversalDate, value);
			}
		}

		ZDateTime fZY_Calc_ReversalDate;
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
			get
			{
				if (fZY_Calc_AC.IsEmpty)
				{
					fZY_Calc_AC = profitLossDetail.ZY_Calc_AC;
				}
				return fZY_Calc_AC;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_ACInfo, ref fZY_Calc_AC, value);
			}
		}

		ZGuid fZY_Calc_AC;
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
			get
			{
				if (fZY_Calc_JH.IsEmpty)
				{
					fZY_Calc_JH = profitLossDetail.ZY_Calc_JH;
				}
				return fZY_Calc_JH;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_JHInfo, ref fZY_Calc_JH, value);
			}
		}

		ZGuid fZY_Calc_JH;
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
			get
			{
				if (fZY_Calc_GB.IsEmpty)
				{
					fZY_Calc_GB = profitLossDetail.ZY_Calc_GB;
				}
				return fZY_Calc_GB;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_GBInfo, ref fZY_Calc_GB, value);
			}
		}

		ZGuid fZY_Calc_GB;
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
			get
			{
				if (fZY_Calc_GE.IsEmpty)
				{
					fZY_Calc_GE = profitLossDetail.ZY_Calc_GE;
				}
				return fZY_Calc_GE;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_GEInfo, ref fZY_Calc_GE, value);
			}
		}

		ZGuid fZY_Calc_GE;
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
			get
			{
				if (fZY_Calc_GC.IsEmpty)
				{
					fZY_Calc_GC = profitLossDetail.ZY_Calc_GC;
				}
				return fZY_Calc_GC;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_GCInfo, ref fZY_Calc_GC, value);
			}
		}

		ZGuid fZY_Calc_GC;
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
			get
			{
				if (fZY_Calc_ChargeCodeDescription.IsEmpty)
				{
					fZY_Calc_ChargeCodeDescription = profitLossDetail.ZY_Calc_ChargeCodeDescription;
				}
				return fZY_Calc_ChargeCodeDescription;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_ChargeCodeDescriptionInfo, ref fZY_Calc_ChargeCodeDescription, value);
			}
		}

		ZString fZY_Calc_ChargeCodeDescription;
		public ZPropertyInfo ZY_Calc_ChargeCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_ChargeCodeDescription)); }
		}

		#endregion

		#region ZY_Calc_InvoiceDate

		public ZDateTime ZY_Calc_InvoiceDate
		{
			get
			{
				if (fZY_Calc_InvoiceDate.IsEmpty)
				{
					fZY_Calc_InvoiceDate = profitLossDetail.ZY_Calc_InvoiceDate;
				}
				return fZY_Calc_InvoiceDate;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_InvoiceDateInfo, ref fZY_Calc_InvoiceDate, value);
			}
		}

		ZDateTime fZY_Calc_InvoiceDate;
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
				if (fZY_Calc_LineAmount.IsEmpty)
				{
					fZY_Calc_LineAmount = profitLossDetail.ZY_Calc_LineAmount;
				}
				return fZY_Calc_LineAmount;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_LineAmountInfo, ref fZY_Calc_LineAmount, value);
			}
		}
		ZDecimal fZY_Calc_LineAmount;

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
			get
			{
				if (fZY_Calc_LineType.IsEmpty)
				{
					fZY_Calc_LineType = profitLossDetail.ZY_Calc_LineType;
				}
				return fZY_Calc_LineType;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_LineTypeInfo, ref fZY_Calc_LineType, value);
			}
		}

		ZString fZY_Calc_LineType;
		public ZPropertyInfo ZY_Calc_LineTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_LineType)); }
		}

		#endregion

		#region ZY_Calc_PostDate

		public ZDateTime ZY_Calc_PostDate
		{
			get
			{
				if (fZY_Calc_PostDate.IsEmpty)
				{
					fZY_Calc_PostDate = profitLossDetail.ZY_Calc_PostDate;
				}
				return fZY_Calc_PostDate;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_PostDateInfo, ref fZY_Calc_PostDate, value);
			}
		}

		ZDateTime fZY_Calc_PostDate;
		public ZPropertyInfo ZY_Calc_PostDateInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_PostDate)); }
		}

		#endregion

		#region ZY_Calc_FullyPaidDate

		public ZDateTime ZY_Calc_FullyPaidDate
		{
			get
			{
				if (fZY_Calc_FullyPaidDate.IsEmpty)
				{
					fZY_Calc_FullyPaidDate = profitLossDetail.ZY_Calc_FullyPaidDate;
				}
				return fZY_Calc_FullyPaidDate;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_FullyPaidDateInfo, ref fZY_Calc_FullyPaidDate, value);
			}
		}

		ZDateTime fZY_Calc_FullyPaidDate;
		public ZPropertyInfo ZY_Calc_FullyPaidDateInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_FullyPaidDate)); }
		}

		#endregion

		#region ZY_Calc_JobLocalReferenceNum

		public ZString ZY_Calc_JobLocalReferenceNum
		{
			get
			{
				if (fZY_Calc_JobLocalReferenceNum.IsEmpty)
				{
					fZY_Calc_JobLocalReferenceNum = profitLossDetail.ZY_Calc_JobLocalReferenceNum;
				}
				return fZY_Calc_JobLocalReferenceNum;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_JobLocalReferenceNumInfo, ref fZY_Calc_JobLocalReferenceNum, value);
			}
		}

		ZString fZY_Calc_JobLocalReferenceNum;
		public ZPropertyInfo ZY_Calc_JobLocalReferenceNumInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_JobLocalReferenceNum)); }
		}

		#endregion

		#region ZY_Calc_TransactionNum

		public ZString ZY_Calc_TransactionNum
		{
			get
			{
				if (fZY_Calc_TransactionNum.IsEmpty)
				{
					fZY_Calc_TransactionNum = profitLossDetail.ZY_Calc_TransactionNum;
				}
				return fZY_Calc_TransactionNum;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_TransactionNumInfo, ref fZY_Calc_TransactionNum, value);
			}
		}

		ZString fZY_Calc_TransactionNum;
		public ZPropertyInfo ZY_Calc_TransactionNumInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_TransactionNum)); }
		}

		#endregion

		#region ZY_Calc_Ledger

		public ZString ZY_Calc_Ledger
		{
			get
			{
				if (fZY_Calc_Ledger.IsEmpty)
				{
					fZY_Calc_Ledger = profitLossDetail.ZY_Calc_Ledger;
				}
				return fZY_Calc_Ledger;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_LedgerInfo, ref fZY_Calc_Ledger, value);
			}
		}

		ZString fZY_Calc_Ledger;
		public ZPropertyInfo ZY_Calc_LedgerInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_Ledger)); }
		}

		#endregion

		#region ZY_Calc_TransactionType

		public ZString ZY_Calc_TransactionType
		{
			get
			{
				if (fZY_Calc_TransactionType.IsEmpty)
				{
					fZY_Calc_TransactionType = profitLossDetail.ZY_Calc_TransactionType;
				}
				return fZY_Calc_TransactionType;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_TransactionTypeInfo, ref fZY_Calc_TransactionType, value);
			}
		}

		ZString fZY_Calc_TransactionType;
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
			get
			{
				if (fZY_Calc_OH.IsEmpty)
				{
					fZY_Calc_OH = profitLossDetail.ZY_Calc_OH;
				}
				return fZY_Calc_OH;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_OHInfo, ref fZY_Calc_OH, value);
			}
		}

		ZGuid fZY_Calc_OH;
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
			get
			{
				if (fZY_Calc_AL.IsEmpty)
				{
					fZY_Calc_AL = profitLossDetail.ZY_Calc_AL;
				}
				return fZY_Calc_AL;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_ALInfo, ref fZY_Calc_AL, value);
			}
		}

		ZGuid fZY_Calc_AL;
		public ZPropertyInfo ZY_Calc_ALInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_AL)); }
		}

		#endregion

		#region ZY_Calc_AH

		public ZGuid ZY_Calc_AH
		{
			get
			{
				if (fZY_Calc_AH.IsEmpty)
				{
					fZY_Calc_AH = profitLossDetail.ZY_Calc_AH;
				}
				return fZY_Calc_AH;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_AHInfo, ref fZY_Calc_AH, value);
			}
		}

		ZGuid fZY_Calc_AH;
		public ZPropertyInfo ZY_Calc_AHInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_AH)); }
		}

		#endregion

		#region ZY_Calc_JR

		public ZGuid ZY_Calc_JR
		{
			get
			{
				if (fZY_Calc_JR.IsEmpty)
				{
					fZY_Calc_JR = profitLossDetail.ZY_Calc_JR;
				}
				return fZY_Calc_JR;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_JRInfo, ref fZY_Calc_JR, value);
			}
		}

		ZGuid fZY_Calc_JR;
		public ZPropertyInfo ZY_Calc_JRInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_JR)); }
		}

		#endregion

		#region ZY_Calc_RecognizedDate

		public ZDateTime ZY_Calc_RecognizedDate
		{
			get
			{
				if (fZY_Calc_RecognizedDate.IsEmpty)
				{
					fZY_Calc_RecognizedDate = profitLossDetail.ZY_Calc_RecognizedDate;
				}
				return fZY_Calc_RecognizedDate;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_RecognizedDateInfo, ref fZY_Calc_RecognizedDate, value);
			}
		}

		ZDateTime fZY_Calc_RecognizedDate;
		public ZPropertyInfo ZY_Calc_RecognizedDateInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_RecognizedDate)); }
		}

		#endregion

		#region ZY_Calc_RecognitionType

		public ZString ZY_Calc_RecognitionType
		{
			get
			{
				if (fZY_Calc_RecognitionType.IsEmpty)
				{
					fZY_Calc_RecognitionType = profitLossDetail.ZY_Calc_RecognitionType;
				}
				return fZY_Calc_RecognitionType;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_RecognitionTypeInfo, ref fZY_Calc_RecognitionType, value);
			}
		}

		ZString fZY_Calc_RecognitionType;
		public ZPropertyInfo ZY_Calc_RecognitionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_RecognitionType)); }
		}

		#endregion

		#region ZY_Calc_AuditedBy

		public ZString ZY_Calc_AuditedBy
		{
			get
			{
				if (fZY_Calc_AuditedBy.IsEmpty)
				{
					fZY_Calc_AuditedBy = profitLossDetail.ZY_Calc_AuditedBy;
				}
				return fZY_Calc_AuditedBy;
			}
			set => SetNonPersistentPropertyValue(ZY_Calc_AuditedByInfo, ref fZY_Calc_AuditedBy, value);
		}

		ZString fZY_Calc_AuditedBy;
		public ZPropertyInfo ZY_Calc_AuditedByInfo => GetZPropertyInfo(nameof(ZY_Calc_AuditedBy));

		#endregion

		#region ZY_Calc_ConsolNum

		public ZString ZY_Calc_ConsolNum
		{
			get
			{
				if (fZY_Calc_ConsolNum.IsEmpty)
				{
					fZY_Calc_ConsolNum = profitLossDetail.ZY_Calc_ConsolNum;
				}
				return fZY_Calc_ConsolNum;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_ConsolNumInfo, ref fZY_Calc_ConsolNum, value);
			}
		}

		ZString fZY_Calc_ConsolNum;
		public ZPropertyInfo ZY_Calc_ConsolNumInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_ConsolNum)); }
		}

		#endregion

		#region ZY_Calc_LocalCurrency

		public ZString ZY_Calc_LocalCurrency
		{
			get
			{
				if (fZY_Calc_LocalCurrency.IsEmpty)
				{
					fZY_Calc_LocalCurrency = profitLossDetail.ZY_Calc_LocalCurrency;
				}
				return fZY_Calc_LocalCurrency;
			}
			set
			{
				SetNonPersistentPropertyValue(ZY_Calc_LocalCurrencyInfo, ref fZY_Calc_LocalCurrency, value);
			}
		}

		ZString fZY_Calc_LocalCurrency;
		public ZPropertyInfo ZY_Calc_LocalCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(ZY_Calc_LocalCurrency)); }
		}

		#endregion

		#region ZY_Calc_APLine

		public ZGuid ZY_Calc_APLine
		{
			get
			{
				if (fZY_Calc_APLine.IsEmpty)
				{
					fZY_Calc_APLine = profitLossDetail.ZY_Calc_APLine;
				}
				return fZY_Calc_APLine;
			}
		}

		ZGuid fZY_Calc_APLine;

		#endregion

		#region ZY_Calc_LocalSellAmt

		public ZGuid ZY_Calc_ARLine
		{
			get
			{
				if (fZY_Calc_ARLine.IsEmpty)
				{
					fZY_Calc_ARLine = profitLossDetail.ZY_Calc_ARLine;
				}
				return fZY_Calc_ARLine;
			}
		}

		ZGuid fZY_Calc_ARLine;

		#endregion

		#region Company Totals

		#region TotalAccrual

		public ZDecimal TotalAccrual
		{
			get
			{
				if (fTotalAccrual.IsEmpty)
				{
					fTotalAccrual = profitLossDetail.TotalAccrual;
				}
				return fTotalAccrual;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalAccrualInfo, ref fTotalAccrual, value);
			}
		}

		ZDecimal fTotalAccrual;
		public ZPropertyInfo TotalAccrualInfo
		{
			get { return GetZPropertyInfo(nameof(TotalAccrual)); }
		}

		#endregion

		#region TotalWIP

		public ZDecimal TotalWIP
		{
			get
			{
				if (fTotalWIP.IsEmpty)
				{
					fTotalWIP = profitLossDetail.TotalWIP;
				}
				return fTotalWIP;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalWIPInfo, ref fTotalWIP, value);
			}
		}

		ZDecimal fTotalWIP;
		public ZPropertyInfo TotalWIPInfo
		{
			get { return GetZPropertyInfo(nameof(TotalWIP)); }
		}

		#endregion

		#region TotalCost

		public ZDecimal TotalCost
		{
			get
			{
				if (fTotalCost.IsEmpty)
				{
					fTotalCost = profitLossDetail.TotalCost;
				}
				return fTotalCost;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalCostInfo, ref fTotalCost, value);
			}
		}

		ZDecimal fTotalCost;
		public ZPropertyInfo TotalCostInfo
		{
			get { return GetZPropertyInfo(nameof(TotalCost)); }
		}

		#endregion

		#region TotalRevenue

		public ZDecimal TotalRevenue
		{
			get
			{
				if (fTotalRevenue.IsEmpty)
				{
					fTotalRevenue = profitLossDetail.TotalRevenue;
				}
				return fTotalRevenue;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalRevenueInfo, ref fTotalRevenue, value);
			}
		}

		ZDecimal fTotalRevenue;
		public ZPropertyInfo TotalRevenueInfo
		{
			get { return GetZPropertyInfo(nameof(TotalRevenue)); }
		}

		#endregion

		#region Company Total Profit / Loss

		public ZDecimal TotalLineAmount
		{
			get
			{
				if (fTotalLineAmount.IsEmpty)
				{
					fTotalLineAmount = profitLossDetail.TotalLineAmount;
				}
				return fTotalLineAmount;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalLineAmountInfo, ref fTotalLineAmount, value);
			}
		}

		ZDecimal fTotalLineAmount;
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
				if (fMarginProfitRev.IsEmpty)
				{
					fMarginProfitRev = profitLossDetail.MarginProfitRev;
				}
				return fMarginProfitRev;
			}
			set
			{
				SetNonPersistentPropertyValue(MarginProfitRevInfo, ref fMarginProfitRev, value);
			}
		}

		ZString fMarginProfitRev;
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
				if (fMarginProfitCost.IsEmpty)
				{
					fMarginProfitCost = profitLossDetail.MarginProfitCost;
				}
				return fMarginProfitCost;
			}
			set
			{
				SetNonPersistentPropertyValue(MarginProfitCostInfo, ref fMarginProfitCost, value);
			}
		}

		ZString fMarginProfitCost;
		public ZPropertyInfo MarginProfitCostInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitCost)); }
		}

		#endregion

		#region TotalRevenueRecognized

		public ZDecimal TotalRevenueRecognized
		{
			get
			{
				if (fTotalRevenueRecognized.IsEmpty)
				{
					fTotalRevenueRecognized = profitLossDetail.TotalRevenueRecognized;
				}
				return fTotalRevenueRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalRevenueRecognizedInfo, ref fTotalRevenueRecognized, value);
			}
		}

		ZDecimal fTotalRevenueRecognized;
		public ZPropertyInfo TotalRevenueRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalRevenueRecognized)); }
		}

		#endregion

		#region TotalWIPRecognized

		public ZDecimal TotalWIPRecognized
		{
			get
			{
				if (fTotalWIPRecognized.IsEmpty)
				{
					fTotalWIPRecognized = profitLossDetail.TotalWIPRecognized;
				}
				return fTotalWIPRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalWIPRecognizedInfo, ref fTotalWIPRecognized, value);
			}
		}

		ZDecimal fTotalWIPRecognized;
		public ZPropertyInfo TotalWIPRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalWIPRecognized)); }
		}

		#endregion

		#region TotalCostRecognized

		public ZDecimal TotalCostRecognized
		{
			get
			{
				if (fTotalCostRecognized.IsEmpty)
				{
					fTotalCostRecognized = profitLossDetail.TotalCostRecognized;
				}
				return fTotalCostRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalCostRecognizedInfo, ref fTotalCostRecognized, value);
			}
		}

		ZDecimal fTotalCostRecognized;
		public ZPropertyInfo TotalCostRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalCostRecognized)); }
		}

		#endregion

		#region TotalAccrualRecognized

		public ZDecimal TotalAccrualRecognized
		{
			get
			{
				if (fTotalAccrualRecognized.IsEmpty)
				{
					fTotalAccrualRecognized = profitLossDetail.TotalAccrualRecognized;
				}
				return fTotalAccrualRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalAccrualRecognizedInfo, ref fTotalAccrualRecognized, value);
			}
		}

		ZDecimal fTotalAccrualRecognized;
		public ZPropertyInfo TotalAccrualRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalAccrualRecognized)); }
		}

		#endregion

		#region Total Profit / Loss Recognized

		public ZDecimal TotalLineAmountRecognized
		{
			get
			{
				if (fTotalLineAmountRecognized.IsEmpty)
				{
					fTotalLineAmountRecognized = profitLossDetail.TotalLineAmountRecognized;
				}
				return fTotalLineAmountRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalLineAmountRecognizedInfo, ref fTotalLineAmountRecognized, value);
			}
		}

		ZDecimal fTotalLineAmountRecognized;
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
				if (fMarginProfitRevRecognized.IsEmpty)
				{
					fMarginProfitRevRecognized = profitLossDetail.MarginProfitRevRecognized;
				}
				return fMarginProfitRevRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(MarginProfitRevRecognizedInfo, ref fMarginProfitRevRecognized, value);
			}
		}

		ZString fMarginProfitRevRecognized;
		public ZPropertyInfo MarginProfitRevRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitRevRecognized)); }
		}

		#endregion

		#region Margin Profit/Cost

		public ZString MarginProfitCostRecognized
		{
			get
			{
				if (fMarginProfitCostRecognized.IsEmpty)
				{
					fMarginProfitCostRecognized = profitLossDetail.MarginProfitCostRecognized;
				}
				return fMarginProfitCostRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(MarginProfitCostRecognizedInfo, ref fMarginProfitCostRecognized, value);
			}
		}

		ZString fMarginProfitCostRecognized;
		public ZPropertyInfo MarginProfitCostRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitCostRecognized)); }
		}

		#endregion

		#region TotalRevenueNotRecognized

		public ZDecimal TotalRevenueNotRecognized
		{
			get
			{
				if (fTotalRevenueNotRecognized.IsEmpty)
				{
					fTotalRevenueNotRecognized = profitLossDetail.TotalRevenueNotRecognized;
				}
				return fTotalRevenueNotRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalRevenueNotRecognizedInfo, ref fTotalRevenueNotRecognized, value);
			}
		}

		ZDecimal fTotalRevenueNotRecognized;
		public ZPropertyInfo TotalRevenueNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalRevenueNotRecognized)); }
		}

		#endregion

		#region TotalWIPNotRecognized

		public ZDecimal TotalWIPNotRecognized
		{
			get
			{
				if (fTotalWIPNotRecognized.IsEmpty)
				{
					fTotalWIPNotRecognized = profitLossDetail.TotalWIPNotRecognized;
				}
				return fTotalWIPNotRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalWIPNotRecognizedInfo, ref fTotalWIPNotRecognized, value);
			}
		}

		ZDecimal fTotalWIPNotRecognized;
		public ZPropertyInfo TotalWIPNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalWIPNotRecognized)); }
		}

		#endregion

		#region TotalCostNotRecognized

		public ZDecimal TotalCostNotRecognized
		{
			get
			{
				if (fTotalCostNotRecognized.IsEmpty)
				{
					fTotalCostNotRecognized = profitLossDetail.TotalCostNotRecognized;
				}
				return fTotalCostNotRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalCostNotRecognizedInfo, ref fTotalCostNotRecognized, value);
			}
		}

		ZDecimal fTotalCostNotRecognized;
		public ZPropertyInfo TotalCostNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalCostNotRecognized)); }
		}

		#endregion

		#region TotalAccrualNotRecognized

		public ZDecimal TotalAccrualNotRecognized
		{
			get
			{
				if (fTotalAccrualNotRecognized.IsEmpty)
				{
					fTotalAccrualNotRecognized = profitLossDetail.TotalAccrualNotRecognized;
				}
				return fTotalAccrualNotRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalAccrualNotRecognizedInfo, ref fTotalAccrualNotRecognized, value);
			}
		}

		ZDecimal fTotalAccrualNotRecognized;
		public ZPropertyInfo TotalAccrualNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalAccrualNotRecognized)); }
		}

		#endregion

		#region Total Profit / Loss Not Recognized

		public ZDecimal TotalLineAmountNotRecognized
		{
			get
			{
				if (fTotalLineAmountNotRecognized.IsEmpty)
				{
					fTotalLineAmountNotRecognized = profitLossDetail.TotalLineAmountNotRecognized;
				}
				return fTotalLineAmountNotRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(TotalLineAmountNotRecognizedInfo, ref fTotalLineAmountNotRecognized, value);
			}
		}

		ZDecimal fTotalLineAmountNotRecognized;
		public ZPropertyInfo TotalLineAmountNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(TotalLineAmountNotRecognized)); }
		}

		#endregion

		#region MarginProfitCostNotRecognized

		public ZString MarginProfitCostNotRecognized
		{
			get
			{
				if (fMarginProfitCostNotRecognized.IsEmpty)
				{
					fMarginProfitCostNotRecognized = profitLossDetail.MarginProfitCostNotRecognized;
				}
				return fMarginProfitCostNotRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(MarginProfitCostNotRecognizedInfo, ref fMarginProfitCostNotRecognized, value);
			}
		}

		ZString fMarginProfitCostNotRecognized;
		public ZPropertyInfo MarginProfitCostNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitCostNotRecognized)); }
		}

		#endregion

		#region MarginProfitRevNotRecognized

		public ZString MarginProfitRevNotRecognized
		{
			get
			{
				if (fMarginProfitRevNotRecognized.IsEmpty)
				{
					fMarginProfitRevNotRecognized = profitLossDetail.MarginProfitRevNotRecognized;
				}
				return fMarginProfitRevNotRecognized;
			}
			set
			{
				SetNonPersistentPropertyValue(MarginProfitRevNotRecognizedInfo, ref fMarginProfitRevNotRecognized, value);
			}
		}

		ZString fMarginProfitRevNotRecognized;
		public ZPropertyInfo MarginProfitRevNotRecognizedInfo
		{
			get { return GetZPropertyInfo(nameof(MarginProfitRevNotRecognized)); }
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
	}
}

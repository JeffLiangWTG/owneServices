using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocJobLineDetail : DocBaseWrapper
	{
		protected DocJobLineDetail(AccTransactionLines line, BusinessObjectFactory factory)
			: base(line, factory)
		{
		}

		public static DocJobLineDetail New(AccTransactionLines line, BusinessObjectFactory factory)
		{
			return new DocJobLineDetail(line, factory);
		}

		public ZString LineType
		{
			get { return Line.AL_LineType; }
		}

		public ZString Branch
		{
			get { return (Line.Branch != null) ? Line.Branch.GB_Code : ZString.Empty; }
		}

		public ZString Department
		{
			get { return (Line.Department != null) ? Line.Department.GE_Code : ZString.Empty; }
		}

		public ZDecimal Revenue
		{
			get { return (LineType == ZArchitecture.Core.TransactionLineTypes.Revenue) ? new ZDecimal(Line.AL_LineAmount) : new ZDecimal(0M); }
		}

		public ZDateTime RecognizeDate
		{
			get
			{
				if (Line.AL_RevRecognitionType == RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate || !Line.AL_ReverseDate.IsEmpty)
				{
					return Line.AL_ReverseDate;
				}
				else
				{
					var sqlFilter = new ZQuery(JobChargeRevRecognitionSchema.D3_JH, Line.AL_JH);
					sqlFilter.AddToFilter(JobChargeRevRecognitionSchema.D3_RecognitionType, Line.AL_RevRecognitionType);
					var recognitionDates = Factory.Load<JobChargeRevRecognition>(sqlFilter);
					return recognitionDates != null && recognitionDates.Length > 0 ? recognitionDates[0].D3_RecognitionDate : ZDateTime.Empty;
				}
			}
		}

		public ZDecimal WIP
		{
			get { return (LineType == ZArchitecture.Core.TransactionLineTypes.WIP) ? new ZDecimal(-Line.AL_LineAmount) : new ZDecimal(0M); }
		}

		public ZDecimal Cost
		{
			get
			{
				return (LineType == TransactionLineTypes.Cost
					|| Line.AL_LineType == TransactionLineTypes.Revenue && Line.TransactionHeader != null && Line.TransactionHeader.AH_TransactionType == TransactionTypes.JobRevenueJournal)
						? new ZDecimal(-Line.AL_LineAmount) : new ZDecimal(0M);
			}
		}

		public ZDecimal Accrual
		{
			get { return (LineType == ZArchitecture.Core.TransactionLineTypes.Accrual) ? new ZDecimal(Line.AL_LineAmount) : new ZDecimal(0M); }
		}

		public ZDecimal Income
		{
			get { return Revenue + WIP; }
		}

		public ZDecimal Expense
		{
			get { return -(Cost + Accrual); }
		}

		public ZDecimal Profit
		{
			get { return Income + Expense; }
		}

		internal bool IsTaxExpense { get; set; }

		public ZString OSValueAsString
		{
			get
			{
				ZString result = new();
				if (!IsTaxExpense)
				{
					if (Currency != null)
					{
						result = Currency.Code + " " + OSValue.ToString(Currency.Decimals);
					}
					else
					{
						result = OSValue.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals);
					}
				}

				return result;
			}
		}

		public ZDecimal OSValue
		{
			get
			{
				ZDecimal result;
				if (Line.AL_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					result = Line.AL_LineAmount;
				}
				else
				{
					if (Line.AL_GSTVAT == 0)
					{
						result = Line.AL_OSAmount;
					}
					else
					{
						result = Enterprise.Environment.Env.CurrentCompany.ExchangeRate.LocalToForeign(Line.AL_LineAmount, Line.AL_ExchangeRate, Line.AL_RX_NKTransactionCurrency);
					}
				}
				return result;
			}
		}

		public ZDecimal LocalValue
		{
			get
			{
				var localValue = 0m;
				if (IsTaxExpense)
				{
					localValue = Line.AL_TotalTaxExpenseAmount;
				}
				else
				{
					localValue = Line.AL_LineAmount;
				}
				return localValue;
			}
		}

		public ZDecimal ExchangeRate
		{
			get { return Line.TransactionHeader != null ? Line.TransactionHeader.AH_ExchangeRate : Line.AL_ExchangeRate; }
		}

		public ZDecimal LineExchangeRate => Line.AL_ExchangeRate;

		public DocChargeCode ChargeCode
		{
			get { return DocChargeCode.New(Line.ChargeCode, Factory); }
		}

		public ZGuid ChargeCodePK
		{
			get { return Line.AL_AC; }
		}

		public ZString ChargeCodeDescription => !IsTaxExpense ? ChargeCode.Desc : (ZString)Res.GetString("D32E36BC-19B7-474B-9701-977248A1DAAE", "Tax Expense");

		public DocOrganisation Organisation
		{
			get
			{
				if (Line.TransactionHeader == null &&
					(Line.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Revenue || Line.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Cost))
				{
					CriticalValidationHelpers.ReportInvalidOperation("DocJobLineDetail.Organisation",
						(NoResString)"Db data is not correct. Cost and Revenue Transaction Lines must have Transaction Header.\r\n" + Line.GetTransactionLineInfo(), true);
				}
				return DocOrganisation.New(Line.TransactionHeader != null ? Line.TransactionHeader.Header : Line.Header, Factory);
			}
		}

		public DocTransactionHeader TransactionHeader
		{
			get
			{
				var header = Factory.Load<TransactionHeader>(Line.AL_AH);
				if (header == null &&
					(Line.AL_LineType == TransactionLineTypes.Revenue || Line.AL_LineType == TransactionLineTypes.Cost))
				{
					CriticalValidationHelpers.ReportInvalidOperation("DocJobLineDetail.TransactionHeader",
						(NoResString)"Db data is not correct. Cost and Revenue Transaction Lines must have Transaction Header.\r\n" + Line.GetTransactionLineInfo(), true);
				}
				return DocTransactionHeader.New(header, Factory);
			}
		}

		public ZGuid TransactionHeaderPK
		{
			get { return Line.AL_AH; }
		}

		public ZDecimal ARTransactionHeaderOSTotal
		{
			get
			{
				var result = ZDecimal.Zero;
				var transactionHeader = TransactionHeader;
				if (transactionHeader != null)
				{
					var multiplier = transactionHeader.TransactionType == TransactionTypes.CreditNote ? -1 : 1;
					result = transactionHeader.OSTotal * multiplier;
				}
				return result;
			}
		}

		public ZDecimal APTransactionHeaderOSTotal
		{
			get
			{
				var result = ZDecimal.Zero;
				var transactionHeader = TransactionHeader;
				if (transactionHeader != null)
				{
					var multiplier = transactionHeader.TransactionType == Enterprise.ZArchitecture.Core.TransactionTypes.Invoice ? -1 : 1;
					result = transactionHeader.OSTotal * multiplier;
				}
				return result;
			}
		}

		public DocCurrency Currency
		{
			get { return DocCurrency.New(Line.TransactionCurrency, Factory); }
		}

		public DocJobInvoicingJob Job
		{
			get { return DocJobInvoicingJob.New(Factory.Load<Job>(Line.AL_JH), Factory); }
		}

		public ZGuid JobPK
		{
			get { return Line.AL_JH; }
		}

		public ZDateTime ReverseDate
		{
			get { return Line.AL_ReverseDate; }
		}

		AccTransactionLines Line
		{
			get { return (AccTransactionLines)WrappedObject; }
		}
	}
}

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.DataTransfer.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class DocAccComplianceReportLine : DocBaseWrapper
	{
		#region Constructor

		protected DocAccComplianceReportLine(AccComplianceReportLine line, BusinessObjectFactory factory)
			: base(line, factory)
		{
		}

		protected DocAccComplianceReportLine(AccComplianceReportLine line, AccComplianceReportLineCollectionBase<AccComplianceReportLine> parentCollection, DocAccComplianceReport parentDocReport)
			: base(line, parentDocReport.Factory)
		{
			ParentCollection = parentCollection;
			ParentDocReport = parentDocReport;
		}

		public static DocAccComplianceReportLine New(AccComplianceReportLine line, BusinessObjectFactory factory)
		{
			return line != null ? new DocAccComplianceReportLine(line, factory) : null;
		}

		public static DocAccComplianceReportLine New(AccComplianceReportLine line, AccComplianceReportLineCollectionBase<AccComplianceReportLine> parentCollection, DocAccComplianceReport parentDocReport)
		{
			return line != null ? new DocAccComplianceReportLine(line, parentCollection, parentDocReport) : null;
		}

		#endregion

		readonly AccComplianceReportLineCollectionBase<AccComplianceReportLine> ParentCollection;
		readonly DocAccComplianceReport ParentDocReport;

		public string ReportUniqueID;

		AccComplianceReportLine Line
		{
			get { return (AccComplianceReportLine)WrappedObject; }
		}

		#region Properties

		public ZInt ACL_ReportSequence
		{
			get { return Line.ACL_ReportSequence; }
		}

		public ZString OH_Code
		{
			get { return Line.OH_Code; }
		}

		public ZString OH_FullName
		{
			get { return Line.OH_FullName; }
		}

		public ZString OrgCountryCode
		{
			get { return Line.OrgCountryCode; }
		}

		public ZString GC_RN_NKCountryCode
		{
			get { return Line.GC_RN_NKCountryCode; }
		}

		public ZString OK_CustomsRegNo
		{
			get { return Line.OK_CustomsRegNo; }
		}

		public ZString TaxRegistrationNumber
		{
			get
			{
				return Line.OK_CustomsRegNo.IsEmpty ? Line.RepCountryRegNo : Line.OK_CustomsRegNo;
			}
		}

		public ZString GC_RX_NKLocalCurrency
		{
			get { return Line.GC_RX_NKLocalCurrency; }
		}

		public ZString AH_Ledger
		{
			get { return Line.AH_Ledger; }
		}

		public ZString AH_TransactionType
		{
			get { return Line.AH_TransactionType; }
		}

		public ZString TransactionTypeDesc
		{
			get
			{
				if (AH_Ledger == LedgerTypes.JobCosting && (AH_TransactionType == TransactionLineTypes.Accrual || AH_TransactionType == TransactionLineTypes.WIP))
				{
					return new CodeDescriptionPairList(OLookUpEditType.WIPAccrualTransactionTypes).GetDescriptionFromCode(AH_TransactionType);
				}
				else
				{
					return new CodeDescriptionPairList(OLookUpEditType.TransactionTypes).GetDescriptionFromCode(AH_TransactionType);
				}
			}
		}

		public ZDateTime PostDate
		{
			get { return Line.PostDate; }
		}

		public ZDateTime InvoiceDate
		{
			get { return Line.InvoiceDate; }
		}

		public ZString AH_TransactionNum
		{
			get { return Line.AH_TransactionNum; }
		}

		public ZString AH_TransactionReference
		{
			get { return Line.AH_TransactionReference; }
		}

		public ZString AH_ComplianceSubType
		{
			get { return Line.AH_ComplianceSubType; }
		}

		public ZString AH_ComplianceSubTypeDescription
		{
			get
			{
				switch (Line.GC_RN_NKCountryCode)
				{
					case Core.Constants.CountryCodes.Italy:
						return (CountryComplianceFactory.GetIComplianceSubTypeCodeProvider(Core.Constants.CountryCodes.Italy)?.GetComplianceSubTypes().GetDescriptionPairList()).GetDescriptionFromCode(AH_ComplianceSubType);
					default:
						return ZString.Empty;
				}
			}
		}

		public ZString ReportSubCode
		{
			get { return Line.ReportSubCode; }
		}

		public ZString AT_Type
		{
			get { return Line.AT_Type; }
		}

		public ZString AT_Code
		{
			get { return Line.AT_Code; }
		}

		public ZString AT_ExtraTaxRateType
		{
			get { return Line.AT_ExtraTaxRateType; }
		}

		public ZString TaxMessage
		{
			get { return Line.TaxMessage; }
		}

		public ZDecimal GoodsExTaxAmount
		{
			get { return Line.GoodsExTaxAmount; }
		}

		public ZDecimal GoodsTaxAmount
		{
			get { return Line.GoodsTaxAmount; }
		}

		public ZDecimal ServiceExTaxAmount
		{
			get { return Line.ServiceExTaxAmount; }
		}

		public ZDecimal ServiceTaxAmount
		{
			get { return Line.ServiceTaxAmount; }
		}

		public ZDecimal TotalExTaxAmount
		{
			get { return Line.TotalExTaxAmount; }
		}

		public ZDecimal TotalTaxAmount
		{
			get { return Line.TotalTaxAmount; }
		}

		public ZDecimal TotalTaxAmountForDisplay
		{
			get
			{
				ZDecimal result;
				if (Line.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.VATRemittedByCustomer)
				{
					result = Line.SPVTaxAmount;
				}
				else if (Line.AT_Type == AccTaxRate.Types.ReverseRated)
				{
					result = Line.TaxReverseChargeAmount;
				}
				else
				{
					result = Line.TotalTaxAmount;
				}
				return result;
			}
		}

		public ZDecimal SPVTaxAmountForDisplay
		{
			get { return -Line.SPVTaxAmount; }
		}

		public ZDecimal TotalInvoiceAmount
		{
			get { return Line.AH_LocalTotal; }
		}

		public ZString ComplianceSequence
		{
			get { return Line.ComplianceSequence; }
		}

		public ZBool IsLastLineOfInvoice
		{
			get
			{
				ZBool result = ZBool.False;
				if (ParentCollection != null)
				{
					var maxSequenceNum = ParentCollection.Cast<AccComplianceReportLine>().Where(x => x.AH_PK == Line.AH_PK).Max(s => s.ACL_ReportSequence);
					if (Line.ACL_ReportSequence == maxSequenceNum)
					{
						result = ZBool.True;
					}
				}
				return result;
			}
		}

		public ZBool IsGoods
		{
			get { return Line.IsGoods; }
		}

		public ZBool IsService
		{
			get { return Line.IsService; }
		}

		public ZString AG_AccountNum
		{
			get { return Line.AG_AccountNum; }
		}

		public ZString AG_Description
		{
			get { return Line.AG_Description; }
		}

		public ZDecimal GeneralLedgerAmount
		{
			get { return Line.GeneralLedgerAmountDR == 0 ? new ZDecimal(-Line.GeneralLedgerAmountCR) : Line.GeneralLedgerAmountDR; }
		}

		public ZDecimal GeneralLedgerAmountDR
		{
			get { return Line.GeneralLedgerAmountDR; }
		}

		public ZDecimal GeneralLedgerAmountCR
		{
			get { return Line.GeneralLedgerAmountCR; }
		}

		public ZDecimal TaxRecoverableAmount
		{
			get { return Line.TaxRecoverableAmount; }
		}

		public ZDecimal TaxNotRecoverableAmount
		{
			get { return Line.TaxNotRecoverableAmount; }
		}

		public ZString Comment
		{
			get { return Line.Comment; }
		}

		#endregion

		#region Properties from other BizOs

		public ZInt AccountingPeriod
		{
			get
			{
				var periods = Factory.GetCachedValue(DocAccComplianceReport.PeriodCacheKeyPrefix + ReportUniqueID, () => { return System.Array.Empty<AccPeriodManagement>(); });    // Report should be created first and initialize cache on loading lines. This is fall back if it was not done
				var period = periods.FirstOrDefault(x => x.AM_StartDate <= Line.PostDate && x.AM_EndDate > Line.PostDate);
				return period != null ? period.AM_Period : ZInt.Zero;
			}
		}

		#endregion

		#region Properties from Data Collector

		public ZString Description
		{
			get
			{
				switch (Line.AH_TransactionType)
				{
					case TransactionLineTypes.WIP:
						return AccountingConstants.DefaultDayBookLineDescriptions.WIP;
					case TransactionLineTypes.Accrual:
						return AccountingConstants.DefaultDayBookLineDescriptions.Accrual;
					default:
						return CollectorHeaderDetails?.Description ?? ZString.Empty;
				}
			}
		}

		public ZDateTime CreateTime => CollectorHeaderDetails?.CreateTime ?? ZDateTime.Empty;

		public ZString CreateUserCode => CollectorHeaderDetails?.CreateUserCode ?? ZString.Empty;

		public ZString CreateUserName => CollectorHeaderDetails?.CreateUserName ?? ZString.Empty;

		IComplianceReportTransactionHeaderDetails CollectorHeaderDetails
		{
			get { return collectorHeaderDetails ?? (collectorHeaderDetails = ParentDocReport?.AdditionalDataCollector?.GetTransactionHeader(Line.AH_PK)); }
		}
		IComplianceReportTransactionHeaderDetails collectorHeaderDetails;

		#endregion
	}
}

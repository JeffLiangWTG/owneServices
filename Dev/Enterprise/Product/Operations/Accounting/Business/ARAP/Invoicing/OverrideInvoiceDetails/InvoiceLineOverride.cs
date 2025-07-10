using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class InvoiceLineOverride : NonPersistentBusinessObject, IObsoleteValidation
	{
		public InvoiceLineOverride(DependentTransactionLine line)
			: base(line.Factory)
		{
			Line = line;
		}

		protected readonly DependentTransactionLine Line;

		protected InvoicingLineBase InvoicingLineBaseLine { get { return Line as InvoicingLineBase; } }

		protected InvoicingBase InvoicingBase
		{
			get
			{
				if (fInvoicingBase == null)
				{
					fInvoicingBase = InvoicingLineBaseLine.InvoiceBase;
					if (fInvoicingBase == null)
					{
						fInvoicingBase = Factory.Load<InvoicingBase>(InvoicingLineBaseLine.AL_AH);
					}
				}

				return fInvoicingBase;
			}
		}
		InvoicingBase fInvoicingBase;

#if DEBUG
		public AccTransactionLines LineForTest { get { return Line; } }
#endif

		public AccTransactionLinesLookups Lookups
		{
			get { return Line.Lookups; }
		}

		public object ChargeList
		{
			get
			{
				if (InvoicingLineBaseLine == null)
				{
					return Line.Lookups.GLHeaders;
				}
				else
				{
					return InvoicingLineBaseLine.ChargeList;
				}
			}
		}

		#region GenericCharge

		[List("ChargeList")]
		public ZGuid GenericCharge
		{
			get
			{
				return InvoicingLineBaseLine != null ? InvoicingLineBaseLine.GenericCharge : Line.AL_AG;
			}
		}
		public ZPropertyInfo GenericChargeInfo
		{
			get
			{
				ZPropertyInfo result = null;
				if (InvoicingLineBaseLine != null)
				{
					result = GetWrappedZPropertyInfo(InvoicingLineBase.Schema.GenericCharge, x => InvoicingLineBaseLine.GenericChargeInfo);
				}
				else if (Line != null)
				{
					result = GetWrappedZPropertyInfo(InvoicingLineBase.Schema.GenericCharge, x => Line.AL_AGInfo);
				}
				return result;
			}
		}

		#endregion

		#region AL_GB

		[List("Lookups.Branches")]
		public ZGuid AL_GB
		{
			get { return Line.AL_GB; }
		}
		public ZPropertyInfo AL_GBInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(InvoicingLineBase.Schema.AL_GB, x => Line.AL_GBInfo); }
		}

		#endregion

		#region AL_GE

		[List("Lookups.Departments")]
		public ZGuid AL_GE
		{
			get { return Line.AL_GE; }
		}
		public ZPropertyInfo AL_GEInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(InvoicingLineBase.Schema.AL_GE, x => Line.AL_GEInfo); }
		}

		#endregion

		#region AL_AT

		[List("Lookups.TaxRates")]
		public ZGuid AL_AT
		{
			get { return Line.AL_AT; }
		}
		public ZPropertyInfo AL_ATInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(InvoicingLineBase.Schema.AL_AT, x => Line.AL_ATInfo); }
		}

		#endregion

		#region AL_RX_NKTransactionCurrency

		public ZString AL_RX_NKTransactionCurrency
		{
			get { return Line.AL_RX_NKTransactionCurrency; }
		}
		public ZPropertyInfo AL_RX_NKTransactionCurrencyInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(InvoicingLineBase.Schema.AL_RX_NKTransactionCurrency, x => Line.AL_RX_NKTransactionCurrencyInfo); }
		}

		#endregion

		#region AL_ExchangeRate

		public ZDecimal AL_ExchangeRate
		{
			get { return Line.AL_ExchangeRate; }
		}
		public ZPropertyInfo AL_ExchangeRateInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(InvoicingLineBase.Schema.AL_ExchangeRate, x => Line.AL_ExchangeRateInfo); }
		}

		#endregion

		#region ChargeTypeWithOverride

		public ZString ChargeTypeWithOverride
		{
			get { return InvoicingLineBaseLine != null ? InvoicingLineBaseLine.ChargeTypeWithOverride : ZString.Empty; }
		}
		public ZPropertyInfo ChargeTypeWithOverrideInfo
		{
			get { return InvoicingLineBaseLine != null ? GetWrappedZPropertyInfo(InvoicingLineBase.Schema.ChargeTypeWithOverride, x => InvoicingLineBaseLine.ChargeTypeWithOverrideInfo) : null; }
		}

		#endregion

		#region AL_JH

		[List("Lookups.Jobs")]
		public ZGuid AL_JH
		{
			get { return Line.AL_JH; }
		}
		public ZPropertyInfo AL_JHInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(InvoicingLineBase.Schema.AL_JH, x => Line.AL_JHInfo); }
		}

		#endregion

		#region JobLocalReference

		public ZString JobLocalReference
		{
			get { return Line.JobLocalReference; }
		}
		public ZPropertyInfo JobLocalReferenceInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(nameof(JobLocalReference), x => Line.JobLocalReferenceInfo); }
		}

		#endregion

		#region AL_LineAmount

		public ZDecimal AL_LineAmount
		{
			get { return Line.AL_LineAmount; }
		}
		public ZPropertyInfo AL_LineAmountInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(AccTransactionLinesSchema.AL_LineAmount.Name, x => Line.AL_LineAmountInfo); }
		}

		#endregion

		#region AL_OSAmount

		public ZDecimal AL_OSAmount
		{
			get { return Line.AL_OSAmount; }
		}
		public ZPropertyInfo AL_OSAmountInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(AccTransactionLinesSchema.AL_OSAmount.Name, x => Line.AL_OSAmountInfo); }
		}

		#endregion

		#region AL_OSExTaxAmount

		public ZDecimal AL_OSExTaxAmount
		{
			get { return Line.AL_OSExTaxAmount; }
		}
		public ZPropertyInfo AL_OSExTaxAmountInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(InvoicingLineBase.Schema.AL_OSExTaxAmount, x => Line.AL_OSExTaxAmountInfo); }
		}

		#endregion

		#region AL_OSTaxAmount

		public ZDecimal AL_OSTaxAmount
		{
			get { return Line.AL_OSTaxAmount; }
		}
		public ZPropertyInfo AL_OSTaxAmountInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(InvoicingLineBase.Schema.AL_OSTaxAmount, x => Line.AL_OSTaxAmountInfo); }
		}

		#endregion

		#region AL_OverseasTotal

		public ZDecimal AL_OverseasTotal
		{
			get { return Line.AL_OverseasTotal; }
		}
		public ZPropertyInfo AL_OverseasTotalInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(InvoicingLineBase.Schema.AL_OverseasTotal, x => Line.AL_OverseasTotalInfo); }
		}

		#endregion

		#region AL_LocalExTaxAmount

		public ZDecimal AL_LocalExTaxAmount
		{
			get { return Line.AL_LocalExTaxAmount; }
		}
		public ZPropertyInfo AL_LocalExTaxAmountInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(InvoicingLineBase.Schema.AL_LocalExTaxAmount, x => Line.AL_LocalExTaxAmountInfo); }
		}

		#endregion

		#region AL_LocalTaxAmount

		public ZDecimal AL_LocalTaxAmount
		{
			get { return Line.AL_LocalTaxAmount; }
		}
		public ZPropertyInfo AL_LocalTaxAmountInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(InvoicingLineBase.Schema.AL_LocalTaxAmount, x => Line.AL_LocalTaxAmountInfo); }
		}

		#endregion

		#region AL_LocalTotalAmount

		public ZDecimal AL_LocalTotalAmount
		{
			get { return Line.AL_LocalTotalAmount; }
		}
		public ZPropertyInfo AL_LocalTotalAmountInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(InvoicingLineBase.Schema.AL_LocalTotalAmount, x => Line.AL_LocalTotalAmountInfo); }
		}

		#endregion

		#region AL_LocalGSTAmount

		public ZDecimal AL_LocalGSTAmount
		{
			get { return Line.AL_LocalGSTAmount; }
		}
		public ZPropertyInfo AL_LocalGSTAmountInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(InvoicingLineBase.Schema.AL_LocalGSTAmount, x => Line.AL_LocalGSTAmountInfo); }
		}

		#endregion

		#region AL_OSGSTAmount

		public ZDecimal AL_OSGSTAmount
		{
			get { return Line.AL_OSGSTAmount; }
		}
		public ZPropertyInfo AL_OSGSTAmountInfo
		{
			get { return Line == null ? null : GetWrappedZPropertyInfo(InvoicingLineBase.Schema.AL_OSGSTAmount, x => Line.AL_OSGSTAmountInfo); }
		}

		#endregion
	}
}

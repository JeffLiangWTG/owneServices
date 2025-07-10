using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocTransactionLine : DocumentWrapper, IGenericTransactionLinePlugIn
	{
		protected DocTransactionLine(TransactionLine transactionLine, BusinessObjectFactory factoryToWrap)
			: base(transactionLine, factoryToWrap)
		{
		}

		protected TransactionLine TransactionLine
		{
			get { return (TransactionLine)WrappedObject; }
		}

		public static DocTransactionLine New(TransactionLine transactionLine, BusinessObjectFactory factoryToWrap)
		{
			return (transactionLine != null) ? new DocTransactionLine(transactionLine, factoryToWrap) : null;
		}

		#region IGenericTransactionLinePlugIn members

		GenericTransactionLineSupporter IGenericTransactionLinePlugIn.LineSupporter
		{
			get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocTransactionLineGenericTransactionSupporter(this)); }
		}
		DocTransactionLineGenericTransactionSupporter fGenericTransactionSupporter;

		#endregion

		class DocTransactionLineGenericTransactionSupporter : GenericTransactionLineSupporter
		{
			public DocTransactionLineGenericTransactionSupporter(DocTransactionLine parent)
			{
				this.Parent = parent;
			}
			protected readonly DocTransactionLine Parent;

			protected internal override DocGLAccount GetGLAccount()
			{
				return Parent.GLAccount;
			}

			protected internal override DocBranch GetBranch()
			{
				return Parent.Branch;
			}

			protected internal override DocDepartment GetDepartment()
			{
				return Parent.Department;
			}

			protected internal override ZString GetDescription()
			{
				return Parent.Description;
			}

			protected internal override DocCurrency GetCurrency()
			{
				return Parent.Currency;
			}

			protected internal override ZDecimal GetOverseasTotal()
			{
				return Parent.DPYOSAmount;
			}

			protected internal override ZDecimal GetOSAmount()
			{
				return Parent.OSAmount;
			}

			protected internal override ZDecimal GetLineAmount()
			{
				return Parent.LineAmount;
			}

			protected internal override ZDecimal GetLocalTotalAmount()
			{
				return Parent.LocalTotalAmount;
			}

			protected internal override ZDecimal GetExchangeRate()
			{
				return Parent.ExchangeRate;
			}

			protected internal override ZDecimal GetLocalAmountAndTax()
			{
				return Parent.DPYLocalAmountAndTax;
			}

			protected internal override ZString GetDebitAmount()
			{
				return Parent.DebitAmount;
			}

			protected internal override ZString GetCreditAmount()
			{
				return Parent.CreditAmount;
			}

			protected internal override ZString GetForeignCurrencyEquivalentAmount()
			{
				return Parent.ForeignCurrencyEquivalent;
			}

			protected internal override ZString GetOSUnsignedAmount()
			{
				return Parent.OSUnsignedAmount;
			}

			protected internal override ZDecimal GetDebitAmountDecimal()
			{
				return Parent.DebitAmountDecimal;
			}

			protected internal override ZDecimal GetCreditAmountDecimal()
			{
				return Parent.CreditAmountDecimal;
			}

			protected internal override DocChargeCode GetChargeCode()
			{
				return Parent.ChargeCode;
			}

			protected internal override DocJobHeader GetJobHeader()
			{
				return Parent.JobHeader;
			}
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		public ZDecimal DPYOSAmount
		{
			get { return TransactionLine.AL_OverseasTotal; }
		}

		public ZDecimal DPYLocalAmountAndTax
		{
			get { return LocalExTaxAmount + LocalTaxAmount; }
		}

		public ZString DebitCreditSign
		{
			get { return DebitCreditAmounts != null ? DebitCreditAmounts.DebitCreditSign : ZString.Empty; }
		}

		protected ZDecimal fMatchedAmount;
		public ZDecimal MatchedAmount
		{
			get { return fMatchedAmount; }
			set { fMatchedAmount = value; }
		}

		protected ZDecimal fMatchedOSAmount;
		public ZDecimal MatchedOSAmount
		{
			get { return fMatchedOSAmount; }
			set { fMatchedOSAmount = value; }
		}

		public ZString DebitAmount
		{
			get { return (DebitCreditSign == DebitCreditDataEntry.DR) ? LocalUnsignedAmount : ZString.Empty; }
		}

		internal ZDecimal DebitAmountDecimal
		{
			get { return !DebitAmount.IsEmpty ? LocalUnsignedAmountDecimal : ZDecimal.Zero; }
		}

		public ZString CreditAmount
		{
			get { return (DebitCreditSign == DebitCreditDataEntry.CR) ? LocalUnsignedAmount : ZString.Empty; }
		}

		internal ZDecimal CreditAmountDecimal
		{
			get { return !CreditAmount.IsEmpty ? LocalUnsignedAmountDecimal : ZDecimal.Zero; }
		}

		public ZString ForeignCurrencyEquivalent
		{
			get
			{
				return ((DebitCreditAmounts != null && DebitCreditAmounts.OSUnsignedLineAmount != 0M)
						&& TransactionHeaderCurrency != TransactionLine.AL_RX_NKTransactionCurrency)
							? ZString.Format("{0} {1}", OSUnsignedAmount, DebitCreditSign)
							: ZString.Empty;
			}
		}

		public ZString OSUnsignedAmount
		{
			get { return DebitCreditAmounts != null ? FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(DebitCreditAmounts.OSUnsignedLineAmount, Currency) : ""; }
		}

		public ZString LocalUnsignedAmount
		{
			get { return DebitCreditAmounts != null ? FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(DebitCreditAmounts.LocalUnsignedLineAmount, GlbCompany.CurrentCompany.LocalCurrency) : ""; }
		}

		ZDecimal LocalUnsignedAmountDecimal
		{
			get { return !LocalUnsignedAmount.IsEmpty ? DebitCreditAmounts.LocalUnsignedLineAmount : ZDecimal.Zero; }
		}

		public ZString CreatingUserName
		{
			get { return TransactionLine.AL_Calc_CreatingUserName; }
		}

		public ZDateTime CreatedDate
		{
			get { return TransactionLine.AL_Calc_CreatedDate; }
		}

		public ZDecimal LocalExTaxAmount
		{
			get { return TransactionLine.AL_LocalExTaxAmount; }
		}

		public ZDecimal LocalTaxAmount
		{
			get { return TransactionLine.AL_LocalTaxAmount; }
		}

		public ZDecimal OSExTaxAmount
		{
			get { return TransactionLine.AL_OSExTaxAmount; }
		}

		public ZDecimal OSTaxAmount
		{
			get { return TransactionLine.AL_OSTaxAmount; }
		}

		public ZDecimal OverseasTotal
		{
			get { return TransactionLine.AL_OverseasTotal; }
		}

		public DocChargeCode ChargeCode
		{
			get { return DocChargeCode.New(TransactionLine.ChargeCode, Factory); }
		}

		public DocGLAccount GLAccount
		{
			get { return DocGLAccount.New(TransactionLine.GLHeader, Factory); }
		}

		public DocGLAccount PercentOf
		{
			get { return DocGLAccount.New(TransactionLine.PercentOf, Factory); }
		}

		public DocTransactionHeader Invoice
		{
			get { return DocTransactionHeader.New(Factory, TransactionLine.AL_AH); }
		}

		public DocTaxRate TaxRate
		{
			get { return DocTaxRate.New(TransactionLine.TaxRate, Factory); }
		}

		public DocWithholdingTaxRate WithholdingTaxRate
		{
			get { return DocWithholdingTaxRate.New(TransactionLine.Withholding, Factory); }
		}

		public ZString JobNumber
		{
			get { return JobHeader != null ? JobHeader.JobNumber : ZString.Empty; }
		}

		public ZString Description
		{
			get { return TransactionLine.AL_Desc; }
		}

		public ZString ChargeDescription
		{
			get { return RelatedCharge != null ? RelatedCharge.Description : ZString.Empty; }
		}

		public ZDecimal InvertedMatchedAmountInInvoiceCurrency
		{
			get
			{
				return -MatchedAmountInInvoiceCurrency;
			}
		}

		public ZDecimal MatchedAmountInInvoiceCurrency
		{
			get
			{
				ZDecimal result = 0;
				if (TransactionLine.TransactionHeader != null)
				{
					if (TransactionHeaderOSOutstandingAmountProvider.IsFeatureEnabled(TransactionLine.TransactionHeader) &&
						TransactionLine.TransactionHeader.AH_RX_NKTransactionCurrency == TransactionLine.AL_RX_NKTransactionCurrency)
					{
						result = MatchedOSAmount;
					}
					else if (TransactionLine.TransactionHeader.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						result = MatchedAmount;
					}
					else if (MatchedAmount == TransactionLine.AL_LineAmount + TransactionLine.AL_GSTVAT)
					{
						result = TransactionLine.AL_OSAmount;
					}
					else
					{
						result = Env.CurrentCompany.ExchangeRate.LocalToForeign(MatchedAmount, TransactionLine.AL_ExchangeRate, TransactionLine.AL_RX_NKTransactionCurrency);
					}
				}
				return result;
			}
		}

		public ZString TransactionHeaderCurrency
		{
			get
			{
				return TransactionLine.TransactionHeader != null ? TransactionLine.TransactionHeader.AH_RX_NKTransactionCurrency : ZString.Empty;
			}
		}

		public ZDecimal InvertedMatchedAmountInJobChargeCurrency
		{
			get
			{
				return -MatchedAmountInJobChargeCurrency;
			}
		}

		public ZDecimal MatchedAmountInJobChargeCurrency
		{
			get
			{
				ZDecimal result = 0;
				if (RelatedCharge != null)
				{
					if (TransactionLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Cost || TransactionLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.UnapprovedCost)
					{
						if (RelatedCharge.JobCharge.JR_RX_NKCostCurrency == TransactionLine.AL_RX_NKTransactionCurrency &&
							TransactionLine.TransactionHeader != null && TransactionHeaderOSOutstandingAmountProvider.IsFeatureEnabled(TransactionLine.TransactionHeader))
						{
							result = MatchedOSAmount;
						}
						else if (RelatedCharge.JobCharge.JR_RX_NKCostCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							result = MatchedAmount;
						}
						else if (MatchedAmount == TransactionLine.AL_LineAmount + TransactionLine.AL_GSTVAT)
						{
							result = -(RelatedCharge.JobCharge.JR_OSCostAmt + RelatedCharge.JobCharge.JR_OSCostGSTAmt_Calc);
						}
						else
						{
							result = Env.CurrentCompany.ExchangeRate.LocalToForeign(MatchedAmount, RelatedCharge.JobCharge.JR_OSCostExRate, RelatedCharge.JobCharge.JR_RX_NKCostCurrency);
						}
					}
					else
					{
						if (RelatedCharge.JobCharge.JR_RX_NKSellCurrency == TransactionLine.AL_RX_NKTransactionCurrency &&
							TransactionLine.TransactionHeader != null && TransactionHeaderOSOutstandingAmountProvider.IsFeatureEnabled(TransactionLine.TransactionHeader))
						{
							result = MatchedOSAmount;
						}
						else if (RelatedCharge.JobCharge.JR_RX_NKSellCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							result = MatchedAmount;
						}
						else if (MatchedAmount == TransactionLine.AL_LineAmount + TransactionLine.AL_GSTVAT)
						{
							result = RelatedCharge.JobCharge.JR_OSSellAmt + RelatedCharge.JobCharge.JR_OSSellGSTAmt_Calc;
						}
						else
						{
							result = Env.CurrentCompany.ExchangeRate.LocalToForeign(MatchedAmount, RelatedCharge.JobCharge.JR_OSSellExRate, RelatedCharge.JobCharge.JR_RX_NKSellCurrency);
						}
					}
				}
				return result;
			}
		}

		public ZString ChargeCurrency
		{
			get
			{
				if (TransactionLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Cost || TransactionLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.UnapprovedCost)
				{
					return RelatedCharge != null ? RelatedCharge.OSCostCurrency.Code : ZString.Empty;
				}
				else
				{
					return RelatedCharge != null ? RelatedCharge.OSSellCurrency.Code : ZString.Empty;
				}
			}
		}

		public DocJobCharge RelatedCharge
		{
			get { return DocJobCharge.New(TransactionLine.RelatedJobCharge, Factory); }
		}

		public virtual ZDecimal ExchangeRate
		{
			get
			{
				if (TransactionLine.TransactionHeader != null)
				{
					return TransactionLine.TransactionHeader.AH_ExchangeRate;
				}
				else
				{
					return TransactionLine.AL_ExchangeRate;
				}
			}
		}

		public DocBranch Branch
		{
			get { return DocBranch.New(TransactionLine.Branch, Factory); }
		}

		public DocDepartment Department
		{
			get { return DocDepartment.New(TransactionLine.Department, Factory); }
		}

		public ZDecimal GSTVAT
		{
			get { return TransactionLine.AL_GSTVAT; }
		}

		public DocJobHeader JobHeader
		{
			get { return DocJobHeader.New(TransactionLine.Job, Factory); }
		}

		public ZDecimal LineAmount
		{
			get { return TransactionLine.AL_LineAmount; }
		}

		public ZDecimal LocalTotalAmount
		{
			get { return TransactionLine.AL_LocalTotalAmount; }
		}

		public ZString LineType
		{
			get { return TransactionLine.AL_LineType; }
		}

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(TransactionLine.Header, Factory); }
		}

		public ZDecimal OSAmount
		{
			get { return TransactionLine.AL_OSAmount; }
		}

		public ZDecimal OSUnitPrice
		{
			get { return TransactionLine.AL_OSUnitPrice; }
		}

		public ZInt PercentageOfPeriod
		{
			get { return TransactionLine.AL_PercentageOfPeriod; }
		}

		public ZDateTime PostDate
		{
			get { return TransactionLine.AL_PostDate; }
		}

		public ZInt PostPeriod
		{
			get { return TransactionLine.AL_PostPeriod; }
		}

		public ZBool PostToGL
		{
			get { return TransactionLine.AL_PostToGL == "Y"; }
		}

		public ZBool PreventInvoicePrintGrouping
		{
			get { return TransactionLine.AL_PreventInvoicePrintGrouping; }
		}

		public ZDateTime ReverseDate
		{
			get { return TransactionLine.AL_ReverseDate; }
		}

		public ZInt ReversePeriod
		{
			get { return TransactionLine.AL_ReversePeriod; }
		}

		public ZBool ReverseToGL
		{
			get { return TransactionLine.AL_ReverseToGL == "Y"; }
		}

		public DocCurrency Currency
		{
			get { return DocCurrency.New(TransactionLine.TransactionCurrency, Factory); }
		}

		public ZShort Sequence
		{
			get { return TransactionLine.AL_Sequence; }
		}

		public ZDecimal UnitPrice
		{
			get { return TransactionLine.AL_UnitPrice; }
		}

		public ZInt UnitQty
		{
			get { return TransactionLine.AL_UnitQty; }
		}

		public ZDecimal WithholdingTax
		{
			get { return TransactionLine.AL_WithholdingTax; }
		}

		IDebitCreditAmounts DebitCreditAmounts
		{
			get { return TransactionLine as IDebitCreditAmounts; }
		}
	}
}

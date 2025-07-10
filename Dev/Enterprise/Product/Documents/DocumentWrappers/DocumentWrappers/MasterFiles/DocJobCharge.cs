using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class DocJobCharge : DocBaseWrapperWithJobHeader
	{
		#region Construction

		protected DocJobCharge(JobCharge jobCharge, BusinessObjectFactory factoryToWrap)
			: base(jobCharge, factoryToWrap)
		{
		}

		public static DocJobCharge New(JobCharge jobCharge, BusinessObjectFactory factoryToWrap)
		{
			return jobCharge != null ? new DocJobCharge(jobCharge, factoryToWrap) : null;
		}

		public JobCharge JobCharge
		{
			get { return (JobCharge)WrappedObject; }
		}

		IReceivablesPostingCharge ReceivableCharge
		{
			get { return JobCharge as IReceivablesPostingCharge; }
		}

		#endregion

		#region INCOTERM Collect Charges Logic

		public ZDecimal CollectChargesAmount(ZString iNCOTerm)
		{
			if (OSSellAmt != 0 && IsCollectCharge(iNCOTerm))
			{
				return OSSellAmt;
			}

			return ZDecimal.Zero;
		}

		public ZString CollectChargesDescription(ZString iNCOTerm, ChargeDescriptionLanguageModes languageMode)
		{
			ZString result = ZString.Empty;

			if (IsCollectCharge(iNCOTerm))
			{
				result = GetChargeDescription(languageMode);
			}

			return result;
		}

		public ZString GetChargeDescription(ChargeDescriptionLanguageModes languageMode)
		{
			ZString result = languageMode == ChargeDescriptionLanguageModes.EnglishOnly ? EnglishOnlyDescription : Description;

			if (result.IsEmpty && ChargeCode != null)
			{
				result = ChargeCode.Desc;
			}

			if (!result.IsEmpty)
			{
				result = result.Replace("\n", " ").SubstringSafe(0, ChargeInfoDescriptionWidth);
			}

			return result;
		}

		public enum ChargeDescriptionLanguageModes
		{
			LocalLanguage,
			EnglishOnly
		}

		public ZBool IsCollectCharge(ZString incoTerm)
		{
			var result = ZBool.False;

			if (ChargeCode != null)
			{
				IncoTerm incoTermBizo;
				if (IncoTermRegistry.TryGetValue(incoTerm, out incoTermBizo))
				{
					var prepaidCollect = IncoTermRegistry.GetPrepaidCollect(ChargeCode.ChargeGroup, incoTermBizo.IncoTermCode);
					result = prepaidCollect == Constants.PaymentType.Collect;
				}
			}

			return result;
		}

		public ZString GetCollectChargesInfo(ZString iNCOTerm, ChargeDescriptionLanguageModes languageMode)
		{
			ZString result = ZString.Empty;

			if (OSSellAmt != 0 && IsCollectCharge(iNCOTerm))
			{
				result = GetChargesInfo(languageMode);
			}

			return result;
		}

		public ZString GetChargesInfo(ChargeDescriptionLanguageModes languageMode)
		{
			return ChargeInfoEnbableColumnarFormat ? GetChargeDescription(languageMode).PadRight(chargeInfoDescriptionWidth + ChargeInfoGapWidth) +
				GetChargeAmount().SubstringSafe(0, ChargeInfoAmountWidth).PadLeft(ChargeInfoAmountWidth) + "\n" :
				GetChargeDescription(languageMode) + " " + GetChargeAmount() + "\n";
		}

		public ZString GetChargeAmount()
		{
			return new Money(OSSellAmt, JobCharge.SellCurrency).ToString();
		}

		#endregion

		#region Related Objects

		public DocBankAccount BankAccount
		{
			get { return DocBankAccount.New(JobCharge.BankAccount, Factory); }
		}

		public DocChargeCode ChargeCode
		{
			get { return DocChargeCode.New(JobCharge.ChargeCode, Factory); }
		}

		public DocProfitShareShipmentDetail ProfitShareShipmentDetail
		{
			get { return fProfitShareShipmentDetail; }
			set { fProfitShareShipmentDetail = value; }
		}

		DocProfitShareShipmentDetail fProfitShareShipmentDetail;

		#endregion

		#region Properties

		public override string ToString()
		{
			return Description;
		}

		public ZDecimal TaxAmount
		{
			get
			{
				ZDecimal result;

				if (IsRevenuePosted && JobCharge.ARLine != null)
				{
					result = JobCharge.ARLine.AL_GSTVAT;
				}
				else
				{
					result = TaxAmountCalculator.GetLocalTaxAmount(Factory, JobCharge.JR_GC, ReceivableCharge.LocalSellAmount, JobCharge.SellGSTRate, JobCharge.SellGSTRate?.GetRate(JobCharge.JR_SellTaxDate)
						, JobCharge.SellGSTRate?.GetEffectiveExtraRate(JobCharge.JR_SellTaxDate), ReceivableCharge.OSSellTaxAmount, ReceivableCharge.SellExchangeRate);
				}

				return result;
			}
		}

		public ZDecimal SumLocalSellAndTaxAmount
		{
			get { return LocalSellAmount + TaxAmount; }
		}

		public ZDateTime APInvoiceDate
		{
			get { return JobCharge.JR_APInvoiceDate; }
		}

		public ZString APInvoiceNum
		{
			get { return JobCharge.JR_APInvoiceNum; }
		}

		public DocTaxRate CostGSTTaxRate
		{
			get { return DocTaxRate.New(JobCharge.CostGSTRate, Factory); }
		}

		public DocTaxRate SellGSTTaxRate
		{
			get { return DocTaxRate.New(JobCharge.SellGSTRate, Factory); }
		}

		public DocWithholdingTaxRate CostWithholdingTaxRate
		{
			get { return DocWithholdingTaxRate.New(JobCharge.CostWHTRate, Factory); }
		}

		public DocWithholdingTaxRate SellWithholdingTaxRate
		{
			get { return DocWithholdingTaxRate.New(JobCharge.SellWHTRate, Factory); }
		}

		public ZString ChequeNo
		{
			get { return JobCharge.JR_ChequeNo; }
		}

		public ZBool CostRated
		{
			get { return JobCharge.JR_CostRated; }
		}

		public ZGuid CostSplitGroup
		{
			get { return JobCharge.JR_E6; }
		}

		public ZDecimal DeclaredOSCostAmt
		{
			get { return JobCharge.JR_DeclaredOSCostAmt; }
		}

		public virtual ZString Description
		{
			get { return JobCharge.JR_Desc.Replace("\r", ""); }
		}

		public virtual ZString EnglishOnlyDescription
		{
			get { return JobCharge.EnglishOnlyDescription.Replace("\r", ""); }
		}

		public DocBranch Branch
		{
			get { return DocBranch.New(JobCharge.Branch, Factory); }
		}

		public DocDepartment Department
		{
			get { return DocDepartment.New(JobCharge.Department, Factory); }
		}

		public override DocJobHeader JobHeader
		{
			get { return DocJobHeader.New(JobCharge.Job, Factory); }
		}

		public ZDecimal LineCFX
		{
			get { return JobCharge.JR_LineCFX; }
		}

		public ZDecimal LocalCostAmount
		{
			get { return JobCharge.JR_LocalCostAmt; }
		}

		public virtual ZDecimal AgentDeclaredCostAmount
		{
			get { return JobCharge.JR_AgentDeclaredCostAmtLocal; }
		}

		public ZDecimal LocalSellAmount
		{
			get { return ReceivableCharge.LocalSellAmount; }
		}

		public ZDecimal LocalSellAmountForIceland
		{
			get { return Env.CurrentCompany.ExchangeRate.ForeignToLocal(OSSellAmtOrEstimatedRevenue, JobCharge.JR_OSSellExRate); }
		}

		public ZDecimal LocalSellAmountIncTax
		{
			get { return LocalSellAmount + TaxAmount; }
		}

		public ZDecimal LocalSellAmountIncTaxForIceland
		{
			get { return LocalSellAmountForIceland + TaxAmount; }
		}

		public virtual ZDecimal AgentDeclaredSellAmount
		{
			get { return JobCharge.JR_AgentDeclaredSellAmtLocal; }
		}

		public ZDecimal LocalSellTaxAmount
		{
			get { return JobCharge.JR_Calc_LocalSellTaxAmt; }
		}

		public ZDecimal AgentDeclaredProfit
		{
			get { return AgentDeclaredSellAmount - AgentDeclaredCostAmount; }
		}

		public ZDecimal EstimatedRevenue
		{
			get { return JobCharge.JR_EstimatedRevenue; }
		}

		public ZDecimal ProfitToShare => ProfitShareShipmentDetail?.CalculateProfitShare(JobCharge) ?? 0m;

		public DocOrganisation CostAccount
		{
			get { return DocOrganisation.New(JobCharge.CostAccount, Factory); }
		}

		public DocOrganisation SellAccount
		{
			get { return DocOrganisation.New(JobCharge.SellAccount, Factory); }
		}

		public ZDecimal OSCostAmt
		{
			get { return JobCharge.JR_OSCostAmt; }
		}

		public ZDecimal OSCostExRate
		{
			get { return JobCharge.JR_OSCostExRate; }
		}

		public ZDecimal OSSellAmt
		{
			get { return JobCharge.JR_OSSellAmt; }
		}

		public ZDecimal OSSellAmtOrEstimatedRevenue
		{
			get { return OSSellAmt.IsEmpty ? JobCharge.JR_EstimatedRevenue : OSSellAmt; }
		}

		public ZDecimal OSSellExRate
		{
			get
			{
				var result = JobCharge.JR_OSSellExRate;
				if (JobCharge.BillInInvoiceCurrency)
				{
					result = ReceivableCharge != null && !ReceivableCharge.OSSellAmount.IsEmpty && !JobCharge.JR_OSSellAmt.IsEmpty ?
						new ZDecimal(Utilities.Round(GlbCompany.CurrentCompany.GC_IsReciprocal ? ReceivableCharge.OSSellAmount / JobCharge.JR_OSSellAmt : JobCharge.JR_OSSellAmt / ReceivableCharge.OSSellAmount, SellExRateDecimals)) :
						ZDecimal.Zero;
				}
				return result;
			}
		}

		int SellExRateDecimals
		{
			get { return MetaData.GetDecimalPlaces(JobCharge, TypeDescriptor.GetProperties(JobCharge)[JobCharge.JR_OSSellExRateInfo.Name]); }
		}

		public ZDateTime PaymentDate
		{
			get { return JobCharge.JR_PaymentDate; }
		}

		public ZString PaymentType
		{
			get { return JobCharge.JR_PaymentType; }
		}

		public DocCurrency OSCostCurrency
		{
			get { return DocCurrency.New(JobCharge.CostCurrency, Factory); }
		}

		internal RefCurrency ForeignSellCurrency
		{
			get { return JobCharge.SellCurrency; }
		}

		public DocCurrency OSSellCurrency
		{
			get { return DocCurrency.New(JobCharge.SellCurrency, Factory); }
		}

		public ZBool SellRated
		{
			get { return JobCharge.JR_SellRated; }
		}

		public ZString ExchangeRateAndAmount
		{
			get
			{
				var result = new ZStringBuilder();

				if (JobHeader != null && JobHeader.LocalCharges != null &&
					ShowLocalAmountAndExRateOnInvoice &&
					OSSellCurrency != null && (OSSellCurrency.Code != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency || JobCharge.BillInInvoiceCurrency))
				{
					result.Append(OSSellCurrency.Code + " ");
					if (OSSellAmt.IsValid)
					{
						result.Append(OSSellAmt.ToString(OSSellCurrency.Decimals));
					}

					if (OSSellExRate.IsValid)
					{
						result.Append(" @ " + OSSellExRate.ToString(SellExRateDecimals));
					}
				}

				return result.ToString();
			}
		}

		public ZBool ShowLocalAmountAndExRateOnInvoice
		{
			get
			{
				ZBool result = ZBool.False;

				if (JobCharge != null && JobCharge.Job != null && JobCharge.Job.LocalCharges != null)
				{
					ZString invoiceLineDisplayOption = new OrgInvoiceRollupOrGroup.Loader(JobCharge.Job.LocalCharges, JobCharge.Branch, JobCharge.Department).GetInvoiceLineDisplayOption(
						GetServiceDirection(GenericInvoicingJob.Origin, GenericInvoicingJob.Destination), GenericInvoicingJob.TransportMode, Mode, GenericInvoicingJob.JobType);

					if (!invoiceLineDisplayOption.IsEmpty)
					{
						result = OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionIncludesExchangeRate(invoiceLineDisplayOption);
					}
				}

				return result;
			}
		}

		public ZString FormattedLocalAmmount
		{
			get { return FormatLocalAmount(LocalSellAmount); }
		}

		public ZString FormattedLocalAmmountIncTax
		{
			get { return FormatLocalAmount(LocalSellAmountIncTax); }
		}

		public ZString FormattedTaxAmount
		{
			get { return FormatLocalAmount(TaxAmount); }
		}

		public ZString FormattedOSSellAmmount
		{
			get { return (OSSellCurrency != null) ? OSSellCurrency.FormatMoney(OSSellAmt) : (ZString)OSSellAmt.ToString(2); }
		}

		public ZBool IsRevenuePosted
		{
			get { return JobCharge.IsRevenuePosted; }
		}

		public ZBool IsProfitShare
		{
			get { return JobCharge.ChargeCode != null && (JobCharge.ChargeCode.PK == AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value || JobCharge.ChargeCode.PK == AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.Value); }
		}

		#region ChargeInfo Columnar Properties
		public ZInt ChargeInfoDescriptionWidth
		{
			get { return chargeInfoDescriptionWidth; }
			set { chargeInfoDescriptionWidth = value; }
		}
		ZInt chargeInfoDescriptionWidth = 25;

		public ZInt ChargeInfoGapWidth
		{
			get { return chargeInfoGapWidth; }
			set { chargeInfoGapWidth = value; }
		}
		ZInt chargeInfoGapWidth = 1;

		public ZInt ChargeInfoAmountWidth
		{
			get { return chargeInfoAmountWidth; }
			set { chargeInfoAmountWidth = value; }
		}
		ZInt chargeInfoAmountWidth = 15;

		public ZBool ChargeInfoEnbableColumnarFormat { get; set; }
		#endregion

		#endregion

		#region Implementation

		ZString FormatLocalAmount(ZDecimal amount)
		{
			if (Branch != null && Branch.Country != null && Branch.Country.Currency != null)
			{
				return Branch.Country.Currency.FormatMoney(amount);
			}
			else
			{
				return amount.ToString(2);
			}
		}

		#endregion
	}
}

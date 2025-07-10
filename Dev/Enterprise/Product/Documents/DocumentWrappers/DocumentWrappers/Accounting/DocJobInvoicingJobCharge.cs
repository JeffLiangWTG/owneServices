using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class DocJobInvoicingJobCharge : DocJobCharge
	{
		DocJobInvoicingJobCharge(Charge charge, BusinessObjectFactory factoryToWrap)
			: base(charge, factoryToWrap)
		{
		}

		public static DocJobInvoicingJobCharge New(Charge charge, BusinessObjectFactory factoryToWrap)
		{
			if (charge == null)
			{
				return null;
			}
			else
			{
				return new DocJobInvoicingJobCharge(charge, factoryToWrap);
			}
		}

		public static DocJobInvoicingJobCharge[] NewForJobProfit(Charge charge, BusinessObjectFactory factoryToWrap)
		{
			List<DocJobInvoicingJobCharge> wrappers = new List<DocJobInvoicingJobCharge>();
			wrappers.Add(New(charge, factoryToWrap));

			if (charge.IsTaxExpense)
			{
				var chargeToDisplay = New(charge, factoryToWrap);
				chargeToDisplay.IsTaxExpense = true;
				wrappers.Add(chargeToDisplay);
			}
			return wrappers.ToArray();
		}

		public static DocJobInvoicingJobCharge[] NewForJobPofit(Charge charge, AccTransactionLines line, BusinessObjectFactory factoryToWrap)
		{
			List<DocJobInvoicingJobCharge> wrappers = new List<DocJobInvoicingJobCharge>();
			wrappers.Add(New(charge, factoryToWrap));

			var taxExpenseDatesAndAmounts = line?.GetTaxExpenses();
			if (taxExpenseDatesAndAmounts != null && taxExpenseDatesAndAmounts.Any())
			{
				foreach (var taxExpenseDateAndAmount in taxExpenseDatesAndAmounts)
				{
					var wrapper = New(charge, factoryToWrap);
					wrapper.IsTaxExpense = true;
					wrapper.TaxExpenseDate = taxExpenseDateAndAmount.TaxExpenseDate;
					wrapper.TaxExpenseAmount = taxExpenseDateAndAmount.TaxExpenseAmount;
					wrappers.Add(wrapper);
				}
			}

			return wrappers.ToArray();
		}

		ZDateTime TaxExpenseDate;
		ZDecimal TaxExpenseAmount;

		public ZString QuoteCurrencyLabel
		{
			get
			{
				ZString result = ZString.Empty;
				if (DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage.Value)
				{
					result = (NoResString)"Quote Currency";
				}
				return result;
			}
		}

		public ZGuid ChargePK
		{
			get { return Charge.PK; }
		}

		public override string ToString()
		{
			return ChargeCode.ToString();
		}

		public ZBool CostPosted
		{
			get { return Charge.JR_IsCostPosted; }
		}

		public ZDecimal Cost_GSTAmount
		{
			get { return Charge.JR_OSCostGSTAmt_Calc; }
		}

		public ZDecimal Cost_LocalGSTAmount
		{
			get { return Charge.JR_Cost_LocalGSTAmount; }
		}

		public ZDecimal Cost_WHTAmount
		{
			get { return Charge.JR_OSCostWHTAmt; }
		}

		public ZDecimal Cost_LocalWHTAmount
		{
			get { return Charge.JR_Cost_LocalWHTAmount; }
		}

		public ZInt DisplaySequence
		{
			get { return Charge.JR_DisplaySequence; }
		}

		public ZBool SellPosted
		{
			get { return Charge.JR_IsRevenuePosted; }
		}

		public ZBool IsPosted
		{
			get { return Charge.JR_IsPosted; }
		}

		public ZBool IsApportioned
		{
			get { return Charge.JR_IsApportioned; }
		}

		public ZDecimal SellGSTAmount
		{
			get { return Charge.JR_OSSellGSTAmt_Calc; }
		}

		public ZDecimal SellWHTAmount
		{
			get { return Charge.JR_OSSellWHTAmt; }
		}

		public ZDecimal OSSellAmtWithGST
		{
			get { return Charge.JR_Calc_OSSellAmtWithGST; }
		}

		public ZDecimal OSCostAmtWithGST
		{
			get { return Charge.JR_Calc_OSCostAmtWithGST; }
		}

		public ZDecimal LocalCostAmtWithGST
		{
			get { return Charge.JR_Calc_LocalCostAmtWithGST; }
		}

		public ZString JobNumber
		{
			get { return Charge.JR_JobNumber; }
		}

		public ZString ChequeOrReferenceLabel
		{
			get { return Charge.JR_ChequeOrReferenceLabel; }
		}

		#region Profit Share

		public override ZDecimal AgentDeclaredSellAmount
		{
			get { return Charge.JR_AgentDeclaredSellAmt; }
		}

		public override ZDecimal AgentDeclaredCostAmount
		{
			get { return Charge.JR_AgentDeclaredCostAmt; }
		}

		public ZDecimal AgentDeclaredCostAmountDueAgent
		{
			get { return IsPayableToOverseasAgent ? AgentDeclaredCostAmount : ZDecimal.Zero; }
		}

		public ZBool IsPayableToOverseasAgent
		{
			get { return Charge.CostAccount != null && Charge.Job != null && Charge.CostAccount == Charge.Job.AgentCollect; }
		}

		public ZBool IncludedInProfitShare
		{
			get { return Charge.JR_IsIncludedInProfitShare; }
		}

		public ZDecimal ProfitShareForCharge
		{
			get
			{
				var parentJob = Charge.Job as Job;
				if (parentJob == null || !IncludedInProfitShare)
				{
					return ZDecimal.Zero;
				}

				var profitShareDetails = parentJob.ProfitShareAgreement;
				if (profitShareDetails == null)
				{
					return ZDecimal.Zero;
				}

				var party = profitShareDetails.PartyDetails.GetParty(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent);
				if (party == null)
				{
					return ZDecimal.Zero;
				}

				var (result, _) = party.CalculateProfitShare(AgentDeclaredProfit, AgentDeclaredSellAmount, 0m, 0);
				return result;
			}
		}

		public DocJobPaymentBasisCollection CostPaymentBases
		{
			get
			{
				if (costPaymentBases == null)
				{
					var docs = Charge.CostPaymentBases.Select(v => DocJobPaymentBasis.New(v, Factory));

					costPaymentBases = new DocJobPaymentBasisCollection(Factory);
					costPaymentBases.AddRange(docs);
				}

				return costPaymentBases;
			}
		}

		public DocJobPaymentBasisCollection SellPaymentBases
		{
			get
			{
				if (sellPaymentBases == null)
				{
					var docs = Charge.SellPaymentBases.Select(v => DocJobPaymentBasis.New(v, Factory));

					sellPaymentBases = new DocJobPaymentBasisCollection(Factory);
					sellPaymentBases.AddRange(docs);
				}

				return sellPaymentBases;
			}
		}

		DocJobPaymentBasisCollection costPaymentBases;
		DocJobPaymentBasisCollection sellPaymentBases;

		#endregion

		public ZString Desc
		{
			get
			{
				ZString result = Charge.JR_Desc;

				if (Charge.InvoicingJob != null
					&& Charge.ShouldDisplayTaxApplicabilityForRatingHeader)
				{
					result += " *";
				}

				return result;
			}
		}

		public ZString CalculationDescription
		{
			get
			{
				if (DocumentsDataRegistry.Instance.ShowCalculationDescriptionOnOneOffQuotes.Value == ShowCalculationDescriptionOnOneOffQuotesCode.Yes)
				{
					ZString result = ORtfTextUtil.RtfToText(Charge.RevenueCalculationDescription);

					if (result.Contains("\n") && result.Contains(": "))
					{
						result = result.Substring(0, result.IndexOf("\n"));

						int semicolon = result.IndexOf(": ");
						result = result.Substring(semicolon + 2);
						if (!result.IsEmpty)
						{
							result = "  " + result;
						}
					}
					else
					{
						result = ZString.Empty;
					}
					return result;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString BranchCode
		{
			get { return Charge.Branch != null ? Charge.Branch.GB_Code : ZString.Empty; }
		}

		public ZString DepartmentCode
		{
			get { return Charge.Department != null ? Charge.Department.GE_Code : ZString.Empty; }
		}

		public ZDecimal EstimatedCost
		{
			get { return ZDecimal.Parse(Charge.JR_EstimatedCost.ToStringTrimZeros()); }
		}

		public ZGuid JobHeaderPK
		{
			get { return Charge.Job != null ? Charge.Job.PK : ZGuid.Empty; }
		}

		public ZDecimal LocalCostAmt
		{
			get { return IsTaxExpense ? (ZDecimal)(-TaxExpenseAmount) : Charge.JR_LocalCostAmt; }
		}

		public ZString LocalSellAmtForReport
		{
			get
			{
				ZString result = ZString.Empty;
				if (DocumentsDataRegistry.Instance.ShowLocalCurrencyonSpotQuotePricingPage.Value)
				{
					result = LocalSellAmt.ToString(2);
				}
				return result;
			}
		}

		public ZInt ExchangeRateDecimalPlaces
		{
			get { return GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces; }
		}

		public ZDecimal LocalSellAmt
		{
			get { return Charge.JR_LocalSellInvoiceAmt; }
		}

		public ZString LocalCurrency
		{
			get { return Charge.JR_LocalCurrencyCode; }
		}

		public ZBool IsApproved
		{
			get { return Charge.JR_IsApproved; }
		}

		public ZDecimal CFXJnl
		{
			get { return Charge.JR_CFXAmtReverseSign; }
		}

		public ZShort ChargeCodePrintSequence
		{
			get { return Charge.ChargeCodePrintSequence; }
		}

		public ZString SellRecognition
		{
			get { return Charge.SellRecognition; }
		}

		public ZString CostRecognition
		{
			get { return Charge.CostRecognition; }
		}

		public ZString ProductName
		{
			get { return Charge.JobChargeAttrib_Product; }
		}

		public ZBool PreventInvoicePrintGrouping
		{
			get { return Charge.JR_PreventInvoicePrintGrouping; }
		}

		public ZDecimal SellAmtBuyRateExchangeRate
		{
			get { return Charge.CostExchangeRate?.Rate ?? 1m; }
		}

		public ZBool ChargeIsGSTApplicable
		{
			get
			{
				return Charge.ChargeCode != null &&
					Charge.ChargeCode.GSTRate != null &&
					Charge.ChargeCode.GSTRate.GetRateRaw(ZDate.Today) > 0;
			}
		}

		public ZBool ShowChargeOnQuotation
		{
			get { return ChargeCode != null && ChargeCode.AccChargeCode.AC_ShowOnQuotation && (LocalSellAmt != 0m || !ChargeCode.AccChargeCode.AC_SuppressOnQuoteIfZero); }
		}

		public ZString JobChargeAttrib_AllCalculatorDescriptions
		{
			get
			{
				var values = Charge?.JobChargeAttrib_AllCalculatorDescriptions.OrderBy(x => x).Select((value, index) => $"{(index + 1)}. {value}");
				return new ZString(string.Join("\r\n", values));
			}
		}

		public ZBool IsDetentionDemurrageChargeSubGroupForOSRA
		{
			get
			{
				var chargeSubGroup = Charge?.ChargeCodeSubGroup ?? ZString.Empty;
				return ((chargeSubGroup == (ZString)ChargeCodeSubGroupList.Storage
					   || chargeSubGroup == (ZString)ChargeCodeSubGroupList.CarrierStorage
					   || chargeSubGroup == (ZString)ChargeCodeSubGroupList.ContainerDetention));
			}
		}

		bool ExcludeThisCharge
		{
			get { return !AccountingConfigurationRegistry.Instance.IncludeDisbursementsPercentageMarginCalculations.Value && Job.GetChargeTypeInformation(Charge.ChargeCode, Charge.Job as Job).IsDisbursement; }
		}

		#region Job Profit Document

		public ZDecimal Revenue
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				result += !LocalSellAmt.IsEmpty && Charge.ARLine != null && Charge.ARLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Revenue
					? LocalSellAmt : ZDecimal.Zero;
				result -= Charge.IsCFXPosted ? Charge.JR_CFXAmt : ZDecimal.Zero;
				return result;
			}
		}

		ZDecimal CostJRJAmount
		{
			get
			{
				return !LocalCostAmt.IsEmpty && IsJobRevenueJournal(Charge.APLine)
					? LocalCostAmt : ZDecimal.Zero;
			}
		}

		public ZDecimal RevenueAndCostJRJ => IsTaxExpense ? TaxExpenseAmount : (ZDecimal)(Revenue - CostJRJAmount);

		bool IsJobRevenueJournal(AccTransactionLines line)
		{
			return line != null && line.AL_LineType == TransactionLineTypes.Revenue && line.TransactionHeader != null && line.TransactionHeader.AH_TransactionType == TransactionTypes.JobRevenueJournal;
		}

		public ZDecimal WIP
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				result += !LocalSellAmt.IsEmpty && (Charge.ARLine == null || Charge.ARLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.WIP)
					? LocalSellAmt : ZDecimal.Zero;
				result -= !Charge.IsCFXPosted ? Charge.JR_CFXAmt : ZDecimal.Zero;
				return result;
			}
		}

		public ZDecimal Cost
		{
			get
			{
				return !LocalCostAmt.IsEmpty && Charge.APLine != null &&
					(Charge.APLine.AL_LineType == TransactionLineTypes.Cost || Charge.APLine.AL_LineType == TransactionLineTypes.Revenue) ? LocalCostAmt : ZDecimal.Zero;
			}
		}

		public ZDecimal Accrual
		{
			get
			{
				return !LocalCostAmt.IsEmpty && (Charge.APLine == null || Charge.APLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Accrual)
					? LocalCostAmt : ZDecimal.Zero;
			}
		}

		public ZDecimal Income
		{
			get
			{
				var income = 0m;
				if (IsTaxExpense)
				{
					income = Charge.JR_TotalTaxExpenseRevenue;
				}
				else
				{
					income = Revenue + WIP;
				}
				return income;
			}
		}

		public ZDecimal Expense
		{
			get
			{
				var expense = 0m;
				if (IsTaxExpense)
				{
					expense = -Charge.JR_TotalTaxExpenseCost;
				}
				else
				{
					expense = Cost + Accrual;
				}
				return expense;
			}
		}

		public ZDecimal IncomeAfterChargeExclusion
		{
			get { return !ExcludeThisCharge ? Income : 0; }
		}

		public ZDecimal ExpenseAfterChargeExclusion
		{
			get { return !ExcludeThisCharge ? Expense : 0; }
		}

		public ZDecimal Profit
		{
			get { return Income - Expense; }
		}

		public ZDateTime RevenueRecognizeDate
		{
			get
			{
				if (IsTaxExpense)
				{
					return TaxExpenseDate;
				}
				if (IsJobRevenueJournal(Charge.APLine))
				{
					return APLine.RecognizeDate;
				}
				else
				{
					return ARLine != null ? ARLine.RecognizeDate : ZDateTime.Empty;
				}
			}
		}

		public ZDateTime RevenueReverseDate
		{
			get
			{
				if (IsTaxExpense)
				{
					return TaxExpenseDate;
				}
				if (IsJobRevenueJournal(Charge.APLine))
				{
					return APLine.ReverseDate;
				}
				else
				{
					return ARLine != null ? ARLine.ReverseDate : ZDateTime.Empty;
				}
			}
		}

		public ZDateTime APLineReverseDate => IsTaxExpense ? TaxExpenseDate : (APLine != null ? APLine.ReverseDate : ZDateTime.Empty);

		public DocJobLineDetail ARLine
		{
			get { return Charge.ARLine != null ? DocJobLineDetail.New(Charge.ARLine, Factory) : null; }
		}

		public DocJobLineDetail APLine
		{
			get { return Charge.APLine != null ? DocJobLineDetail.New(Charge.APLine, Factory) : null; }
		}

		public ZString SellLineType
		{
			get { return Charge.ARLine != null ? Charge.ARLine.AL_LineType : ZString.Empty; }
		}

		public ZString CostLineType
		{
			get { return Charge.APLine != null ? Charge.APLine.AL_LineType : ZString.Empty; }
		}

		internal bool IsTaxExpense { get; set; }

		#endregion

		#region Implementation

		Charge Charge
		{
			get { return (Charge)WrappedObject; }
		}

		#endregion
	}
}

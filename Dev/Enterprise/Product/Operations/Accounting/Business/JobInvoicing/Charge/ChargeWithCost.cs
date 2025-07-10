using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Journal;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	#region Cost / Revenue Amount Type Enum - Actual or Agent Declared

	public enum CostRevenueAmountType
	{
		Actual,
		AgentDeclared
	}

	#endregion

	public abstract partial class ChargeWithCost : BaseCharge
	{
		#region Schema

		public new abstract class Schema : BaseCharge.Schema
		{
			public const string JR_CFXAmtReverseSign = "JR_CFXAmtReverseSign";
			public const string JR_ARInvoiceNumber = "JR_ARInvoiceNumber";
		}

		#endregion

		public ChargeWithCost(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (!factory.HasContext(BusinessContext.PeriodicInvoicePosting))
			{
				ChargeToExRateLinker.GetOrCreate(factory).AddLinks(this);
			}
			SetFieldsReadOnlyInitially();
			SuspendDefaulting();
		}

		public static new readonly TypeDecider TypeDecider = BaseCharge.TypeDecider;

		#region Default Values / Loading

		public override void OnLoaded()
		{
			base.OnLoaded();
			if (!IsFunctionalitySuspended())
			{
				UpdateCoreFieldsReadOnly();
				UpdateCostFieldsReadOnly();
				UpdateRevenueFieldsReadOnly();
			}

			bool IsFunctionalitySuspended()
			{
				return ServiceContainerSuspenderHelper.FunctionalitySuspender<InvoicingBaseBulkChargeImporter.FunctionalitySuspender>.IsSuspended(Factory)
					|| ServiceContainerSuspenderHelper.FunctionalitySuspender<InvoicingBaseLineImporter.FunctionalitySuspender>.IsSuspended(Factory);
			}
		}

		protected bool IsCurrentlyDefaulting;

		public void SuspendDefaulting()
		{
			IsCurrentlyDefaulting = false;
		}

		public void ResumeDefaulting()
		{
			IsCurrentlyDefaulting = true;
		}

		#endregion

		#region Saving

		protected override void OnSavedInCompanyContext(bool saveSucceeded)
		{
			base.OnSavedInCompanyContext(saveSucceeded);
			UpdateCoreFieldsReadOnly();
			UpdateCostFieldsReadOnly();
			UpdateRevenueFieldsReadOnly();
		}

		#endregion

		#region Related Business Objects

		#region Calculations

		protected internal CalculationTrigger Calculations
		{
			get
			{
				if (fCalculations == null)
				{
					fCalculations = new CalculationTrigger(this);
				}
				return fCalculations;
			}
		}

		CalculationTrigger fCalculations;

		#endregion

		#region Cost Transaction Line

		public AccTransactionLines Cost
		{
			get { return (APLine != null && APLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Cost) ? APLine : null; }
		}

		#endregion

		#region Revenue Transaction Line

		public AccTransactionLines Revenue
		{
			get { return (ARLine != null && ARLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Revenue) ? ARLine : null; }
		}

		#endregion

		#endregion

		#region Delete

		public override bool CanDelete
		{
			get
			{
				return base.CanDelete && !IsCostPosted && !IsRevenuePosted && !JR_IsApportioned && !IsDeletePreventedDueToRegistrySettingAndCostSellRated
					&& (!IsInDatabaseAndReadyForCostPosting && !IsInDatabaseAndReadyForRevenuePosting || Job.IsCancelled);
			}
		}

		bool IsDeletePreventedDueToRegistrySettingAndCostSellRated
		{
			get
			{
				return IsBaseCostFieldsReadonly || IsBaseSellFieldsReadonly;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = base.ReasonForNotAbleToDelete;
				string chargeCodeDescription = ChargeCode == null ? "" : ChargeCode.AC_Code.ToString();
				if (IsCostPosted)
				{
					result = ResString.GetMultilingualString("7ca64ea4-edd4-45f9-bba6-36ffa6eba515", "You cannot delete {0} because its cost part is posted already.", chargeCodeDescription);
				}
				else if (IsRevenuePosted)
				{
					result = ResString.GetMultilingualString("8926bb16-9931-4a20-92a0-b9280f75a11d", "You cannot delete {0} because its revenue part is posted already.", chargeCodeDescription);
				}
				else if (JR_IsApportioned)
				{
					result = ResString.GetMultilingualString("e6fabaa7-f2a9-47b3-b65e-2663c1933abd", "You cannot delete {0} because its cost was apportioned on the consol level.", chargeCodeDescription);
				}
				else if (IsDeletePreventedDueToRegistrySettingAndCostSellRated)
				{
					result = ResString.GetMultilingualString("a4fc294f-4e14-43dc-9162-5be5720f154b", "You cannot delete {0} because it's either sell rated or cost rated and the registry 'Prevent Operator From Changing Rated Line' is set to Yes.", chargeCodeDescription);
				}
				else if (IsInDatabaseAndReadyForCostPosting && IsInDatabaseAndReadyForRevenuePosting)
				{
					result = ResString.GetMultilingualString("e7016a3b-e407-4bfa-8f5c-5b7f2b9414c5", "You cannot delete {0} because it is ready to be posted.", chargeCodeDescription);
				}
				else if (IsInDatabaseAndReadyForCostPosting)
				{
					result = ResString.GetMultilingualString("b8cbb858-f404-4414-b903-99b2cf792deb", "You cannot delete {0} because its cost part is ready to be posted.", chargeCodeDescription);
				}
				else if (IsInDatabaseAndReadyForRevenuePosting)
				{
					result = ResString.GetMultilingualString("a641ed8d-17f8-4ed1-abe4-4c6b093fa37b", "You cannot delete {0} because its revenue part is ready to be posted.", chargeCodeDescription);
				}

				return result;
			}
		}

		#endregion

		#region CalculateDueDate

		void CalculateDueDate()
		{
			if (!Factory.HasContext(BusinessContext.ChargeProcessingForAPTransactionPosting) && CostAccount != null)
			{
				var calculateDate = DueDateCalculation.GetCalculateDate(CostAccount.CompanyData.GetAPTerm(), JR_APInvoiceDate, JR_APDocumentReceivedDate);
				if (calculateDate.IsValid)
				{
					JR_PaymentDate = new InvoiceAndDueDateCalculator(InvoicingJob == null ? null : InvoicingJob.PlugInData,
																		calculateDate,
																		CostAccount).DueDate;
				}
			}
		}

		#endregion

		#region Properties

		#region JR_OH_CostAccount

		public override ZGuid JR_OH_CostAccount
		{
			get { return base.JR_OH_CostAccount; }
			set
			{
				if (JR_OH_CostAccount != value)
				{
					base.JR_OH_CostAccount = value;
					CalculateDueDate();
					FetchAPInvoiceDetails();
					UpdateReadOnlyStatusOnAPInvoiceFields();
					Validation.ValidateJR_OSCostAmt();
				}
			}
		}

		public void ResetUnpostedCostTaxDefault()
		{
			if (!IsCostPosted)
			{
				if (!IsCostGSTRateActual)
				{
					ResetCostGSTTaxDefault();
				}
				if (!(ChargeCode?.IsComment ?? true))
				{
					SetCostTaxBranchDefault();
				}
			}
		}

		public void ResetCostGSTTaxDefault()
		{
			if (!JR_IsApportioned && !IsCostPosted)
			{
				SetCostTaxRateAndMessage(false);
				UpdateJR_AT_CostGSTRateReadOnly();
			}
		}

		#endregion

		#region JR_ChequeNo

		public override ZString JR_ChequeNo
		{
			get { return base.JR_ChequeNo; }
			set
			{
				if (JR_ChequeNo != value)
				{
					if (!Factory.HasContext(BusinessContext.ChargeProcessingForAPTransactionPosting))
					{
						if (JR_PaymentType == ReceiptTypes.Cheque)
						{
							value = AccValidationHelper.PadChequeDigitsWithLeadingZeros(BankAccount, value);
						}
					}

					base.JR_ChequeNo = value;
					if (!Factory.HasContext(BusinessContext.ChargeProcessingForAPTransactionPosting))
					{
						if (!IsChequeNumberAutoAllocated)
						{
							SetCurrentNo(value);
						}
					}
				}
			}
		}

		void SetCurrentNo(ZString value)
		{
			if (!JR_AB.IsEmpty && !JR_AK.IsEmpty && !JR_ChequeNo.IsEmpty)
			{
				try
				{
					int nextChequeNumber = int.Parse(value.ToString()) + 1;
					if (ChequeBook != null)
					{
						AccChequeBook.UpdateCurrentNumber(ChequeBook.PK, nextChequeNumber);
					}
				}
				catch (OverflowException) { }
				catch (FormatException) { }
				catch (ArgumentNullException) { }
			}
		}

		AccValidationHelper AccValidationHelper
		{
			get
			{
				if (fAccValidationHelper == null)
				{
					fAccValidationHelper = new AccValidationHelper();
				}
				return fAccValidationHelper;
			}
		}

		AccValidationHelper fAccValidationHelper;

		#endregion

		#region JR_AK

		public override ZGuid JR_AK
		{
			get { return base.JR_AK; }
			set
			{
				if (JR_AK != value)
				{
					base.JR_AK = value;
					if (!Factory.HasContext(BusinessContext.ChargeProcessingForAPTransactionPosting))
					{
						if (!IsChequeNumberAutoAllocated)
						{
							if (IsCurrentlyDefaulting)
							{
								base.JR_ChequeNo = ChequeBook != null ? ChequeBook.AK_CurrentNo.ToString() : "";
							}
							else
							{
								JR_ChequeNo = ChequeBook != null ? ChequeBook.AK_CurrentNo.ToString() : "";
							}
						}
						UpdateChequeNumberIsAutoAllocated();
					}
				}
			}
		}

		#endregion

		#region JR_PaymentType

		public override ZString JR_PaymentType
		{
			get { return base.JR_PaymentType; }
			set
			{
				if (JR_PaymentType != value)
				{
					base.JR_PaymentType = value;
					if (!Factory.HasContext(BusinessContext.ChargeProcessingForAPTransactionPosting))
					{
						if (value != ZArchitecture.Core.ReceiptTypes.Cheque)
						{
							JR_AK = ZGuid.Empty;
						}
						UpdateJR_AKReadOnly();
						UpdateJR_PaymentDateReadOnly();
						UpdateJR_ABReadOnly();
						UpdateJR_ChequeNoReadOnly();
						if (value == ZArchitecture.Core.ReceiptTypes.Cash && JR_ChequeNo.IsEmpty)
						{
							JR_ChequeNo = "CASH";
						}

						Calc_ChequeNumberIsAutoAllocatedLabelInfo.RefreshBinding();
					}
				}
			}
		}

		#endregion

		#region JR_APInvoiceDate

		public override ZDateTime JR_APInvoiceDate
		{
			get { return base.JR_APInvoiceDate; }
			set
			{
				if (JR_APInvoiceDate != value)
				{
					base.JR_APInvoiceDate = value;
					SetDefaultDocumentReceivedDate();
					CalculateDueDate();
					Validation.ValidateJR_OSCostAmt();
				}
			}
		}

		#endregion

		#region JR_APDocumentReceivedDate

		public override ZDateTime JR_APDocumentReceivedDate
		{
			get { return base.JR_APDocumentReceivedDate; }
			set
			{
				if (JR_APDocumentReceivedDate != value)
				{
					base.JR_APDocumentReceivedDate = value;
					CalculateDueDate();
				}
			}
		}

		void SetDefaultDocumentReceivedDate()
		{
			if (JR_APDocumentReceivedDate.IsEmpty)
			{
				var defaultLogic = AccountingMasterFilesRegistry.Instance.DocumentReceivedDateDefaultingLogic.Value;
				if (defaultLogic == AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.CreateDate)
				{
					JR_APDocumentReceivedDate = ZDateTime.Now;
				}
				else if (defaultLogic == AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.InvoiceDate)
				{
					JR_APDocumentReceivedDate = JR_APInvoiceDate;
				}
			}
		}

		#endregion

		#region JR_APInvoiceNum

		public override ZString JR_APInvoiceNum
		{
			get { return base.JR_APInvoiceNum; }
			set
			{
				if (JR_APInvoiceNum != value)
				{
					base.JR_APInvoiceNum = value;
					if (!Factory.HasContext(BusinessContext.ChargeProcessingForAPTransactionPosting))
					{
						FetchAPInvoiceDetails();
						UpdateJR_APInvoiceDateReadOnly();
						UpdateJR_APDocumentReceivedDateReadOnly();
						UpdateJR_PaymentDateReadOnly();
						UpdateJR_PaymentTypeReadOnly();
						UpdateJR_CostReferenceReadOnly();
					}
					Validation.ValidateJR_OSCostAmt();
					CheckMaximumLength(JR_APInvoiceNumInfo, value);
				}
			}
		}

		#endregion

		#region JR_AB

		public override ZGuid JR_AB
		{
			get { return base.JR_AB; }
			set
			{
				if (JR_AB != value)
				{
					base.JR_AB = value;
					if (!Factory.HasContext(BusinessContext.ChargeProcessingForAPTransactionPosting))
					{
						UpdateJR_PaymentDateReadOnly();
						UpdateJR_ChequeNoReadOnly();
					}
				}
			}
		}

		#endregion

		#region JR_AL_APLine

		public override ZGuid JR_AL_APLine
		{
			get { return base.JR_AL_APLine; }
			set
			{
				if (JR_AL_APLine != value)
				{
					base.JR_AL_APLine = value;

					JR_IsCostPostedInfo.RefreshBinding();

					UpdateCoreFieldsReadOnly();
					UpdateCostFieldsReadOnly();
					Validation.ValidateJR_APInvoiceNum();
					Validation.ValidateJR_APInvoiceDate();
				}
			}
		}

		#endregion

		#region JR_AL_ARLine

		public override ZGuid JR_AL_ARLine
		{
			get
			{
				return base.JR_AL_ARLine;
			}
			set
			{
				if (base.JR_AL_ARLine != value)
				{
					InvoicingJob?.InvalidateGroupValidation();
				}

				base.JR_AL_ARLine = value;

				JR_IsRevenuePostedInfo.RefreshBinding();

				UpdateReadOnlyStatus();
			}
		}

		#endregion

		#region JR_AC

		public override ZGuid JR_AC
		{
			get { return base.JR_AC; }
			set
			{
				if (value != JR_AC)
				{
					InvoicingJob?.InvalidateGroupValidation();

					base.JR_AC = value;

					//Updating ReadOnly status
					UpdateJR_ChequeNoReadOnly();
					UpdateJR_AKReadOnly();
					UpdateJR_PaymentTypeReadOnly();
					UpdateJR_OH_CostAccountReadOnly();
					UpdateJR_OSCostAmtReadOnly();
					UpdateJR_LocalCostAmtReadOnly();
					UpdateJR_ABReadOnly();
					UpdateJR_RX_NKCostCurrencyReadOnly();
					UpdateJR_OSSellAmtReadOnly();
					UpdateJR_LocalSellAmtReadOnly();

					UpdateJR_APInvoiceNumReadOnly();
					UpdateJR_APInvoiceDateReadOnly();
					UpdateJR_APDocumentReceivedDateReadOnly();
					UpdateJR_PaymentDateReadOnly();
					UpdateJR_CostReferenceReadOnly();

					ClearCurrencyFieldsAndAmounts();

					InvoiceTypeCalculator.UpdateInvoiceType();
				}
			}
		}

		/// <summary>
		/// Clear the currency fields
		/// </summary>
		protected virtual void ClearCurrencyFieldsAndAmounts()
		{
			if (!IsCostPosted)
			{
				if (IsRevenueCharge)
				{
					JR_RX_NKCostCurrency = ZString.Empty;
				}
				else
				{
					JR_RX_NKCostCurrency = !DefaultCostCurrencyFromCostAccount.IsEmpty ? DefaultCostCurrencyFromCostAccount : GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				}
				JR_OSCostAmt = 0;
				JR_CostRatingOverride = false;
				SetEstimatedCost(0);
			}
			if (!IsRevenuePosted)
			{
				JR_RX_NKSellCurrency = !DefaultSellCurrencyFromSellAccount.IsEmpty ? DefaultSellCurrencyFromSellAccount : GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				JR_OSSellAmt = 0;
				JR_SellRatingOverride = false;
			}
		}

		#endregion

		#region JR_CostRated

		public override ZBool JR_CostRated
		{
			get { return base.JR_CostRated; }
			set
			{
				base.JR_CostRated = value;
				if (value)
				{
					UpdateCostFieldsReadonlyIfAutorated();
				}
			}
		}

		#endregion

		#region JR_OSCostAmt

		public override ZDecimal JR_OSCostAmt
		{
			get { return base.JR_OSCostAmt; }
			set
			{
				if ((AccountingValuesRoundingHelper.PropertyHasChanges(this, JR_OSCostAmt != value)))
				{
					base.JR_OSCostAmt = value;
					Calculations.ForeignCostAmountChange();
					UpdateTotals();
					EmptyCostCalculationDescription();
				}
			}
		}

		#endregion

		#region JR_LocalCostAmt

		public override ZDecimal JR_LocalCostAmt
		{
			get { return base.JR_LocalCostAmt; }
			set
			{
				if (JR_LocalCostAmt != value)
				{
					base.JR_LocalCostAmt = value;
					Calculations.LocalCostAmountChange();
					UpdateReadOnlyStatusOnAPInvoiceFields();
					EmptyCostCalculationDescription();
				}
			}
		}

		#endregion

		#region JR_RX_NKCostCurrency

		protected override void SetJR_RX_NKCostCurrencyCore(ZString value)
		{
			if (JR_RX_NKCostCurrency != value)
			{
				using (Calculations.SuspendCalculations())
				{
					InitializeCostExchangeRate();
				}
				var oldCurrency = JR_RX_NKCostCurrency;
				base.SetJR_RX_NKCostCurrencyCore(value);
				using (Calculations.SuspendCalculations())
				{
					if (InvoicingJob != null && !JR_IsApportioned && !IsCostPosted)
					{
						InvoicingJob.AddCurrency(CostCurrency, JR_OH_CostAccount, ExchangeRateValidLedgerEnum.AP);
					}
					UpdateCostExchangeRate();
				}
				Calculations.CostCurrencyChange(oldCurrency, value);
				JR_CostCurrencyInfo.RefreshBinding();
				UpdateJR_OSSellAmtReadOnly();
				UpdateLineCFX();
				FetchAPInvoiceDetails();
				Validation.ValidateJR_APInvoiceDate();
			}
		}

		public ZBool IsCostForeign
		{
			get { return !JR_RX_NKCostCurrency.IsEmpty && JR_RX_NKCostCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		public ZBool IsCostLocal
		{
			get { return !JR_RX_NKCostCurrency.IsEmpty && JR_RX_NKCostCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		#endregion

		#region JR_OSCostExRate

		protected override void SetJR_OSCostExRateChangedCore(ZDecimal value)
		{
			base.SetJR_OSCostExRateChangedCore(value);

			if (IsInDatabase
				&& HasChanges
				&& IsCostPosted
				&& APLine.AL_LineAmount == -JR_LocalCostAmt
				&& APLine.AL_ExchangeRate.Round(GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces) != JR_OSCostExRate.Round(GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces))
			{
				//For WI00102941: Critical Validation Failure: Related CST amount is not the same as charge amount.
				ErrorReporter.ReportOnce("ChargeWithCost.SetJR_OSCostExRateChangedCore", string.Format(CultureInfo.InvariantCulture, "JR_OSCostExRate has been changed from {0} to {1} after the charge posted. The information can be used to solve WI00102941", APLine.AL_ExchangeRate.Round(GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces), JR_OSCostExRate.Round(GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces)));
			}

			Calculations.CostExRateChanged();
			UpdateJR_OSCostAmtReadOnly();
		}

		#endregion

		#region JR_SellRated

		public override ZBool JR_SellRated
		{
			get { return base.JR_SellRated; }
			set
			{
				base.JR_SellRated = value;
				if (value)
				{
					UpdateRevenueFieldsReadonlyIfAutorated();
				}
			}
		}

		#endregion

		#region JR_OSSellAmt

		public override ZDecimal JR_OSSellAmt
		{
			get { return base.JR_OSSellAmt; }
			set
			{
				ZDecimal roundedValue = value;
				if (IsIcelandAndKronurOSSellCurrency)
				{
					roundedValue = roundedValue.Round(0);
				}

				bool hasChanges = base.JR_OSSellAmt != roundedValue;
				base.JR_OSSellAmt = roundedValue;
				if (AccountingValuesRoundingHelper.PropertyHasChanges(this, hasChanges))
				{
					Calculations.ForeignSellAmountChanged();
					UpdateTotals();
					JR_CFXAmtInfo.RefreshBinding();
					EmptyRevenueCalculationDescription();
				}
			}
		}

		bool IsIceland
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland; }
		}

		protected bool IsIcelandAndKronurOSSellCurrency
		{
			get { return JR_RX_NKSellCurrency == Core.Constants.CurrencyCodes.Iceland && IsIceland; }
		}

		protected bool IsIcelandAndKronurLocalCurrency
		{
			get { return JR_LocalCurrencyCode == Core.Constants.CurrencyCodes.Iceland && IsIceland; }
		}

		#endregion

		#region JR_LocalSellAmt

		public override ZDecimal JR_LocalSellAmt
		{
			get { return base.JR_LocalSellAmt; }
			set
			{
				if (JR_LocalSellAmt != value)
				{
					InvoicingJob?.InvalidateGroupValidation();
					if (isInitialisingRevenueEnchangeRate && this.HasContext(BusinessContext.InvoicingPlugInGUI) && IsInDatabase && RevenueExchangeRate != null && RevenueExchangeRate.IsUserDefinedOrTransformed)
					{
						LocalSellAmtOverridenAndUserNeedToCheck = true;
						LocalSellAmtPreviousValue = base.JR_LocalSellAmt;
					}
					base.JR_LocalSellAmt = value;
					Calculations.LocalSellAmountChanged();
					JR_CFXAmtInfo.RefreshBinding();
					EmptyRevenueCalculationDescription();
				}
			}
		}

		public void RecalculateMarginCost()
		{
			Calculations.UpdateCostBasedOnRevenueForCleanup();
		}

		public void RecalculateMarginRevenue()
		{
			Calculations.UpdateRevenueBasedOnCostForCleanup();
		}

		#endregion

		#region JR_OH_SellAccount

		public override ZGuid JR_OH_SellAccount
		{
			get { return base.JR_OH_SellAccount; }
			set
			{
				bool hasChanges = value != JR_OH_SellAccount;

				if (hasChanges)
				{
					InvoicingJob?.InvalidateGroupValidation();
				}

				base.JR_OH_SellAccount = value;

				if (hasChanges)
				{
					SetChargeDescription();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public void SetChargeDescription()
		{
			JR_Desc = ChargeCode?.GetDescriptionOrLocalDescription(JR_Desc, IsLocalClient)
				?? ZString.Empty;
		}

		protected override void RunSellAccountCalculations()
		{
			JR_AW_SellWHTRate = LocalWHTId;
			UpdateJR_AW_SellWHTRateReadOnly();
			UpdateLineCFX();
		}

		public bool IsSisterCompanyCharge(bool shouldApplyLocalCompanyFilter)
		{
			var orgHeader = Factory.Load<OrgHeader>(JR_OH_SellAccount);

			if (orgHeader == null)
			{
				return false;
			}

			return orgHeader.IsProxyOrgOfAnyCompany(true) &&
				(!shouldApplyLocalCompanyFilter || (orgHeader.UNLOCO?.Code.StartsWith(Company.GC_RN_NKCountryCode) ?? false));
		}

		#endregion

		#region JR_RX_NKSellCurrency

		protected override void SetJR_RX_NKSellCurrencyCore(ZString value)
		{
			if (JR_RX_NKSellCurrency == value)
			{
				return;
			}
			using (Calculations.SuspendCalculations())
			{
				InitializeRevenueExchangeRate();
			}
			var oldCurrency = JR_RX_NKSellCurrency;
			base.SetJR_RX_NKSellCurrencyCore(value);
			using (Calculations.SuspendCalculations())
			{
				if (InvoicingJob != null && !IsRevenuePosted)
				{
					InvoicingJob.AddCurrency(SellCurrency, JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, InvoiceCurrencyTypeForAR);
				}
				UpdateRevenueExchangeRate();
			}
			Calculations.SellCurrencyChange(oldCurrency, value);
			UpdateJR_OSSellAmtReadOnly();
			UpdateJR_LocalSellAmtReadOnly();
		}

		public void UpdateInvoiceType()
		{
			if (!InvoiceTypeUpdateSuspender.IsSuspended)
			{
				InvoiceTypeCalculator.UpdateInvoiceType();
			}
		}

		internal override string[] SellOSPropertiesRequiringRounding()
		{
			return base.SellOSPropertiesRequiringRounding().Append(new[]
			{
				nameof(JR_OSSellAmt),
				nameof(JR_OSSellWHTAmt),
			}).ToArray();
		}

		#endregion

		#region JR_RX_NKCostCurrency

		internal override string[] CostOSPropertiesRequiringRounding()
		{
			return base.CostOSPropertiesRequiringRounding().Append(new[]
			{
				nameof(JR_OSCostAmt),
				nameof(JR_OSCostGSTAmt),
				nameof(JR_OSCostGSTAmt_Calc),
				nameof(JR_OSCostWHTAmt),
			}).ToArray();
		}

		#endregion

		#region JR_OSSellExRate

		public override ZDecimal JR_OSSellExRate
		{
			get
			{
				return base.JR_OSSellExRate;
			}
			set
			{
				var roundedValue = new ZDecimal(Utilities.Round(value, ExchangeRateDecimalPlaces));

				if (base.JR_OSSellExRate == roundedValue)
				{
					return;
				}

				base.JR_OSSellExRate = roundedValue;

				Calculations.SellExRateChanged();
				UpdateLineCFX();
				UpdateJR_OSSellAmtReadOnly();
			}
		}
		protected internal ZDecimal SellRateWithoutCFX { get; private set; }

		internal ZDecimal CalculateSellExRate()
		{
			var result = JR_OSSellExRate;

			if (!IsRevenuePosted
				&& !this.HasContext(BusinessContext.PostingReceivableCharges))
			{
				if (IsLocalCurrency(JR_RX_NKSellCurrency))
				{
					result = 1m;
				}
				else if (RevenueExchangeRate != null)
				{
					SellRateWithoutCFX = RevenueExchangeRate.Rate;

					if (BIllInInvoiceCurrencySameAsSellCurrency || InvoiceTypeRequiresZeroCFX
						|| !IsCFXPercentApplied || RefCurrency.IsExcludedCFXCalculation(JR_RX_NKSellCurrency))
					{
						result = RevenueExchangeRate.Rate;
					}
					else
					{
						result = ExchangeRateHelper.GetSellExRateAdjustedByCFXMinimum(JR_OSSellAmt, RevenueExchangeRate.Rate, RevenueExchangeRate.SellRate, RevenueExchangeRate.CFXMinimum, Company ?? GlbCompany.CurrentCompany);
					}
				}
			}

			return Utilities.Round(result, ExchangeRateDecimalPlaces);
		}

		protected bool IsLocalCurrency(ZString currencyCode)
		{
			return currencyCode == (Company ?? GlbCompany.CurrentCompany).GC_RX_NKLocalCurrency;    //Somehow in some tests we have Company == null
		}

		#endregion

		#region JR_CFXAmtReverseSign

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_CFXAmtReverseSign
		{
			get { return -JR_CFXAmt; }
		}

		public ZPropertyInfo JR_CFXAmtReverseSignInfo
		{
			get { return GetZPropertyInfo(Schema.JR_CFXAmtReverseSign); }
		}

		#endregion

		#region IsChequeNumberAutoAllocated

		public ZBool IsChequeNumberAutoAllocated
		{
			get
			{
				if (ChequeBook != null)
				{
					return ChequeBook.IsAutoPrint && IsCheque;
				}
				else
				{
					return ZBool.False;
				}
			}
		}

		#endregion

		#region Calc_ChequeNumberIsAutoAllocatedLabel

		public ZString Calc_ChequeNumberIsAutoAllocatedLabel
		{
			get { return IsChequeNumberAutoAllocated && JR_ChequeNo.IsEmpty && !JR_IsCostPosted ? AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel : ""; }
		}

		public ZPropertyInfo Calc_ChequeNumberIsAutoAllocatedLabelInfo
		{
			get { return GetZPropertyInfo(nameof(Calc_ChequeNumberIsAutoAllocatedLabel)); }
		}

		#endregion

		#region JR_SellReference

		public override ZString JR_SellReference
		{
			get
			{
				return base.JR_SellReference;
			}
			set
			{
				if (base.JR_SellReference != value)
				{
					base.JR_SellReference = value;
					InvoicingJob?.InvalidateGroupValidation();
				}
			}
		}

		#endregion

		public bool IsCustomsCharge
		{
			get { return CustomsChargeCodePKs.Contains(JR_AC); }
		}

		public IEnumerable<ZGuid> CustomsChargeCodePKs
		{
			get
			{
				if (customsChargeCodePKs == null)
				{
					var branch = Branch ?? (Job != null ? (Job.Branch ?? GlbBranch.CurrentBranch) : GlbBranch.CurrentBranch);
					customsChargeCodePKs = Factory.GetCachedValue("827eea6b-9769-4217-bc44-843e7b7b736d" + branch.PK,  () => Enterprise.Registry.Business.Customs.EntryChargeTypeList.GetAllChargeCodePKsOf(branch.GB_GC, branch.Company.GC_RN_NKCountryCode));
				}
				return customsChargeCodePKs;
			}
		}
		IEnumerable<ZGuid> customsChargeCodePKs;

		#endregion

		#region Cash Advance

		[BusinessObjectTestExclude]
		public APCashAdvanceChargeCollection RelevantChargesForAPCashAdvance
		{
			get
			{
				if (relevantChargesForAPCashAdvance == null)
				{
					relevantChargesForAPCashAdvance = new APCashAdvanceChargeCollection(this);
				}
				return relevantChargesForAPCashAdvance;
			}
		}

		APCashAdvanceChargeCollection relevantChargesForAPCashAdvance;

		public void LoadRelevantChargesForAPCashAdvance()
		{
			ResetRelevantChargesForAPCashAdvance();
			RelevantChargesForAPCashAdvance.Load();
		}

		void ResetRelevantChargesForAPCashAdvance()
		{
			relevantChargesForAPCashAdvance = null;
		}

		AccCashAdvanceRequestHeader LinkedAPCashAdvance => APCashAdvanceRequirement?.CashAdvanceRequest;
		public ZBool IsEligibleForNewCashAdvanceRequest => !IsCostPosted && JR_CAL_APLine.IsEmpty;
		public ZString APCashAdvanceReferenceNumber => LinkedAPCashAdvance?.CAH_RequestReferenceNumber ?? ZString.Empty;

		public ZString APCashAdvanceStatus
		{
			get
			{
				var result = ZString.Empty;
				var cah = LinkedAPCashAdvance;
				if (cah == null)
				{
					result = CashAdvanceStatusCodes.RequestHeader.PendingDescription;
				}
				else
				{
					result = CashAdvanceStatusCodes.RequestHeader.CodesList.GetDescriptionFromCode(cah.CAH_Status);
				}
				return result;
			}
		}

		public ZString APCashAdvanceCreatedDateTime
		{
			get
			{
				var result = ZString.Empty;
				var cah = LinkedAPCashAdvance;
				if (cah == null)
				{
					result = ZDate.Today.ToShortDateString();
				}
				else
				{
					result = new ZDate(cah.CreatedDateTimeLocal).ToShortDateString();
				}
				return result;
			}
		}

		[DecimalPlaces(nameof(OSCostCurrencyDecimals))]
		public ZDecimal APCashAdvanceTotalTax => RelevantChargesForAPCashAdvance.OfType<Charge>().Sum(x => x.JR_OSCostGSTAmt_Calc);

		[DecimalPlaces(nameof(OSCostCurrencyDecimals))]
		public ZDecimal APCashAdvanceTotalInvoiceAmount => RelevantChargesForAPCashAdvance.OfType<Charge>().Sum(x => x.JR_OSCostAmtWithGSTAmt);

		#endregion

		#region Profit Share

		public ZDecimal GetProfitInLocalCurrency(CostRevenueAmountType amountType)
		{
			ZDecimal profit = 0m;

			if (amountType == CostRevenueAmountType.Actual)
			{
				profit = JR_LocalSellAmt - JR_LocalCostAmt;
			}
			else if (amountType == CostRevenueAmountType.AgentDeclared)
			{
				profit = JR_AgentDeclaredSellAmtLocal - JR_AgentDeclaredCostAmtLocal;
			}

			return profit;
		}

		#endregion

		#region Exchange Rates

		#region Sell

		public ExchangeRateType SellExchangeRateType
		{
			get
			{
				return BIllInInvoiceCurrencySameAsSellCurrency || BillInInvoiceCurrencyWithLocalSellCurrency ? ExchangeRateType.Buy : ExchangeRateType.Sell;
			}
		}

		#endregion

		protected override sealed void OnRevenueExchangeRateChangedCore(object sender, EventArgs e)
		{
			if (IsDeleted)
			{
				return;
			}

			var newValue = CalculateSellExRate();
			if (JR_OSSellExRate != newValue)
			{
				RedefaultCFXMinimumOrSetRowErrorIfRequired(ref newValue);
				JR_OSSellExRate = newValue;
			}

			base.OnRevenueExchangeRateChangedCore(sender, e);
		}

		internal void UpdateSellExRateWithBaseRate(ZDecimal baseRate)
		{
			SellRateWithoutCFX = baseRate;
			var sellRate = baseRate;

			if (IsCFXPercentApplied)
			{
				var cfxPercent = JR_LineCFX;
				var cfxMinimum = ZDecimal.Zero;

				if (RevenueExchangeRate != null)
				{
					cfxPercent = RevenueExchangeRate.CFXPercent;
					cfxMinimum = RevenueExchangeRate.CFXMinimum;
				}
				else
				{
					var relatedExRate = InvoicingJob?.GetExchangeRate(JR_RX_NKSellCurrency, JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, false, InvoiceCurrencyTypeForAR);
					if (relatedExRate != null)
					{
						cfxPercent = relatedExRate.CFXPercent;
						cfxMinimum = relatedExRate.CFXMinimum;
					}
					else if (SellAccount != null)
					{
						//This will be updated in WI00876283 to pass in the actual date based on the "AR Invoice Posting Exchange Rate Option" registry setting.
						InvoicingJob?.GetCFXPairFromOrganization(SellAccount, JR_RX_NKSellCurrency, date: null, out cfxPercent, out cfxMinimum);
					}
				}

				sellRate = ExchangeRateHelper.GetBaseRateAdjustedByCFXPercentAndMinimum(JR_OSSellAmt, JR_RX_NKSellCurrency, baseRate, cfxPercent, cfxMinimum, Company);
			}

			JR_OSSellExRate = sellRate;
		}

		internal void UpdateSellExRateWithBaseRateFromLocalAmt(ZDecimal baseRate)
		{
			SellRateWithoutCFX = baseRate;
			var sellRate = baseRate;

			if (IsCFXPercentApplied)
			{
				var cfxPercent = JR_LineCFX;
				var cfxMinimum = ZDecimal.Zero;

				if (RevenueExchangeRate != null)
				{
					cfxPercent = RevenueExchangeRate.CFXPercent;
					cfxMinimum = RevenueExchangeRate.CFXMinimum;
				}
				else
				{
					var relatedExRate = InvoicingJob?.GetExchangeRate(JR_RX_NKSellCurrency, JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, false, InvoiceCurrencyTypeForAR);
					if (relatedExRate != null)
					{
						cfxPercent = relatedExRate.CFXPercent;
						cfxMinimum = relatedExRate.CFXMinimum;
					}
					else if (SellAccount != null)
					{
						//This will be updated in WI00876283 to pass in the actual date based on the "AR Invoice Posting Exchange Rate Option" registry setting.
						InvoicingJob?.GetCFXPairFromOrganization(SellAccount, JR_RX_NKSellCurrency, date: null, out cfxPercent, out cfxMinimum);
					}
				}

				sellRate = ExchangeRateHelper.GetBaseRateAdjustedByCFXPercentAndMinimumFromLocalAmt(JR_LocalSellAmt, JR_RX_NKSellCurrency, baseRate, cfxPercent, cfxMinimum, Company);
			}

			JR_OSSellExRate = sellRate;

			if (Factory.IsInSaveTransaction)
			{
				var jobChargeService = Factory.ServiceContainer.GetService<NegativeJobChargeOSSellExRateWhenSaveRecorder>()
									   ?? Factory.ServiceContainer.AddService(new NegativeJobChargeOSSellExRateWhenSaveRecorder());
				jobChargeService.AddOrUpdate(PK, JR_OSSellExRate);
			}
		}

		protected ZDecimal SellInvoiceRateWithoutCFX { get; set; }

		void RedefaultCFXMinimumOrSetRowErrorIfRequired(ref ZDecimal newValue)
		{
			if (isInitialisingRevenueEnchangeRate && RevenueExchangeRate != null && RevenueExchangeRate.CFXMinimum.IsEmpty &&
				this.HasContext(BusinessContext.InvoicingPlugInGUI) &&
				IsInDatabase &&
				newValue != JR_OSSellExRate &&
				JR_OH_SellAccount == RevenueExchangeRate.OrgPk &&
				JR_RX_NKSellCurrency == RevenueExchangeRate.CurrencyCode &&
				RevenueExchangeRate.IsUserDefinedOrTransformed)
			{
				RevenueExchangeRate.RefreshCFXMinimum();
				if (!RevenueExchangeRate.CFXMinimum.IsEmpty)
				{
					newValue = CalculateSellExRate();
				}
			}
		}

		protected override sealed void OnCostExchangeRateChangedCore(object sender, EventArgs e)
		{
			base.OnCostExchangeRateChangedCore(sender, e);
			JR_OSCostExRate = CalculateCostExRate();
		}

		ZDecimal CalculateCostExRate()
		{
			var result = JR_OSCostExRate;

			if (!IsCostPosted)
			{
				result = CostExchangeRate?.Rate ?? 1m;
			}

			return result;
		}

		#endregion

		#region CFX

		protected
#if DEBUG
			virtual
#endif
			bool InvoiceTypeRequiresZeroCFX =>
				new ZString[]
				{
					InvoiceTypesList.Codes.ForeignCurrencyInvoice,
					InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching,
					InvoiceTypesList.Codes.DisbursementInForeignCurrency,
					InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching,
					InvoiceTypesList.Codes.FreightInvoice,
					InvoiceTypesList.Codes.FreightInvoice_Batching,
					InvoiceTypesList.Codes.SelfBillingInvoice,
					InvoiceTypesList.Codes.SelfBillingInvoice_Batching
				}.Contains(JR_InvoiceType);

		public void UpdateLineCFX()
		{
			if (SellInvoiceExchangeRate != null)
			{
				SellInvoiceRateWithoutCFX = SellInvoiceExchangeRate.Rate;
			}

			if (this.HasContext(BusinessContext.PostingReceivableCharges))
			{
				if (CFXLine != null)
				{
					var amount = CalculateCFXAmt();
					CFXLine.AL_LineAmount = -amount;
					CFXLine.AL_OSAmount = -amount;
				}
				JR_CFXAmtInfo.RefreshBinding();
				return;
			}

			var cfxPercent = ZDecimal.Zero;

			if (IsCFXPercentApplied)
			{
				bool isLocal(ZString c) => c.IsEmpty || c == this.Company.GC_RX_NKLocalCurrency;

				if (isLocal(JR_RX_NKSellInvoiceCurrency) && isLocal(JR_RX_NKSellCurrency) || JR_RX_NKSellInvoiceCurrency == JR_RX_NKSellCurrency
					|| InvoiceTypeRequiresZeroCFX)
				{
					JR_CFXAmtInfo.RefreshBinding();
					return;
				}

				var currencyCode = isLocal(JR_RX_NKSellCurrency) ? JR_RX_NKSellInvoiceCurrency : JR_RX_NKSellCurrency;
				var linkedExRate = RevenueExchangeRate ?? SellInvoiceExchangeRate;
				if (linkedExRate != null)
				{
					cfxPercent = linkedExRate.CFXPercent;
				}
				else
				{
					//This will be updated in WI00876283 to pass in the actual date based on the "AR Invoice Posting Exchange Rate Option" registry setting.
					InvoicingJob.GetCFXPairFromOrganization(SellAccount, currencyCode, date: null, out cfxPercent, out var notused);
				}
			}

			JR_LineCFX = cfxPercent;
			JR_CFXAmtInfo.RefreshBinding();
		}

		bool IsCFXPercentApplied => !(IsRevenuePosted && IsRevenueInDatabase) &&
				(IsSellForeign && AllowsSellInvoiceCurrency && !BIllInInvoiceCurrencySameAsSellCurrency || BillInInvoiceCurrencyWithLocalSellCurrency) &&
				InvoicingJob != null;

		#endregion

		#region Read-Only

		void UpdateCostFieldsReadonlyIfAutorated()
		{
			JR_LocalCostAmtInfo.RefreshBinding();
			JR_OSCostAmtInfo.RefreshBinding();
			JR_RX_NKCostCurrencyInfo.RefreshBinding();
			JR_OH_CostAccountInfo.RefreshBinding();
		}

		protected bool IsBaseCostFieldsReadonly
		{
			get { return AccountingConfigurationRegistry.Instance.PreventOperatorFromChangingRatedLine.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK) && JR_CostRated; }
		}

		protected virtual bool JR_LocalCostAmt_ReadOnly
		{
			get
			{
				return
					IsBaseCostFieldsReadonly ||
					IsAllCostFieldsReadonly;
			}
		}

		protected virtual bool JR_OSCostAmt_ReadOnly
		{
			get
			{
				return
					IsBaseCostFieldsReadonly ||
					IsAllCostFieldsReadonly;
			}
		}

		protected virtual bool JR_RX_NKCostCurrency_ReadOnly
		{
			get
			{
				return
					IsBaseCostFieldsReadonly ||
					IsAllCostFieldsReadonly;
			}
		}

		protected virtual bool JR_OH_CostAccount_ReadOnly
		{
			get
			{
				return
					IsBaseCostFieldsReadonly && JR_OH_CostAccount.IsValid ||
					IsAllCostFieldsReadonly;
			}
		}

		void UpdateRevenueFieldsReadonlyIfAutorated()
		{
			JR_LocalSellAmtInfo.RefreshBinding();
			JR_OSSellAmtInfo.RefreshBinding();
			JR_RX_NKSellCurrencyInfo.RefreshBinding();
			JR_OH_SellAccountInfo.RefreshBinding();
		}

		protected virtual bool JR_LocalSellAmt_ReadOnly
		{
			get { return IsBaseSellFieldsReadonly || IsMainAllFieldsReadonly; }
		}

		protected virtual bool JR_OSSellAmt_ReadOnly
		{
			get { return IsBaseSellFieldsReadonly || IsMainAllFieldsReadonly; }
		}

		protected bool JR_SellReference_ReadOnly
		{
			get { return IsMainAllFieldsReadonly; }
		}

		protected bool JR_RX_NKSellCurrency_ReadOnly
		{
			get { return IsBaseSellFieldsReadonly || IsMainAllFieldsReadonly; }
		}

		protected bool IsOSSellAmtReadOnly
		{
			get { return !IsRevenueCharge; }
		}

		protected virtual void UpdateJR_APInvoiceNumReadOnly()
		{
			JR_APInvoiceNumInfo.RefreshBinding();
			if (!PreventReadOnlyFromChangingValues && IsRevenueCharge)
			{
				JR_APInvoiceNum = ZString.Empty;
			}
			JR_APInvoiceNumInitialReadOnly = false;
		}

		protected virtual bool JR_APInvoiceNum_ReadOnly
		{
			get { return IsAllCostFieldsReadonly || JR_APInvoiceNumInitialReadOnly; }
		}

		protected virtual void UpdateJR_RX_NKCostCurrencyReadOnly()
		{
			JR_RX_NKCostCurrencyInfo.RefreshBinding();
			if (!PreventReadOnlyFromChangingValues && IsRevenueCharge)
			{
				JR_RX_NKCostCurrency = ZString.Empty;
			}
		}

		protected virtual void UpdateJR_APInvoiceDateReadOnly()
		{
			JR_APInvoiceDateInfo.RefreshBinding();
			if (!PreventReadOnlyFromChangingValues && IsRevenueCharge)
			{
				JR_APInvoiceDate = ZDateTime.Empty;
			}

			JR_APInvoiceDateInitialReadOnly = false;
		}

		protected virtual bool JR_APInvoiceDate_ReadOnly
		{
			get { return IsAllCostFieldsReadonly || JR_APInvoiceDateInitialReadOnly; }
		}

		protected virtual void UpdateJR_APDocumentReceivedDateReadOnly()
		{
			JR_APDocumentReceivedDateInfo.RefreshBinding();
			if (!PreventReadOnlyFromChangingValues && IsRevenueCharge)
			{
				JR_APDocumentReceivedDate = ZDateTime.Empty;
			}

			JR_APDocumentReceivedDateInitialReadOnly = false;
		}

		protected virtual bool JR_APDocumentReceivedDate_ReadOnly
		{
			get { return IsAllCostFieldsReadonly || JR_APDocumentReceivedDateInitialReadOnly; }
		}

		protected void UpdateJR_ABReadOnly()
		{
			JR_ABInfo.RefreshBinding();
			if (!PreventReadOnlyFromChangingValues && IsRevenueCharge)
			{
				JR_AB = ZGuid.Empty;
			}

			JR_ABInitialReadOnly = false;
		}

		protected virtual bool JR_AB_ReadOnly
		{
			get { return IsAllCostFieldsReadonly || JR_ABInitialReadOnly; }
		}

		protected virtual void UpdateJR_PaymentDateReadOnly()
		{
			JR_PaymentDateInfo.RefreshBinding();
			if (!PreventReadOnlyFromChangingValues && IsRevenueCharge)
			{
				JR_PaymentDate = ZDateTime.Empty;
			}

			JR_PaymentDateInitialReadOnly = false;
		}

		protected virtual bool JR_PaymentDate_ReadOnly
		{
			get { return IsAllCostFieldsReadonly || JR_PaymentDateInitialReadOnly; }
		}

		protected void UpdateJR_ChequeNoReadOnly()
		{
			JR_ChequeNoInfo.RefreshBinding();
			if ((!PreventReadOnlyFromChangingValues && IsRevenueCharge) || (IsChequeNumberAutoAllocated && !JR_IsCostPosted))
			{
				JR_ChequeNo = ZString.Empty;
			}

			JR_ChequeNoInitialReadOnly = false;
		}

		protected virtual bool JR_ChequeNo_ReadOnly
		{
			get { return IsChequeNoReadOnly() || JR_ChequeNoInitialReadOnly; }
		}

		protected void UpdateJR_LocalSellAmtReadOnly()
		{
			JR_LocalSellAmtInfo.RefreshBinding();
		}

		protected bool IsChequeNoReadOnly()
		{
			return IsAllCostFieldsReadonly || IsChequeNumberAutoAllocated;
		}

		protected void UpdateJR_AKReadOnly()
		{
			JR_AKInfo.RefreshBinding();
			if (!PreventReadOnlyFromChangingValues && IsRevenueCharge)
			{
				JR_AK = ZGuid.Empty;
			}

			JR_AKInitialReadOnly = false;
		}

		protected virtual bool JR_AK_ReadOnly
		{
			get { return IsAllCostFieldsReadonly || JR_AKInitialReadOnly || !IsCheque; }
		}

		protected virtual void UpdateJR_PaymentTypeReadOnly()
		{
			JR_PaymentTypeInfo.RefreshBinding();
			if (!PreventReadOnlyFromChangingValues && IsRevenueCharge)
			{
				JR_PaymentType = ZString.Empty;
			}

			JR_PaymentTypeInitialReadOnly = false;
		}

		protected virtual bool JR_PaymentType_ReadOnly
		{
			get { return IsAllCostFieldsReadonly || JR_PaymentTypeInitialReadOnly; }
		}

		protected virtual void UpdateJR_OH_CostAccountReadOnly()
		{
			JR_OH_CostAccountInfo.RefreshBinding();
			if (!PreventReadOnlyFromChangingValues && IsRevenueCharge)
			{
				JR_OH_CostAccount = ZGuid.Empty;
			}
		}

		protected virtual void UpdateJR_OSCostAmtReadOnly()
		{
			JR_OSCostAmtInfo.RefreshBinding();
			if (!PreventReadOnlyFromChangingValues && IsRevenueCharge)
			{
				JR_OSCostAmt = 0;
			}
		}

		protected void UpdateJR_LocalCostAmtReadOnly()
		{
			JR_LocalCostAmtInfo.RefreshBinding();
			if (!PreventReadOnlyFromChangingValues && IsRevenueCharge)
			{
				JR_LocalCostAmt = 0;
			}
		}

		protected void UpdateJR_AW_SellWHTRateReadOnly()
		{
			JR_AW_SellWHTRateInfo.RefreshBinding();
			if (!PreventReadOnlyFromChangingValues && !IsSellWHTApplicable)
			{
				JR_AW_SellWHTRate = ZGuid.Empty;
			}

			JR_AW_SellWHTRateInitialReadOnly = false;
		}

		protected bool JR_AW_SellWHTRate_ReadOnly
		{
			get { return IsSellWHTFieldReadOnly || JR_AW_SellWHTRateInitialReadOnly; }
		}

		protected void UpdateJR_ACReadOnly()
		{
			JR_ACInfo.RefreshBinding();
		}

		protected bool JR_AC_ReadOnly
		{
			get
			{
				return IsCostPosted
					|| IsRevenuePosted
					|| JR_IsApportioned
					|| IsInDatabaseAndReadyForCostPosting
					|| IsInDatabaseAndReadyForRevenuePosting
					|| IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity;
			}
		}

		protected void UpdateJR_DescReadOnly()
		{
			JR_DescInfo.RefreshBinding();
		}

		protected bool JR_Desc_ReadOnly => IsRevenuePosted || (ChargeCode != null && InvoicingJob != null && !(ChargeCode.AC_AllowDescriptionOvertype && InvoicingJob.AllowModifyDefaultChargeCodeDescription)) ||
																			 IsInDatabaseAndReadyForCostPosting || IsInDatabaseAndReadyForRevenuePosting || IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity;

		protected void UpdateJR_GEReadOnly()
		{
			JR_GEInfo.RefreshBinding();
		}

		protected void UpdateJR_GBReadOnly()
		{
			JR_GBInfo.RefreshBinding();
		}

		protected void UpdateJR_PreventInvoicePrintGrouping()
		{
			JR_PreventInvoicePrintGroupingInfo.RefreshBinding();
		}

		protected bool JR_PreventInvoicePrintGrouping_ReadOnly
		{
			get { return IsRevenuePosted; }
		}

		protected void UpdateJR_RX_NKSellCurrencyReadOnly()
		{
			JR_RX_NKSellCurrencyInfo.RefreshBinding();
		}

		protected void UpdateJR_OH_SellAccountReadOnly()
		{
			JR_OH_SellAccountInfo.RefreshBinding();
		}

		protected virtual void UpdateJR_OSSellAmtReadOnly()
		{
			JR_OSSellAmtInfo.RefreshBinding();
		}

		protected virtual void UpdateJR_CostReferenceReadOnly()
		{
			JR_CostReferenceInfo.RefreshBinding();
			if (!PreventReadOnlyFromChangingValues && IsRevenueCharge)
			{
				JR_CostReference = ZString.Empty;
			}
			JR_CostReferenceInitialReadOnly = false;
		}

		protected virtual bool JR_CostReference_ReadOnly
		{
			get { return IsAllCostFieldsReadonly || JR_CostReferenceInitialReadOnly; }
		}

		public virtual void UpdateReadOnlyStatus()
		{
			UpdateCoreFieldsReadOnly();
			UpdateCostFieldsReadOnly();
			UpdateRevenueFieldsReadOnly();
		}

		/// <summary>
		/// Updates read-only status of core fields
		/// </summary>
		protected void UpdateCoreFieldsReadOnly()
		{
			if (!ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.GUIRelatedActionsSuspend>.IsSuspended(Factory))
			{
				PreventReadOnlyFromChangingValues = true;
				UpdateJR_ACReadOnly();
				UpdateJR_DescReadOnly();
				UpdateJR_GEReadOnly();
				UpdateJR_GBReadOnly();
				PreventReadOnlyFromChangingValues = false;
			}
		}

		/// <summary>
		/// Updates read-only status of cost fields
		/// </summary>
		protected void UpdateCostFieldsReadOnly()
		{
			if (!ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.GUIRelatedActionsSuspend>.IsSuspended(Factory))
			{
				PreventReadOnlyFromChangingValues = true;
				UpdateJR_RX_NKCostCurrencyReadOnly();
				UpdateJR_OSCostAmtReadOnly();
				UpdateJR_LocalCostAmtReadOnly();
				UpdateJR_OH_CostAccountReadOnly();

				UpdateJR_AT_CostGSTRateReadOnly();
				UpdateJR_AW_CostWHTRateReadOnly();
				UpdateJR_APInvoiceNumReadOnly();
				UpdateJR_APInvoiceDateReadOnly();
				UpdateJR_APDocumentReceivedDateReadOnly();
				UpdateJR_PaymentTypeReadOnly();
				UpdateJR_PaymentDateReadOnly();
				UpdateJR_ABReadOnly();
				UpdateJR_AKReadOnly();
				UpdateJR_ChequeNoReadOnly();
				UpdateJR_CostReferenceReadOnly();
				UpdateCostFieldsReadonlyIfAutorated();
				PreventReadOnlyFromChangingValues = false;
			}
		}

		/// <summary>
		/// Updates read-only status of revenue fields
		/// </summary>
		protected void UpdateRevenueFieldsReadOnly()
		{
			if (!ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.GUIRelatedActionsSuspend>.IsSuspended(Factory))
			{
				PreventReadOnlyFromChangingValues = true;
				UpdateJR_RX_NKSellCurrencyReadOnly();
				UpdateJR_RX_NKSellCurrencyReadOnly();
				UpdateJR_OH_SellAccountReadOnly();
				UpdateJR_OSSellAmtReadOnly();
				UpdateJR_LocalSellAmtReadOnly();
				UpdateJR_AT_SellGSTRateReadOnly();
				UpdateJR_AW_SellWHTRateReadOnly();
				UpdateJR_PreventInvoicePrintGrouping();
				UpdateRevenueFieldsReadonlyIfAutorated();
				PreventReadOnlyFromChangingValues = false;
			}
		}

		/// <summary>
		/// Sets fields readonly when the object is being created
		/// </summary>
		protected void SetFieldsReadOnlyInitially()
		{
			PreventReadOnlyFromChangingValues = true;
			JR_APInvoiceNumInitialReadOnly = true;
			JR_APInvoiceDateInitialReadOnly = true;
			JR_APDocumentReceivedDateInitialReadOnly = true;
			JR_PaymentTypeInitialReadOnly = true;
			JR_PaymentDateInitialReadOnly = true;
			JR_ABInitialReadOnly = true;
			JR_AKInitialReadOnly = true;
			JR_ChequeNoInitialReadOnly = true;
			JR_AT_CostGSTRateInitialReadOnly = true;
			JR_AW_CostWHTRateInitialReadOnly = true;
			JR_AW_SellWHTRateInitialReadOnly = true;
			JR_CostReferenceInitialReadOnly = true;
			PreventReadOnlyFromChangingValues = false;
		}

		protected bool JR_APInvoiceNumInitialReadOnly { get; set; }
		protected bool JR_APInvoiceDateInitialReadOnly { get; set; }
		protected bool JR_APDocumentReceivedDateInitialReadOnly { get; set; }
		protected bool JR_PaymentTypeInitialReadOnly { get; set; }
		protected bool JR_PaymentDateInitialReadOnly { get; set; }
		protected bool JR_ABInitialReadOnly { get; set; }
		protected bool JR_AKInitialReadOnly { get; set; }
		protected bool JR_ChequeNoInitialReadOnly { get; set; }
		protected bool JR_AT_CostGSTRateInitialReadOnly { get; set; }
		protected bool JR_AW_CostWHTRateInitialReadOnly { get; set; }
		protected bool JR_AW_SellWHTRateInitialReadOnly { get; set; }
		protected bool JR_CostReferenceInitialReadOnly { get; set; }

		#endregion

		#region WIPs and Accurals
		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateJR_ACWhenCreateWIPandAccrual();
		}

		public void ValidateJR_ACWhenCreateWIPandAccrual()
		{
			if (ShouldCreateAccrual && ChargeCode != null && ChargeCode.AC_AG_AccrualAccount == ZGuid.Empty)
			{
				this.AddRowError(GetInvalidChargeCodeError(ChargeCode.AC_Code, Res.GetString("6284f24e-61bf-4888-b081-cf993302f66d", "Accrual")));
			}
			if (ShouldCreateWIP && ChargeCode != null && ChargeCode.AC_AG_WIPAccount == ZGuid.Empty)
			{
				this.AddRowError(GetInvalidChargeCodeError(ChargeCode.AC_Code, Res.GetString("4ec32b85-d96f-4576-ba6d-ec5fa8cefb92", "WIP")));
			}
		}

		public static ZString GetInvalidChargeCodeError(ZString chargeCode, string account)
		{
			return Res.GetString("15ffb44d-5be9-4b89-9cdc-fe89e8760380", "Charge Code '{0}' must have {1} GL Account entered.", chargeCode, account);
		}

		protected override void OnFactorySavingBeforeTransactionCoreInCompanyContext()
		{
			base.OnFactorySavingBeforeTransactionCoreInCompanyContext();
			CreateAutoJobRevenueJournal();
		}

		protected override void OnFactorySavingInCompanyContext()
		{
			base.OnFactorySavingInCompanyContext();

			if (!ServiceContainerSuspenderHelper.FunctionalitySuspender<OnFactorySavingInCompanyContextSuspender>.IsSuspended(Factory))
			{
				if (Factory.RefreshEnabled
					|| Factory.HasContext(CargoWise.Definitions.BusinessContext.JCLServiceTask)
					|| Factory.HasContext(Enterprise.Integration.Accounting.BusinessContext.HasDeletedExchangeRate))
				{
					var reloader = Factory.ServiceContainer.GetService<ChargeReloader>() ?? Factory.ServiceContainer.AddService(new ChargeReloader(Factory));
					reloader.Reload();
				}

				if (!Factory.HasContext(BusinessContext.NonAccountingCode))
				{
					ProcessAccrualAndWIP();
				}
			}
		}

		void ProcessAccrualAndWIP()
		{
			if (!IsDeleted && InvoicingJob != null)
			{
				if (!IsJobClosing)
				{
					CreateAccrualAndWIP();
				}

				if (!InvoicingJob.ConsumerTypeShouldCreateAccrual(JR_InvoiceType))
				{
					ReverseAccrual(ZDateTime.Now);
				}
				if (!InvoicingJob.ConsumerTypeShouldCreateWIP(JR_InvoiceType))
				{
					ReverseWIP(ZDateTime.Now);
				}
			}
#if DEBUG
			SimulateAnotherUserProcessAccrualAndWIP_ForTestOnly();
#endif
		}

#if DEBUG
		protected virtual void SimulateAnotherUserProcessAccrualAndWIP_ForTestOnly() { }
#endif

		protected override void OnSavingInCompanyContext()
		{
			base.OnSavingInCompanyContext();

			if (Factory.HasContext(BusinessContext.NonAccountingCode))
			{
				ProcessAccrualAndWIP();
			}
		}

		bool IsJobClosing
		{
			get { return InvoicingJob.JH_Status == JobHeaderStatus.Closed.Code && InvoicingJob.JH_StatusInfo.HasChanges; }
		}

		#region OnFactorySavingInCompanyContext Suspender

		internal class OnFactorySavingInCompanyContextSuspender : ServiceContainerSuspenderHelper.FunctionalitySuspenderService
		{
		}

		#endregion

		protected override void OnFactorySavedInCompanyContext(bool saveSucceeded)
		{
			base.OnFactorySavedInCompanyContext(saveSucceeded);
			if (Factory.ServiceContainer.GetService<ChargeReloader>() != null)
			{
				Factory.ServiceContainer.RemoveService<ChargeReloader>();
			}
		}

		void CreateAccrualAndWIP()
		{
			if (ShouldReverseAccrual)
			{
				ReverseAccrual(WIPAccrualCreationDate);
			}

			if (ShouldReverseWIP)
			{
				ReverseWIP(WIPAccrualCreationDate);
			}

			bool createAccrual = !JR_AL_APLine.IsValid && IsCreditorValidToCreateAccrualWhenAccrualMustHaveCreditor && ShouldCreateAccrual;
			bool createWIP = !JR_AL_ARLine.IsValid && IsDebtorValidToCreateWIPWhenWIPMustHaveDebtor && ShouldCreateWIP;

			if (createAccrual || createWIP)
			{
				if (CanRecognizeProfitOnWIPsAndAccruals)
				{
					ApplyRevenueRecognitionDate(this);
				}

				if (IsRevenueRecognized(CostRecognition) && createAccrual)
				{
					CreateAccrualCore();
				}

				if (IsRevenueRecognized(SellRecognition) && createWIP)
				{
					CreateWIPCore();
				}
			}
		}

		public void CreateAutoJobRevenueJournal()
		{
			if (!AutoJRJRegistryStatusHelper.IsAutoJRJEnabled() || InvoicingJob == null || IsJobClosing)
			{
				return;
			}

			if (ShouldCreateCostJRJ)
			{
				ApplyRevenueRecognitionDateForCostPart(this);

				if (!CostRecognition.IsEmpty)
				{
					AutoJobRevenueJournalCreator.CreateJournalFromCostCharge(this);
				}

				if (APLine != null && JR_E6.IsValid)
				{
					var jobConsolCost = Factory.Load<JobConsolCost>(JR_E6);
					if (jobConsolCost != null && !jobConsolCost.E6_AH_APInvoice.IsValid)
					{
						jobConsolCost.E6_AH_APInvoice = APLine.AL_AH;
					}
				}
			}

			if (ShouldCreateSellJRJ)
			{
				ApplyRevenueRecognitionDateForSellPart(this);

				if (!SellRecognition.IsEmpty)
				{
					AutoJobRevenueJournalCreator.CreateJournalFromSellCharge(this);
				}
			}
		}

		bool IsRevenueRecognized(ZString recognitionType) => IsRevenueRecognized(InvoicingJob, recognitionType);

		internal static bool IsRevenueRecognized(Job job, ZString recognitionType)
		{
			ZDateTime revenueRecognitionDate = job == null ? ZDateTime.Empty : job.GetRevenueRecognitionDate(recognitionType);
			return !revenueRecognitionDate.IsEmpty && revenueRecognitionDate < AccountingConstants.RevenueRecognitionDateConstants.MinSpecialDate;
		}

		internal static void ApplyRevenueRecognitionDate(ChargeWithCost charge)
		{
			if (!ServiceContainerSuspenderHelper.IsApplyRevenueRecognitionDateSuspended(charge.Factory))
			{
				charge.InvoicingJob.ApplyRevenueRecognitionDate(charge);
			}
		}

		internal static void ApplyRevenueRecognitionDateForCostPart(ChargeWithCost charge)
		{
			if (!ServiceContainerSuspenderHelper.IsApplyRevenueRecognitionDateSuspended(charge.Factory))
			{
				charge.InvoicingJob.ApplyRevenueRecognitionDateForCostPart(charge);
			}
		}

		internal static void ApplyRevenueRecognitionDateForSellPart(ChargeWithCost charge)
		{
			if (!ServiceContainerSuspenderHelper.IsApplyRevenueRecognitionDateSuspended(charge.Factory))
			{
				charge.InvoicingJob.ApplyRevenueRecognitionDateForSellPart(charge);
			}
		}

		#endregion

		#region Create Cost and Revenue Transaction Lines

		public void CreateCostTransactionLine(APInvoice invoice, ZDateTime postTime)
		{
			if (!IsCostPosted)
			{
				APInvoiceLine invoiceLine = (APInvoiceLine)invoice.Lines.AddNew();
				invoiceLine.SetCostValues(InvoicingJob, this);
				invoiceLine.AL_PostDate = postTime;
				invoiceLine.UpdateAL_ReverseDate();

				if (Accrual != null)
				{
					ZDateTime reverseDate = invoiceLine.AL_PostDate;
					if (!invoiceLine.AL_ReverseDate.IsEmpty)
					{
						reverseDate = invoiceLine.AL_ReverseDate;
					}
					ReverseAccrual(reverseDate, true);
				}

				JR_AL_APLine = invoiceLine.PK;
				if (invoiceLine.AL_RevRecognitionType.IsEmpty)
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(ChargeCode.PK,
							CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeFromJobDuringLineCreation,
							() => string.Format(CultureInfo.InvariantCulture, (NoResString)"ChargeCode PK: {0}, Revenue Recognition Type: {1}", ChargeCode.PK, InvoicingJob.GetRevenueRecognitionType(ChargeCode)));
				}
			}
		}

		public void CreateCFXTransactionLine(JCJournalHeader cFXHeader, ZDateTime postTime)
		{
			if (!(IsRevenuePosted && IsRevenueInDatabase))
			{
				JCJournalLine cFXLine = cFXHeader.Lines.AddNew();
				cFXLine.SetCFXValues(InvoicingJob, this);
				cFXLine.AL_PostDate = postTime;
				JR_AL_CFXLine = cFXLine.PK;
			}
		}

		#endregion

		#region AP Invoice details

		protected void UpdateReadOnlyStatusOnAPInvoiceFields()
		{
			if (!Factory.HasContext(BusinessContext.ChargeProcessingForAPTransactionPosting))
			{
				UpdateJR_APInvoiceNumReadOnly();
				UpdateJR_APInvoiceDateReadOnly();
				UpdateJR_APDocumentReceivedDateReadOnly();
				UpdateJR_PaymentTypeReadOnly();
				UpdateJR_PaymentDateReadOnly();
				UpdateJR_ABReadOnly();
				UpdateJR_ChequeNoReadOnly();
				UpdateJR_CostReferenceReadOnly();
			}
		}

		protected void FetchAPInvoiceDetails()
		{
			if (!Factory.HasContext(BusinessContext.ChargeProcessingForAPTransactionPosting))
			{
				if (InvoicingJob != null && !IsCostPosted)
				{
					ChargeWithCost anotherChargeWithSameAPInvoiceDetails = InvoicingJob.Charges.GetChargeByAPInvoiceCreditorCurrency(this);
					if (anotherChargeWithSameAPInvoiceDetails != null)
					{
						ResumeDefaulting();
						JR_APInvoiceDate = anotherChargeWithSameAPInvoiceDetails.JR_APInvoiceDate;
						JR_CostReference = anotherChargeWithSameAPInvoiceDetails.JR_CostReference;
						JR_PaymentDate = anotherChargeWithSameAPInvoiceDetails.JR_PaymentDate;
						JR_PaymentType = anotherChargeWithSameAPInvoiceDetails.JR_PaymentType;
						JR_AB = anotherChargeWithSameAPInvoiceDetails.JR_AB;
						JR_AK = anotherChargeWithSameAPInvoiceDetails.JR_AK;
						UpdateChequeNumberWithoutUpdatingChequeBook(anotherChargeWithSameAPInvoiceDetails.JR_ChequeNo);
						UpdateChequeNumberIsAutoAllocated();
						SuspendDefaulting();
					}
				}
			}
		}

		#endregion

		#region AutoratingCalculationDescription

		void EmptyRevenueCalculationDescription()
		{
			if (!AutoRatingOverrideSuppressed && InvoicingJob != null && !InvoicingJob.IsReversingInProcess && JR_E6_GatewaySellHeader.IsEmpty && !RevenueCalculationDescription.IsEmpty)
			{
				RevenueCalculationDescription = ZBlob.Empty;
			}
		}

		void EmptyCostCalculationDescription()
		{
			if (!AutoRatingOverrideSuppressed && InvoicingJob != null && !InvoicingJob.IsReversingInProcess && !CostCalculationDescription.IsEmpty)
			{
				CostCalculationDescription = ZBlob.Empty;
			}
		}

		#endregion

		#region Implementation

		void UpdateChequeNumberIsAutoAllocated()
		{
			Calc_ChequeNumberIsAutoAllocatedLabelInfo.RefreshBinding();
			UpdateJR_ChequeNoReadOnly();
		}

		#endregion

		#region Invoice Type Suspender

		public FunctionalitySuspender InvoiceTypeUpdateSuspender => invoiceTypeUpdateSuspender ?? (invoiceTypeUpdateSuspender = new FunctionalitySuspender());
		FunctionalitySuspender invoiceTypeUpdateSuspender;

		#endregion
	}
}

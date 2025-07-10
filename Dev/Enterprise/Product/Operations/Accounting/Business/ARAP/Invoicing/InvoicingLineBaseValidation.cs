using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;
using ExRateOption = Enterprise.Accounting.Business.AccountingConstants.InvoicePostingExchangeRateOption;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoicingLineBaseValidation : DependentTransactionLineValidation
	{
		public InvoicingLineBaseValidation(InvoicingLineBase parent)
			: base(parent)
		{
		}

		InvoicingLineBase InvoicingLine
		{
			get { return (InvoicingLineBase)Parent; }
		}

		AccComplianceDocumentHeaderDetailValidationHelper AccComplianceDocumentHeaderDetailValidationHelper => new AccComplianceDocumentHeaderDetailValidationHelper(Parent.Factory, InvoicingLine);

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDependantLineItems();
			ValidateGSTInclusiveAmount();
			AddWarningIfMarked();
			ValidateConsolCostThisLineWasApportionedFrom();
			ValidateConsolIDFromApportionedCharge();
			ValidateOriginalJobCharge();
			ValidateLinkedChargeNotDeleted();
			ValidateComplianceDocumentNumber();
			ValidateComplianceSubType();
			ValidateComplianceDocumentVATRegistrationNum();
			ValidateComplianceDocumentDate();
			ValidateComplianceDocumentReportingPeriod();
			ValidateComplianceSupportingDocumentType();
			ValidateComplianceDocumentSupportingReason();
			ValidateComplianceSupportingDocumentNumber();
			ValidateComplianceDocumentOrganization();
			ValidateCreateJobHeader();

			var invoiceBase = InvoicingLine.InvoiceBase;
			if (invoiceBase != null && invoiceBase.SupportMultiPeriodApportionment)
			{
				ValidatePeriodApportionmentMethod();
				ValidatePeriodStartDate();
				ValidatePeriodEndDate();
				ValidatePeriodClearingGLAccountPK();
			}
		}

		void ValidateCreateJobHeader()
		{
			if (InvoicingLine.CreatingJobHeaderErrorMessage != null)
			{
				InvoicingLine.AddRowError(InvoicingLine.CreatingJobHeaderErrorMessage);
			}
		}

		void AddWarningIfMarked()
		{
			if (InvoicingLine.MarkForWarningAsCreditorOrLoginCompanyIsNotTaxRegisteredForTaxedTransaction)
			{
				InvoicingLine.AddRowWarning(GetTaxRecalculationWarningMessage());
			}
		}

		public static string GetTaxRecalculationWarningMessage()
		{
			return Res.GetString("f5712235-824c-438f-a5dd-5c6426d0d266", "Line Tax Amount has been re-calculated because current login company is not tax registered and/or Creditor is not tax applicable");
		}

		protected override void CheckAL_TaxDate()
		{
			var isWritingOffBadDebt = InvoicingLine.InvoiceBase?.IsBadDebtWritingOff ?? false;
			if (!isWritingOffBadDebt)
			{
				base.CheckAL_TaxDate();
				if ((Parent is APInvoiceLine || Parent is APCreditNoteLine)
					&& Parent.AL_TaxDate.IsEmpty && InvoicingLine.InvoicingJob != null
					&& InvoicingLine.ConsolIDFromApportionedCharge.IsEmpty)
				{
					var parentJob = InvoicingLine.InvoicingJob;
					var taxDateOption = ((IPostingJob)parentJob).GetTaxDateDefaultingOptionForJob(InvoicingLine.Factory, LedgerTypes.AccountsPayable);
					if (taxDateOption != null)
					{
						var option = taxDateOption.TaxDateOption;
						if (option != TaxDateDefaultingOption.Code.InvoiceDate && option != TaxDateDefaultingOption.Code.Today)
						{
							var plugIn = parentJob.GetInvoicingSupporter();
							if (plugIn != null)
							{
								var operationalDate = ZDate.Empty;
								var taxDateDescription = ZString.Empty;
								(operationalDate, taxDateDescription) = ((IPostingJob)parentJob).GetTaxDateBasedOnRegistryDefaultingOption(plugIn, option, ZDate.Empty);
								if (operationalDate.IsEmpty)
								{
									InvoicingLine.AL_TaxDateInfo.AddError(Res.GetString("ce6aa6d9-2576-41f3-b8e2-33e5837c8b6d", @"Tax Date configuration for this job type requires {0} date, which has not been entered on the job.
This date needs to be added to the job before costs to this job can be posted.", taxDateDescription));
								}
							}
						}
					}
				}
			}
		}

		protected override void CheckAL_OverseasTotal()
		{
			if (InvoicingLine?.InvoiceBase?.IsSourceReferenceUsed ?? false)
			{
				TypeValidation.CheckValidDecimal(InvoicingLine.AL_OverseasTotalInfo, 19, 4);
				if (InvoicingLine.AL_OverseasTotal != (InvoicingLine.AL_OSExTaxAmount + InvoicingLine.AL_OSTaxAmount))
				{
					InvoicingLine.AL_OverseasTotalInfo.AddWarning(Res.GetString("8e46b643-6d93-4cd4-91de-afb0473efcb6", "Overseas Total does not equal Overseas Amount + Overseas Tax."));
				}
			}
			else
			{
				base.CheckAL_OverseasTotal();
			}
		}

		protected override void CheckAL_Sequence()
		{
			base.CheckAL_Sequence();
			InvoicingBase invoiceBase = InvoicingLine.InvoiceBase;
			if (invoiceBase != null && !invoiceBase.IsReverseTransaction && !InvoicingLine.AL_Sequence_ReadOnly)
			{
				MandatoryValidation.CheckNotNegative(Parent.AL_SequenceInfo);
				if (!Parent.AL_SequenceInfo.HasErrors())
				{
					var key = new Tuple<ZGuid, ZShort>(InvoicingLine.AL_JH, InvoicingLine.AL_Sequence);
					if (invoiceBase.IsDuplicateLineSequenceDetected(key))
					{
						Parent.AL_SequenceInfo.AddError(Res.GetString("d465e5b5-8432-4479-90e0-ae8422ace361", "The Line Sequence Number must be unique."));
					}
				}
			}

			var overrideTransactionLineSequenceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(InvoicingLine.Company.GC_RN_NKCountryCode) as IOverrideTransactionLineSequenceProvider;

			if (invoiceBase != null
				&& (overrideTransactionLineSequenceProvider?.CanOverrideTransactionLineSequence(invoiceBase) ?? false)
				&& InvoicingLine.AL_LineAmount < 0)
			{
				Parent.AL_SequenceInfo.AddWarning(Res.GetString("3525BF5B-98CB-4615-9A91-4FEA61D32159", "The 'discount line' should be recorded immediately after the discounted line in the same invoice group."));
			}
		}

		public Charge JobContainsUnpostedApportionments(ZGuid jobPK, ZGuid chargePK)
		{
			ZDBOnlyQuery chargeQuery = new ZDBOnlyQuery(typeof(Charge));
			chargeQuery.AddToFilter(JobChargeSchema.JR_JH, jobPK);
			chargeQuery.AddToFilter(JobChargeSchema.JR_AC, chargePK);
			chargeQuery.AddToFilter(JobChargeSchema.JR_E6, SQLComparisonOperator.NotEqual, DBNull.Value);
			ZDBOnlyQuery notPostedQuery = new ZDBOnlyQuery(typeof(Charge));
			notPostedQuery.AddToFilter(JobChargeSchema.JR_AL_APLine, SQLComparisonOperator.Equal, DBNull.Value);
			ZDBOnlySubQuery lineSubquery = new ZDBOnlySubQuery(typeof(InvoicingLineBase), JobChargeSchema.JR_AL_APLine);
			lineSubquery.AddToFilter(new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Accrual));
			notPostedQuery.AddSubQuery(JobChargeSchema.JR_AL_APLine, AccTransactionLinesSchema.PK, lineSubquery, JoinCondition.Or);
			chargeQuery.AddToFilter(notPostedQuery);

			return InvoicingLine.Factory.GetCachedReadOnlyFactory().LoadTop1<Charge>(chargeQuery);
		}

		public void ValidateGenericCharge()
		{
			ValidateCalculatedProperty(InvoicingLine.GenericChargeInfo);
			if (InvoicingLine.AL_JHInfo.HasErrors())
			{
				ValidateAL_JH();
			}
		}

		#region Compliance Document Related Property

		public void ValidateComplianceDocumentOrganization()
		{
			ValidateCalculatedProperty(InvoicingLine.ComplianceDocumentOrganizationInfo);
		}

		protected void CheckComplianceDocumentOrganization()
		{
			if (InvoicingLine.CreateComplianceDocumentRecordOnPosting)
			{
				ListValidation.ErrorIfInvalidPK(InvoicingLine.ComplianceDocumentOrganizationInfo, InvoicingLine.ComplianceOrganization);

				if (!InvoicingLine.ComplianceDocumentOrganizationInfo.HasErrors())
				{
					CheckPropertyValuesForSameDocumentNumber(InvoicingLine.ComplianceDocumentOrganizationInfo);
				}
			}
		}

		public void ValidateComplianceDocumentNumber()
		{
			ValidateCalculatedProperty(InvoicingLine.ComplianceDocumentNumberInfo);
		}

		protected void CheckComplianceDocumentNumber()
		{
			if (InvoicingLine.CreateComplianceDocumentRecordOnPosting)
			{
				MandatoryValidation.CheckEntered(InvoicingLine.ComplianceDocumentNumberInfo);

				var organisation = InvoicingLine.ComplianceDocumentOrganization.IsEmpty ? InvoicingLine.InvoiceBase.AH_OH : InvoicingLine.ComplianceDocumentOrganization;
				if (!InvoicingLine.ComplianceDocumentNumberInfo.HasErrors() && AccComplianceDocumentHeaderDetailValidationHelper.ShouldValidateDuplicateNumber)
				{
					var error = AccComplianceDocumentHeaderDetailValidationHelper.ValidateDuplicateAPDocumentNumber(organisation);
					if (!error.IsEmpty)
					{
						InvoicingLine.ComplianceDocumentNumberInfo.AddError(error);
					}
				}

				if (!InvoicingLine.ComplianceDocumentNumberInfo.HasErrors())
				{
					var error = AccComplianceDocumentHeaderDetailValidationHelper.ValidateDocumentNmberMatchINVForCRD(organisation, InvoicingLine.InvoiceBase.AH_Ledger);
					if (!error.IsEmpty)
					{
						InvoicingLine.ComplianceDocumentNumberInfo.AddError(error);
					}
				}

				if (!InvoicingLine.ComplianceDocumentNumberInfo.HasErrors() && !InvoicingLine.ComplianceDocumentNumber.IsEmpty && Parent.AL_AT.IsEmpty)
				{
					InvoicingLine.ComplianceDocumentNumberInfo.AddError(Res.GetString("6F74C90C-DDDD-482C-8EBF-46C897245F17", "The Tax ID should be entered before Compliance Document Number."));
				}

				if (!InvoicingLine.ComplianceDocumentNumberInfo.HasErrors())
				{
					var error = AccComplianceDocumentHeaderDetailValidationHelper.ValidateDocumentNumberWithValidPrefix();
					if (!error.IsEmpty)
					{
						InvoicingLine.ComplianceDocumentNumberInfo.AddError(error);
					}
				}

				if (!InvoicingLine.ComplianceDocumentNumberInfo.HasErrors()
						&& CannotCreateNegativeComplianceDocumentLine
						&& (InvoicingLine.InvoiceBase.HasPCDSettingForAP || InvoicingLine.InvoiceBase.HasPCDSettingForIN))
				{
					InvoicingLine.ComplianceDocumentNumberInfo.AddError(ComplianceDocumentNegativeLinesMessage);
				}
			}
		}

		bool CannotCreateNegativeComplianceDocumentLine => !AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.Value && InvoicingLine.AL_OSExTaxAmount < 0;

		public void ValidateComplianceSubType()
		{
			ValidateCalculatedProperty(InvoicingLine.ComplianceSubTypeInfo);
		}

		protected void CheckComplianceSubType()
		{
			if (InvoicingLine.CreateComplianceDocumentRecordOnPosting)
			{
				MandatoryValidation.CheckEntered(InvoicingLine.ComplianceSubTypeInfo);

				if (!InvoicingLine.ComplianceSubTypeInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(InvoicingLine.ComplianceSubTypeInfo);
				}

				if (!InvoicingLine.ComplianceSubTypeInfo.HasErrors())
				{
					CheckPropertyValuesForSameDocumentNumber(InvoicingLine.ComplianceSubTypeInfo);
				}
			}
		}

		public void ValidateComplianceDocumentVATRegistrationNum()
		{
			ValidateCalculatedProperty(InvoicingLine.ComplianceDocumentVATRegistrationNumInfo);
		}

		protected void CheckComplianceDocumentVATRegistrationNum()
		{
			if (InvoicingLine.CreateComplianceDocumentRecordOnPosting)
			{
				var regNumInfo = InvoicingLine.ComplianceDocumentVATRegistrationNumInfo;

				MandatoryValidation.CheckEntered(regNumInfo);

				if (!regNumInfo.HasErrors() && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
				{
					TaiwanUnifiedBusinessNumberValidator.Validate(regNumInfo, InvoicingLine.ComplianceDocumentVATRegistrationNum, CargoWise.ComponentModel.NotificationType.Error);
				}

				if (!regNumInfo.HasErrors())
				{
					CheckPropertyValuesForSameDocumentNumber(regNumInfo);
				}
			}
		}

		public void ValidateComplianceDocumentDate()
		{
			ValidateCalculatedProperty(InvoicingLine.ComplianceDocumentDateInfo);
		}

		protected void CheckComplianceDocumentDate()
		{
			if (InvoicingLine.CreateComplianceDocumentRecordOnPosting)
			{
				MandatoryValidation.CheckEntered(InvoicingLine.ComplianceDocumentDateInfo);

				if (!InvoicingLine.ComplianceDocumentDateInfo.HasErrors())
				{
					TypeValidation.CheckValidZDateTimeAndRange(InvoicingLine.ComplianceDocumentDateInfo);
				}

				if (!InvoicingLine.ComplianceDocumentDateInfo.HasErrors())
				{
					CheckPropertyValuesForSameDocumentNumber(InvoicingLine.ComplianceDocumentDateInfo);
				}

				if (!InvoicingLine.ComplianceDocumentDateInfo.HasErrors())
				{
					var error = AccComplianceDocumentHeaderDetailValidationHelper.ValidateDocumentDate();
					if (!error.IsEmpty)
					{
						InvoicingLine.ComplianceDocumentDateInfo.AddError(error);
					}
				}
			}
		}

		public void ValidateComplianceSupportingDocumentType()
		{
			ValidateCalculatedProperty(InvoicingLine.ComplianceSupportingDocumentTypeInfo);
		}

		protected void CheckComplianceSupportingDocumentType()
		{
			if (InvoicingLine.CreateComplianceDocumentRecordOnPosting)
			{
				ListValidation.ErrorIfInvalidCode(InvoicingLine.ComplianceSupportingDocumentTypeInfo);

				if (!InvoicingLine.ComplianceSupportingDocumentTypeInfo.HasErrors())
				{
					CheckPropertyValuesForSameDocumentNumber(InvoicingLine.ComplianceSupportingDocumentTypeInfo);
				}
			}
		}

		public void ValidateComplianceDocumentSupportingReason()
		{
			ValidateCalculatedProperty(InvoicingLine.ComplianceDocumentSupportingReasonInfo);
		}

		protected void CheckComplianceDocumentSupportingReason()
		{
			if (InvoicingLine.CreateComplianceDocumentRecordOnPosting)
			{
				ListValidation.ErrorIfInvalidCode(InvoicingLine.ComplianceDocumentSupportingReasonInfo);

				if (!InvoicingLine.ComplianceDocumentSupportingReasonInfo.HasErrors())
				{
					CheckPropertyValuesForSameDocumentNumber(InvoicingLine.ComplianceDocumentSupportingReasonInfo);
				}
			}
		}

		public void ValidateComplianceSupportingDocumentNumber()
		{
			ValidateCalculatedProperty(InvoicingLine.ComplianceSupportingDocumentNumberInfo);
		}

		protected void CheckComplianceSupportingDocumentNumber()
		{
			if (InvoicingLine.CreateComplianceDocumentRecordOnPosting)
			{
				CheckPropertyValuesForSameDocumentNumber(InvoicingLine.ComplianceSupportingDocumentNumberInfo);

				if (!InvoicingLine.ComplianceSupportingDocumentNumberInfo.HasErrors() && !InvoicingLine.ComplianceSupportingDocumentNumber.IsEmpty && InvoicingLine.ComplianceSupportingDocumentType.IsEmpty)
				{
					InvoicingLine.ComplianceSupportingDocumentNumberInfo.AddError(Res.GetString("DD25513B-579B-4266-8952-359D540B3884", "The Supporting Document Number should have a Supporting Document Type."));
				}
			}
		}

		public void ValidateComplianceDocumentReportingPeriod()
		{
			ValidateCalculatedProperty(InvoicingLine.ComplianceDocumentReportingPeriodInfo);
		}

		protected void CheckComplianceDocumentReportingPeriod()
		{
			if (InvoicingLine.CreateComplianceDocumentRecordOnPosting)
			{
				MandatoryValidation.CheckEntered(InvoicingLine.ComplianceDocumentReportingPeriodInfo);

				if (!InvoicingLine.ComplianceDocumentReportingPeriodInfo.HasErrors())
				{
					CheckPropertyValuesForSameDocumentNumber(InvoicingLine.ComplianceDocumentReportingPeriodInfo);
				}

				if (!InvoicingLine.ComplianceDocumentReportingPeriodInfo.HasErrors())
				{
					var error = AccComplianceDocumentHeaderDetailValidationHelper.ValidateDocumentReportingPeriod();
					if (!error.IsEmpty)
					{
						InvoicingLine.ComplianceDocumentReportingPeriodInfo.AddError(error);
					}
				}
			}
		}

		void CheckPropertyValuesForSameDocumentNumber(ZPropertyInfo propertyInfo)
		{
			if (InvoicingLine.InvoiceBase?.Lines.Cast<InvoicingLineBase>().Any(x => x.ComplianceDocumentNumber == InvoicingLine.ComplianceDocumentNumber && x.CreateComplianceDocumentRecordOnPosting && x.FindPropertyInfo(propertyInfo.Name).Value.ToString() != propertyInfo.Value.ToString()) ?? false)
			{
				propertyInfo.AddError(Res.GetString("B0C2824F-CA8C-4BF9-ACEE-7A42E0B73870", "For all transaction lines with the same document number, the {0} must be the same.", propertyInfo.HumanReadableName));
			}
		}

		#endregion

		protected override void CheckAL_ExchangeRate()
		{
			base.CheckAL_ExchangeRate();

			var invBase = InvoicingLine.InvoiceBase;
			if (invBase != null)
			{
				MandatoryValidation.CheckNotNegative(InvoicingLine.AL_ExchangeRateInfo);
				MandatoryValidation.CheckNotZero(InvoicingLine.AL_ExchangeRateInfo);

				if (!InvoicingLine.ExchangeRateValidationSuspender.IsSuspended
					&& !invBase.IsReversalTransaction
					&& !(invBase.IsAmendingTransaction && invBase.IsCopyExRateForAmendingAllowedByRegistry())
					&& (!invBase.IsPosted || invBase.IsIncompleteInvoice || invBase.IsAllocatingInvoice))
				{
					var consumerToUse = InvoicingLine.IsPopulatedFromImportedApportionment && InvoicingLine.ApportionmentChargeImportedFrom?.ParentConsolCost != null
						? InvoicingLine.ApportionmentChargeImportedFrom.ParentConsolCost.ExchangeRateConfigurationRateConsumer
						: invBase.GetExchangeRateConfigurationRateConsumer(InvoicingLine.Job as Job);

					var exRateLedger = invBase.GetExRateLedger();
					var isLocalCurrency = invBase.AH_RX_NKTransactionCurrency == invBase.Company.GC_RX_NKLocalCurrency;
					var rateType = AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationRateType(consumerToUse, invBase.Header, exRateLedger, Parent.AL_RX_NKTransactionCurrency, invBase.InvoiceCurrencyType) ?? InvoicingLine.RateType;

					var isForeignDefault = InvoicingBaseCommonValidation.GetInvoicePostingExchangeRateOptionAP(InvoicePostingExchangeRateCurrencyType.Code.Foreign) == ExRateOption.Default.Code;
					var isLocalDefault = InvoicingBaseCommonValidation.GetInvoicePostingExchangeRateOptionAP(InvoicePostingExchangeRateCurrencyType.Code.Local) == ExRateOption.Default.Code;

					var skipValidationForOverridingExchangeRateWithHeaderForeignCurrency = invBase.AH_OverrideExchangeRate && invBase.UseJobExchangeRate && !invBase.IsLocalCurrencyTransaction && !isForeignDefault;

					var skipValidationForOverridingExchangeRateWithHeaderLocalCurrency = invBase.AH_OverrideExchangeRate && invBase.IsLocalCurrencyTransaction && !isLocalDefault;

					if ((!invBase.IsLocalCurrencyTransaction && !skipValidationForOverridingExchangeRateWithHeaderForeignCurrency)
						|| (invBase.IsLocalCurrencyTransaction && !skipValidationForOverridingExchangeRateWithHeaderLocalCurrency))
					{
						var notification = ExchangeRateCalculator.CheckExchangeRate(InvoicingLine.AL_RX_NKTransactionCurrency, isLocalCurrency,
						InvoicingLine.Company, rateType, exRateLedger, invBase.AH_InvoiceDate, invBase.AH_PostDate, invBase.InvoiceTaxDate, InvoicingLine.AL_ExchangeRate);
						if (notification != null && (Parent.AL_RX_NKTransactionCurrency != invBase.AH_RX_NKTransactionCurrency || invBase.UseJobExchangeRate))
						{
							InvoicingLine.AL_ExchangeRateInfo.AddError(notification.Message);
						}
					}
				}
			}
		}

		protected override void CheckAL_OSExTaxAmount()
		{
			base.CheckAL_OSExTaxAmount();

			if (InvoicingLine.AL_OSExTaxAmount == 0M && (InvoicingLine.ChargeCode == null || (InvoicingLine.ChargeCode != null && !InvoicingLine.ChargeCode.IsComment)))
			{
				InvoicingLine.AL_OSExTaxAmountInfo.AddError(AmountExcludingTaxError);
			}

			if (InvoicingLine.AL_OSExTaxAmount != 0M && InvoicingLine.ChargeCode != null && InvoicingLine.ChargeCode.IsComment)
			{
				InvoicingLine.AL_OSExTaxAmountInfo.AddError(AccountingConstants.AmountCannotBeSetErrorMessage);
			}

			if (CheckNegativeAmountsOnAccountReceivableTransactionsIsNotPermitted())
			{
				InvoicingLine.AL_OSExTaxAmountInfo.AddError(Res.GetString("fdac3059-0350-41cd-9f6c-186beee67027", "Negative revenue charges are not allowed. This is controlled by the registry setting at Accounting > Receivable defaults > Default Settings > Negative Charges on Accounts Receivable Transactions."));
			}
		}

		protected bool CheckNegativeAmountsOnAccountReceivableTransactionsIsNotPermitted()
		{
			return InvoicingLine.AL_OSExTaxAmount < 0
				&& Parent.AL_LineType == TransactionLineTypes.Revenue
				&& !(Parent.TransactionHeader?.IsCancelled ?? false)
				&& !AccTransactionLinesValidationHelper.IsNegativeChargeAllowed(InvoicingLine);
		}

		protected override bool ShouldValidateTaxAmountSign
		{
			get
			{
				return !InvoicingLine.IsInDatabase || InvoicingLine.IsInvoicingBaseApproving;
			}
		}

		protected override void CheckAL_OSTaxAmount()
		{
			base.CheckAL_OSTaxAmount();

			if (InvoicingLine.IsGSTMandatory && !InvoicingLine.TaxAmountErrorDetailForInterCompanyInvoiceImport.TaxAmountErroeMessage.IsEmpty &&
				InvoicingLine.TaxAmountErrorDetailForInterCompanyInvoiceImport.OSTaxAmount == InvoicingLine.AL_OSTaxAmount &&
				(InvoicingLine.TaxAmountErrorDetailForInterCompanyInvoiceImport.TaxRatePK == InvoicingLine.TaxRate?.PK ||
				InvoicingLine.TaxAmountErrorDetailForInterCompanyInvoiceImport.TaxRateCalc == InvoicingLine.AL_TaxRateCalc))
			{
				InvoicingLine.AL_OSTaxAmountInfo.AddError(InvoicingLine.TaxAmountErrorDetailForInterCompanyInvoiceImport.TaxAmountErroeMessage);
			}

			if (!InvoicingLine.AL_OSTaxAmountInfo.HasErrors() && InvoicingLine.IsGSTMandatory && InvoicingLine.TaxRate != null)
			{
				if (!InvoicingLine.TaxRate.IsVATRemittedByCustomer
					&& InvoicingLine.AL_TaxRateCalc > 0M
					&& InvoicingLine.AL_OSTaxAmount == 0M &&
					InvoicingLine.CachedCalculatedTaxAmount != 0m &&
					!ShouldSkipValidateForEmptyTaxAmount())
				{
					InvoicingLine.AL_OSTaxAmountInfo.AddError(EnforceGSTAmountEntry);
				}
				else
				{
					var (shouldAddErrorOrWarning, addError) = CountrySpecificValidationHelper.ShouldAddErrorOrWarningWhenIsOutsideExpectedTaxAmount(InvoicingLine);
					if (shouldAddErrorOrWarning)
					{
						if (addError)
						{
							InvoicingLine.AL_OSTaxAmountInfo.AddError(TaxAmountRangeWarning);
						}
						else
						{
							InvoicingLine.AL_OSTaxAmountInfo.AddWarning(TaxAmountRangeWarning);
						}
					}
					else
					{
						if (InvoicingLine.IsOutsideExpectedTaxAmount())
						{
							InvoicingLine.AL_OSTaxAmountInfo.AddWarning(TaxAmountRangeWarning);
						}
					}
				}
			}
		}

		bool ShouldSkipValidateForEmptyTaxAmount()
		{
			var result = false;

			if (InvoicingLine.ApportionmentChargeImportedFrom != null &&
				InvoicingLine.ApportionmentChargeImportedFrom.ParentConsolCost != null)
			{
				var consolCostTotalOSTax = InvoicingLine.ApportionmentChargeImportedFrom.ParentConsolCost.InvoiceOSTax;
				var relatedLinesTotalOSTax = InvoicingLine.InvoiceBase.Lines.Cast<InvoicingLineBase>().
					Where(x => x.ApportionmentChargeImportedFrom != null &&
					x.ApportionmentChargeImportedFrom.JR_E6 == InvoicingLine.ApportionmentChargeImportedFrom.JR_E6)
					.Sum(x => x.AL_OSTaxAmount);

				result = relatedLinesTotalOSTax == consolCostTotalOSTax;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		protected override void CheckAL_Desc()
		{
			base.CheckAL_Desc();
			MandatoryValidation.CheckEntered(Parent.AL_DescInfo);
			if (!AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.Value && Parent.AL_LineType == TransactionLineTypes.Revenue && Parent.ChargeCode != null && !Parent.AL_Desc.StartsWith(Parent.ChargeCode.AC_Desc, StringComparison.OrdinalIgnoreCase))
			{
				Parent.AL_DescInfo.AddWarning(Res.GetString("ee873fc1-e5d0-48d8-a851-bb10852e1591", "Charge description was changed from default. This description will appear on AR Invoice without translation."));
			}

			if (Parent.TransactionHeader != null
				&& Parent.Company != null
				&& Parent.Company.GC_RN_NKCountryCode == CountryCodes.India
				&& Parent.TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable
				&& (Parent.TransactionHeader.AH_TransactionType == TransactionTypes.Invoice || Parent.TransactionHeader.AH_TransactionType == TransactionTypes.CreditNote)
				&& Parent.AL_Desc.TrimEnd().Length < 3)
			{
				Parent.AL_DescInfo.AddError(Res.GetString("9dd9bd34-b3c2-4573-be33-4e32b236fb95", "Description has less than 3 characters."));
			}
		}

		protected override void CheckAL_JHIsValidZGuid()
		{
			if (InvoicingLine.ShouldValidateJob)
			{
				ListValidation.ErrorIfInvalidPK(InvoicingLine.AL_JHInfo, InvoicingLine.JobCollection, ResString.GetMultilingualString("e395ccd6-94da-410d-8f5c-d2a715454200", "Job billing record for this job does not exist. To rectify, click on the billing tab of the job and save."));
			}
		}

		protected override void CheckAL_JH()
		{
			if (InvoicingLine.ShouldValidateJob)
			{
				if (InvoicingLine.AL_JHInfo.HasErrors() && InvoicingLine.IsAmendingOriginalViaStrongReference)
				{
					InvoicingLine.AL_JHInfo.AddError(Res.GetString("cfed326e-eeb1-49f3-bea1-b7513fee5919", "Only Jobs from Original transaction can be used on Amending transaction."));
				}

				CheckAL_JHAgainstGenericCharge(InvoicingLine.AL_JHInfo);

				if (Parent.Job != null)
				{
					if (InvoicingLine.ChargeCode != null &&
						!(this is IncompleteInvoicingLineBaseValidation) &&
						Parent.Job.IsReadyForFinancialClosureWithoutPostSecurity)
					{
						if (InvoicingLine.ChargeCode.AC_ChargeType == ChargeType.Disbursement &&
							AccountingConfigurationRegistry.Instance.AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC.Value &&
							(InvoicingLine.IsPopulatedFromImportedJobCharge || InvoicingLine.IsPopulatedFromImportedApportionment))
						{
							Parent.AL_JHInfo.AddWarning(Res.GetString("D526C8D0-69FB-4170-9FF7-F7C314AEF0F1", "You are posting Disbursement Charge when the job has Jobs Ready for Financial Closure status."));
						}
						else
						{
							Parent.AL_JHInfo.AddError(Res.GetString("7C81B418-D59A-4731-983B-F6365213ACA1", "Cannot post this charge, because the job has Jobs Ready for Financial Closure status."));
						}
					}

					if (Parent.Job.JH_ParentTableCode == RatingHeaderSchema.Constants.Prefix)
					{
						InvoicingLine.AL_JHInfo.AddError(Res.GetString("8e6a701a-a7cf-4464-aa71-db312d6225d9", "This type of job cannot be used."));
					}

					if (Parent.Job.JH_Status == JobHeaderStatus.Closed.Code)
					{
						if (InvoicingLine.InvoiceBase == null || !JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(InvoicingLine.InvoiceBase.Factory, Parent.Job as Job))
						{
							InvoicingLine.AL_JHInfo.AddWarning(ReopenClosedJobSecurityMessage);
						}
						else
						{
							InvoicingLine.AL_JHInfo.AddWarning(ReopenClosedJobWarningMessage);
						}
					}
				}

				if (InvoicingLine.GenericChargeInfo.HasErrors())
				{
					ValidateCalculatedProperty(InvoicingLine.GenericChargeInfo);
				}

				if ((Parent is APInvoiceLine || Parent is APCreditNoteLine) &&
					!Parent.AL_JHInfo.HasErrors() && Parent.AL_JH.IsValid)
				{
					if (InvoicingLine.InvoiceBase != null && InvoicingLine.InvoiceBase.IsInvoiceApproving && InvoicingLine.AL_RevRecognitionType.IsEmpty)
					{
						InvoicingLine.AL_JHInfo.AddError(JobValidation.EmptyRevenueRecognitionTypeOnJobRelatedLineErrorMessage);
					}
					else
					{
						ValidateRevenueRecognition(Res.GetString("4A18FE3F-C7E5-4F62-8DD3-F2D1517792AA", "This invoice cannot be posted until the {0:G} for this job is recorded. This job and charge code combination requires this date for revenue recognition purposes."));
					}

					CriticalValidationInfoCollectorService.GetOrCreateService(Parent.Factory).AddInfoWhenAllowed(Parent.AL_AC,
	CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeFromJobDuringPreSaveValidation,
	() => FormattableString.Invariant(
$@"ChargeCode PK: {Parent.AL_AC}
Job PK: {Parent.AL_JH}

Revenue Recognition Type Details: 
{Parent.Job.GetRevenueRecognitionDetails(Parent.ChargeCode)}

GetRevenueRecognitionValidationError result: {InvoicingLine.AL_JHInfo.GetErrors().FirstOrDefault()?.Message}"));
				}
			}

			if (!InvoicingLine.AL_JH.IsValid && InvoicingLine.AL_JHInfo.HasErrors() && !InvoicingLine.UXml_JobErrorMessages.IsEmpty)
			{
				InvoicingLine.AL_JHInfo.AddWarning(Res.GetString("1bf1433c-af86-4d73-a2a3-68db346e7ae1", "Source XML errors: {0}", InvoicingLine.UXml_JobErrorMessages));
			}

			CheckEmptyStates();
		}

		#region ValidateConsolIDFromApportionedCharge

		public void ValidateConsolIDFromApportionedCharge()
		{
			ValidateCalculatedProperty(InvoicingLine.ConsolIDFromApportionedChargeInfo);
		}

		protected void CheckConsolIDFromApportionedCharge()
		{
			if (InvoicingLine.ConsolIDFromApportionedCharge.IsEmpty && !InvoicingLine.UXml_ConsolErrorMessages.IsEmpty)
			{
				InvoicingLine.ConsolIDFromApportionedChargeInfo.AddWarning(Res.GetString("5565BBB8-3A15-45AF-A5F2-6DF03F204930", "Source XML errors: {0}", InvoicingLine.UXml_ConsolErrorMessages));
			}
		}

		#endregion

		#region OriginalJobCharge

		public void ValidateOriginalJobCharge()
		{
			ValidateCalculatedProperty(InvoicingLine.OriginalJobChargePKInfo);
		}

		protected void CheckOriginalJobChargePK()
		{
			if (!InvoicingLine.OriginalJobChargePK.IsEmpty)
			{
				if (InvoicingLine.OriginalJobCharge.IsCostPosted)
				{
					InvoicingLine.OriginalJobChargePKInfo.AddError(Res.GetString("887F5D57-0235-437D-A272-B93BCF8C480E", "The associated charge is already posted by another user. Please delete this row and try to post again."));
				}
				else
				{
					var originalJobCharge = InvoicingLine.OriginalJobCharge;
					InvoicingBase linkedIncompleteInvoice = null;
					if (originalJobCharge.JR_E6.IsValid && originalJobCharge.ParentConsolCost is JobConsolCost cost)
					{
						linkedIncompleteInvoice = cost?.GetTheImportingIncompleteInvoiceIfAny();
					}
					else
					{
						linkedIncompleteInvoice = GetTheImportingIncompleteInvoiceIfAny();
					}

					if (linkedIncompleteInvoice != null)
					{
						var errorMessage = Res.GetString("ffc98b4f-5965-4a67-b04c-e495a1e6e194", "The associated charge is already used in an Incomplete Invoice {0} dated {1}. Please delete this row. If required, you can manually enter a new invoice line without importing the accrual.", linkedIncompleteInvoice.AH_TransactionNum, linkedIncompleteInvoice.AH_InvoiceDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
						if (InvoicingLine.InvoiceBase.IsIncompleteInvoice || InvoicingLine.InvoiceBase.IsCompletingInvoice)
						{
							if (linkedIncompleteInvoice.PK != InvoicingLine.InvoiceBase.PK)
							{
								InvoicingLine.OriginalJobChargePKInfo.AddError(errorMessage);
							}
						}
						else
						{
							InvoicingLine.OriginalJobChargePKInfo.AddError(errorMessage);
						}
					}
				}
			}

			InvoicingBase GetTheImportingIncompleteInvoiceIfAny()
			{
				if (InvoicingLine.OriginalJobCharge != null)
				{
					var attribs = InvoicingLine.OriginalJobCharge.JobChargeAttributes.OfType<JobChargeAttrib>()
															.Where(a => a.EC_Name == JobChargeAttribTypeList.Codes.LinkedToIncompleteInvoice)
															.ToList();
					if (attribs.Count == 1 && ZGuid.TryParse(attribs[0].EC_Value, out var invoicePK))
					{
						return InvoicingLine.OriginalJobCharge.Factory.Load<InvoicingBase>(invoicePK);
					}
				}

				return null;
			}
		}

		#endregion

		public void ValidateLinkedChargeNotDeleted()
		{
			if (InvoicingLine.IsLinkedChargeDeleted)
			{
				InvoicingLine.AddRowError(Res.GetString("81eca090-8a60-4a72-89f8-e903c2eb5407", "The charge linked to the line is deleted. Please try to import the cost again using 'Apportion to Consols' button."));
			}
		}

		protected override void CheckAL_GE()
		{
			base.CheckAL_GE();

			var message = AccountingUtils.GetDeptNotInChargeDeptListMessage(InvoicingLine);
			if (!message.IsEmpty)
			{
				InvoicingLine.AL_GEInfo.AddError(message);
			}

			if (!InvoicingLine.AL_GEInfo.HasErrors() && (!InvoicingLine.IsInDatabase || InvoicingLine.AL_GEInfo.HasChanges))
			{
				if (InvoicingLine.AL_JH.IsValid && InvoicingLine.OriginalJobCharge == null)
				{
					if (InvoicingLine.Department != null && InvoicingLine.Department.GE_Misc)
					{
						InvoicingLine.AL_GEInfo.AddError(MiscDeptAndJobNotAllowedError);
					}
				}
			}

			ValidateGenericCharge();
		}

		protected override void CheckAL_AT()
		{
			base.CheckAL_AT();

			if (InvoicingLine.IsGSTMandatory && !Parent.ReadOnly && !InvoicingLine.IsCommentCharge)
			{
				if (Parent.AL_AT.IsEmpty && !Parent.AL_ATInfo.ReadOnly)
				{
					MandatoryValidation.CheckEntered(Parent.AL_ATInfo);
				}
				else if (!Parent.AL_AT.IsEmpty)
				{
					ListValidation.ErrorIfInvalidPK(InvoicingLine.AL_ATInfo, InvoicingLine.Lookups.TaxRates);

					if (!InvoicingLine.AL_ATInfo.HasErrors() &&
						InvoicingLine.InvoiceBase != null && InvoicingLine.InvoiceBase.HasInvalidPostingGroups)
					{
						var message = Res.GetString("9b51a343-8cbd-45b4-83fe-e72326728c20", @"You have prepared charges using a mix of Tax ID Posting Groups. 
This transaction line’s Tax ID Posting Group value is {0}.", InvoicingLine.PostingGroupID);
						InvoicingLine.AL_ATInfo.AddWarning(message);
					}
				}
			}
		}

		protected virtual void CheckGenericCharge()
		{
			MandatoryValidation.CheckEntered(InvoicingLine.GenericChargeInfo);

			if (InvoicingLine.InvoiceBase != null &&
				!InvoicingLine.IsInDatabase && !InvoicingLine.InvoiceBase.IsBadDebtWritingOff &&
				(InvoicingLine.InvoiceBase.IsReversing || InvoicingLine.IsAmendingOriginal))
			{
				if (!InvoicingLine.ChargeList.IsLoaded)
				{
					InvoicingLine.ChargeList.Load();
				}

				if (InvoicingLine.ChargeList.FindByPK(InvoicingLine.GenericCharge) == null)
				{
					InvoicingLine.GenericChargeInfo.AddWarning(ChargeCodeIsNoLongerValidWarning);
				}
				if (InvoicingLine.IsAmendingOriginal && InvoicingLine.InvoiceBase.Job != null && InvoicingLine.GenericChargeBizO != null && InvoicingLine.GenericChargeBizO.VC_IsGLAccount)
				{
					InvoicingLine.GenericChargeInfo.AddError(Res.GetString("eb9d882d-6b75-48d3-8193-ac3583dfae69", "You cannot select a GL Account for an Amending Transaction."));
				}
			}
			else
			{
				ListValidation.ErrorIfInvalidPK(InvoicingLine.GenericChargeInfo, InvoicingLine.ChargeList);
			}
			string error = InvoicingLine.ErrorMessageIfInvalidAL_AC_AL_AG();
			if (!string.IsNullOrEmpty(error))
			{
				InvoicingLine.GenericChargeInfo.AddError(error);
			}

			if (InvoicingLine.ShouldCreateChargeCodeMatchingRule)
			{
				var currentLineChargeCode = InvoicingLine.ChargeCode.AC_Code;
				var lineWithDifferentChargeCode = InvoicingLine.InvoiceBase?.Lines.Cast<InvoicingLineBase>().FirstOrDefault(line => line.ImportedChargeCodeXmlCode == InvoicingLine.ImportedChargeCodeXmlCode && line.ChargeCode != null && line.ChargeCode.AC_Code != currentLineChargeCode);
				if (lineWithDifferentChargeCode != null)
				{
					if (InvoicingLine.ImportedChargeCode.IsEmpty)
					{
						InvoicingLine.GenericChargeInfo.AddWarning(Res.GetString("07705788-F0D1-4280-AB79-E40A564D7BC7",
							"Charge code is not found for imported XML code '{0}'. However it also different to values in lines with the same charge code value in the imported XML. Charge code matching rule for foreign code '{0}' won’t be created.",
							InvoicingLine.ImportedChargeCodeXmlCode));
					}
					else
					{
						InvoicingLine.GenericChargeInfo.AddWarning(Res.GetString("28F2B5D1-CC9F-4037-8D79-EE7E646B2297",
							"Charge code value is different to a value matched by default. However it also different to values in lines with the same charge code value in the imported XML. Charge code matching rule for foreign code '{0}' won’t be updated.",
							InvoicingLine.ImportedChargeCodeXmlCode));
					}
				}
				else
				{
					if (InvoicingLine.ImportedChargeCode.IsEmpty)
					{
						InvoicingLine.GenericChargeInfo.AddWarning(Res.GetString("6F0187AA-9DDA-4DE3-93BF-653DD125442F", "Charge code is not found for imported XML code '{0}'. New matching rule will be created for foreign code '{0}'.",
							InvoicingLine.ImportedChargeCodeXmlCode));
					}
					else
					{
						InvoicingLine.GenericChargeInfo.AddWarning(Res.GetString("07C03C57-E6A6-4C7D-B3C0-F60DFCDAD4BF", "Charge code value is different to a value matched by default. Charge code matching rule for foreign code '{0}' will be updated.",
								InvoicingLine.ImportedChargeCodeXmlCode));
					}
				}
			}
		}

		public void ValidateDependantLineItems()
		{
			ValidateAL_AT();
			ValidateAL_AW();
		}

		protected void CheckAL_JHAgainstGenericCharge(ZPropertyInfo aL_JHInfo)
		{
			GenericCharge.GenericCharge charge = InvoicingLine.GenericChargeBizO;
			if (charge != null)
			{
				if (InvoicingLine.AL_JH.IsEmpty)
				{
					if (charge.VC_Type == Constants.ChargeType.Margin || charge.VC_Type == Constants.ChargeType.Disbursement || charge.VC_Type == Constants.ChargeType.ManualJobAccrual ||
						(InvoicingLine.IsAmendingOriginal && InvoicingLine.InvoiceBase.Job != null && !InvoicingLine.InvoiceBase.HasContext(BusinessContext.SystemCreatedAmending)))
					{
						aL_JHInfo.AddError(Res.GetString("0d6e3741-915d-41ce-83dc-a1ca110f9e78", "You must select a job for this charge code."));
					}
				}
				else
				{
					if (charge.VC_IsGLAccount)
					{
						aL_JHInfo.AddError(Res.GetString("22ef944b-d76a-4f57-9905-2b03cd91b6c6", "You cannot select a job for a GL Account charge code."));
					}
					else
					{
						if (charge.VC_Type == Constants.ChargeType.NonAccrual || charge.VC_Type == Constants.ChargeType.Overhead)
						{
							aL_JHInfo.AddError(Res.GetString("988f8824-baa5-4b19-8a64-94c03de1e4da", "You cannot select a job for a charge of type {0}.", charge.VC_Type));
						}
					}
				}
			}
		}

		protected override bool ShouldValidateNoTaxMessage => true;

		protected override bool IsAPTaxMessageMandatoryRegistry()
		{
			return InvoicingLine.IsAP();
		}

		protected override bool IsARTaxMessageMandatoryRegistry()
		{
			return InvoicingLine.IsAR();
		}

		#region CheckAL_AC

		protected override void CheckAL_AC()
		{
			base.CheckAL_AC();

			CheckEmptyStates();
		}

		#endregion

		#region CheckAL_GB

		protected override void CheckAL_GB()
		{
			base.CheckAL_GB();

			CheckEmptyStates();
		}

		#endregion

		#region PeriodApportionmentFields

		public void ValidatePeriodApportionmentMethod()
		{
			ValidateCalculatedProperty(InvoicingLine.PeriodApportionmentMethodInfo);
		}

		protected void CheckPeriodApportionmentMethod()
		{
			if (!InvoicingLine.PeriodApportionmentMethod_ReadOnly)
			{
				MandatoryValidation.CheckEntered(InvoicingLine.PeriodApportionmentMethodInfo);
				if (!InvoicingLine.PeriodApportionmentMethodInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(InvoicingLine.PeriodApportionmentMethodInfo);
				}
			}
		}

		public void ValidatePeriodClearingGLAccountPK()
		{
			ValidateCalculatedProperty(InvoicingLine.PeriodClearingGLAccountPKInfo);
		}

		protected void CheckPeriodClearingGLAccountPK()
		{
			if (!InvoicingLine.PeriodClearingGLAccountPK_ReadOnly)
			{
				MandatoryValidation.CheckEntered(InvoicingLine.PeriodClearingGLAccountPKInfo);
				if (!InvoicingLine.PeriodClearingGLAccountPKInfo.HasErrors())
				{
					TypeValidation.CheckValidGuid(InvoicingLine.PeriodClearingGLAccountPKInfo);
				}
				if (!InvoicingLine.PeriodClearingGLAccountPKInfo.HasErrors())
				{
					if (InvoicingLine.PeriodClearingGLAccount != null)
					{
						if (InvoicingLine.PeriodClearingGLAccount.AG_AccountType != Constants.AccountType.BalanceSheetAccount)
						{
							InvoicingLine.PeriodClearingGLAccountPKInfo.AddError(Res.GetString("229E602A-7946-434E-8B2A-B3B714F3AA18", "Period Apportionment Clearing Account must be a Balance Sheet Account."));
						}
						else if (InvoicingLine.PeriodClearingGLAccount.AG_DisallowDirectPosting)
						{
							InvoicingLine.PeriodClearingGLAccountPKInfo.AddError(Res.GetString("ea516509-a922-40e6-8287-00c58efa5ae2", "Period Apportionment Clearing Account must allow Direct Posting."));
						}

						if (InvoicingLine.PeriodClearingGLAccount.SubAccountTypes.Count > 0)
						{
							InvoicingLine.PeriodClearingGLAccountPKInfo.AddWarning(Res.GetString("704d4816-5259-4b4e-9730-236dd2916bbb",
								"This Clearing Account has {0} sub account type(s). You can specify the sub accounts value by editing the GL Journals in Manage > General Ledger > Journals module on posting of the Expense/Revenue Apportionment.",
								InvoicingLine.PeriodClearingGLAccount.SubAccountTypes.Count));
						}
					}
				}
			}
		}

		public void ValidatePeriodStartDate()
		{
			ValidateCalculatedProperty(InvoicingLine.PeriodStartDateInfo);
		}

		protected void CheckPeriodStartDate()
		{
			if (!InvoicingLine.PeriodStartDate_ReadOnly)
			{
				MandatoryValidation.CheckEntered(InvoicingLine.PeriodStartDateInfo);
				if (!InvoicingLine.PeriodStartDateInfo.HasErrors())
				{
					TypeValidation.CheckValidZDateRange(InvoicingLine.PeriodStartDateInfo);
				}
				if (!InvoicingLine.PeriodStartDateInfo.HasErrors())
				{
					CheckDateInPeriod(InvoicingLine.PeriodStartDateInfo, InvoicingLine.PeriodStartDate);
				}
				if (!InvoicingLine.PeriodStartDateInfo.HasErrors())
				{
					CheckPeriodStartDateAndEndDateSetupCore(InvoicingLine.PeriodStartDateInfo);
				}
			}
		}

		void CheckDateInPeriod(ZPropertyInfo dateInfoToAddError, ZDate dateToCheck)
		{
			if (!PeriodCalculator.IsPeriodValid(PeriodCalculator.GetPeriodFromDate(dateToCheck, InvoicingLine.AL_GC)))
			{
				dateInfoToAddError.AddError(Res.GetString("3af1a19a-6d15-4eb2-86ad-46991f0add8a", @"No accounting period exists for the selected date.
An expense cannot be apportioned to a non - existent period.
Please amend the date or go to General Ledger > Period Management and create a Financial Year to cover the period of apportionment."));
			}
		}

		void CheckPeriodStartDateAndEndDateSetupCore(ZPropertyInfo dateInfoToAddError)
		{
			if (!dateInfoToAddError.HasErrors())
			{
				if (InvoicingLine.PeriodStartDate.IsValid && !InvoicingLine.PeriodStartDate.IsEmpty && InvoicingLine.PeriodEndDate.IsValid && !InvoicingLine.PeriodEndDate.IsEmpty)
				{
					if (InvoicingLine.PeriodStartDate > InvoicingLine.PeriodEndDate)
					{
						dateInfoToAddError.AddError(Res.GetString("A1396251-6433-49A5-97CE-27A2FFAC9656", "Service Period Start Date cannot be after the Service Period End Date."));
					}
					else
					{
						var rangeOfPeriods = PeriodCalculator.GetRangeOfPeriods(InvoicingLine.PeriodStartDate, InvoicingLine.PeriodEndDate);
						if (rangeOfPeriods.Any(x => x.AM_IsGeneralLedgerClosed))
						{
							dateInfoToAddError.AddError(Res.GetString("80261C7C-0137-4A36-8BBB-8DBE44B327D4", "Service Period covers GL periods that are already closed. Unable to post period apportionment journals."));
						}
					}
				}
			}
		}

		AccountingPeriodCalculator PeriodCalculator => periodCalculator ?? (periodCalculator = new AccountingPeriodCalculator(InvoicingLine.InvoiceBase.Factory));
		AccountingPeriodCalculator periodCalculator;

		public void ValidatePeriodEndDate()
		{
			ValidateCalculatedProperty(InvoicingLine.PeriodEndDateInfo);
		}

		protected void CheckPeriodEndDate()
		{
			if (!InvoicingLine.PeriodEndDate_ReadOnly)
			{
				MandatoryValidation.CheckEntered(InvoicingLine.PeriodEndDateInfo);
				if (!InvoicingLine.PeriodEndDateInfo.HasErrors())
				{
					TypeValidation.CheckValidZDateRange(InvoicingLine.PeriodEndDateInfo);
				}
				if (!InvoicingLine.PeriodEndDateInfo.HasErrors())
				{
					CheckDateInPeriod(InvoicingLine.PeriodEndDateInfo, InvoicingLine.PeriodEndDate);
				}
				if (!InvoicingLine.PeriodEndDateInfo.HasErrors())
				{
					CheckPeriodStartDateAndEndDateSetupCore(InvoicingLine.PeriodEndDateInfo);
				}
			}
		}

		#endregion

		#region CheckEmptyStates

		void CheckEmptyStates()
		{
			if (Parent.HasRowErrors)
			{
				Parent.RemoveRowError(IndiaCompanyEmptyStateErrorMessages.EmptyStateErrorMessageForLineBranch);
			}
			if (Parent.HasRowWarnings)
			{
				Parent.RemoveRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForOrigin);
				Parent.RemoveRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForDestination);
				Parent.RemoveRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForFixedPlaceOfSupply);
				Parent.RemoveRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForOrganization);
			}

			if (CheckEmptyStateForLineBranch())
			{
				Parent.AddRowError(IndiaCompanyEmptyStateErrorMessages.EmptyStateErrorMessageForLineBranch);
			}
			if (CheckEmptyStateForOrigin())
			{
				Parent.AddRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForOrigin);
			}
			if (CheckEmptyStateForDestination())
			{
				Parent.AddRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForDestination);
			}
			if (CheckEmptyStateForFixedPlaceOfSupply())
			{
				Parent.AddRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForFixedPlaceOfSupply);
			}
			if (CheckEmptyStateForOrganization())
			{
				Parent.AddRowWarning(IndiaCompanyEmptyStateErrorMessages.EmptyStateWarningMessageForOrganization);
			}
		}

		bool CheckEmptyStateForLineBranch()
		{
			return AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value
				&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.India
				&& !InvoicingLine.IsInDatabase && InvoicingLine.IsGSTMandatory
				&& InvoicingLine.Branch != null && string.IsNullOrWhiteSpace(AccountingTaxLocations.GetBranchState(InvoicingLine.Branch));
		}

		bool CheckEmptyStateForOrigin()
		{
			if (AllowValidateEmptyState() && InvoicingLine.InvoicingJob?.PlugInData?.InvoicingSupporter?.Origin != null)
			{
				var state = (InvoicingLine.InvoicingJob.PlugInData.InvoicingSupporter.Origin as ILocation)?.State?.RW_Code;
				if (string.IsNullOrWhiteSpace(state))
				{
					return true;
				}
			}

			return false;
		}

		bool CheckEmptyStateForDestination()
		{
			if (AllowValidateEmptyState() && InvoicingLine.InvoicingJob?.PlugInData?.InvoicingSupporter?.Destination != null)
			{
				var state = (InvoicingLine.InvoicingJob.PlugInData.InvoicingSupporter.Destination as ILocation)?.State?.RW_Code;
				if (string.IsNullOrWhiteSpace(state))
				{
					return true;
				}
			}

			return false;
		}

		bool CheckEmptyStateForFixedPlaceOfSupply()
		{
			if (AllowValidateEmptyState() && AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.Value && InvoicingLine.GenericJobObject?.InvoicingSupporter?.FixedPlaceOfSupply != null)
			{
				var state = InvoicingLine.GenericJobObject.InvoicingSupporter.FixedPlaceOfSupply.State?.RW_Code;
				if (string.IsNullOrWhiteSpace(state))
				{
					return true;
				}
			}

			return false;
		}

		bool CheckEmptyStateForOrganization()
		{
			return AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value
				&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.India
				&& InvoicingLine.IsGSTMandatory
				&& InvoicingLine.InvoiceBase?.Header != null && string.IsNullOrWhiteSpace(InvoicingLine.InvoiceBase.Header.MainAddress?.OA_State);
		}

		bool AllowValidateEmptyState()
		{
			return AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value
				&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.India
				&& InvoicingLine.GenericJobObject != null
				&& InvoicingLine.GenericJobObject.Consumer != null
				&& InvoicingLine.ChargeCode != null
				&& !InvoicingLine.ChargeCode.IsComment
				&& InvoicingLine.IsGSTMandatory;
		}

		#endregion

		#region Validating GST & WHT

		protected OrgHeader CurrentOrganisation
		{
			get
			{
				if (InvoicingLine.InvoiceBase != null && InvoicingLine.InvoiceBase.Header != null)
				{
					return InvoicingLine.InvoiceBase.Header;
				}
				return null;
			}
		}

		protected bool AllowUserToModifyGstId
		{
			get
			{
				return (InvoicingLine.InvoiceBase.AH_Ledger == LedgerTypes.AccountsReceivable ?
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK) :
					AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		protected bool AllowUserToModifyWhtId
		{
			get
			{
				return (InvoicingLine.InvoiceBase.AH_Ledger == LedgerTypes.AccountsReceivable ?
					AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyWHTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK) :
					AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyWHTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		#endregion

		#region GSTInclusiveAmount

		public void ValidateGSTInclusiveAmount()
		{
			ValidateCalculatedProperty(InvoicingLine.GSTInclusiveAmountInfo);
		}

		protected virtual void CheckGSTInclusiveAmount()
		{
			TypeValidation.CheckValidDecimal(InvoicingLine.GSTInclusiveAmountInfo, 19, 4);
			if (!InvoicingLine.GSTInclusiveAmountInfo.HasErrors() &&
				InvoicingLine.InvoiceBase != null && InvoicingLine.InvoiceBase.GSTInclusiveAmounts &&
				InvoicingLine.AL_OSExTaxAmount + InvoicingLine.AL_OSTaxAmount != InvoicingLine.GSTInclusiveAmount)
			{
				InvoicingLine.GSTInclusiveAmountInfo.AddError(Res.GetString("a4091fd8-8d17-402d-a391-ca4a470c8821", "GST Inclusive Amount must be equal to Amount + Tax."));
			}
		}

		#endregion

		#region ValidateConsolCostThisLineWasApportionedFrom

		protected void ValidateConsolCostThisLineWasApportionedFrom()
		{
			var appCharge = InvoicingLine.ApportionmentChargeImportedFrom;
			if (appCharge == null || appCharge.IsDeleted || appCharge.ParentConsolCost == null || InvoicingLine.InvoiceBase == null)
			{
				return;
			}

			var parentConsolCost = appCharge.ParentConsolCost;
			InvoicingLine.InvoiceBase.ValidateConsolCostIfRequired(parentConsolCost);

			if (parentConsolCost.HasErrors)
			{
				InvoicingLine.AddRowError(GetImportedFromApportionmentChargeWithErrorsMessage(parentConsolCost));
			}
			else
			{
				InvoicingLine.RemoveRowError(GetImportedFromApportionmentChargeWithErrorsMessage(parentConsolCost), containing: true);
			}

			if (parentConsolCost.HasWarnings)
			{
				InvoicingLine.AddRowWarning(ImportedFromApportionmentChargeWithWarningsMessage);
			}
			else
			{
				InvoicingLine.RemoveRowWarning(ImportedFromApportionmentChargeWithWarningsMessage);
			}
		}

		#endregion

		#region Error Messages

		static string ChargeCodeIsNoLongerValidWarning
		{
			get { return Res.GetString("5d85c7c2-6056-4a21-b3b7-2a527c752a91", "Charge code is no longer valid."); }
		}
		static string EnforceGSTAmountEntry
		{
			get { return Res.GetString("b0ebc97a-41e5-4921-8985-592272de7341", "You must enter a tax amount"); }
		}

		static string TaxAmountRangeWarning
		{
			get { return Res.GetString("669c482a-7901-41f7-a522-f3a9c7356a48", "Tax amount entered is outside the expected value for the selected tax rate"); }
		}
		static string AmountExcludingTaxError
		{
			get { return Res.GetString("d3c496fd-f23d-426f-8511-71cd5346c936", "Please enter an Amount Excluding Tax."); }
		}

		static string MiscDeptAndJobNotAllowedError
		{
			get { return Res.GetString("ad8e09b7-2fe5-45e6-918e-ee2765097d8b", "Cannot issue job charges for a miscellaneous department."); }
		}

		public static string ImportedFromApportionmentChargeStartingErrorsMessage
		{
			get { return ResString.GetMultilingualString("1ec6d71a-79c1-4fe1-980f-104d4fbd1664", "The related consol cost is invalid, please fix the following errors in the consol cost from which this line was apportioned:"); }
		}

		public string GetImportedFromApportionmentChargeWithErrorsMessage(JobConsolCost consolCost)
		{
			var result = new StringBuilder();
			result.AppendLine(ImportedFromApportionmentChargeStartingErrorsMessage);
			result.AppendLine(string.Join("\r\n", new ZNotificationCollector(consolCost, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors().GetUniqueMessageList()));
			return result.ToString();
		}

		public static string ImportedFromApportionmentChargeWithWarningsMessage
		{
			get { return ResString.GetMultilingualString("e3e169c1-74fc-4932-83cb-09d7d232ef49", "The related consol cost this line was apportioned from has warnings that should be reviewed before saving."); }
		}

		public static string ReopenClosedJobSecurityMessage
		{
			get
			{
				return string.Format("{0} {1}",
					Res.GetString("80e79f71-95d5-416c-8724-af1a8e1825eb", "This job is currently closed."),
					AccountingConstants.ReopenClosedJobSecurityMessages.ErrorMessage);
			}
		}

		public static string ReopenClosedJobWarningMessage
		{
			get
			{
				return string.Format("{0} {1}",
					Res.GetString("e24e42a1-c6de-4b7f-a1d7-01f581f7c2ff", "This job is currently closed."),
					AccountingConstants.ReopenClosedJobSecurityMessages.WarningMessage);
			}
		}

		#endregion
	}
}

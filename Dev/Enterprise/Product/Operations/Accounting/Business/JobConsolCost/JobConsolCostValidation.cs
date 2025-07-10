using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Validation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public class JobConsolCostValidation : AutoJobConsolCostValidation
	{
		public JobConsolCostValidation(AutoJobConsolCost parent)
			: base(parent)
		{
			JobConsolCost = parent as JobConsolCost;
		}

		readonly JobConsolCost JobConsolCost;

		#region E6_AC_ChargeCode

		protected override void CheckE6_AC_ChargeCode()
		{
			base.CheckE6_AC_ChargeCode();

			if (Parent.ChargeCode != null && !Parent.ChargeCode.AC_IsGroupageCharge)
			{
				var msg = Res.GetString(
					"fd4fbb8f-4a57-11e6-9f5b-fcaa14295823",
					"The {0} code is not a Consol Level Charge. Creditor only defaults for Consol Level charges.", Parent.ChargeCode.AC_Code);

				Parent.E6_AC_ChargeCodeInfo.AddWarning(msg);
			}

			foreach (ApportionSplitCharge charge in JobConsolCost.ApportionmentCharges)
			{
				if (charge.IsRevenuePosted && charge.ChargeCode.PK != Parent.E6_AC_ChargeCode)
				{
					Parent.E6_AC_ChargeCodeInfo.AddError(Res.GetString("4645f831-8918-4f4a-8e1b-8122981564f0",
						"The revenue on one or more of the apportioned charges is flagged as posted. You cannot change the charge code on this consol cost. Please change the value back to the original value of '{0}'.",
						charge.ChargeCode.AC_Code));
				}
			}
		}

		#endregion

		#region IsFinal

		public void ValidateIsFinal()
		{
			ValidateCalculatedProperty(JobConsolCost.IsFinalInfo);
		}

		protected virtual void CheckIsFinal()
		{
			if (JobConsolCost.IsFinal && !Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed)
			{
				JobConsolCost.IsFinalInfo.AddError(Res.GetString("F1190A39-0C9D-43A1-BC07-A5FE33376018", @"You do not have appropriate security rights to tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Tick Final Flag on Payables Invoice"));
			}
		}

		#endregion

		#region PlaceOfSupply

		protected override void CheckE6_PlaceOfSupply()
		{
			base.CheckE6_PlaceOfSupply();

			if (!Parent.E6_PlaceOfSupply.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.E6_PlaceOfSupplyInfo);
			}
			else if (!Parent.E6_PlaceOfSupplyType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.E6_PlaceOfSupplyInfo);
			}
		}

		protected override void CheckE6_PlaceOfSupplyType()
		{
			base.CheckE6_PlaceOfSupplyType();

			if (!Parent.E6_PlaceOfSupplyType.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.E6_PlaceOfSupplyTypeInfo);
			}
			else if (!Parent.E6_PlaceOfSupply.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.E6_PlaceOfSupplyTypeInfo);
			}
		}

		#endregion

		#region GSTInclusiveAmount

		public void ValidateGSTInclusiveAmount()
		{
			ValidateCalculatedProperty(JobConsolCost.GSTInclusiveAmountInfo);
		}

		protected virtual void CheckGSTInclusiveAmount()
		{
			TypeValidation.CheckValidDecimal(JobConsolCost.GSTInclusiveAmountInfo, 19, 4);
			if (!JobConsolCost.GSTInclusiveAmountInfo.HasErrors() &&
				JobConsolCost.IsGSTInclusiveAmount &&
				JobConsolCost.E6_OSCostAmount + JobConsolCost.E6_OSGSTAmount_Calc != JobConsolCost.GSTInclusiveAmount)
			{
				JobConsolCost.GSTInclusiveAmountInfo.AddError(Res.GetString("4455dd28-99f3-4fae-b617-5ad2d04e2c58", "GST Inclusive Amount must be equal to OS Cost Amount + OS GST Amount."));
			}
		}

		#endregion

		#region CostGovtChargeCode

		public void ValidateE6_CostGovtChargeCode()
		{
			ValidateCalculatedProperty(JobConsolCost.E6_CostGovtChargeCodeInfo);
		}

		protected void CheckE6_CostGovtChargeCode()
		{
			if (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value && !JobConsolCost.IsPosted)
			{
				if (IsGovernmentChargeCodeMandatory)
				{
					MandatoryValidation.CheckEntered(JobConsolCost.E6_CostGovtChargeCodeInfo);
				}
				else
				{
					if (JobConsolCost.E6_CostGovtChargeCode.IsEmpty)
					{
						JobConsolCost.E6_CostGovtChargeCodeInfo.AddWarning(Res.GetString("026753ee-08da-401c-9384-dd8cf9880dd5", "Cost Government Charge Code is empty."));
					}
				}
			}
		}

		#endregion

		#region SellGovtChargeCode

		public void ValidateE6_SellGovtChargeCode()
		{
			ValidateCalculatedProperty(JobConsolCost.E6_SellGovtChargeCodeInfo);
		}

		protected void CheckE6_SellGovtChargeCode()
		{
			if (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value && !JobConsolCost.IsRevenuePosted && !JobConsolCost.IsPosted)
			{
				if (IsGovernmentChargeCodeMandatory)
				{
					MandatoryValidation.CheckEntered(JobConsolCost.E6_SellGovtChargeCodeInfo);
				}
				else
				{
					if (JobConsolCost.E6_SellGovtChargeCode.IsEmpty)
					{
						JobConsolCost.E6_SellGovtChargeCodeInfo.AddWarning(Res.GetString("83d87c77-7cb9-467b-b16d-bcd9e2a3f148", "Sell Government Charge Code is empty."));
					}
				}
			}
		}

		#endregion

		bool IsGovernmentChargeCodeMandatory => JobConsolCost.TaxRate != null && !JobConsolCost.TaxRate.IsIndiaServiceTax;

		#region Withholding tax

		public void ValidateE6_AW()
		{
			ValidateCalculatedProperty(JobConsolCost.E6_AWInfo);
		}

		#endregion

		#region Overriden Methods

		protected override void CheckE6_AB_BankAccount()
		{
			base.CheckE6_AB_BankAccount();
			if (Parent.BankAccount != null)
			{
				if (Parent.BankAccount.AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.EPA && Parent.E6_PaymentType != ReceiptTypes.EPayment)
				{
					Parent.E6_AB_BankAccountInfo.AddError(Res.GetString("26e38ea4-d284-4f33-885c-e473956ab25c", "This bank account is an E-Payment Account. Please set Payment Type to EPA - E-Payment."));
				}
				else if (Parent.BankAccount.AB_AccountType != AccountTypeCodeDescriptionPairList.Codes.EPA && Parent.E6_PaymentType == ReceiptTypes.EPayment)
				{
					Parent.E6_AB_BankAccountInfo.AddError(Res.GetString("02794c08-f6b7-4bdf-8bf1-877b495c333e", "Bank Account is not an E-Payment Account."));
				}
			}
		}

		protected override void CheckE6_OH_Creditor()
		{
			base.CheckE6_OH_Creditor();
			if (Parent.E6_OH_Creditor.IsValid && Parent.E6_PaymentType == ReceiptTypes.eNettCreditCard
				&& Parent.Creditor.ENettRegistrationNumber.IsEmpty)
			{
				Parent.E6_OH_CreditorInfo.AddError(AccountingConstants.ENettErrorMessages.OrganisationNotRegisteredForENett);
			}

			var checkDate = Parent.E6_InvoiceDate.IsEmpty ? ZDateTime.Now : Parent.E6_InvoiceDate;
			var (warnings, _) = ExporterExemptionValidationHelper.CheckExporterExemption(Parent.Creditor, LedgerTypes.AccountsPayable, checkDate);
			if (!warnings.IsEmpty)
			{
				Parent.E6_OH_CreditorInfo.AddWarning(warnings);
			}
		}

		protected override void CheckE6_RX_NKCurrency()
		{
			base.CheckE6_RX_NKCurrency();
			MandatoryValidation.CheckEntered(Parent.E6_RX_NKCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.E6_RX_NKCurrencyInfo);
		}

		protected override void CheckE6_AK_ChequeBook()
		{
			base.CheckE6_AK_ChequeBook();
			if (Parent.ChequeBook != null)
			{
				Parent.ChequeBook.AddWarningSamePrinter(Parent.E6_AK_ChequeBookInfo);
			}
		}

		protected override void CheckE6_PaymentType()
		{
			base.CheckE6_PaymentType();
			if (!JobConsolCost.IsPosted && !Parent.E6_PaymentType.IsEmpty && !JobConsolCost.MatchedWithTNFJournalNum.IsEmpty)
			{
				Parent.E6_PaymentTypeInfo.AddError(Res.GetString("7a9082b9-e8c9-474e-b587-d320fa5c3652", "Payment details cannot be entered as this AP Invoice Number matches an unpaid �Carried Forward� journal�s payment reference. When this invoice is posted, it will automatically be matched against the journal, up to the value of the invoice. The unpaid journal transaction number is [{0}].", JobConsolCost.MatchedWithTNFJournalNum));
			}
			if (Parent.E6_PaymentType == ReceiptTypes.eNettCreditCard && !AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.Value)
			{
				Parent.E6_PaymentTypeInfo.AddError(Res.GetString("9fcc6917-7dd8-4734-9a7f-6fcb731aaee8", "'Pay via ComPay Credit Card' payment type is not enabled."));
			}
			if (Parent.E6_PaymentType == ReceiptTypes.EPayment)
			{
				Parent.E6_PaymentTypeInfo.AddError(Res.GetString("37b112c5-0e47-42f0-8ffb-401fec083a2d", "To process E-Payments, please create a Payment or Payment Batch in the Payables Transactions module or a Payment Approval in the Payment Processing module."));
			}
			if (Parent.E6_PaymentType != ReceiptTypes.Cash && (Parent.BankAccount?.IsCashAccount ?? false))
			{
				Parent.E6_PaymentTypeInfo.AddError(TransactionHeaderValidation.GetCashAccountTypeErrorMessage(Parent.E6_PaymentTypeInfo.HumanReadableName));
			}
		}

		protected override void CheckE6_LocalCostAmount()
		{
			base.CheckE6_LocalCostAmount();
			CheckE6_LocalCostAmountDecimalPlaces();
		}

		void CheckE6_LocalCostAmountDecimalPlaces()
		{
			int currencyDecimals = GlbCompany.CurrentCompany.LocalCurrency.Decimals;
			int localCostDecimals = Parent.E6_LocalCostAmount.DecimalPlaces;
			if (localCostDecimals > currencyDecimals)
			{
				Parent.E6_LocalCostAmountInfo.AddError(Res.GetString("96f2af83-b431-430b-8397-c1a732697432", "You have entered {0} decimal places for the local cost amount. The {1} currency only allows entering amounts up to {2} decimal places."
					, localCostDecimals, GlbCompany.CurrentCompany.LocalCurrency.RX_Code, currencyDecimals));
			}
		}

		protected override void CheckE6_ApportionmentMethod()
		{
			base.CheckE6_ApportionmentMethod();
			MandatoryValidation.CheckEntered(Parent.E6_ApportionmentMethodInfo);
			ListValidation.ErrorIfInvalidCode(Parent.E6_ApportionmentMethodInfo);

			CheckCapacityPerContainerApportionmentIfRequired();
		}

		void CheckCapacityPerContainerApportionmentIfRequired()
		{
			if (Parent.E6_ApportionmentMethod == AllocationMethod.CapacityPerContainer)
			{
				var paymentBases = JobConsolCost.PaymentBases.Cast<IJobPaymentBasis>();
				if (!paymentBases.Any())
				{
					Parent.E6_ApportionmentMethodInfo.AddWarning(Res.GetString("333d2824-7ea5-4f69-9791-fed2b4503513", @"Apportionment by Capacity Per Container method cannot be performed as no calculation data exist for this cost. {0}", AutoRatingRunner.CalculationXMLDoesNotExistReason));
				}
				else
				{
					var consol = JobConsolCost.Consol as ForwardingConsol;
					if (consol != null && ConsolCostApportionmentHelper.ContainersSetupHasChanged(paymentBases, consol))
					{
						Parent.E6_ApportionmentMethodInfo.AddWarning(Res.GetString("3c117562-a91f-4226-8a8f-903f2961aead", @"Apportionment by Capacity Per Container cannot be accurately performed due to one of the following reasons:
 - Container setup has changed since AutoRating
 - Unpacked Container included in the Cost Rate Audit
 - Cost Rate Audit shows the rate is not applied per Container"));
					}
				}
			}
		}

		protected sealed override void CheckE6_OSGSTAmount()
		{
			base.CheckE6_OSGSTAmount();

			//CheckE6_OSGSTAmount_Calc should be used instead.
		}

		public sealed override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			ValidateAllCore();
		}

		protected virtual void ValidateAllCore()
		{
			base.ValidateAll();
			ValidateGSTInclusiveAmount();
			ValidateIsFinal();
			ValidateUnApportionedAmount();
			ValidateE6_CostGovtChargeCode();
			ValidateE6_SellGovtChargeCode();
			ValidateE6_OSGSTAmount_Calc();
			CheckHasCurrencyExchangeRate();
		}

		public virtual void ValidateE6_OSGSTAmount_Calc()
		{
			ValidateCalculatedProperty(JobConsolCost.E6_OSGSTAmount_CalcInfo);
		}

		protected void CheckE6_OSGSTAmount_Calc()
		{
			if (!Parent.Factory.HasContext(BusinessContext.WarningOnlyValidation))
			{
				CheckE6_OSGSTAmount_CalcError();
			}

			CheckE6_OSGSTAmount_CalcWarning();
		}

		protected virtual void CheckE6_OSGSTAmount_CalcError()
		{
			TypeValidation.CheckValidMoney(JobConsolCost.E6_OSGSTAmount_CalcInfo, 19, 4);
		}

		protected virtual void CheckE6_OSGSTAmount_CalcWarning()
		{
			if (!JobConsolCost.E6_IsTaxAmountOverridden && !JobConsolCost.IsPosted && JobConsolCost.HasUnApportionedGSTAmount())
			{
				JobConsolCost.E6_OSGSTAmount_CalcInfo.AddWarning(Res.GetString("fe2fd25a-e6bd-4e8d-80d1-bdfeee644448", "The charge value is not precise and for reference only and will be recalculated during posting with higher precision."));
			}
			else if (JobConsolCost.E6_IsTaxAmountOverridden && !JobConsolCost.IsAPInvoiceConsolCost && !JobConsolCost.IsPosted && !JobConsolCost.IsGSTAmountEqualToCalculatedGST)
			{
				JobConsolCost.E6_OSGSTAmount_CalcInfo.AddWarning(Res.GetString("9db8f31e-302a-4b83-bdbf-6fd2d7f23a06", "This value is different to default tax amount, make sure that this value is correct for invoice posting."));
			}
		}

		public virtual void ValidateUnApportionedAmount()
		{
			ValidateCalculatedProperty(JobConsolCost.UnApportionedAmountInfo);
		}

		protected virtual void CheckUnApportionedAmount()
		{
			if (!JobConsolCost.UnApportionedAmount.IsEmpty)
			{
				JobConsolCost.UnApportionedAmountInfo.AddError(Res.GetString("eee8463e-14d2-4aab-b347-7592f7755cac", "Please ensure that this Cost Amount is fully apportioned."));
			}

			if (JobConsolCost.UnApportionedAmountInfo.HasErrors() && JobConsolCost.ShouldBeReadOnlyWhenPosted)
			{
				JobConsolCost.UnApportionedAmountInfo.AddError(Res.GetString("758d6e78-84a4-4f79-b79d-92471894d476", "Please close the form and try again."));
			}

			if (!JobConsolCost.IsPosted && JobConsolCost.E6_ApportionmentMethod != AllocationMethod.Manual && AccountingConfigurationRegistry.Instance.CheckDifferentSignsWhenApportionConsolCost.Value)
			{
				var consolCostSign = Math.Sign(JobConsolCost.E6_OSCostAmount);
				var otherSignCharges = from ApportionSplitCharge charge in JobConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>() where !charge.JR_OSCostAmt.IsEmpty && Math.Sign(charge.JR_OSCostAmt) != consolCostSign select charge;

				if (otherSignCharges.Any())
				{
					JobConsolCost.UnApportionedAmountInfo.AddError(Res.GetString("BD8507C0-385B-411D-9C2C-91AB34163BFD", "Using different signs on Amounts is allowed only for MAN apportionment method."));
				}
			}
		}

		public static string JobChargesWithoutShipmentsOnConsol
		{
			get { return Res.GetString("9478f61b-42b3-4561-bb1c-307802fdbd21", "Some shipments for costs on this consol have apportioned costs, but are no longer attached to this consol.\r\n\r\nReview the following list of shipments. You will need to reattach those shipments indicated and review the costs:") + "\r\n"; }
		}

		protected override void CheckE6_TaxDate()
		{
			base.CheckE6_TaxDate();

			if (JobConsolCost.TaxRate == null || JobConsolCost.IsPosted)
			{
				return;
			}

			var rateExists = JobConsolCost.TaxRate.DoesRateExists(JobConsolCost.E6_TaxDate);
			if (!rateExists)
			{
				JobConsolCost.E6_TaxDateInfo.AddError(Res.GetString("74c8a742-92f1-4db6-88c4-c57bef8d2193", "No rate found for selected date."));
			}
		}

		protected override void CheckE6_A9_VATClass()
		{
			base.CheckE6_A9_VATClass();

			if (!JobConsolCost.IsPosted)
			{
				var errorMessage = new TaxIdAndTaxMessageMappingHelper().ValidateMapping(TransactionLineTypes.Cost, JobConsolCost.TaxRate, JobConsolCost.VATClass);
				if (errorMessage != null)
				{
					JobConsolCost.E6_A9_VATClassInfo.AddError(errorMessage);
				}
			}
		}

		protected override void CheckE6_SupplyType()
		{
			base.CheckE6_SupplyType();

			if (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value && !JobConsolCost.IsPosted)
			{
				var shouldShowError = Parent.Factory.HasContext(BusinessContext.APInvoiceApportionToConsol);

				if (shouldShowError && AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.Value)
				{
					MandatoryValidation.CheckEntered(Parent.E6_SupplyTypeInfo);
				}
				else if (Parent.E6_SupplyType.IsEmpty)
				{
					Parent.E6_SupplyTypeInfo.AddWarning(Res.GetString("8E3DD14E-85F3-48C3-B998-B2A777F17E40", "The Cost Supply Type is not specified. Please check if a supply type is needed before posting."));
				}

				if (JobConsolCost.ApportionmentCharges.OfType<ApportionSplitCharge>().Any(x => x.JR_CostSupplyType != JobConsolCost.E6_SupplyType))
				{
					Parent.E6_SupplyTypeInfo.AddError(Res.GetString("2F3D1698-B2EF-4016-BB0D-E0240F045486", "The Cost Supply Type value of all apportioned charges must be the same. Please re-enter the Consol Cost's Cost Supply Type to update the apportioned charges."));
				}

				ListValidation.ErrorIfInvalidCode(Parent.E6_SupplyTypeInfo, JobConsolCost.Lookups.SupplyTypes);
			}
		}

		protected override void CheckE6_GB_CostTaxBranch()
		{
			base.CheckE6_GB_CostTaxBranch();

			if (!JobConsolCost.E6_GB_CostTaxBranchInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.E6_GB_CostTaxBranchInfo);
				ListValidation.ErrorIfInvalidPK(Parent.E6_GB_CostTaxBranchInfo);

				if (!Parent.E6_GB_CostTaxBranchInfo.HasErrors()
					&& JobConsolCost.ApportionmentCharges.OfType<ApportionSplitCharge>().Any(x => x.JR_GB_CostTaxBranch != JobConsolCost.E6_GB_CostTaxBranch))
				{
					Parent.E6_GB_CostTaxBranchInfo.AddError(Res.GetString("A951552C-FC85-4032-8B2C-37FB12C8AF26", "The Cost Tax Branch value of all apportioned charges must be the same. Please re-enter the Consol Cost's Tax Branch to update the apportioned charges."));
				}

				if (!Parent.E6_GB_CostTaxBranchInfo.HasErrors() && !JobConsolCost.E6_InvoiceNum.IsEmpty && JobConsolCost.E6_OH_Creditor.IsValid)
				{
					var costsCollection = (JobConsolCost.Consol as ForwardingConsol)?.GetApportionments()?.CostsCollection;
					if (costsCollection != null && costsCollection.Find(x => !x.IsPosted
						&& x.E6_InvoiceNum == JobConsolCost.E6_InvoiceNum
						&& x.E6_OH_Creditor == JobConsolCost.E6_OH_Creditor
						&& x.E6_GB_CostTaxBranch != JobConsolCost.E6_GB_CostTaxBranch).Any())
					{
						Parent.E6_GB_CostTaxBranchInfo.AddError(Res.GetString("A3F9B42C-76FA-47C6-8535-3F8E94E26587", "All cost lines with the same Creditor and AP Invoice Number must have the same Cost Tax Branch."));
					}
				}
			}

			if (!Parent.E6_GB_CostTaxBranchInfo.HasErrors() && !JobConsolCost.IsPosted && !JobConsolCost.IsCostTaxBranchActual)
			{
				Parent.E6_GB_CostTaxBranchInfo.AddError(AccountingConstants.TaxBranchConflictErrorMessageWithSuggestion);
			}
		}

		#endregion

		#region ExchangeRates

		void CheckHasCurrencyExchangeRate()
		{
			if (AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.GetFallBackValueAtAllLevels(JobConsolCost.E6_GC.IsEmpty ? Guid.Empty : JobConsolCost.E6_GC.ToGuid(), Guid.Empty, Guid.Empty))
			{
				foreach (var shipment in JobConsolCost.ShipmentsToApportion)
				{
					var job = JobConsolCost.GetJob(shipment);

					if (job != null)
					{
						Parent.ClearRowNotificationsContaining(AccountingUtils.GetElectronicProcessingChargeCurrencyNoExchangeRateErrorMessage(job));
						if (!ObjectFactory.Get<IElectronicProcessingChargeProvider>().HasElectronicProcessingChargeCurrencyExchangeRate(job))
						{
							Parent.AddRowError(AccountingUtils.GetElectronicProcessingChargeCurrencyNoExchangeRateErrorMessage(job));
							break;
						}
					}
				}
			}
		}

		#endregion
	}
}

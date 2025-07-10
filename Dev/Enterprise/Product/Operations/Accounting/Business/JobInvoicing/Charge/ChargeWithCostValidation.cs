using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Validation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public abstract partial class ChargeWithCostValidation : BaseChargeValidation
	{
		public ChargeWithCostValidation(ChargeWithCost parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		protected new ChargeWithCost Parent;

		#region JR_AC

		protected override void CheckJR_AC()
		{
			base.CheckJR_AC();
			if (Parent.InvoicingJob != null
				&& Parent.JR_AC.IsValid
				&& !Parent.JR_IsRevenuePosted
				&& Parent.InvoicingJob.HasSameRelatedJobNumberAndSellReferenceNumberMoreThanOnce(Parent))
			{
				var emptyPlaceholder = Res.GetString("d7054966-d275-43d9-a539-2b0f40f90b12", "<empty>");

				var warningMessage = Parent.Job.IsGatewayBillingJob()
					? Res.GetString("84aea643-7176-4280-901d-8db55e9e3597", @"There is more than one charge using this charge code for the same Related Job Number: {0}.
Please confirm that the selection is valid.", Parent.JR_Calc_RelatedJobNumber.IsEmpty ? emptyPlaceholder : (string)Parent.JR_Calc_RelatedJobNumber)
					: Res.GetString("6056217e-9f4b-4309-a29d-74f0c76524d5", @"There is more than one charge using this charge code for the same Sell Reference Number: {0}.
You are advised to review these charges before posting.", Parent.JR_SellReference.IsEmpty ? emptyPlaceholder : (string)Parent.JR_SellReference);

				Parent.JR_ACInfo.AddWarning(warningMessage);
			}
			ValidateJR_AK();
		}

		#endregion

		#region JR_Desc

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		protected override void CheckJR_Desc()
		{
			base.CheckJR_Desc();
			if (!Parent.JR_IsRevenuePosted || !Parent.JR_IsCostPosted)
			{
				if (Parent.JR_Desc.IsEmpty)
				{
					Parent.JR_DescInfo.AddError(Res.GetString("23cd83bc-72ea-4703-b433-374eea5e5e60", "Description cannot be empty."));
				}
				else if (!Parent.JR_DescInfo.ReadOnly && Parent.JR_Desc.IndexOf(RatingConstants.RateNotePrefix) > -1)
				{
					Parent.JR_DescInfo.AddError(Res.GetString("5617feaf-49af-4130-8c1c-c248cad197bb", "A Rate Note was added to your invoice. You have two choices:\r\n- modify the description and enter the appropriate amount (see description for details), or\r\n- remove the charge if it doesn't apply."));
				}
				else if (!AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.Value && Parent.ChargeCode != null && !Parent.JR_Desc.StartsWith(Parent.ChargeCode.AC_Desc, StringComparison.OrdinalIgnoreCase))
				{
					Parent.JR_DescInfo.AddWarning(Res.GetString("2a58d339-40d0-49dc-84a7-2773b4a550ea", "Charge description was changed from default. This description will appear on AR Invoice without translation."));
				}
				else if (!Parent.JR_DescInfo.ReadOnly
					&& Parent.Company.GC_RN_NKCountryCode == CountryCodes.India
					&& Parent.JR_Desc.TrimEnd().Length < 3)
				{
					Parent.JR_DescInfo.AddError(Res.GetString("fa2e6c35-9aae-490b-8b09-2bbd82c3c92f", "Charge description has less than 3 characters."));
				}
			}
		}

		#endregion

		#region JR_OH_SellAccount

		protected override void CheckJR_OH_SellAccount()
		{
			base.CheckJR_OH_SellAccount();
			if (!Parent.JR_IsRevenuePosted)
			{
				if (Parent.Factory.HasContext(BusinessContext.JobChargeInvalidDebtorWarningInsteadOfError) && Parent.JR_OH_SellAccount != ZGuid.Invalid && Parent.JR_OH_SellAccount != ZGuid.Missing)
				{
					ListValidation.WarnIfInvalidPK(Parent.JR_OH_SellAccountInfo, Parent.Debtors);
				}
				else
				{
					ListValidation.ErrorIfInvalidPK(Parent.JR_OH_SellAccountInfo, Parent.Debtors);
				}

				var sellAccount = Parent.SellAccount;
				if (sellAccount != null)
				{
					var jobIsGateway = Parent.Job?.IsGatewayBillingJob() ?? false;
					if (Parent.Job != null
						&& !jobIsGateway
						&& sellAccount != Parent.Job.AgentCollect
						&& sellAccount != Parent.Job.LocalCharges
						&& !Parent.InvoicingJob.CanCrossTradeDebtorDefaultingBeApplied)
					{
						Parent.JR_OH_SellAccountInfo.AddWarning(Res.GetString("7b02161f-4336-41b0-b863-c5a8db3164f6", "You have selected a Debtor that is neither your Local Client or your Overseas Agent.\r\nWhilst invoicing any party is valid, this should be confirmed."));
					}
					else if (jobIsGateway && !sellAccount.IsProxyOrgOfAnyCompany())
					{
						Parent.JR_OH_SellAccountInfo.AddWarning(Res.GetString("1df7016c-3703-402b-b7f7-6890795d4cdc", "You have selected a Debtor that is not an Organization Proxy of any sister company or branch. Generally, gateway agents perform services for other branches in their own network and not third party customers. Please confirm that the selection is valid."));
					}

					if (!sellAccount.OH_Code.IsEmpty && sellAccount.OH_IsDebtor)
					{
						sellAccount.CreditChecker.ValidateIsCreditLimitExceeded(Parent.JR_OH_SellAccountInfo, LedgerTypes.AccountsReceivable);
					}
				}
			}
		}

		#endregion

		#region CheckJR_OH_CostAccount

		protected override void CheckJR_OH_CostAccount()
		{
			base.CheckJR_OH_CostAccount();
			if (Parent.JR_OH_CostAccount.IsValid && !Parent.JR_OH_CostAccountInfo.HasErrors())
			{
				ValidateJR_APInvoiceNum();
				ValidateJR_APInvoiceDate();
				ValidateJR_APDocumentReceivedDate();
				ValidateJR_PaymentDate();
				ValidateJR_CostReference();
				ValidateJR_AB();
				ValidateJR_PaymentType();
				ValidateJR_ChequeNo();
				Parent.RefreshBinding();
			}
		}

		#endregion

		#region JR_LocalSellAmt

		protected override void CheckJR_LocalSellAmt()
		{
			base.CheckJR_LocalSellAmt();
			CheckMatchingSignsForSellAmounts(Parent.JR_LocalSellAmtInfo);
		}

		#endregion

		#region JR_LocalCostAmt

		protected override void CheckJR_LocalCostAmt()
		{
			base.CheckJR_LocalCostAmt();
			CheckMatchingSignsForCostAmounts(Parent.JR_LocalCostAmtInfo);
		}

		#endregion

		#region JR_RX_NKCostCurrency

		protected override void CheckJR_RX_NKCostCurrency()
		{
			base.CheckJR_RX_NKCostCurrency();
			RunCostCurrencyValidation();
		}

		protected virtual void RunCostCurrencyValidation()
		{
			if (!Parent.IsRevenueCharge)
			{
				MandatoryValidation.CheckEntered(Parent.JR_RX_NKCostCurrencyInfo);
				ListValidation.ErrorIfInvalidCode(Parent.JR_RX_NKCostCurrencyInfo, Parent.Currencies);
			}
			if (Parent.JR_APInvoiceNum.Trim() != ZString.Empty && Parent.JR_OH_CostAccount.IsValid && !Parent.IsCostPosted)
			{
				BaseCharge previousCharge = GetChargeForSameInvoiceWithUnequalColumn(JobChargeSchema.JR_RX_NKCostCurrency);
				if (previousCharge != null)
				{
					Parent.JR_RX_NKCostCurrencyInfo.AddWarning(AccountingConstants.ChargeOrConsolCostIsNotNullForSameInvoiceWithUnequalColumnErrorMessage);
				}
			}
		}

		#endregion

		#region JR_OSCostAmt

		protected override void CheckJR_OSCostAmt()
		{
			base.CheckJR_OSCostAmt();

			bool aPInvoiceDetailsProvided = Parent.JR_APInvoiceDate.IsValid && !Parent.JR_APInvoiceNum.IsEmpty;
			bool aPCostsSelfBilled = (bool)(Parent.CostAccount?.CompanyData?.OB_APCostsSelfBilled ?? false);
			if (!Parent.JR_IsCostPosted &&
				Parent.JR_OSCostAmt < 0 &&
				AccountingConfigurationRegistry.Instance.NegativeCostValidationEnforced.Value &&
				!aPInvoiceDetailsProvided &&
				!aPCostsSelfBilled)
			{
				Parent.JR_OSCostAmtInfo.AddError(Res.GetString("9a526c0e-b6d3-479f-9044-c23968d2d2a2", "A negative cost can only be entered if you specify an AP Invoice Date and Invoice Number or if the Creditor is setup to issue self billing invoices (Organization -> A/P -> Configuration -> Issue Self Billing Invoice).\r\nNote that negative accruals are not created."));
			}
			if (!Parent.JR_IsCostPosted && Parent.JR_OSCostAmt < 0 && !Parent.JR_PaymentType.IsEmpty)
			{
				Parent.JR_OSCostAmtInfo.AddWarning(Res.GetString("1dbb5a08-3388-4dcb-acd5-8374a30f2e6f", "A negative invoice with bank details may create an AP Credit note, If Posting this charge creates an AP Credit Note, Bank details will be ignored"));
			}
			CheckMatchingSignsForCostAmounts(Parent.JR_OSCostAmtInfo);
		}

		#endregion

		#region JR_AT_CostGSTRate

		protected override void CheckJR_AT_CostGSTRate()
		{
			base.CheckJR_AT_CostGSTRate();
			if ((IsCostGSTRateMandatory && Parent.JR_AT_CostGSTRate.IsEmpty) || IsCostGSTRateDefinedButNotGSTRegistered)
			{
				Parent.JR_AT_CostGSTRateInfo.AddError(Res.GetString("6f010a6f-7bd0-4fc1-8531-bdc605a86c47", "Tax IDs on unposted charges conflict with the creditor \"Tax is Applicable\" flag.\r\nOne possible way to resolve this is to go into the \"Job Invoicing\" menu and click \"Reset Unposted lines Tax Default\" option."));
			}
		}

		bool IsCostGSTRateDefinedButNotGSTRegistered => Parent.IsCostButNotGSTRegistered && !Parent.IsCostPosted && !Parent.JR_AT_CostGSTRate.IsEmpty;

		protected virtual bool IsCostGSTRateMandatory
		{
			get { return Parent.IsCostGSTApplicable && !Parent.IsCostPosted; }
		}

		#endregion

		#region JR_APInvoiceNum

		protected override void CheckJR_APInvoiceNum()
		{
			base.CheckJR_APInvoiceNum();
			ValidateJR_RX_NKCostCurrency();
			if (!Parent.JR_IsCostPosted)
			{
				RunAPInvoiceNumberExistsValidation();
				if (!Parent.JR_APInvoiceNumInfo.HasErrors())
				{
					SecurityCheckpoint unapprovedInvoiceCheckPoint = new UnapprovedTransactionValidationHelper().GetSecurityCheckPoint(Parent.TotalLocalAmountWithGSTOnInvForJob);
					if (!unapprovedInvoiceCheckPoint.IsAllowed)
					{
						if (Parent.JR_PaymentType.IsEmpty)
						{
							Parent.JR_APInvoiceNumInfo.AddWarning(Res.GetString("ba908585-7aeb-442a-8913-dbbd6735df83", "You do not have security rights to post this invoice as an Accounts Payable Invoice. When you post this cost, it will be posted as an Un-Approved Invoice.\r\n\r\nThe Un-Approved Invoice must be approved by a user with the appropriate security rights."));
						}
						else
						{
							Parent.JR_APInvoiceNumInfo.AddWarning(UnapprovedInvoicesWithPaymentWarningMessage);
						}
					}
				}
			}
			if (!Parent.JR_APInvoiceNum.IsEmpty && !Parent.JR_APInvoiceNumInfo.HasErrors())
			{
				ValidateJR_APInvoiceDate();
				ValidateJR_APDocumentReceivedDate();
				ValidateJR_PaymentDate();
				ValidateJR_AB();
				ValidateJR_PaymentType();
				ValidateJR_CostReference();
				Parent.RefreshBinding();
			}
		}

		internal static string UnapprovedInvoicesWithPaymentWarningMessage
		{
			get
			{
				return Res.GetString("2D360DC7-69EE-4c4f-AF6C-F241756510F4", @"You are posting costs where some of the invoices being posted will be posted as unapproved.
Some of the charge lines that you are posting also contain Payment information.
{0} cannot proceed with posting where the job is in this state.
You should either:
 - Remove the payment details and organize for the payment to be posted through the Accounts Payable system.
   In this case, {0} will post an unapproved invoice where the total of the invoice is greater than your authorization settings allow.
 OR
 - Have another user with invoice posting authorization security rights that will allow them to post these charges as an actual Accounts Payable Invoice.
   In this case, {0} will also post the Payment.", Core.Constants.ProductName);
			}
		}

		protected virtual void RunAPInvoiceNumberExistsValidation()
		{
			if (!Parent.JR_APInvoiceNumInfo.ReadOnly)
			{
				if (!Parent.JR_APInvoiceNum.IsEmpty)
				{
					CheckCostAccountIsValid(Parent.JR_APInvoiceNumInfo, Res.GetString("c12f09f3-c3b4-48e9-935c-64139c233a40", "an AP Invoice number"));

					if (Parent.JR_OH_CostAccount.IsValid)
					{
						var numberCheckingDetails = AccountingUtils.APTransactionNumberExists(ZArchitecture.Core.TransactionTypes.Invoice, Parent.JR_APInvoiceNum, Parent.JR_OH_CostAccount.ToGuid(), Parent.JR_APInvoiceDate);
						if (numberCheckingDetails.HasNotification)
						{
							Parent.JR_APInvoiceNumInfo.AddNotification(numberCheckingDetails.NotificationType, numberCheckingDetails.NotificationMessage);
						}

						numberCheckingDetails = AccountingUtils.UATransactionNumberExists(ZArchitecture.Core.TransactionTypes.Invoice, Parent.JR_APInvoiceNum, Parent.JR_OH_CostAccount.ToGuid(), ZGuid.Empty, Parent.JR_APInvoiceDate);
						if (numberCheckingDetails.HasNotification)
						{
							Parent.JR_APInvoiceNumInfo.AddNotification(numberCheckingDetails.NotificationType, numberCheckingDetails.NotificationMessage);
						}

						if (!Parent.JR_IsApportioned && !Parent.JR_APInvoiceNum.IsEmpty)
						{
							if (!IsInvoiceNumberApplicable())
							{
								Parent.JR_APInvoiceNumInfo.AddError(InvoiceNumberErrorMessage);
							}

							if (!Parent.JR_APInvoiceNumInfo.HasErrors()
								&& AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.Value.EnableBranchLevelPosting
								&& ChargesForDifferentBranchHasSameInvoiceNumber)
							{
								Parent.JR_APInvoiceNumInfo.AddError(Res.GetString("7494af87-a6bc-4789-890e-68622377381a", @"Please review the charge lines entered and ensure all charges for each Payables invoice have been assigned branch from the same Posting Group. All charges posted in one transaction must be within the same Branch Posting Group.
Posting is prevented because charges for the same Invoice number have been entered using a mix of branch Posting Groups."));
							}
						}
					}
				}
			}
		}

		protected bool ChargesForDifferentBranchHasSameInvoiceNumber
		{
			get
			{
				var query = new ZQuery(JobChargeSchema.JR_APInvoiceNum, Parent.JR_APInvoiceNum);
				query.AddToFilter(JobChargeSchema.JR_GC, Parent.JR_GC);
				query.AddToFilter(JobChargeSchema.JR_OH_CostAccount, Parent.JR_OH_CostAccount);
				query.AddToFilter(JobChargeSchema.JR_JH, Parent.JR_JH);
				query.AddToFilter(JobChargeSchema.JR_GB, SQLComparisonOperator.NotEqual, BranchLevelPostingHelper.GetAssociatedBranches(AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting, Parent.JR_GB));

				return Parent.Factory.LoadTop1<Charge>(query) != null;
			}
		}

		protected abstract string InvoiceNumberErrorMessage { get; }

		protected bool IsInvoiceNumberApplicable()
		{
			return new BusinessObjectFactory().LoadTop1<Charge>(InvoiceNumberApplicableQuery) == null;
		}

		protected virtual ZQuery InvoiceNumberApplicableQuery
		{
			get
			{
				ZQuery query = new ZQuery(JobChargeSchema.JR_APInvoiceNum, Parent.JR_APInvoiceNum);
				query.AddToFilter(JobChargeSchema.JR_OH_CostAccount, Parent.JR_OH_CostAccount);
				List<ZGuid> currentCompanyBranches = new List<ZGuid>();
				foreach (GlbBranch branch in GlbCompany.CurrentCompany.Branches)
				{
					currentCompanyBranches.Add(branch.PK);
				}
				query.AddToFilter(JobChargeSchema.JR_GB, currentCompanyBranches);

				return query;
			}
		}

		#endregion

		#region JR_APInvoiceDate

		protected override void CheckJR_APInvoiceDate()
		{
			base.CheckJR_APInvoiceDate();
			RunInvoiceDateValidation();
		}

		protected virtual void RunInvoiceDateValidation()
		{
			if (!Parent.JR_APInvoiceDateInfo.ReadOnly)
			{
				if (!Parent.JR_APInvoiceNum.IsEmpty && Parent.JR_OH_CostAccount.IsValid)
				{
					if (!Parent.IsCustomsCharge)
					{
						MandatoryValidation.CheckEntered(Parent.JR_APInvoiceDateInfo);
					}

					BaseCharge previousCharge = GetChargeForSameInvoiceWithUnequalColumn(JobChargeSchema.JR_APInvoiceDate);
					if (previousCharge != null)
					{
						Parent.JR_APInvoiceDateInfo.AddError(GetDifferenceBetweenChargesForSameTransactionErrorMessage(Parent.JR_APInvoiceDateInfo, previousCharge.JR_APInvoiceDate.IsEmpty ? NullDescription : (ZString)previousCharge.JR_APInvoiceDate.ToShortDateString()));
					}
				}

				if (!Parent.JR_APInvoiceDate.IsEmpty)
				{
					CheckAPInvoiceNumIsEntered(Parent.JR_APInvoiceDateInfo, Res.GetString("7610acf2-4b24-4bfa-98ad-cb27e91de9cc", "an Invoice date is entered"));
					CheckCostAccountIsValid(Parent.JR_APInvoiceDateInfo, Res.GetString("7610acf2-4b24-4bfa-98ad-cb27e91de9cc", "an Invoice date is entered"));

					if (Parent.JR_APInvoiceDate.Date > ZDate.Today)
					{
						if (!AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.Value)
						{
							Parent.JR_APInvoiceDateInfo.AddError(
								Res.GetString("B303E66F-9448-4AE2-A888-3FF003DE6992",
											"Invoice date cannot be in the future.\r\nThis is determined by registry: {0}",
											((IRegistryItemInternals)AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate).Location));
						}
						else
						{
							CountrySpecificValidationHelper.AddWarningIfDateIsInTheFuture(Parent.JR_APInvoiceDateInfo);
						}
					}

					var chargeIsInLocalInvoiceCurrencyForPosting = Parent.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AP);
					if (!Parent.JR_APInvoiceDateInfo.HasErrors()
						&& Parent.JR_CostCurrency != Parent.Company.GC_RX_NKLocalCurrency
						&& ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AP, chargeIsInLocalInvoiceCurrencyForPosting, Parent.JR_GC))
					{
						var regItem = AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateRegistryItem(ExchangeRateValidLedgerEnum.AP);
						var invoiceCurrencyType = chargeIsInLocalInvoiceCurrencyForPosting ? Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local : Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;
						var exchangeRateOptionCode = regItem.Value.Cast<InvoicePostingExRateOption>().FirstOrDefault(x => x.InvoiceCurrencyType == invoiceCurrencyType).ExRateOption;
						var exchangeRateOption = AccountingConstants.InvoicePostingExchangeRateOption.CodeList.GetDescriptionFromCode(exchangeRateOptionCode);
						Parent.JR_APInvoiceDateInfo.AddWarning(AccountingConstants.GetIsNotLocalCurrencyAndExRateOptionIsApplicableErrorMessage(AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateRegistryItemCaption(ExchangeRateValidLedgerEnum.AP), exchangeRateOption));
					}
				}
			}
		}

		#endregion

		protected override void CheckJR_APDocumentReceivedDate()
		{
			base.CheckJR_APDocumentReceivedDate();

			if (!Parent.JR_APDocumentReceivedDateInfo.ReadOnly)
			{
				if (!Parent.JR_APInvoiceNum.IsEmpty && Parent.JR_OH_CostAccount.IsValid)
				{
					if (!Parent.IsCustomsCharge && AccountingMasterFilesRegistry.Instance.DocumentReceivedDateMustBeEntered.Value)
					{
						MandatoryValidation.CheckEntered(Parent.JR_APDocumentReceivedDateInfo);
					}

					BaseCharge previousCharge = GetChargeForSameInvoiceWithUnequalColumn(JobChargeSchema.JR_APDocumentReceivedDate);
					if (previousCharge != null)
					{
						Parent.JR_APDocumentReceivedDateInfo.AddError(GetDifferenceBetweenChargesForSameTransactionErrorMessage(Parent.JR_APDocumentReceivedDateInfo, previousCharge.JR_APDocumentReceivedDate.IsEmpty ? NullDescription : (ZString)previousCharge.JR_APDocumentReceivedDate.ToShortDateString()));
					}
				}

				if (!Parent.JR_APDocumentReceivedDate.IsEmpty)
				{
					CheckAPInvoiceNumIsEntered(Parent.JR_APDocumentReceivedDateInfo, Res.GetString("D5A170EF-CFF0-42B9-B37F-17354A264B2B", "a Document Received date is entered"));
					CheckCostAccountIsValid(Parent.JR_APDocumentReceivedDateInfo, Res.GetString("D48DD218-81E8-4F13-8ED0-E75F5F941626", "a Document Received date is entered"));
				}
			}
		}

		#region JR_PaymentDate

		protected override void CheckJR_PaymentDate()
		{
			base.CheckJR_PaymentDate();
			RunPaymentDateValidation();
		}

		protected virtual void RunPaymentDateValidation()
		{
			if (!Parent.JR_PaymentDateInfo.ReadOnly)
			{
				if (Parent.JR_OH_CostAccount.IsValid && !Parent.JR_APInvoiceNum.IsEmpty)
				{
					if (!Parent.IsCustomsCharge)
					{
						MandatoryValidation.CheckEntered(Parent.JR_PaymentDateInfo);
					}

					BaseCharge previousCharge = GetChargeForSameInvoiceWithUnequalColumn(JobChargeSchema.JR_PaymentDate);
					if (previousCharge != null)
					{
						Parent.JR_PaymentDateInfo.AddError(GetDifferenceBetweenChargesForSameTransactionErrorMessage(Parent.JR_PaymentDateInfo, previousCharge.JR_PaymentDate.IsEmpty ? NullDescription : (ZString)previousCharge.JR_PaymentDate.ToShortDateString()));
					}
				}

				if (!Parent.JR_PaymentDate.IsEmpty)
				{
					CheckAPInvoiceNumIsEntered(Parent.JR_PaymentDateInfo, Res.GetString("e542a8f1-d25f-4215-b4ac-7ba3d77ba49d", "a Payment Date is entered"));
					CheckCostAccountIsValid(Parent.JR_PaymentDateInfo, Res.GetString("e542a8f1-d25f-4215-b4ac-7ba3d77ba49d", "a Payment Date is entered"));
				}

				if (Parent.JR_PaymentDate.IsValid && !Parent.JR_PaymentDate.IsEmpty
				&& Parent.JR_APInvoiceDate.IsValid && !Parent.JR_APInvoiceDate.IsEmpty)
				{
					if (Parent.JR_PaymentDate.Date < Parent.JR_APInvoiceDate.Date)
					{
						Parent.JR_PaymentDateInfo.AddError(Res.GetString("FEE83389-7358-49F1-A4C7-4B2D6161E9FE", "Due date should be after or equal to Invoice Date"));
					}
				}
			}
		}

		#endregion

		#region JR_AB

		protected override void CheckJR_AB()
		{
			base.CheckJR_AB();

			if (!Parent.JR_ABInfo.ReadOnly)
			{
				RefCurrency validBankAccountCurrency = Parent.CostCurrency;

				if (Parent.JR_OH_CostAccount.IsValid && !Parent.JR_APInvoiceNum.IsEmpty
					&& !Parent.JR_PaymentType.Trim().IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.JR_ABInfo);

					BaseCharge previousCharge = GetChargeForSameInvoiceWithUnequalColumn(JobChargeSchema.JR_AB);
					if (previousCharge != null)
					{
						Parent.JR_ABInfo.AddError(GetDifferenceBetweenChargesForSameTransactionErrorMessage(Parent.JR_ABInfo, previousCharge.BankAccount == null ? NullDescription : previousCharge.BankAccount.AB_Code));
					}

					BaseCharge previousChargeWithDifferentCurrency = GetChargeForSameInvoiceWithUnequalColumn(null, new ZQuery(JobChargeSchema.JR_RX_NKCostCurrency, SQLComparisonOperator.NotEqual, Parent.JR_RX_NKCostCurrency));
					if (previousChargeWithDifferentCurrency != null)
					{
						validBankAccountCurrency = GlbCompany.CurrentCompany.LocalCurrency;
					}
				}
				if (Parent.BankAccount != null && validBankAccountCurrency != null && Parent.BankAccount.AB_RX_NKAccountCurrency != validBankAccountCurrency.RX_Code)
				{
					Parent.JR_ABInfo.AddError(Res.GetString("340DDDDD-DEA2-42f4-B25C-03DA2EDF6E92", "Bank Account currency is incorrect. Choose {0} currency Bank Account.", validBankAccountCurrency.RX_Code));
				}
			}

			RunBankAccountValidation();
		}

		protected virtual void RunBankAccountValidation()
		{
			if (!Parent.JR_ABInfo.ReadOnly && !Parent.JR_AB.IsEmpty)
			{
				CheckAPInvoiceNumIsEntered(Parent.JR_ABInfo, Res.GetString("eafa759f-ee59-4e77-941a-0a65a0d69195", "a Bank Account can be entered"));
				CheckCostAccountIsValid(Parent.JR_ABInfo, Res.GetString("eafa759f-ee59-4e77-941a-0a65a0d69195", "a Bank Account can be entered"));
				if (Parent.JR_PaymentType.Trim().IsEmpty)
				{
					Parent.JR_ABInfo.AddError(Res.GetString("216abf88-b4af-4fd7-90e1-e933a977da00", "A Payment type must be entered before a Bank Account can be entered"));
				}
			}
		}

		#endregion

		#region JR_AK

		protected override void CheckJR_AK()
		{
			if (!Parent.JR_IsCostPosted)
			{
				base.CheckJR_AK();
				RunChequeBookValidation();
			}
		}

		protected virtual void RunChequeBookValidation()
		{
			if (!Parent.JR_AKInfo.ReadOnly)
			{
				if (!Parent.JR_AK.IsEmpty)
				{
					CheckChargeCodeIsValid(Parent.JR_AKInfo, Res.GetString("2c2d7c21-a3c7-4df3-b4c9-746f9184fd97", "a check book is entered"));
				}

				if (Parent.JR_AC.IsValid && Parent.IsCheque)
				{
					MandatoryValidation.CheckEntered(Parent.JR_AKInfo);
				}
				else if (Parent.JR_AK.IsValid)
				{
					Parent.JR_AKInfo.AddError(Res.GetString("5ad8060f-2ed7-4bb6-a639-42e16c203c9e", "The payment type must be check for a check book to be entered"));
				}

				if (Parent.JR_AK.IsValid && Parent.JR_OH_CostAccount.IsValid && !Parent.JR_APInvoiceNum.IsEmpty)
				{
					BaseCharge previousCharge = GetChargeForSameInvoiceWithUnequalColumn(JobChargeSchema.JR_AK);

					if (previousCharge != null)
					{
						Parent.JR_AKInfo.AddError(GetDifferenceBetweenChargesForSameTransactionErrorMessage(Parent.JR_AKInfo, previousCharge.ChequeBook == null ? NullDescription : previousCharge.ChequeBook.AK_Code));
					}
				}

				ZString errorMessage = AutoAllocationValidation.GetErrorsForChequeBook(Parent.ChequeBook, Parent.IsChequeNumberAutoAllocated);
				if (!errorMessage.IsEmpty)
				{
					Parent.JR_AKInfo.AddError(errorMessage);
				}
			}
		}

		#endregion

		#region JR_PaymentType

		protected override void CheckJR_PaymentType()
		{
			base.CheckJR_PaymentType();
			RunPaymentTypeValidation();
			ValidateJR_AB();
			ValidateJR_AK();
			ValidateJR_ChequeNo();
			ValidateJR_APInvoiceNum();
			if (!Parent.JR_IsCostPosted && Parent.JR_OSCostAmt < 0 && !Parent.JR_PaymentType.IsEmpty)
			{
				Parent.JR_PaymentTypeInfo.AddWarning(Res.GetString("1dbb5a08-3388-4dcb-acd5-8374a30f2e6f", "A negative invoice with bank details may create an AP Credit note, If Posting this charge creates an AP Credit Note, Bank details will be ignored"));
			}
		}

		protected virtual void RunPaymentTypeValidation()
		{
			if (!Parent.JR_PaymentTypeInfo.ReadOnly)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JR_PaymentTypeInfo, Parent.PaymentTypes);
				if (!Parent.JR_APInvoiceNum.IsEmpty && Parent.JR_OH_CostAccount.IsValid)
				{
					BaseCharge previousCharge = GetChargeForSameInvoiceWithUnequalColumn(JobChargeSchema.JR_PaymentType);
					if (previousCharge != null)
					{
						Parent.JR_PaymentTypeInfo.AddError(GetDifferenceBetweenChargesForSameTransactionErrorMessage(Parent.JR_PaymentTypeInfo, previousCharge.JR_PaymentType.IsEmpty ? NullDescription : previousCharge.JR_PaymentType));
					}
				}
				if (!Parent.JR_PaymentType.IsEmpty)
				{
					CheckAPInvoiceNumIsEntered(Parent.JR_PaymentTypeInfo, Res.GetString("eb724682-9b4b-46df-a945-bb3a91692340", "a Payment Type is entered"));
					CheckChargeCodeIsValid(Parent.JR_PaymentTypeInfo, Res.GetString("eb724682-9b4b-46df-a945-bb3a91692340", "a Payment Type is entered"));
					CheckCostAccountIsValid(Parent.JR_PaymentTypeInfo, Res.GetString("eb724682-9b4b-46df-a945-bb3a91692340", "a Payment Type is entered"));
				}

				if (Parent.CostAccount != null &&
					 Parent.JR_PaymentType == ZArchitecture.Core.ReceiptTypes.eNettDirectDebit)
				{
					if (!eNettHelper.IsOrganisationeNettRegistered(Parent.CostAccount) &&
						 !eNettHelper.DoesOrgHaveeNettDDRAccount(Parent.CostAccount))
					{
						Parent.JR_PaymentTypeInfo.AddError(eNettHelper.NoEnettBankInformationOnOrganisationError);
					}
					if (Parent.BankAccount != null && !eNettHelper.IsBankAccountEnettRegistered(Parent.BankAccount))
					{
						Parent.JR_PaymentTypeInfo.AddError(eNettHelper.BankAccountNotEnettRegistered);
					}
					if (!eNettHelper.IsOrganisationeNettRegistered(Parent.CostAccount) &&
							  eNettHelper.DoesOrgHaveeNettDDRAccount(Parent.CostAccount))
					{
						Parent.JR_PaymentTypeInfo.AddWarning(eNettHelper.PayAnyoneWarning);
					}
				}
			}

			if (!Parent.JR_PaymentTypeInfo.HasErrors() && (!IsInDatabaseForPaymentTypeSecurityCheck || DoesPaymentTypeHaveChanges))
			{
				CheckJR_PaymentTypeSecurity();
			}
		}

		protected virtual bool DoesPaymentTypeHaveChanges
		{
			get { return Parent.JR_PaymentTypeInfo.HasChanges; }
		}

		protected virtual bool IsInDatabaseForPaymentTypeSecurityCheck
		{
			get { return Parent.IsInDatabase; }
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CheckJR_PaymentTypeSecurity()
		{
			bool result = true;

			switch (Parent.JR_PaymentType)
			{
				case ReceiptTypes.Cheque:
					result = Env.Security.NewPayablesPaymentCheque.IsAllowed || Env.Security.APPaymentProcessingNewCheque.IsAllowed;
					break;
				case ReceiptTypes.Cash:
					result = Env.Security.NewPayablesPaymentCash.IsAllowed || Env.Security.APPaymentProcessingNewCash.IsAllowed;
					break;
				case ReceiptTypes.CreditCard:
					result = Env.Security.NewPayablesPaymentCreditCard.IsAllowed || Env.Security.APPaymentProcessingNewCreditCard.IsAllowed;
					break;
				case ReceiptTypes.DirectDebit:
					result = Env.Security.NewPayablesPaymentDirectDebit.IsAllowed || Env.Security.APPaymentProcessingNewDirectDebit.IsAllowed;
					break;
				case ReceiptTypes.EFT:
					result = Env.Security.NewPayablesPaymentEFT.IsAllowed || Env.Security.APPaymentProcessingNewEFT.IsAllowed;
					break;
				case ReceiptTypes.ScheduledEFT:
					result = Env.Security.NewPayablesPaymentSFT.IsAllowed || Env.Security.APPaymentProcessingNewSFT.IsAllowed;
					break;
				case ReceiptTypes.CollectionRequest:
					result = Env.Security.NewPayablesPaymentCRQ.IsAllowed || Env.Security.APPaymentProcessingNewCRQ.IsAllowed;
					break;
			}

			if (!result)
			{
				Parent.JR_PaymentTypeInfo.AddError(Res.GetString("4bfef6fc-5974-469d-a8d2-60dbc3354bb4", "You do not have appropriate security rights to select this payment type."));
			}
		}

		#endregion

		#region Place Of Supply

		protected override void CheckJR_CostPlaceOfSupply()
		{
			base.CheckJR_CostPlaceOfSupply();
			CheckAPEnforcePostingFPOS();
		}

		void CheckAPEnforcePostingFPOS()
		{
			if (!Parent.JR_CostPlaceOfSupplyInfo.HasErrors()
				&& !Parent.JR_APInvoiceNum.IsEmpty
				&& Parent.JR_OH_CostAccount.IsValid
				&& PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(Parent.Company)
				&& AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.Value)
			{
				var previousCharge = GetChargeForSameInvoiceWithUnequalColumn(JobChargeSchema.JR_CostPlaceOfSupply, null);
				if (previousCharge != null)
				{
					Parent.JR_CostPlaceOfSupplyInfo.AddError(SameAPInvoiceNumberAndCreditorButDifferentFPOSErrorMessage);
				}
			}
		}

		#endregion

		#region JR_ChequeNo

		protected override void CheckJR_ChequeNo()
		{
			base.CheckJR_ChequeNo();
			RunChequeNumberValidation();
		}

		protected virtual void RunChequeNumberValidation()
		{
			if (!Parent.JR_ChequeNoInfo.HasErrors() && !Parent.IsRevenueCharge && !Parent.IsCostPosted && !Parent.IsChequeNumberAutoAllocated
				&& Parent.JR_OH_CostAccount.IsValid && Parent.JR_AC.IsValid && !Parent.JR_PaymentType.Trim().IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.JR_ChequeNoInfo);
				if (!Parent.JR_ChequeNoInfo.HasErrors())
				{
					string errorMessage = ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(Parent.IsCheque, Parent.JR_ChequeNo);
					if (!string.IsNullOrEmpty(errorMessage))
					{
						Parent.JR_ChequeNoInfo.AddError(errorMessage);
					}

					if (!Parent.JR_ChequeNoInfo.HasErrors() && Parent.JR_PaymentType == ZArchitecture.Core.ReceiptTypes.Cheque)
					{
						errorMessage = ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(Parent.ChequeBook, Parent.JR_ChequeNo);
						if (!string.IsNullOrEmpty(errorMessage))
						{
							Parent.JR_ChequeNoInfo.AddError(errorMessage);
						}
						else
						{
							ZDecimal chequeNumber;
							if (!ZDecimal.TryParse(Parent.JR_ChequeNo.ToString(), out chequeNumber))
							{
								Parent.JR_ChequeNoInfo.AddError(Res.GetString("22333d09-2b7e-4c3a-8cc5-b696cf98aa25", "This cheque number is too large."));
							}
							else if (HasChequeNumberBeenUsed)
							{
								Parent.JR_ChequeNoInfo.AddError(Res.GetString("86ae1c4a-76a0-455c-a971-cbdccd601193", "This check number is already in use."));
							}
							else if (IsChequeNoInUse())
							{
								errorMessage = Res.GetString("16ab688e-ac0b-440f-9e4c-9b21a763afed", "This check number is already used on") + " " + GetReferenceToDisplay(OtherChargesUsingTheSameChequeNo[0]) + ".";
								Parent.JR_ChequeNoInfo.AddError(errorMessage);
							}
							else if (!Parent.JR_IsApportioned && !IsChequeNumberApplicable())
							{
								Parent.JR_ChequeNoInfo.AddError(ChequeNumberErrorMessage);
							}
							else if (Parent.ChequeBook != null && Parent.ChequeBook.BankAccount != null)
							{
								AccValidationHelper.ValidateChequeDigits(Parent.JR_ChequeNoInfo, Parent.ChequeBook.BankAccount.AB_ChequeNumDigits);
							}
						}
					}
					if (!Parent.JR_ChequeNoInfo.HasErrors() && !Parent.JR_ChequeNoInfo.ReadOnly)
					{
						BaseCharge previousCharge = GetChargeForSameInvoiceWithUnequalColumn(JobChargeSchema.JR_ChequeNo);
						if (previousCharge != null)
						{
							Parent.JR_ChequeNoInfo.AddError(GetDifferenceBetweenChargesForSameTransactionErrorMessage(Parent.JR_ChequeNoInfo, previousCharge.JR_ChequeNo.IsEmpty ? NullDescription : previousCharge.JR_ChequeNo));
						}
					}
				}
			}

			if (!Parent.JR_ChequeNoInfo.HasErrors() && !Parent.JR_ChequeNoInfo.ReadOnly)
			{
				if (!Parent.JR_ChequeNo.IsEmpty)
				{
					if (Parent.JR_PaymentType.Trim().IsEmpty)
					{
						Parent.JR_ChequeNoInfo.AddError(Res.GetString("e1cf114d-5b21-4c3c-ae16-4cf6f6ad3726", "Payment type should be entered before check number is entered"));
					}
					CheckChargeCodeIsValid(Parent.JR_ChequeNoInfo, " " + Res.GetString("70baba31-9f01-4b60-bbdc-5da523f217d1", "a check number is entered"));
					CheckCostAccountIsValid(Parent.JR_ChequeNoInfo, Res.GetString("70baba31-9f01-4b60-bbdc-5da523f217d1", "a check number is entered"));
				}
			}
		}

		protected abstract bool HasChequeNumberBeenUsed
		{
			get;
		}

		protected abstract string ChequeNumberErrorMessage
		{
			get;
		}

		protected bool IsChequeNumberApplicable()
		{
			return new BusinessObjectFactory().LoadTop1<Charge>(ChequeNoApplicableQuery) == null;
		}

		protected virtual ZQuery ChequeNoApplicableQuery
		{
			get
			{
				ZQuery query = new ZQuery(JobChargeSchema.JR_ChequeNo, Parent.JR_ChequeNo);
				query.AddToFilter(JobChargeSchema.JR_AK, Parent.JR_AK);
				query.AddToFilter(JobChargeSchema.JR_AK, SQLComparisonOperator.NotEqual, null);
				query.AddToFilter(JobChargeSchema.JR_PaymentType, Parent.JR_PaymentType);

				return query;
			}
		}

		protected bool IsChequeNoInUse()
		{
			return OtherChargesUsingTheSameChequeNo.Length > 0;
		}

		protected BaseCharge[] OtherChargesUsingTheSameChequeNo
		{
			get
			{
				ZQuery filter = new ZQuery(JobChargeSchema.JR_ChequeNo, Parent.JR_ChequeNo);
				filter.AddToFilter(JobChargeSchema.JR_OH_CostAccount, SQLComparisonOperator.NotEqual, Parent.JR_OH_CostAccount);
				filter.AddToFilter(JobChargeSchema.JR_AK, SQLComparisonOperator.Equal, Parent.JR_AK);
				return Parent.Factory.Load<BaseCharge>(filter);
			}
		}

		ZString GetReferenceToDisplay(BaseCharge charge)
		{
			ZString consolNumber = GetConsolNumber(charge);
			return !consolNumber.IsEmpty ? (Res.GetString("5dad3fd4-a09a-41c0-b554-2df9cc6c8a0d", "Consol {0}", consolNumber)) : (Res.GetString("02e13589-aedc-4095-b493-9f04c8d70382", "Job {0}", charge.Job.JH_JobNum));
		}

		ZString GetConsolNumber(BaseCharge charge)
		{
			return charge.ParentConsolCost != null && charge.ParentConsolCost.Consol != null ? charge.ParentConsolCost.Consol.JK_UniqueConsignRef : ZString.Empty;
		}

		#endregion

		#region JR_AT_SellGSTRate

		protected override void CheckJR_AT_SellGSTRate()
		{
			base.CheckJR_AT_SellGSTRate();
			if ((CheckForSellGSTRateMandatory() && Parent.JR_AT_SellGSTRate.IsEmpty) || IsSellGSTRateDefinedButNotGSTRegistered)
			{
				Parent.JR_AT_SellGSTRateInfo.AddError(Res.GetString("060faa0d-2678-4a9a-95a9-dc90d55135a6", "Tax IDs on unposted charges conflict with the debtor \"Tax is Applicable\" flag.\r\nOne possible way to resolve this is to go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option."));
			}
		}

		bool IsSellGSTRateDefinedButNotGSTRegistered => Parent.IsSellButNotGSTRegistered && !Parent.IsRevenuePosted && !Parent.JR_AT_SellGSTRate.IsEmpty;

		protected virtual bool CheckForSellGSTRateMandatory()
		{
			return Parent.IsSellGSTApplicable && !Parent.IsRevenuePosted;
		}

		#endregion

		#region JR_OSSellAmt

		protected override void CheckJR_OSSellAmt()
		{
			base.CheckJR_OSSellAmt();
			if (!Parent.JR_IsRevenuePosted && (Parent.IsDisbursementCharge || Parent.IsMarginCharge))
			{
				if (Parent.JR_OSSellAmt == 0)
				{
					Parent.JR_OSSellAmtInfo.AddWarning(Res.GetString("8657fc63-0272-4f95-8066-d75e63d7dae9", "Sell Amount for a Margin or Disbursement charge cannot be 0."));
				}
			}
			CheckMatchingSignsForSellAmounts(Parent.JR_OSSellAmtInfo);

			if (!Parent.JR_IsRevenuePosted
				&& Parent.JR_OSSellAmt < 0
				&& !AccountingMasterFilesRegistry.Instance.AllowNegativeRevenueChargesOnJob.Value)
			{
				Parent.JR_OSSellAmtInfo.AddError(Res.GetString("a5ce4a3c-df82-4ea7-8bc3-76fc512cf11f", "Negative revenue charges are not allowed. This is controlled by the registry setting at Accounting > Job Invoicing > Allow negative charges on a job."));
			}
		}

		#endregion

		#region JR_RX_NKSellCurrency

		protected override void CheckJR_RX_NKSellCurrency()
		{
			base.CheckJR_RX_NKSellCurrency();
			MandatoryValidation.CheckEntered(Parent.JR_RX_NKSellCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JR_RX_NKSellCurrencyInfo, Parent.Currencies);
		}

		#endregion

		#region JR_RX_NKSellInvoiceCurrency

		protected override void CheckJR_RX_NKSellInvoiceCurrency()
		{
			base.CheckJR_RX_NKSellInvoiceCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.JR_RX_NKSellInvoiceCurrencyInfo, Parent.Currencies);
		}

		#endregion

		#region JR_GB

		protected override void CheckJR_GB()
		{
			base.CheckJR_GB();
			if (!Parent.JR_GBInfo.HasErrors())
			{
				if (!Parent.IsInDatabase && !Parent.Branch.GB_IsActive)
				{
					Parent.JR_GBInfo.AddError(ListValidation.GetNotificationMessage(Parent.JR_GBInfo.Description).ToString());
				}
			}
		}

		#endregion

		#region JR_CostReference

		protected override void CheckJR_CostReference()
		{
			base.CheckJR_CostReference();
			RunCostReferenceValidation();
		}

		void RunCostReferenceValidation()
		{
			if (!Parent.JR_CostReferenceInfo.ReadOnly)
			{
				if (!Parent.JR_APInvoiceNum.IsEmpty && Parent.JR_OH_CostAccount.IsValid)
				{
					BaseCharge previousCharge = GetChargeForSameInvoiceWithUnequalColumn(JobChargeSchema.JR_CostReference);
					if (previousCharge != null)
					{
						Parent.JR_CostReferenceInfo.AddError(GetDifferenceBetweenChargesForSameTransactionErrorMessage(Parent.JR_CostReferenceInfo, previousCharge.JR_CostReference.IsEmpty ? NullDescription : previousCharge.JR_CostReference));
					}
				}
			}
		}

		#endregion

		#region TaxDate 

		protected override void CheckJR_CostTaxDate()
		{
			base.CheckJR_CostTaxDate();

			if (Parent.CostGSTRate == null || Parent.JR_IsApportioned || Parent.IsCostPosted)
			{
				return;
			}

			var rateExists = Parent.CostGSTRate.DoesRateExists(Parent.JR_CostTaxDate);
			if (!rateExists)
			{
				Parent.JR_CostTaxDateInfo.AddError(invalidRateErrorMessage);
			}
		}

		protected override void CheckJR_SellTaxDate()
		{
			base.CheckJR_SellTaxDate();

			if (Parent.SellGSTRate == null || Parent.IsRevenuePosted)
			{
				return;
			}

			var rateExists = Parent.SellGSTRate.DoesRateExists(Parent.JR_SellTaxDate);
			if (!rateExists)
			{
				Parent.JR_SellTaxDateInfo.AddError(invalidRateErrorMessage);
			}
		}

		#endregion

		#region Tax Branch

		protected override void CheckJR_GB_SellTaxBranch()
		{
			base.CheckJR_GB_SellTaxBranch();
			if (!Parent.IsRevenuePosted && !Parent.IsSellTaxBranchActual)
			{
				Parent.JR_GB_SellTaxBranchInfo.AddError(AccountingConstants.TaxBranchConflictErrorMessageWithSuggestion);
			}
		}

		protected override void CheckJR_GB_CostTaxBranch()
		{
			base.CheckJR_GB_CostTaxBranch();

			if (!Parent.JR_IsApportioned && !Parent.IsCostPosted && !Parent.IsCostTaxBranchActual)
			{
				Parent.JR_GB_CostTaxBranchInfo.AddError(AccountingConstants.TaxBranchConflictErrorMessageWithSuggestion);
			}
		}

		#endregion

		static string invalidRateErrorMessage => Res.GetString("5792213c-72ae-4cd5-b596-10df12b7cfb6", "No rate found for selected date.");

		#region Implementation

		AccChequeBookAutoAllocationValidation AutoAllocationValidation
		{
			get
			{
				if (fAutoAllocationValidation == null)
				{
					fAutoAllocationValidation = new AccChequeBookAutoAllocationValidation();
				}
				return fAutoAllocationValidation;
			}
		}

		AccChequeBookAutoAllocationValidation fAutoAllocationValidation;

		void CheckChargeCodeIsValid(ZPropertyInfo info, ZString messageEnd)
		{
			if (!Parent.JR_AC.IsValid)
			{
				info.AddError(InvalidChargeCodeErrorMessage + messageEnd);
			}
		}

		void CheckCostAccountIsValid(ZPropertyInfo info, ZString messageEnd)
		{
			if (!Parent.JR_OH_CostAccount.IsValid)
			{
				info.AddError(InvalidCostAccountErrorMessage + messageEnd);
			}
		}

		void CheckMatchingSignsForSellAmounts(ZPropertyInfo propertyInfo)
		{
			if ((Parent.JR_LocalSellAmt > 0 && Parent.JR_OSSellAmt < 0) || (Parent.JR_LocalSellAmt < 0 && Parent.JR_OSSellAmt > 0))
			{
				propertyInfo.AddError(Res.GetString("74945ec4-d39e-4eac-9858-2637c0434d16", "OS Sell Amount and Local Sell Amount should have the same sign."));
			}
		}

		void CheckMatchingSignsForCostAmounts(ZPropertyInfo propertyInfo)
		{
			if ((Parent.JR_LocalCostAmt > 0 && Parent.JR_OSCostAmt < 0) || (Parent.JR_LocalCostAmt < 0 && Parent.JR_OSCostAmt > 0))
			{
				propertyInfo.AddError(Res.GetString("68de2864-3de4-4d2c-b234-b6dc65420010", "OS Cost Amount and Local Cost Amount should have the same sign."));
			}
		}

		void CheckAPInvoiceNumIsEntered(ZPropertyInfo info, ZString messageEnd)
		{
			if (Parent.JR_APInvoiceNum.Trim().IsEmpty &&
				!(Parent.CostAccount != null && Parent.CostAccount.CompanyData.OB_APCostsSelfBilled))
			{
				info.AddError(InvoiceNumberNotEnteredMessage + messageEnd);
			}
		}

		ZString InvoiceNumberNotEnteredMessage
		{
			get { return Res.GetString("583cf428-ee9e-4bf2-a2cb-9857bf5bc821", "An AP Invoice Number must be entered before") + " "; }
		}

		ZString InvalidCostAccountErrorMessage
		{
			get { return Res.GetString("84909fc0-c5ef-4efb-a380-4aa2b8343df9", "A valid Creditor must be entered before") + " "; }
		}

		ZString InvalidChargeCodeErrorMessage
		{
			get { return Res.GetString("a8da48ab-c8cb-4126-ac13-f4b7566dadaf", "A valid Charge Code must be entered before") + " "; }
		}

		ZString NullDescription
		{
			get { return Res.GetString("46acc549-9d01-409e-9be4-f62d3d8ef45e", "empty"); }
		}

		string SameAPInvoiceNumberAndCreditorButDifferentFPOSErrorMessage =>
			Res.GetString("d73b2b4e-0176-4457-be25-3c228fb577c7", "This Charge has a Creditor and an AP Invoice number same as another Charge, but the Cost Fixed Place of Supply does not match.");

		protected abstract string GetDifferenceBetweenChargesForSameTransactionErrorMessage(ZPropertyInfo field, string expected);

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
	}
}

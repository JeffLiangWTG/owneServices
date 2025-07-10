using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Validation;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public class ForwardingConsolCostingValidation : CommonConsolCostValidation
	{
		public ForwardingConsolCostingValidation(JobConsolCost parent, JobConsolCostCollection relatedCosts)
			: base(parent)
		{
			this.RelatedCosts = relatedCosts;
		}

		readonly JobConsolCostCollection RelatedCosts;

		#region E6_IsTaxAmountOverridden

		protected override void CheckE6_IsTaxAmountOverridden()
		{
			if ((!Parent.IsInDatabase || Parent.E6_IsTaxAmountOverriddenInfo.HasChanges) && Parent.E6_IsTaxAmountOverridden && !Env.Security.MaintainConsolJobInvoicingAllowTickOverrideTaxAmountCheckbox.IsAllowed)
			{
				Parent.E6_IsTaxAmountOverriddenInfo.AddError(Res.GetString("862B3D60-3EE9-4A4E-96F6-DE221E254C8C", @"You do not have appropriate security rights to tick this checkbox.
Please contact your system administrator for the following security right: Operate > Forwarding > Consolidations > Costing/Invoicing > Allow Tick Override Tax Amount Checkbox"));
			}
		}

		#endregion

		#region E6_GS_NKConsolCostOwner

		protected override void CheckE6_GS_NKConsolCostOwner()
		{
			base.CheckE6_GS_NKConsolCostOwner();

			if ((!Parent.IsInDatabase || Parent.E6_GS_NKConsolCostOwnerInfo.HasChanges) && Parent.E6_GS_NKConsolCostOwner != GlbStaff.CurrentUser.GS_Code && !Env.Security.MaintainConsolJobInvoicingAllowOverrideConsolCostOwner.IsAllowed)
			{
				Parent.E6_GS_NKConsolCostOwnerInfo.AddError(Res.GetString("82008AA7-997A-4CD5-805A-252D3C6EFDD6", @"You do not have appropriate security rights to modify this field.
Please contact your system administrator for the following security right: Operate > Forwarding > Consolidations > Costing/Invoicing > Allow Override of Consol Cost Owner"));
			}
		}

		#endregion

		#region Invoice Validation

		protected override void CheckE6_InvoiceNum()
		{
			base.CheckE6_InvoiceNum();
			if (Parent.E6_OH_Creditor.IsValid && !Parent.E6_InvoiceNum.IsEmpty)
			{
				ValidateDuplicateNumbers();

				if (IsInvoiceNumberUsedOnOtherConsolOrCharge())
				{
					Parent.E6_InvoiceNumInfo.AddError(Res.GetString("3d54df43-b77c-4a3b-a55f-df590accb271", "This invoice number is used on another consol or another shipment charge"));
				}

				if (!Parent.E6_InvoiceNumInfo.HasErrors())
				{
					ZDecimal totalInvoiceAmount = 0M;
					foreach (ApportionSplitCharge splitCharge in Parent.ApportionmentCharges)
					{
						totalInvoiceAmount += splitCharge.JR_Calc_LocalCostAmtWithGST;
					}
					foreach (JobConsolCost relatedConsolCost in FindConsolCostWithSameInvoiceDetails())
					{
						foreach (ApportionSplitCharge splitCharge in relatedConsolCost.ApportionmentCharges)
						{
							totalInvoiceAmount += splitCharge.JR_Calc_LocalCostAmtWithGST;
						}
					}

					SecurityCheckpoint unapprovedInvoiceCheckPoint = new UnapprovedTransactionValidationHelper().GetSecurityCheckPoint(totalInvoiceAmount);
					if (!unapprovedInvoiceCheckPoint.IsAllowed)
					{
						if (Parent.E6_PaymentType.IsEmpty)
						{
							Parent.E6_InvoiceNumInfo.AddWarning(Res.GetString("4d04f89d-16dd-4abd-a5f4-0d839b3ffe5f",
	@"You do not have security rights to post this invoice as an Accounts Payable Invoice. When you post this cost, it will be posted as an Unapproved Invoice.
The Unapproved Invoice must be approved by a user with the appropriate security rights."));
						}
						else
						{
							Parent.E6_InvoiceNumInfo.AddWarning(UnapprovedInvoicesWithPaymentWarningMessage);
						}
					}
				}

				if (!Parent.E6_InvoiceNumInfo.HasErrors())
				{
					if (Parent.ApportionmentCharges.Any()
						&& AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.Value.EnableBranchLevelPosting)
					{
						var uniqueBranches = Parent.ApportionmentCharges.Cast<ApportionSplitCharge>().Select(x => x.JR_GB).ToHashSet();
						if (BranchLevelPostingHelper.DoesAllBranchesBelongToSamePostingGroup(AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting, uniqueBranches))
						{
							if (Parent.Factory.LoadTop1<Charge>(GetChargeQuery()) != null)
							{
								Parent.E6_InvoiceNumInfo.AddError(Res.GetString("a044c50e-f2c7-4d93-b0c0-56ed0533bc26", @"Please review the Consol Cost allocations and ensure all charges for each Payables invoice have been assigned branch from the same Posting Group. All charges posted in one transaction must be within the same Branch Posting Group.
Saving is prevented because charges for the same Invoice number have been entered using a mix of branch Posting Groups."));
							}
						}
					}
				}
			}
			else
			{
				if (Parent.E6_OH_Creditor.IsEmpty && !Parent.E6_InvoiceNum.IsEmpty)
				{
					Parent.E6_InvoiceNumInfo.AddError(Res.GetString("84211d89-e8cc-49b3-9219-2966e86d353b", "A valid Creditor must be entered"));
				}
			}

			if (!Parent.E6_InvoiceNumInfo.HasErrors())
			{
				ValidateInvoiceNumRelatedProperties();
			}
		}

		void ValidateDuplicateNumbers()
		{
			var addNotification = new Action<AccountingUtils.PreviousSameNumberTransactionDetails>((x) => {
				Parent.E6_InvoiceNumInfo.AddNotification(x.NotificationType, x.NotificationMessage);
			});

			var numberCheckingDetails = AccountingUtils.APTransactionNumberExists(ZArchitecture.Core.TransactionTypes.Invoice, Parent.E6_InvoiceNum, Parent.E6_OH_Creditor, Parent.E6_InvoiceDate);
			if (numberCheckingDetails.HasNotification)
			{
				addNotification(numberCheckingDetails);
			}

			numberCheckingDetails = AccountingUtils.UATransactionNumberExists(ZArchitecture.Core.TransactionTypes.Invoice, Parent.E6_InvoiceNum, Parent.E6_OH_Creditor, ZGuid.Empty, Parent.E6_InvoiceDate);
			if (numberCheckingDetails.HasNotification)
			{
				addNotification(numberCheckingDetails);
			}
		}

		ZQuery GetChargeQuery()
		{
			var associatedBranches = BranchLevelPostingHelper.GetAssociatedBranches(AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting
				, Parent.ApportionmentCharges.Cast<ApportionSplitCharge>().First().JR_GB);

			var result = new ZQuery(JobChargeSchema.JR_APInvoiceNum, Parent.E6_InvoiceNum);
			result.AddToFilter(JobChargeSchema.JR_GC, GlbCompany.CurrentCompany.PK);
			result.AddToFilter(JobChargeSchema.JR_OH_CostAccount, Parent.E6_OH_Creditor);
			result.AddToFilter(JobChargeSchema.JR_GB, SQLComparisonOperator.NotEqual, associatedBranches);
			result.AddToFilter(JobChargeSchema.JR_E6, SQLComparisonOperator.NotEqual, Parent.PK);

			return result;
		}

		void ValidateInvoiceNumRelatedProperties()
		{
			ValidateE6_OH_Creditor();
			ValidateE6_InvoiceDate();
			ValidateE6_DocumentReceivedDate();
			ValidateE6_PaymentDate();
			ValidateE6_PaymentType();
			ValidateE6_CostReference();
			ValidateE6_AB_BankAccount();
			ValidateE6_AK_ChequeBook();
			ValidateE6_ChequeOrReference();
			ValidateE6_IsForCollectInvoice();
			ValidateE6_RX_NKCurrency();
			ValidateE6_ExchangeRate();
			Parent.RefreshBinding();
		}

		internal static string UnapprovedInvoicesWithPaymentWarningMessage
		{
			get
			{
				return Res.GetString("BFAF0DE1-C482-47a4-B4F1-2C73271644ED",
@"You are posting costs where some of the invoices being posted will be posted as unapproved.
Some of the consol costs that you are posting also contain Payment information.
{0} cannot proceed with posting where the costs are in this state.
You should either:
- Remove the payment details and organize for the payment to be posted through the Accounts Payable system.
  In this case, {0} will post an unapproved invoice where the total of the invoice is greater than your authorization settings allow.
OR
- Have another user with invoice posting authorization security rights that will allow them to post these costs as an actual Accounts Payable Invoice.
  In this case, {0} will also post the Payment.", Core.Constants.ProductName);
			}
		}

		bool IsInvoiceNumberUsedOnOtherConsolOrCharge()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZQuery query = new ZQuery(JobChargeSchema.JR_APInvoiceNum, Parent.E6_InvoiceNum);
			query.AddToFilter(JobChargeSchema.JR_OH_CostAccount, Parent.E6_OH_Creditor);
			List<ZGuid> currentCompanyBranches = new List<ZGuid>();
			foreach (GlbBranch branch in GlbCompany.CurrentCompany.Branches)
			{
				currentCompanyBranches.Add(branch.PK);
			}
			query.AddToFilter(JobChargeSchema.JR_GB, currentCompanyBranches);
			ZDBOnlyQuery consolIdFilter = new ZDBOnlyQuery(typeof(JobCharge));
			ZDBOnlySubQuery consolCostFilter = new ZDBOnlySubQuery(typeof(JobConsolCost), JobConsolCostSchema.PK, true);
			consolCostFilter.AddToFilter(JobConsolCostSchema.E6_ParentID, Parent.E6_ParentID);
			consolCostFilter.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, Parent.E6_ParentTableCode);
			consolIdFilter.AddSubQuery(JobChargeSchema.JR_E6, consolCostFilter, JoinCondition.And);
			consolIdFilter.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_E6, SQLComparisonOperator.Equal, null);
			query.AddToFilter(consolIdFilter);
			return factory.LoadTop1<Charge>(query) != null;
		}

		protected override void CheckE6_OH_Creditor()
		{
			base.CheckE6_OH_Creditor();
			ListValidation.ErrorIfInvalidPK(Parent.E6_OH_CreditorInfo);
			JobConsolCost relatedCostWithUnequalColumn = GetConsolCostForSameInvoiceWithUnequalColumn(JobConsolCostSchema.E6_OH_Creditor);
			if (relatedCostWithUnequalColumn != null)
			{
				Parent.E6_OH_CreditorInfo.AddError(GetTheSameInvoiceCostError(Res.GetString("36031A60-EFC0-42a1-B803-008613BE712D", "creditor")));
			}

			if (AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.Value && Parent.E6_OH_Creditor.IsEmpty && Parent.E6_LocalCostAmount != 0m)
			{
				string error = Res.GetString("ed332f31-c4b5-4fd9-b7db-a49d97569ede",
					"Please enter a Creditor before the job is autorated or saved. Your system has been configured so that the 'creditor' is mandatory when entering an unposted cost of non zero value.\r\n\r\nThe registry setting that governs this rule is Accounting > Job Costing > Accrual Must Have Creditor Code");
				Parent.E6_OH_CreditorInfo.AddError(error);
			}
			if (Parent.E6_OH_Creditor.IsValid && !Parent.E6_OH_CreditorInfo.HasErrors())
			{
				if (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value
					&& Parent.E6_IsForCollectInvoice
					&& (!Parent.Creditor.OH_IsDebtor
						|| !Parent.Creditor.OH_IsCreditor
						|| (Parent.Creditor.CompanyData?.IsARTaxApplicable ?? true) != (Parent.Creditor.CompanyData?.IsAPTaxApplicable ?? true)))
				{
					Parent.E6_OH_CreditorInfo.AddError(Res.GetString("0FDB3159-45D8-4F4C-A774-4768DBD024DE", "The organization {0} used for posting Collect Consol Cost has an invalid configuration. It should be both Receivables and Payables. Make sure that it has the same AR and AP Tax Recognition Rule.", Parent.Creditor.OH_Code));
				}

				ValidateE6_InvoiceNum();
				ValidateE6_InvoiceDate();
				ValidateE6_PaymentDate();
				ValidateE6_CostReference();
				ValidateE6_ExchangeRate();
				Parent.RefreshBinding();
			}
		}

		#region E6_DocumentReceivedDate

		protected override void CheckE6_DocumentReceivedDate()
		{
			base.CheckE6_DocumentReceivedDate();

			if (!Parent.E6_DocumentReceivedDate.IsEmpty)
			{
				if (Parent.E6_OH_Creditor.IsEmpty)
				{
					Parent.E6_DocumentReceivedDateInfo.AddError(Res.GetString("7FB904D0-B5C0-4F23-881D-C0226174A3E8", "A valid Creditor must be entered when Document Received Date is entered"));
				}
				if (Parent.E6_InvoiceNum.IsEmpty)
				{
					Parent.E6_DocumentReceivedDateInfo.AddError(Res.GetString("B6FEBC40-F55D-4977-AFDC-6BA07B9609F5", "An AP Invoice Number must be entered when Document Received Date is entered"));
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(Parent.E6_InvoiceNum) && AccountingMasterFilesRegistry.Instance.DocumentReceivedDateMustBeEntered.Value)
				{
					Parent.E6_DocumentReceivedDateInfo.AddError(Res.GetString("24D4B65E-B2CD-4CF8-91CB-3A16160EBED3", "Document Received Date must be entered when AP Invoice Number is entered."));
				}
			}

			if (!Parent.E6_DocumentReceivedDateInfo.HasErrors())
			{
				JobConsolCost relatedCostWithUnequalColumn = GetConsolCostForSameInvoiceWithUnequalColumn(JobConsolCostSchema.E6_DocumentReceivedDate);
				if (relatedCostWithUnequalColumn != null)
				{
					Parent.E6_DocumentReceivedDateInfo.AddError(GetTheSameInvoiceCostError(Res.GetString("3858B112-1907-4AB7-9F74-00AA662CDBB1", "document received date")));
				}
			}
		}

		#endregion

		protected override void CheckE6_InvoiceDate()
		{
			base.CheckE6_InvoiceDate();

			if (!Parent.E6_InvoiceDate.IsEmpty)
			{
				if (Parent.E6_OH_Creditor.IsEmpty)
				{
					Parent.E6_InvoiceDateInfo.AddError(Res.GetString("b238b2b9-0c60-4f4c-b374-89b84d10e536", "A valid Creditor must be entered when Invoice Date is entered"));
				}
				if (Parent.E6_InvoiceNum.IsEmpty)
				{
					Parent.E6_InvoiceDateInfo.AddError(Res.GetString("0c1d375b-1769-418f-82b1-32e57a0bfe61", "An AP Invoice Number must be entered when Invoice Date is entered"));
				}
				if (Parent.E6_InvoiceDate.Date > ZDate.Today && !AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.Value)
				{
					Parent.E6_InvoiceDateInfo.AddError(
						Res.GetString("AD7D4374-D967-4326-B8A5-0561567BDEEE",
									"Invoice date cannot be in the future.\r\nThis is determined by registry: {0}",
									((IRegistryItemInternals)AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate).Location));
				}
			}
			else
			{
				if (IsInvoiceDetailsEntered)
				{
					Parent.E6_InvoiceDateInfo.AddError(Res.GetString("dd71e980-a880-4586-803a-5d5974cc629e", "Invoice date must be entered when AP invoice number is entered"));
				}
			}

			if (!Parent.E6_InvoiceDateInfo.HasErrors())
			{
				JobConsolCost relatedCostWithUnequalColumn = GetConsolCostForSameInvoiceWithUnequalColumn(JobConsolCostSchema.E6_InvoiceDate);
				if (relatedCostWithUnequalColumn != null)
				{
					Parent.E6_InvoiceDateInfo.AddError(GetTheSameInvoiceCostError(Res.GetString("AA96230C-492B-401b-9B68-CA63CD18B53B", "invoice date")));
				}
			}

			var localCurrency = Parent.Company?.GC_RX_NKLocalCurrency ?? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			if (!Parent.E6_InvoiceDateInfo.HasErrors()
				&& Parent.E6_RX_NKCurrency != localCurrency
				&& ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AP, Parent.E6_RX_NKCurrency == localCurrency, Parent.E6_GC))
			{
				var regItem = AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateRegistryItem(ExchangeRateValidLedgerEnum.AP);
				var invoiceCurrencyType = Parent.E6_RX_NKCurrency == Parent.LocalCurrency ? Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local : Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;
				var exchangeRateOptionCode = regItem.Value.Cast<InvoicePostingExRateOption>().FirstOrDefault(x => x.InvoiceCurrencyType == invoiceCurrencyType).ExRateOption;
				var exchangeRateOption = AccountingConstants.InvoicePostingExchangeRateOption.CodeList.GetDescriptionFromCode(exchangeRateOptionCode);
				Parent.E6_InvoiceDateInfo.AddWarning(AccountingConstants.GetIsNotLocalCurrencyAndExRateOptionIsApplicableErrorMessage(AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateRegistryItemCaption(ExchangeRateValidLedgerEnum.AP), exchangeRateOption));
			}
		}

		protected override void CheckE6_PaymentDate()
		{
			base.CheckE6_PaymentDate();

			if (!Parent.E6_PaymentDate.IsEmpty)
			{
				if (Parent.E6_OH_Creditor.IsEmpty)
				{
					Parent.E6_PaymentDateInfo.AddError(Res.GetString("c14e9df7-cb88-42bd-b554-4338e0404a1c", "A valid Creditor must be entered when Payment Date is entered"));
				}
				if (Parent.E6_InvoiceNum.IsEmpty)
				{
					Parent.E6_PaymentDateInfo.AddError(Res.GetString("ee1e9462-be0c-41b5-a44d-495390b3668b", "An AP Invoice Number must be entered when Payment Date is entered"));
				}
				if (Parent.E6_PaymentDate.IsValid && Parent.E6_InvoiceDate.IsValid && !Parent.E6_InvoiceDate.IsEmpty)
				{
					if (Parent.E6_PaymentDate.Date < Parent.E6_InvoiceDate.Date)
					{
						Parent.E6_PaymentDateInfo.AddError(Res.GetString("C22C8C53-65BE-4F35-A8D2-908ADC7D1CE7", "Due date should be after or equal to Invoice Date"));
					}
				}
			}
			else
			{
				if (IsInvoiceDetailsEntered)
				{
					Parent.E6_PaymentDateInfo.AddError(Res.GetString("21ec4800-b26b-443e-b81a-90771995d192", "Invoice due date must be entered when AP invoice number is entered"));
				}
			}

			if (!Parent.E6_PaymentDateInfo.HasErrors())
			{
				JobConsolCost relatedCostWithUnequalColumn = GetConsolCostForSameInvoiceWithUnequalColumn(JobConsolCostSchema.E6_PaymentDate);
				if (relatedCostWithUnequalColumn != null)
				{
					Parent.E6_PaymentDateInfo.AddError(GetTheSameInvoiceCostError(Res.GetString("C2CC363E-D1F9-4bb3-B4E7-69121789205E", "invoice due date")));
				}
			}
		}

		protected override void CheckE6_CostReference()
		{
			base.CheckE6_CostReference();

			if (!Parent.E6_CostReferenceInfo.HasErrors())
			{
				JobConsolCost relatedCostWithUnequalColumn = GetConsolCostForSameInvoiceWithUnequalColumn(JobConsolCostSchema.E6_CostReference);
				if (relatedCostWithUnequalColumn != null)
				{
					Parent.E6_CostReferenceInfo.AddError(GetTheSameInvoiceCostError(Res.GetString("5A81512E-4465-47AA-952D-367493364971", "Supplier Cost Reference")));
				}
			}
		}

		readonly ZString PaymentTypeNotEnteredMessage = Res.GetString("c1af7b57-f6eb-4829-aa77-2a68c4e9f57d", "A valid Payment Type must be entered");

		bool IsInvoiceDetailsEntered
		{
			get { return !Parent.E6_InvoiceNum.IsEmpty && !Parent.E6_OH_Creditor.IsEmpty; }
		}

		#endregion

		#region PlaceOfSupply

		protected override void CheckE6_PlaceOfSupply()
		{
			base.CheckE6_PlaceOfSupply();

			if (
				!Parent.E6_PlaceOfSupplyInfo.HasErrors()
				&& !Parent.E6_InvoiceNum.IsEmpty
				&& Parent.E6_OH_Creditor.IsValid
				&& PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(Parent.Company)
				&& AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.Value)
			{
				var costWithSamePlaceOfSupply = GetConsolCostForSameInvoiceWithUnequalColumn(JobConsolCostSchema.E6_PlaceOfSupply);
				if (costWithSamePlaceOfSupply != null)
				{
					Parent.E6_PlaceOfSupplyInfo.AddError(GetTheSameInvoiceCostError(Res.GetString("9596004d-0f0b-43ba-8dbe-5a1ef26c23c7", "Cost Fixed Place of Supply")));
				}
			}
		}

		#endregion

		#region Payment Validation

		void CheckPaymentTypeIsEntered(ZPropertyInfo info)
		{
			if (Parent.E6_PaymentType.IsEmpty && (!Parent.E6_AB_BankAccount.IsEmpty || !Parent.E6_AK_ChequeBook.IsEmpty || !Parent.E6_ChequeOrReference.IsEmpty))
			{
				info.AddError(PaymentTypeNotEnteredMessage);
			}
		}

		protected override void CheckE6_PaymentType()
		{
			base.CheckE6_PaymentType();
			if (!Parent.E6_PaymentType.IsEmpty)
			{
				if (Parent.Creditor == null)
				{
					Parent.E6_PaymentTypeInfo.AddError(Res.GetString("b74b1028-6a3b-41ff-beba-b7a0938a9565", "A valid Creditor must be entered"));
				}
				else
				{
					if (Parent.E6_InvoiceNum.IsEmpty && !Parent.Creditor.CompanyData.OB_APCostsSelfBilled)
					{
						Parent.E6_PaymentTypeInfo.AddError(Res.GetString("a4c2edf9-24d3-40b0-8082-b2ef0a309779", "A valid Invoice Number must be entered"));
					}
				}
			}

			ListValidation.ErrorIfInvalidCode(Parent.E6_PaymentTypeInfo);

			if (Parent.E6_Calc_IncludeOnAgentInvoice && !Parent.E6_PaymentType.IsEmpty)
			{
				Parent.E6_PaymentTypeInfo.AddError(Res.GetString("0002ab77-8f6d-4210-98f3-3440b93459ce", "This is an agent related charge and cannot have payment details"));
			}

			if (!Parent.E6_PaymentTypeInfo.HasErrors())
			{
				JobConsolCost relatedCostWithUnequalColumn = GetConsolCostForSameInvoiceWithUnequalColumn(JobConsolCostSchema.E6_PaymentType);
				if (relatedCostWithUnequalColumn != null)
				{
					Parent.E6_PaymentTypeInfo.AddError(GetTheSameInvoiceCostError(Res.GetString("3BF43D34-FD88-457a-BFF7-8A57D470B109", "payment type")));
				}
			}

			if (!Parent.E6_PaymentTypeInfo.HasErrors() && (!Parent.IsInDatabase || Parent.E6_PaymentTypeInfo.HasChanges))
			{
				CheckE6_PaymentTypeSecurity();
			}

			ValidateE6_AB_BankAccount();
			ValidateE6_InvoiceNum();
			ValidateE6_ExchangeRate();
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CheckE6_PaymentTypeSecurity()
		{
			bool result = true;

			switch (Parent.E6_PaymentType)
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
				Parent.E6_PaymentTypeInfo.AddError(Res.GetString("1E9B02AF-1158-460A-AA11-6D9A0C3A62A4", "You do not have appropriate security rights to select this payment type."));
			}
		}

		protected override void CheckE6_AB_BankAccountIsValidZGuid()
		{
			if (!Parent.E6_AB_BankAccount.IsMissing)
			{
				base.CheckE6_AB_BankAccountIsValidZGuid();
			}
		}

		protected override void CheckE6_AB_BankAccount()
		{
			CheckPaymentTypeIsEntered(Parent.E6_AB_BankAccountInfo);

			if (!Parent.E6_PaymentType.IsEmpty && Parent.E6_PaymentType.IsValid && Parent.E6_AB_BankAccount.IsEmpty)
			{
				Parent.E6_AB_BankAccountInfo.AddError(Res.GetString("554bf295-ad3b-4e12-878f-086daebd90ae", "A valid bank account is required for selected payment type"));
			}

			if (Parent.E6_Calc_IncludeOnAgentInvoice && Parent.E6_AB_BankAccount.IsValid)
			{
				Parent.E6_AB_BankAccountInfo.AddError(Res.GetString("e9d4ba9b-dad6-41af-bc1b-8e766e73b53c", "This is an agent related charge and cannot have payment details"));
			}

			if (!Parent.E6_AB_BankAccountInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(Parent.E6_AB_BankAccountInfo, ResString.GetMultilingualString("2d2b95a2-68be-4fe3-8ff5-845229f31043", "Please select a bank account belonging to the current company."));
			}

			if (!Parent.E6_AB_BankAccountInfo.HasErrors())
			{
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

				JobConsolCost relatedCostWithUnequalColumn = GetConsolCostForSameInvoiceWithUnequalColumn(JobConsolCostSchema.E6_AB_BankAccount);
				if (relatedCostWithUnequalColumn != null)
				{
					Parent.E6_AB_BankAccountInfo.AddError(GetTheSameInvoiceCostError(Res.GetString("6D90EDC7-486D-4b4b-8096-860EBC90FB57", "bank account")));
				}

				var validBankAccountCurrency = Parent.Currency;
				var relatedCostWithUnequalCurrency = GetConsolCostForSameInvoiceWithUnequalColumn(JobConsolCostSchema.E6_RX_NKCurrency);
				if (relatedCostWithUnequalCurrency != null)
				{
					validBankAccountCurrency = GlbCompany.CurrentCompany.LocalCurrency;
				}
				if (Parent.BankAccount != null && validBankAccountCurrency != null && Parent.BankAccount.AB_RX_NKAccountCurrency != validBankAccountCurrency.RX_Code)
				{
					Parent.E6_AB_BankAccountInfo.AddError(Res.GetString("340DDDDD-DEA2-42f4-B25C-03DA2EDF6E92", "Bank Account currency is incorrect. Choose {0} currency Bank Account.", validBankAccountCurrency.RX_Code));
				}
			}

			ValidateE6_AK_ChequeBook();
			ValidateE6_ChequeOrReference();
			ValidateE6_ExchangeRate();
		}

		protected override void CheckE6_AK_ChequeBook()
		{
			base.CheckE6_AK_ChequeBook();

			if (!Parent.E6_AK_ChequeBook_ReadOnly)
			{
				CheckPaymentTypeIsEntered(Parent.E6_AK_ChequeBookInfo);

				if (!Parent.E6_AK_ChequeBookInfo.HasErrors())
				{
					if (Parent.E6_Calc_IncludeOnAgentInvoice && Parent.E6_AK_ChequeBook.IsValid)
					{
						Parent.E6_AK_ChequeBookInfo.AddError(Res.GetString("f7369c14-58c3-4aa8-9c9a-a9f057dc2dbf", "This is an agent related charge and cannot have payment details"));
					}

					if (!Parent.E6_PaymentType.IsEmpty && Parent.E6_PaymentType.IsValid && Parent.E6_AK_ChequeBook.IsEmpty)
					{
						Parent.E6_AK_ChequeBookInfo.AddError(Res.GetString("382e6616-4c51-4d69-a3f8-893741e4fd53", "A valid checkbook is required for selected payment type"));
					}

					if (Parent.E6_AK_ChequeBook.IsValid && !Parent.E6_AB_BankAccount.IsValid)
					{
						Parent.E6_AK_ChequeBookInfo.AddError(Res.GetString("db416c78-3d19-463e-bdb1-51a06844e988", "A valid bank account is required when entering a check book"));
					}

					JobConsolCost relatedCostWithUnequalColumn = GetConsolCostForSameInvoiceWithUnequalColumn(JobConsolCostSchema.E6_AK_ChequeBook);
					if (relatedCostWithUnequalColumn != null)
					{
						Parent.E6_AK_ChequeBookInfo.AddError(GetTheSameInvoiceCostError(Res.GetString("4481A432-31A6-4a07-92F4-35A347E24384", "check book")));
					}

					ZString errorMessage = AutoAllocationValidation.GetErrorsForChequeBook(Parent.ChequeBook, Parent.IsChequeNumberAutoAllocated);
					if (!errorMessage.IsEmpty)
					{
						Parent.E6_AK_ChequeBookInfo.AddError(errorMessage);
					}

					ValidateE6_ExchangeRate();
				}
			}
		}

		protected override void CheckE6_ChequeOrReference()
		{
			base.CheckE6_ChequeOrReference();

			if (!Parent.IsChequeNumberAutoAllocated)
			{
				CheckPaymentTypeIsEntered(Parent.E6_ChequeOrReferenceInfo);
				if (!Parent.E6_ChequeOrReferenceInfo.HasErrors())
				{
					if (Parent.E6_ChequeOrReference.IsEmpty)
					{
						if (!Parent.E6_PaymentType.IsEmpty && Parent.E6_PaymentType.IsValid && Parent.E6_ChequeOrReference.IsEmpty)
						{
							Parent.E6_ChequeOrReferenceInfo.AddError(Res.GetString("cd8a3877-226a-4fbc-9bcc-abd3b5dc4283", "A valid check or reference number is required for selected payment type"));
						}
					}
					else
					{
						if (Parent.E6_Calc_IncludeOnAgentInvoice)
						{
							Parent.E6_ChequeOrReferenceInfo.AddError(Res.GetString("20e98ad1-3547-4ef2-81ea-990765df390e", "This is an agent related charge and cannot have payment details"));
						}

						if (!Parent.E6_AB_BankAccount.IsValid)
						{
							Parent.E6_ChequeOrReferenceInfo.AddError(Res.GetString("e8809aa0-49e0-46ce-b97c-f0dd801be8c8", "A valid bank account is required when entering a check or reference number"));
						}

						if (Parent.E6_PaymentType == ZArchitecture.Core.ReceiptTypes.Cheque && !Parent.E6_AK_ChequeBook.IsValid)
						{
							Parent.E6_ChequeOrReferenceInfo.AddError(Res.GetString("f71361d6-783a-400a-961b-6f1cd84b8175", "A valid check book is required when entering a check or reference number"));
						}

						JobConsolCost relatedCostWithUnequalColumn = GetConsolCostForSameInvoiceWithUnequalColumn(JobConsolCostSchema.E6_ChequeOrReference);
						if (relatedCostWithUnequalColumn != null)
						{
							Parent.E6_ChequeOrReferenceInfo.AddError(GetTheSameInvoiceCostError(Res.GetString("8AF18A0F-3F20-47ea-9C07-545BA7DA201F", "payment reference")));
						}

						if (!Parent.E6_ChequeOrReferenceInfo.HasErrors() && Parent.Creditor != null
								&& Parent.E6_PaymentType == ZArchitecture.Core.ReceiptTypes.Cheque
								&& !Parent.E6_AK_ChequeBook.IsEmpty && Parent.E6_AK_ChequeBook.IsValid
								&& HasChequeNumberBeenUsed(Parent.E6_ChequeOrReference))
						{
							Parent.E6_ChequeOrReferenceInfo.AddError(Res.GetString("68dbc5e3-1cab-4e4f-890b-de9b7b52c55c", "This check number is used in another Cost, Charge, Payment or Hot Check"));
						}

						if (!Parent.E6_ChequeOrReferenceInfo.HasErrors() && Parent.E6_PaymentType == ZArchitecture.Core.ReceiptTypes.Cheque
							&& Parent.ChequeBook != null && !Parent.IsChequeNumberAutoAllocated)
						{
							string errorMessage = ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(true, Parent.E6_ChequeOrReference);
							if (string.IsNullOrEmpty(errorMessage))
							{
								errorMessage = ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(Parent.ChequeBook, Parent.E6_ChequeOrReference);
							}
							if (!string.IsNullOrEmpty(errorMessage))
							{
								Parent.E6_ChequeOrReferenceInfo.AddError(errorMessage);
							}
							else if (Parent.ChequeBook.BankAccount != null)
							{
								new AccValidationHelper().ValidateChequeDigits(Parent.E6_ChequeOrReferenceInfo, Parent.ChequeBook.BankAccount.AB_ChequeNumDigits);
							}
						}

						ValidateE6_ExchangeRate();
					}
				}
			}
		}

		bool HasChequeNumberBeenUsed(ZString chequeNumber)
		{
			List<ZGuid> consolCostsWithSameInvoice = new List<ZGuid>();
			consolCostsWithSameInvoice.Add(Parent.PK);
			foreach (JobConsolCost consolCost in FindConsolCostWithSameInvoiceDetails())
			{
				consolCostsWithSameInvoice.Add(consolCost.PK);
			}

			return Parent.BankAccount.HasChequeNumberBeenUsedOnAJobCharge(chequeNumber, consolCostsWithSameInvoice)
				|| Parent.BankAccount.HasChequeNumberBeenUsedOnAPayment(chequeNumber, ZGuid.Empty)
				|| Parent.BankAccount.HasChequeNumberBeenUsedOnAPaymentApproval(chequeNumber, ZGuid.Empty, ZGuid.Empty)
				|| HasChequeNumberBeenUsedOnAHotCheque(chequeNumber, Parent.ChequeBook, ZGuid.Empty)
				|| HasChequeNumberBeenUsedOnAConsolCost(chequeNumber, Parent.ChequeBook, consolCostsWithSameInvoice);
		}

		bool HasChequeNumberBeenUsedOnAHotCheque(ZString chequeNumber, AccChequeBook chequeBook, ZGuid hotChequePKToExlude)
		{
			ZDBOnlyQuery accHotChequeFilter = new ZDBOnlyQuery(typeof(AccHotCheque));
			accHotChequeFilter.AddToFilter(AccHotChequeSchema.AQ_ChequeNumber, chequeNumber);
			if (!hotChequePKToExlude.IsEmpty)
			{
				accHotChequeFilter.AddToFilter(AccHotChequeSchema.PK, SQLComparisonOperator.NotEqual, hotChequePKToExlude);
			}
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccChequeBook), AccHotChequeSchema.AQ_AK);
			subQuery.AddToFilter(AccChequeBookSchema.AK_AB, chequeBook.BankAccount.PK);
			accHotChequeFilter.AddSubQuery(subQuery, JoinCondition.And);

			return chequeBook.Factory.LoadTop1<AccHotCheque>(accHotChequeFilter) != null;
		}

		bool HasChequeNumberBeenUsedOnAConsolCost(ZString chequeNumber, AccChequeBook chequeBook, List<ZGuid> consolCostsToExclude)
		{
			ZQuery consolCostFilter = new ZQuery();
			consolCostFilter.AddToFilter(JobConsolCostSchema.E6_PaymentType, ZArchitecture.Core.ReceiptTypes.Cheque);
			consolCostFilter.AddToFilter(JobConsolCostSchema.E6_AB_BankAccount, chequeBook.BankAccount.PK);
			consolCostFilter.AddToFilter(JobConsolCostSchema.E6_AK_ChequeBook, chequeBook.PK);
			consolCostFilter.AddToFilter(JobConsolCostSchema.E6_ChequeOrReference, chequeNumber);
			consolCostFilter.AddToFilter(JobConsolCostSchema.E6_AH_APInvoice, SQLComparisonOperator.Equal, null);
			consolCostFilter.AddToFilter(JobConsolCostSchema.PK, SQLComparisonOperator.NotEqual, consolCostsToExclude);

			return chequeBook.Factory.LoadTop1<JobConsolCost>(consolCostFilter) != null;
		}

		#endregion

		#region Charge Code Validation

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		protected override void CheckE6_AC_ChargeCode()
		{
			base.CheckE6_AC_ChargeCode();

			JobConsolCostCollection allConsolCosts = null;
			foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)Parent).ParentCollections)
			{
				if (collection is JobConsolCostCollection)
				{
					allConsolCosts = (JobConsolCostCollection)collection;
					break;
				}
			}
			if (allConsolCosts != null)
			{
				var bringForwardAgainstCreditor = AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);

				var costsWithSameChargeCode = allConsolCosts.Where(x => x.PK != Parent.PK && !x.IsPosted && x.E6_AC_ChargeCode == Parent.E6_AC_ChargeCode);

				if (costsWithSameChargeCode.Any())
				{
					if (bringForwardAgainstCreditor)
					{
						Parent.E6_AC_ChargeCodeInfo.AddWarning(Res.GetString("cdd95e7a-dd31-4e9d-8005-90563ed85ed5", "There are more than one unposted charges with the same charge code, you are advised to review them before posting."));
					}
					else
					{
						Parent.E6_AC_ChargeCodeInfo.AddError(Res.GetString("b2ca6c0d-5690-47ac-be4b-c4d044849cba", @"There is an unposted consol cost with the same charge code.
The same charge code on unposted lines are allowed only when
- The registry 'Accrual Reversal Behavior When Allocated To Creditor' is set to YES; and

Please ensure that the registry is configured accordingly or use another charge code / creditor."));
					}
				}
			}
			if (Parent.IsInDatabase)
			{
				foreach (ApportionSplitCharge charge in Parent.ApportionmentCharges)
				{
					if (charge.IsInDatabase && charge.IsRevenuePosted)
					{
						if (charge.JR_AC != (ZGuid)charge.JR_ACInfo.OriginalValue)
						{
							AccChargeCode originalChargeCode = Parent.Factory.Load<AccChargeCode>((ZGuid)charge.JR_ACInfo.OriginalValue);
							string error = Res.GetString("6a030126-ffe7-4f2e-b241-a980b4e709c3", "There is revenue posted for this charge code on one of the shipments in this apportionment. You must delete the apportionment to change the charge code.");
							if (originalChargeCode != null)
							{
								error += " " + Res.GetString("889b4d4d-605b-479c-97a4-e402b39625c7", "Reset the charge code to the original value of {0}.", originalChargeCode.AC_Code);
							}
							Parent.E6_AC_ChargeCodeInfo.AddError(error);
							break;
						}
					}
				}
			}
		}

		#endregion

		#region Is For Collect Invoice Validation

		protected override void CheckE6_IsForCollectInvoice()
		{
			if (Parent.IsGatewayConsolCost)
			{
				return;
			}

			base.CheckE6_IsForCollectInvoice();
			if (Parent.E6_IsForCollectInvoice && !Parent.IsCreditorOverseasAgent)
			{
				IJobCostingPlugIn consol = Parent.Consol;
				if (consol != null)
				{
					bool isLoadPortLocal = consol.IsLoadPortLocal();

					string agentKind = isLoadPortLocal
						? Res.GetString("08fcde8b-d5f6-4350-84fc-39344644d156", "Receiving")
						: Res.GetString("3c9449e9-64ec-4c57-931e-3887ddea9e08", "Sending");

					OrgHeader agent = isLoadPortLocal
						? consol.ReceivingAgent
						: consol.SendingAgent;

					OrgHeader agentOrAPNettingGroup = isLoadPortLocal
						? consol.ReceivingAgentAPInvoicingParty
						: consol.SendingAgentAPInvoicingParty;

					OrgHeader agentOrARNettingGroup = isLoadPortLocal
						? consol.ReceivingAgentARInvoicingParty
						: consol.SendingAgentARInvoicingParty;

					string message = Res.GetString("2DCA28EC-1973-4822-9122-735D798348E4", "You can only mark this cost as 'Include on Collect Invoice' when the creditor is the '{0} Agent' for this consol.\r\nIf the {0} Agent has an AR or AP Netting Group, you must enter the AR or AP Netting group as the creditor.", agentKind);

					if (agent != null && agentOrAPNettingGroup != null && agentOrAPNettingGroup.PK != agent.PK)
					{
						message += "\r\n" + Res.GetString("2ba46046-90e3-448b-a33a-f7a8e31691e1", "{0} has a related AP Netting Group of {1}.", agent.OH_Code, agentOrAPNettingGroup.OH_Code);
					}

					if (agent != null && agentOrARNettingGroup != null && agentOrARNettingGroup.PK != agent.PK)
					{
						message += "\r\n" + Res.GetString("BBF5EFB6-723D-4f3c-94EB-81BDC470E385", "{0} has a related AR Netting Group of {1}.", agent.OH_Code, agentOrARNettingGroup.OH_Code);
					}

					Parent.E6_IsForCollectInvoiceInfo.AddError(message);
				}
			}

			JobConsolCost relatedCostWithUnequalColumn = GetConsolCostForSameInvoiceWithUnequalColumn(JobConsolCostSchema.E6_IsForCollectInvoice);
			if (relatedCostWithUnequalColumn != null)
			{
				Parent.E6_IsForCollectInvoiceInfo.AddError(GetTheSameInvoiceCostError(Res.GetString("6C0C2CCD-1B6A-4574-9DD8-AB55441C7610", "'Include on Collect Invoice'")));
			}
		}

		#endregion

		#region Currency and Exchange Rate Validation

		protected override void CheckE6_RX_NKCurrency()
		{
			base.CheckE6_RX_NKCurrency();
			JobConsolCost relatedCostWithEqualColumn = GetConsolCostForSameInvoiceWithEqualColumn(JobConsolCostSchema.E6_RX_NKCurrency);
			if (relatedCostWithEqualColumn != null && relatedCostWithEqualColumn.E6_ExchangeRate != Parent.E6_ExchangeRate)
			{
				Parent.E6_RX_NKCurrencyInfo.AddError(GetTheSameInvoiceCostErrorForCurrencyAndExRate());
			}

			JobConsolCost relatedCostWithUnequalColumn = GetConsolCostForSameInvoiceWithUnequalColumn(JobConsolCostSchema.E6_RX_NKCurrency);
			if (relatedCostWithUnequalColumn != null)
			{
				Parent.E6_RX_NKCurrencyInfo.AddWarning(AccountingConstants.ChargeOrConsolCostIsNotNullForSameInvoiceWithUnequalColumnErrorMessage);
			}

			ValidateE6_ExchangeRate();
		}

		protected override void CheckE6_ExchangeRate()
		{
			base.CheckE6_ExchangeRate();
			MandatoryValidation.CheckEntered(Parent.E6_ExchangeRateInfo);
			JobConsolCost relatedCostWithUnequalColumn = GetConsolCostForSameInvoiceWithUnequalColumn(JobConsolCostSchema.E6_ExchangeRate);
			if (relatedCostWithUnequalColumn != null && relatedCostWithUnequalColumn.E6_RX_NKCurrency == Parent.E6_RX_NKCurrency)
			{
				Parent.E6_ExchangeRateInfo.AddError(GetTheSameInvoiceCostErrorForCurrencyAndExRate());
			}

			if (!Parent.E6_ExchangeRateInfo.HasErrors())
			{
				var relatedConsolCostsWithSamePaymentDetails = FindConsolCostWithSamePaymentDetails();
				var relatedCostWithDifferentExchangeRate = FindConsolCostBasedOnColumn(JobConsolCostSchema.E6_ExchangeRate, false, relatedConsolCostsWithSamePaymentDetails);
				if (relatedCostWithDifferentExchangeRate != null)
				{
					Parent.E6_ExchangeRateInfo.AddError(GetTheSamePaymentDetailsErrorForCurrencyAndExRate());
				}
			}
		}

		#endregion

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

		JobConsolCost GetConsolCostForSameInvoiceWithUnequalColumn(SchemaColumn column)
		{
			return GetConsolCostForSameInvoice(column, false);
		}

		JobConsolCost GetConsolCostForSameInvoiceWithEqualColumn(SchemaColumn column)
		{
			return GetConsolCostForSameInvoice(column, true);
		}

		JobConsolCost GetConsolCostForSameInvoice(SchemaColumn column, bool needEqual)
		{
			JobConsolCost[] relatedCostsWithSameInfo = FindConsolCostWithSameInvoiceDetails();
			return FindConsolCostBasedOnColumn(column, needEqual, relatedCostsWithSameInfo);
		}

		JobConsolCost FindConsolCostBasedOnColumn(SchemaColumn column, bool needEqual, JobConsolCost[] relatedCostsWithSameInfo)
		{
			JobConsolCost result = null;
			foreach (JobConsolCost cost in relatedCostsWithSameInfo)
			{
				if (((IZType)cost[column.Name]).IsValid &&
					((IZType)Parent[column.Name]).IsValid &&
					((needEqual && cost[column.Name].Equals(Parent[column.Name])) ||
					(!needEqual && !cost[column.Name].Equals(Parent[column.Name])))
					)
				{
					if (!(column is SchemaDateTimeColumn) || new ZDateTime(cost[column.Name]).Date != new ZDateTime(Parent[column.Name]).Date)
					{
						result = cost;
						break;
					}
				}
			}
			return result;
		}

		JobConsolCost[] FindConsolCostWithSameInvoiceDetails()
		{
			JobConsolCost[] result = Array.Empty<JobConsolCost>();
			if (!Parent.E6_InvoiceNum.IsEmpty)
			{
				ZQuery costWithSameInvoiceInfoQuery = new ZQuery(JobConsolCostSchema.E6_InvoiceNum, Parent.E6_InvoiceNum); // Same Invoice number is the main key
				costWithSameInvoiceInfoQuery.AddToFilter(JobConsolCostSchema.E6_AH_APInvoice, null); // unposted costs only
				costWithSameInvoiceInfoQuery.AddToFilter(JobConsolCostSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK); // not the current item being validated
				costWithSameInvoiceInfoQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, Parent.E6_ParentID); // only for this consol - other validation takes care of other consols
				costWithSameInvoiceInfoQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, Parent.E6_ParentTableCode); // only for this consol - other validation takes care of other consols
				costWithSameInvoiceInfoQuery.AddToFilter(JobConsolCostSchema.E6_OH_Creditor, Parent.E6_OH_Creditor); //you can have the same invoice number for different creditors
				return (JobConsolCost[])RelatedCosts.Find(costWithSameInvoiceInfoQuery);
			}
			return result;
		}

		JobConsolCost[] FindConsolCostWithSamePaymentDetails()
		{
			//Search in a set of unposted consol Costs which belongs to the same consol as Parent and have same Creditor, Currency, Payment Type, Bank Account, Check book, Reference as Parent

			var result = Array.Empty<JobConsolCost>();

			var costWithSamePaymentDetailsInfoQuery = new ZQuery(JobConsolCostSchema.E6_OH_Creditor, Parent.E6_OH_Creditor); // Same Creditor
			costWithSamePaymentDetailsInfoQuery.AddToFilter(JobConsolCostSchema.E6_RX_NKCurrency, Parent.E6_RX_NKCurrency); // Same Currency
			costWithSamePaymentDetailsInfoQuery.AddToFilter(JobConsolCostSchema.E6_PaymentType, Parent.E6_PaymentType); // Same Payment Type
			costWithSamePaymentDetailsInfoQuery.AddToFilter(JobConsolCostSchema.E6_AB_BankAccount, Parent.E6_AB_BankAccount); // Same Bank Account
			costWithSamePaymentDetailsInfoQuery.AddToFilter(JobConsolCostSchema.E6_AK_ChequeBook, Parent.E6_AK_ChequeBook.IsValid ? Parent.E6_AK_ChequeBook : DBNull.Value); // Same Check Book
			costWithSamePaymentDetailsInfoQuery.AddToFilter(JobConsolCostSchema.E6_ChequeOrReference, Parent.E6_ChequeOrReference); // Same Reference
			costWithSamePaymentDetailsInfoQuery.AddToFilter(JobConsolCostSchema.E6_AH_APInvoice, DBNull.Value); // unposted costs only
			costWithSamePaymentDetailsInfoQuery.AddToFilter(JobConsolCostSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK); // not the current item being validated
			costWithSamePaymentDetailsInfoQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, Parent.E6_ParentID); // only for this consol - other validation takes care of other consols
			costWithSamePaymentDetailsInfoQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, Parent.E6_ParentTableCode); // only for this consol - other validation takes care of other consols

			return (JobConsolCost[])RelatedCosts.Find(costWithSamePaymentDetailsInfoQuery);
		}

		string GetTheSameInvoiceCostError(string propertyNameDoesntMatch)
		{
			return Res.GetString("dc0f4cf2-f60f-4ca6-85e3-60c503da0c69", "This consol cost has a Creditor and an AP Invoice number the same as another cost, but the {0} does not match.", propertyNameDoesntMatch);
		}

		string GetTheSameInvoiceCostErrorForCurrencyAndExRate()
		{
			return Res.GetString("f961d736-bb2f-480c-8000-fa31ace432d5", "This consol cost has an AP Invoice number and Currency the same as another cost but the exchange rate does not match.");
		}

		string GetTheSamePaymentDetailsErrorForCurrencyAndExRate()
		{
			return Res.GetString("2af99b15-d0fb-42f9-8573-8591146c73c0", "This consol cost has same payment details as another cost but the exchange rate does not match.");
		}

		public JobConsolCostCollection GetRelatedCostCollectionOfValidationForTest()
		{
			return this.RelatedCosts;
		}
		#endregion
	}
}

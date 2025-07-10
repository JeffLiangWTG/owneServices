using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing.Journal;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ChargeValidation : ChargeWithCostValidation
	{
		public ChargeValidation(Charge parent, IClosedJobReopener closedJobReopener)
			: base(parent)
		{
			this.Parent = parent;
			this.ClosedJobReopener = closedJobReopener;
		}

		readonly IClosedJobReopener ClosedJobReopener;

		#region Parent

		new Charge Parent
		{
			get { return (Charge)base.Parent; }
			set { base.Parent = value; }
		}

		#endregion

		Dictionary<string, bool> invoiceTypeMap;

		/// <summary>
		/// A map to identify if an invoice type is foriegn or not.
		/// </summary>
		Dictionary<string, bool> InvoiceTypeMap
		{
			get
			{
				if (invoiceTypeMap == null)
				{
					invoiceTypeMap = new Dictionary<string, bool>()
					{
						{ InvoiceTypesList.Codes.ForeignCurrencyInvoice, true },
						{ InvoiceTypesList.Codes.DisbursementInForeignCurrency, true },
						{ InvoiceTypesList.Codes.FreightInvoice, true },
						{ InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching, true },
						{ InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching, true },
						{ InvoiceTypesList.Codes.FreightInvoice_Batching, true },
						{ InvoiceTypesList.Codes.SelfBillingInvoice_Batching, true },
						{ AgencyInvoiceTypesList.Codes.ForeignCollect, true },
						{ AgencyInvoiceTypesList.Codes.ForeignCollect_Batching, true },
						{ AgencyInvoiceTypesList.Codes.ForeignPrePaid, true },
						{ AgencyInvoiceTypesList.Codes.ForeignPrePaid_Batching, true },
						{ InvoiceTypesList.Codes.FinalInvoice, false },
						{ InvoiceTypesList.Codes.DisbursementInvoice, false },
						{ InvoiceTypesList.Codes.InvoicePerTaxCode, false },
						{ InvoiceTypesList.Codes.DestinationChargesInvoice, false },
						{ InvoiceTypesList.Codes.DisbursementInvoice_Batching, false },
						{ InvoiceTypesList.Codes.FinalInvoice_Batching, false },
						{ InvoiceTypesList.Codes.InvoicePerTaxCode_Batching, false },
						{ InvoiceTypesList.Codes.DestinationChargesInvoice_Batching, false },
						{ InvoiceTypesList.Codes.SelfBillingInvoice, false },
						{ AgencyInvoiceTypesList.Codes.LocalCollect, false },
						{ AgencyInvoiceTypesList.Codes.LocalCollect_Batching, false },
						{ AgencyInvoiceTypesList.Codes.LocalPrePaid, false },
						{ AgencyInvoiceTypesList.Codes.LocalPrePaid_Batching, false },
						{ AgencyInvoiceTypesList.Codes.Misc, false },
						{ AgencyInvoiceTypesList.Codes.Misc_Batching, false },
					};
				}

				return invoiceTypeMap;
			}
		}

		static ZString EmptySellAmountErrorMessage => Res.GetString("d20332ff-3ffb-4dde-99f9-35a7e24760e6", "Sell amount and Debtor cannot be blank when charge line is flagged 'Advance Payment Required'");
		static ZString NegativeSellAmountErrorMessage => Res.GetString("51eef02a-8c04-4e51-8aea-6eb75750188f", "Sell amount cannot be negative when charge line is flagged 'Advance Payment Required'");

		protected override void CheckJR_EstimatedCost()
		{
			base.CheckJR_EstimatedCost();

			if (!Parent.JR_EstimatedCost.IsEmpty && Parent.IsInDatabase && Parent.JR_OSCostAmtInfo.HasChanges && !Parent.IsCostPosted)
			{
				Parent.JR_EstimatedCostInfo.AddWarning(Res.GetString("92c71f30-23bd-44d3-82c5-3930db1ebc81", "The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount."));
			}
		}

		protected override void CheckJR_EstimatedRevenue()
		{
			base.CheckJR_EstimatedRevenue();

			if (!Parent.JR_EstimatedRevenue.IsEmpty && Parent.IsInDatabase && Parent.JR_OSSellAmtInfo.HasChanges && !Parent.IsRevenuePosted)
			{
				Parent.JR_EstimatedRevenueInfo.AddWarning(Res.GetString("82f327a3-c8e8-43fa-a659-cbd4b0c29451", "The Amount will not be updated automatically because it has been previously saved and has currently a non-zero amount."));
			}
		}

		protected override void CheckJR_AK()
		{
			base.CheckJR_AK();
			if (Parent.ChequeBook != null)
			{
				Parent.ChequeBook.AddWarningSamePrinter(Parent.JR_AKInfo);
			}
		}

		protected override void CheckJR_GE()
		{
			base.CheckJR_GE();

			if (!Parent.IsInDatabase || Parent.JR_GEInfo.HasChanges || Parent.JR_ACInfo.HasChanges)
			{
				ListValidation.ErrorIfInvalidPK(Parent.JR_GEInfo, Parent.Departments);

				if (!Parent.JR_GEInfo.HasErrors())
				{
					CheckDepartmentIsValidForThisChargeCode(Parent.ChargeCode, Parent.Department, Parent.JR_GEInfo);
				}

				if (!Parent.JR_GEInfo.HasErrors() && Parent.InvoicingJob != null)
				{
					ValidateMiscDepartment(Parent.Department, Parent.JR_GEInfo);
				}
			}
		}

		protected override void CheckJR_JH_InternalJob()
		{
			base.CheckJR_JH_InternalJob();

			if (Parent.JR_JH_InternalJob.IsValid && Parent.ShouldCreateJRJ)
			{
				ClosedJobReopener?.ValidateClosedJob(Parent.JR_JH_InternalJobInfo, Parent.InternalInvoicingJob);
			}

			if (!Parent.IsOrgProxyAccountPosted
				&& (Parent.Job?.IsGatewayBillingJob() ?? false)
				&& Parent.RelatedJob != null
				&& GatewaySellToCostSynchroniser.IsGatewaySynchronizable(Parent))
			{
				Parent.JR_JH_InternalJobInfo.AddWarning(Res.GetString("c5e4d97a-233d-4320-ba51-4406f7433f89", "This charge relates to a single job {0}, but setting Internal Job to the current consol will post gateway revenue as cost on multiple shipments. Please review the ‘Related Job Number’ of this charge and confirm the selection is correct.", Parent.JR_Calc_RelatedJobNumber));
			}
		}

		protected override void CheckJR_IsARCashAdvance()
		{
			base.CheckJR_IsARCashAdvance();

			if (ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsReceivablesCashAdvanceFunctionalityEnabled && Parent.JR_IsARCashAdvance)
			{
				if (Parent.SellAccount == null || Parent.JR_OSSellAmt == 0)
				{
					Parent.JR_IsARCashAdvanceInfo.AddError(Res.GetString("1af6e282-1366-49af-a0a8-2af543822816", "Advance Payment cannot be requested until Debtor and OS Amount are specified. Please enter charge amount and debtor code."));
				}

				if (Parent.JR_OSSellAmt < 0)
				{
					Parent.JR_IsARCashAdvanceInfo.AddError(Res.GetString("f50ed1ed-04f6-4dc7-8f4a-d86e94c8a317", "Advance Payment cannot be requested for negative charge amounts. Please enter a positive charge amount."));
				}
			}
		}

		protected override void CheckJR_OSSellAmt()
		{
			base.CheckJR_OSSellAmt();

			if (Parent.ParentConsolRevenue?.UnApportionedAmount != 0 && Parent.IsUsedForApportionment)
			{
				Parent.JR_OSSellAmtInfo.AddError(Res.GetString("5d62cec0-aee5-422d-b0e6-534fa6fc5097", "The sum of the apportioned OS amounts must be equal to the consol sell OS amount."));
			}

			if (ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsReceivablesCashAdvanceFunctionalityEnabled && Parent.JR_IsARCashAdvance)
			{
				if (Parent.JR_OSSellAmt == 0)
				{
					Parent.JR_OSSellAmtInfo.AddError(EmptySellAmountErrorMessage);
				}
				else if (Parent.JR_OSSellAmt < 0)
				{
					Parent.JR_OSSellAmtInfo.AddError(NegativeSellAmountErrorMessage);
				}

				if (!(Parent.ARCashAdvanceRequirement.IsPending || Parent.ARCashAdvanceRequirement.IsCancelled) && !InvoiceTypeCalculationProvider.BillInLocalCurrency(Parent.JR_InvoiceType) && Parent.ARCashAdvanceRequirement.OSAmount > Parent.JR_Calc_OSSellAmtWithGST)
				{
					Parent.JR_OSSellAmtInfo.AddWarning(Res.GetString("8113a6a3-301f-48ad-8ada-e33eb7af99b1", "This charge has an active Advance Payment Request for {0} {1}. Please review that the charge amount is correct.", Parent.ARCashAdvanceRequirement.OSAmount, Parent.ARCashAdvanceRequirement.OSCurrency));
				}
			}
		}

		protected override void CheckJR_LocalSellAmt()
		{
			base.CheckJR_LocalSellAmt();

			if (ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsReceivablesCashAdvanceFunctionalityEnabled && Parent.JR_IsARCashAdvance)
			{
				if (Parent.JR_LocalSellAmt == 0)
				{
					Parent.JR_LocalSellAmtInfo.AddError(EmptySellAmountErrorMessage);
				}
				else if (Parent.JR_LocalSellAmt < 0)
				{
					Parent.JR_LocalSellAmtInfo.AddError(NegativeSellAmountErrorMessage);
				}

				if (!(Parent.ARCashAdvanceRequirement.IsPending || Parent.ARCashAdvanceRequirement.IsCancelled) && InvoiceTypeCalculationProvider.BillInLocalCurrency(Parent.JR_InvoiceType) && Parent.ARCashAdvanceRequirement.LocalAmount > Parent.TotalLocalRevenueAmount)
				{
					Parent.JR_LocalSellAmtInfo.AddWarning(Res.GetString("a6460cd4-e549-4de7-adf9-83134251a589", "This charge has an active Advance Payment Request for {0} {1}. Please review that the charge amount is correct.", Parent.ARCashAdvanceRequirement.LocalAmount, Parent.ARCashAdvanceRequirement.OSCurrency));
				}
			}
		}

		protected override void CheckJR_APInvoiceNum()
		{
			base.CheckJR_APInvoiceNum();

			if (!Parent.JR_APInvoiceNum.IsEmpty && !Parent.IsCostPosted && !Parent.JR_IsApportioned)
			{
				if (Parent.Job != null && Parent.Job.Parent != null && Parent.JR_OH_CostAccount.IsValid && Parent.IsCustomsCharge)
				{
					ICustomsJobInfoProvider parentAsIProvider = Parent.Job.Parent as ICustomsJobInfoProvider;

					if (parentAsIProvider != null)
					{
						ICustomsJobInfo customsJob = parentAsIProvider.GetCustomsJobInfo(Parent.Job.JH_GC);

						if (customsJob != null)
						{
							List<ZString> validAPInvoiceNums = new List<ZString>(customsJob.GetValidAPInvoiceNumsToMatchAndValidateAgainst());

							if (validAPInvoiceNums.Count > 0 && !validAPInvoiceNums.Exists(x => Parent.JR_APInvoiceNum.StartsWith(x)))
							{
								Parent.JR_APInvoiceNumInfo.AddWarning(Res.GetString("5d6fb3a4-cf67-4d6f-aed1-7280c1c6d7ee", "This AP invoice number is incorrect. For Customs Disbursement Charges, the AP invoice number should match one of the entry numbers of a declaration. You can leave this number blank and it will be filled in automatically when the system does auto-billing."));
							}
						}
					}
				}
			}
		}

		protected override void CheckJR_OH_CostAccount()
		{
			base.CheckJR_OH_CostAccount();

			if (Parent.JR_OH_CostAccount.IsValid
				&& Parent.JR_PaymentType == ReceiptTypes.eNettCreditCard
				&& Parent.CostAccount.ENettRegistrationNumber.IsEmpty)
			{
				Parent.JR_OH_CostAccountInfo.AddError(AccountingConstants.ENettErrorMessages.OrganisationNotRegisteredForENett);
			}
		}

		protected override void CheckJR_OH_SellAccount()
		{
			base.CheckJR_OH_SellAccount();

			if (ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsReceivablesCashAdvanceFunctionalityEnabled && Parent.JR_IsARCashAdvance && Parent.JR_OH_SellAccount == ZGuid.Empty)
			{
				Parent.JR_OH_SellAccountInfo.AddError(Res.GetString("02a88c65-ca94-486d-b736-04f973d5c25b", "Sell amount and Debtor cannot be blank when charge line is flagged 'Advance Payment Required'"));
			}
		}

		protected override void CheckJR_RX_NKSellInvoiceCurrency()
		{
			base.CheckJR_RX_NKSellInvoiceCurrency();

			if (ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsReceivablesCashAdvanceFunctionalityEnabled && Parent.JR_IsARCashAdvance && !string.IsNullOrEmpty(Parent.JR_RX_NKSellInvoiceCurrency) && !Parent.JR_IsRevenuePosted)
			{
				Parent.JR_RX_NKSellInvoiceCurrencyInfo.AddError(Res.GetString("eea34d27-b4d2-4258-99ac-45f9e23b33fd", "Advance Payments are not currently supported on charges with non-empty Sell Invoice Currency. Please either clear the Sell Invoice Currency, or remove the \"Advance Payment Required\" flag"));
			}
		}

		public void ValidateRow()
		{
			CheckRow();
		}

		protected virtual void CheckRow()
		{
			INotification warningMessage = new Notification(CargoWise.ComponentModel.NotificationType.Warning, Res.GetString("5fb8c3a7-43f3-4712-aa6b-e55ee98ff056", "Any changes will require authorization from your supervisor or accountant upon saving and/or posting."));
			Parent.RemoveRowNotification(warningMessage);

			var osSellAmountErrorMessage = Res.GetString("118835fd-852b-4a0d-addd-994c8f3ff0fc", "OS sell amount should be same as local sell amount when sell currency is local currency.");
			Parent.RemoveRowError(osSellAmountErrorMessage);

			var osCostAmountErrorMessage = Res.GetString("fdc0774d-2f04-4d6e-8e97-f1300ed1a2d4", "OS cost amount should be same as local cost amount when cost currency is local currency.");
			Parent.RemoveRowError(osCostAmountErrorMessage);

			var invoicingJob = Parent.InvoicingJob;
			if (invoicingJob != null)
			{
				var plugInData = invoicingJob.PlugInData;
				if (plugInData != null && Parent.HasChanges && plugInData.InvoicingSupporter.EditSecurityLock)
				{
					Parent.AddRowNotification(warningMessage);
				}
			}

			if (!Parent.IsRevenuePosted && !AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency(Parent.JR_OSSellCurrencyCode, Parent.JR_OSSellAmt, Parent.JR_LocalSellAmt))
			{
				Parent.AddRowError(osSellAmountErrorMessage);
			}

			if (!Parent.IsCostPosted && !AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency(Parent.JR_OSCostCurrencyCode, Parent.JR_OSCostAmt, Parent.JR_LocalCostAmt))
			{
				Parent.AddRowError(osCostAmountErrorMessage);
			}

			ValidateGatewayJRJ();
		}

		void ValidateGatewayJRJ()
		{
			Parent.RemoveRowError(AutoJobRevenueJournalCreator.JRJCreationErrorPrefix, true);

			(ChargeWithCost gatewayBillingSellCharge, ChargeWithCost[] costCharges) gatewaySellAndCosts = default;

			bool validateGatewayJRJ = AutoJRJRegistryStatusHelper.IsAutoJRJEnabled()
				&& !Parent.IsRevenuePosted
				&& GatewaySellToCostSynchroniser.IsGatewaySynchronizable(Parent)
				&& Parent.TryFindGatewaySellAndCostsFromGatewaySell(out gatewaySellAndCosts);

			if (validateGatewayJRJ)
			{
				var invalidReason = AutoJobRevenueJournalCreator.EnsureOneToManyJournalCanBeCreated(gatewaySellAndCosts);

				if (!string.IsNullOrWhiteSpace(invalidReason))
				{
					Parent.AddRowError(AutoJobRevenueJournalCreator.JRJCreationErrorPrefix + " " + invalidReason);
				}
			}
		}

		public void ValidateJR_OSSellInvoiceExRate_ForDisplay()
		{
			ValidateCalculatedProperty(Parent.JR_OSSellInvoiceExRate_ForDisplayInfo);
		}

		protected virtual void CheckJR_OSSellInvoiceExRate_ForDisplay()
		{
			if (Parent.BillInInvoiceCurrency
				&& !Parent.IsRevenuePosted
				&& (!Parent.SellInvoiceExchangeRate?.IsDeleted ?? true)
				&& Parent.JR_OSSellInvoiceExRate <= 0)
			{
				Parent.JR_OSSellInvoiceExRate_ForDisplayInfo.AddError(Res.GetString("E43833A4-CCE0-44ce-B46C-46A64BAEEF6E", "Sell Invoice Exchange Rate for Currency {0} must be greater than 0.", Parent.JR_RX_NKSellInvoiceCurrency));
			}
		}

		protected override bool AdditionalCheckForMandatoryGovChargeCode() => !Parent.JR_IsApportioned;

		protected override string JR_CostGovtChargeCodeMandatoryMessage
		{
			get
			{
				return Parent.InvoicingJob != null && !Parent.InvoicingJob.InvoicingAllowOverrideCostGovtChargeCode ?
						Res.GetString("f64c90d2-cade-492f-9aea-6cb574c01727", "You have not been granted security rights to {0}. Please enter a Cost Government Charge Code.", Parent.InvoicingJob.InvoicingAllowOverrideCostGovtChargeCodeSecurityDisplayTextPath) :
						base.JR_CostGovtChargeCodeMandatoryMessage;
			}
		}

		protected override string JR_SellGovtChargeCodeMandatoryMessage
		{
			get
			{
				return Parent.InvoicingJob != null && !Parent.InvoicingJob.InvoicingAllowOverrideSellGovtChargeCode ?
						Res.GetString("2e2d6f52-2d93-460d-bbd1-386c185534fa", "You have not been granted security rights to {0}. Please enter a Sell Government Charge Code.", Parent.InvoicingJob.InvoicingAllowOverrideSellGovtChargeCodeSecurityDisplayTextPath) :
						base.JR_SellGovtChargeCodeMandatoryMessage;
			}
		}

		protected override string JR_CostGovtChargeCodeWarningMessage
		{
			get
			{
				return Parent.JR_IsApportioned ?
						Res.GetString("ff0bb896-4c01-46df-a162-c7bd11eaa1ed", "Please enter a Cost Government Charge Code via Consolidation > Consol Costing tab.") :
						base.JR_CostGovtChargeCodeWarningMessage;
			}
		}

		protected override string JR_SellGovtChargeCodeWarningMessage
		{
			get
			{
				return Parent.JR_IsApportioned ?
						Res.GetString("cb7b9083-cfba-469b-a1ca-fbe7a1f24704", "Please enter a Sell Government Charge Code via Consolidation > Consol Costing tab.") :
						base.JR_SellGovtChargeCodeWarningMessage;
			}
		}

		public List<ZPropertyInfo> PropertyInfosNeedToBeValidateWhenPostCost
		{
			get
			{
				if (propertyInfosNeedToBeValidateWhenPostCost == null)
				{
					propertyInfosNeedToBeValidateWhenPostCost = new List<ZPropertyInfo>
					{
						Parent.JR_GBInfo,
						Parent.JR_GEInfo,
						Parent.JR_ACInfo,
						Parent.JR_DescInfo,
						Parent.JR_OSCostAmtInfo,
						Parent.JR_LocalCostAmtInfo,
						Parent.JR_OH_CostAccountInfo,
						Parent.JR_OSCostExRateInfo,
						Parent.JR_AT_CostGSTRateInfo,
						Parent.JR_APInvoiceDateInfo,
						Parent.JR_APInvoiceNumInfo,
						Parent.JR_PaymentDateInfo,
						Parent.JR_RX_NKCostCurrencyInfo,
						Parent.JR_PaymentTypeInfo,
						Parent.JR_AKInfo,
						Parent.JR_ABInfo,
						Parent.JR_PreventInvoicePrintGroupingInfo,
						Parent.JR_OSCostGSTAmt_CalcInfo,
						Parent.JR_CostGovtChargeCodeInfo,
						Parent.JR_A9_CostVATClassInfo,
						Parent.JR_CostPlaceOfSupplyInfo,
						Parent.JR_CostPlaceOfSupplyTypeInfo,
						Parent.JR_AW_CostWHTRateInfo,
						Parent.JR_CostReferenceInfo,
						Parent.JR_APDocumentReceivedDateInfo,
						Parent.JR_CostTaxDateInfo,
						Parent.JR_AgentDeclaredCostAmtInfo,
						Parent.JR_CostSupplyTypeInfo,
						Parent.JR_GB_CostTaxBranchInfo,
						Parent.JR_IsAPCashAdvanceInfo,
						Parent.JR_CAL_APLineInfo
					};
				}
				return propertyInfosNeedToBeValidateWhenPostCost;
			}
		}
		List<ZPropertyInfo> propertyInfosNeedToBeValidateWhenPostCost;

		public string[] ValidateCostPropertyInfos()
		{
			var result = System.Array.Empty<string>();
			foreach (ZPropertyInfo info in PropertyInfosNeedToBeValidateWhenPostCost)
			{
				((IBusinessObjectInternals)Parent).Validate(info);
				if (info.HasErrors())
				{
					result = info.Notifications.GetErrors().Select(n => n.Message).Distinct().ToArray();
					break;
				}
			}

			return result;
		}

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateRow();
			ValidateJR_OSSellInvoiceExRate_ForDisplay();
		}

		#region JR_AC

		protected override void CheckJR_AC()
		{
			base.CheckJR_AC();

			if (Parent.ChargeCode != null && !Parent.IsCostPosted && !Parent.IsRevenuePosted)
			{
				if (!Parent.IsInDatabase && ((Parent.Job != null) ? Parent.ChargeCode.AC_GC != Parent.Job.JH_GC : Parent.ChargeCode.AC_GC != GlbCompany.CurrentCompany.PK))
				{
					// We can remove this validation if no below exceptions come through.
					// It is no longer possible to have rates from an incorrect company as rates are now company specific - ZA.
					ZString companyCode = Parent.ChargeCode.Company != null ? Parent.ChargeCode.Company.GC_Code : (ZString)Res.GetString("2f0aa249-24e3-4d03-bd19-4597c15fd657", "Not Set");
					ErrorReporter.ReportOnce("ChargeFromWrongCompany", "A charge code from company [" + companyCode + "] was autorated on an invoice in company [" + GlbCompany.CurrentCompany.GC_Code + "]. Charge Code: " + Parent.ChargeCode.AC_Code);

					Parent.JR_ACInfo.AddError(Res.GetString("ee96d8f2-8fcc-4841-951a-7c6c99693cf5", "This charge code is not valid for the current company."));
				}

				if (!Parent.JR_ACInfo.HasErrors()
					&& !AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.IsEmpty
					&& Parent.SellAccount != null
					&& !Parent.SellAccount.ENettRegistrationNumber.IsEmpty
					&& Parent.ChargeCode.GetMappingForOrganisation(AccountingConfigurationRegistry.Instance.ENettRegistration.Value.OrganisationPK).IsEmpty)
				{
					Parent.JR_ACInfo.AddWarning(eNettHelper.NoEnettMappingError);
				}

				if (!Parent.JR_ACInfo.HasErrors() && ShouldCheckForGatewayChargesUsingSameChargeCode)
				{
					var multipleGatewayChargesUsingSameChargeCodeWarning = ResString.GetMultilingualString("44192dab-5b62-46b0-b472-9b1c4d814b60", "Gateway Sell Apportionment is posted only when there is only one unposted billing line with this charge code.");

					Parent.JR_ACInfo.AddWarning(multipleGatewayChargesUsingSameChargeCodeWarning);
				}
			}
		}

		bool ShouldCheckForGatewayChargesUsingSameChargeCode
		{
			get
			{
				var job = Parent.InvoicingJob;

				var isValidForGateway = job != null
					&& job.Charges.Count > 1
					&& job.Parent.IsGatewayBillingEnabled();

				var gatewayAgents = isValidForGateway ? job.Parent.GatewayAgent() : default;

				if (isValidForGateway && gatewayAgents != default)
				{
					return job.HasMultipleGatewayChargesUsingSameChargeCode(Parent, gatewayAgents);
				}

				return false;
			}
		}

		#endregion

		#region JR_DisplaySequence
		protected override void CheckJR_DisplaySequence()
		{
			base.CheckJR_DisplaySequence();

			if (!Parent.JR_DisplaySequenceInfo.HasErrors() && !Parent.IsRevenuePosted)
			{
				if (Parent.JR_DisplaySequence < 0)
				{
					Parent.JR_DisplaySequenceInfo.AddError(Res.GetString("c6b0f815-7fd9-4640-bd63-435b7d4aecb6", "Display Sequence should be between 0 and 32767"));
				}
			}
		}
		#endregion

		#region JR_InvoiceType

		protected override void CheckJR_InvoiceType()
		{
			base.CheckJR_InvoiceType();
			if (!Parent.JR_IsRevenuePosted)
			{
				if (Parent.SellAccount != null)
				{
					MandatoryValidation.CheckEntered(Parent.JR_InvoiceTypeInfo);
				}

				ListValidation.ErrorIfInvalidCode(Parent.JR_InvoiceTypeInfo, Parent.Lookups.InvoiceTypeList);

				switch (Parent.JR_InvoiceType)
				{
					case InvoiceTypesList.Codes.FreightInvoice:
					case InvoiceTypesList.Codes.FreightInvoice_Batching:
						if (!(Parent.IsFreight || Parent.IsCommentChargeCode))
						{
							Parent.JR_InvoiceTypeInfo.AddError(ErrorMessageForInvoice(Res.GetString("73a3bdd2-afa6-4f5a-8fed-f254c6de8964", "Freight Charges"), Parent.Lookups.InvoiceTypeList.GetDescriptionFromCode(Parent.JR_InvoiceType)));
						}
						if (!Parent.IsSellForeign)
						{
							Parent.JR_InvoiceTypeInfo.AddError(ErrorMessageForInvalidSellCurrency(Parent.JR_InvoiceType));
						}
						break;
					case InvoiceTypesList.Codes.ForeignCurrencyInvoice:
					case InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching:
					case InvoiceTypesList.Codes.DisbursementInForeignCurrency:
					case InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching:
					case AgencyInvoiceTypesList.Codes.ForeignCollect:
					case AgencyInvoiceTypesList.Codes.ForeignPrePaid:
						if (!Parent.IsSellForeign)
						{
							Parent.JR_InvoiceTypeInfo.AddError(ErrorMessageForInvalidSellCurrency(Parent.JR_InvoiceType));
						}
						break;
					case InvoiceTypesList.Codes.DoNotPost:
						if (Parent.InvoicingJob != null && Parent.InvoicingJob.PlugInData != null)
						{
							if (!Parent.IsInDatabase || Parent.JR_InvoiceTypeInfo.HasChanges)
							{
								ISecurityCheckpoint rootSecurity = Parent.InvoicingJob.PlugInData.InvoicingSupporter.JobInvoicingSecurity;
								ISecurityCheckpoint invoicingSecurity = rootSecurity != null ? rootSecurity.FindChild(rootSecurity.Code + SecurityCore.Invoicing) : null;
								ISecurityCheckpoint security = invoicingSecurity != null ? invoicingSecurity.FindChild(rootSecurity.Code + SecurityCore.AllowUseOfNONInvoiceType) : null;
								if (security != null && !security.IsAllowed)
								{
									Parent.JR_InvoiceTypeInfo.AddError(Res.GetString("f7eaa51e-7b6c-4675-9a7b-ffa2bcfd7e14", "You do not have sufficient security rights to change the invoice type to 'NON - Do Not Post.") + System.Environment.NewLine +
										Res.GetString("58e49898-7175-4bec-8e96-e0eef80aabf3", "Contact your system administrator for rights to set this charge to '{0}'.", Parent.Lookups.InvoiceTypeList.GetDescriptionFromCode(Parent.JR_InvoiceType)) + System.Environment.NewLine +
										Res.GetString("e70c5b99-136b-4595-8e02-1412540db0d2", "This security right can be found in the following location: '{0}'.", security.DisplayTextPathToSecurityRight));
								}
							}
						}
						break;
					case InvoiceTypesList.Codes.SelfBillingInvoice:
					case InvoiceTypesList.Codes.SelfBillingInvoice_Batching:
						if (AccountingConfigurationRegistry.Instance.DisallowSelectionOfSBRandSBDInvoiceType.Value && Parent.SellAccount != null && !Parent.SellAccount.CompanyData.OB_ARCustomerSelfBillsRevenue)
						{
							Parent.JR_InvoiceTypeInfo.AddError(Res.GetString("C09375F6-CD9F-4551-8F5A-F04DAE6FDF9C", "Invoice Type 'SBR' and 'SBD' cannot be selected as the Charge Debtor's Organization > A/R > Invoicing > Invoicing > Customer Self Bills check box is NOT ticked."));
						}
						break;
				}

				if (!Parent.JR_InvoiceTypeInfo.HasErrors() && Parent.ChargeCode != null)
				{
					if (!Parent.HasDeferredConfiguration && InvoiceTypeCalculationProvider.IsDeferredInvoiceType(Parent.JR_InvoiceType))
					{
						Parent.JR_InvoiceTypeInfo.AddError(Res.GetString("9f8a8422-a9a0-4bd1-a486-aa028db3ac6c",
							"There are deferred charges on this job that cannot post.  Please amend the Debtor's Periodic Invoicing setup and/or the Invoice Types used on charges on this job.  The {0} on this job has a Deferred Invoice Type however the Debtor's Periodic Invoicing configuration does not allow this charge code to be deferred.",
							Parent.ChargeCode.AC_Code));
					}
				}
			}

			if (ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsReceivablesCashAdvanceFunctionalityEnabled && Parent.JR_IsARCashAdvance && Parent.ARCashAdvanceRequestHeader != null && Parent.ARCashAdvanceRequestHeader.CAH_Status != CashAdvanceStatusCodes.RequestHeader.Cancelled)
			{
				var originalInvoiceType = Parent.JR_InvoiceTypeInfo.OriginalValue.ToString();
				var newInvoiceType = Parent.JR_InvoiceType;
				bool canRetrieveOriginalInvoiceType, canRetrieveNewInvoiceType;
				bool oldMappedValue, newMappedValue;
				canRetrieveOriginalInvoiceType = InvoiceTypeMap.TryGetValue(originalInvoiceType, out oldMappedValue);
				canRetrieveNewInvoiceType = InvoiceTypeMap.TryGetValue(newInvoiceType, out newMappedValue);

				if (originalInvoiceType != newInvoiceType && canRetrieveOriginalInvoiceType == canRetrieveNewInvoiceType && oldMappedValue != newMappedValue)
				{
					Parent.JR_InvoiceTypeInfo.AddError(Res.GetString("ddbb7337-122a-40c7-b55e-65b3d0d25590", "This charge has an active Advance Payment. Changing the Invoice Type will result in an invoice currency different to the Advance Payment currency. Please cancel the Advance Payment if you need to change the Invoice Type of this charge."));
				}
			}
		}

		ZString ErrorMessageForInvalidSellCurrency(ZString invoiceType)
		{
			return ErrorMessageForInvoice(Res.GetString("d8d2f2ce-2ee4-4650-bc59-812c3c3e9a0a", "charges with a foreign Sell currency"), Parent.Lookups.InvoiceTypeList.GetDescriptionFromCode(invoiceType));
		}

		ZString ErrorMessageForInvoice(ZString expectedType, ZString invoiceType)
		{
			return new ZString(Res.GetString("a08f1a7f-8c5b-42ec-94e5-c911238e940a", "Only {0} may appear on the {1}.", expectedType, invoiceType));
		}

		#endregion

		#region JR_PaymentType

		protected override void CheckJR_PaymentType()
		{
			base.CheckJR_PaymentType();
			if (!Parent.JR_IsPosted && !Parent.JR_PaymentType.IsEmpty && !Parent.MatchedWithTNFJournalNum.IsEmpty)
			{
				Parent.JR_PaymentTypeInfo.AddError(Res.GetString("7b59e320-e097-406e-9d5d-0b71a5d387c2", "Payment details cannot be entered as this AP Invoice Number matches an unpaid ‘Carried Forward’ journal’s payment reference. When this invoice is posted, it will automatically be matched against the journal, up to the value of the invoice. The unpaid journal transaction number is [{0}].", Parent.MatchedWithTNFJournalNum));
			}
			if (Parent.JR_PaymentType == ReceiptTypes.eNettCreditCard && !AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.Value)
			{
				Parent.JR_PaymentTypeInfo.AddError(Res.GetString("e7ee42b6-8261-40fc-b24b-444dec24eae3", "'Pay via ComPay Credit Card' payment type is not enabled."));
			}
		}

		#endregion

		#region RunAPInvoiceNumberExistsValidation

		protected override void RunAPInvoiceNumberExistsValidation()
		{
			base.RunAPInvoiceNumberExistsValidation();

			if (!Parent.JR_APInvoiceNum.IsEmpty && !IsCommentCharge)
			{
				if (Parent.JR_LocalCostAmt == 0 && Parent.JR_OSCostAmt == 0)
				{
					var apportionSplitCharge = Parent.Factory.GetBizOsForPK(Parent.PK.ToGuid()).FirstOrDefault(x => x is ApportionSplitCharge) as ApportionSplitCharge;
					if (apportionSplitCharge == null || apportionSplitCharge.JR_IsUsedForApportionment)
					{
						Parent.JR_APInvoiceNumInfo.AddError(Res.GetString("29bb6586-7c0f-49b3-a519-59bba3b53f8b", "Please add cost amounts to this charge"));
					}
				}
			}
		}

		#endregion

		#region Invoice Number Applicable

		protected override ZQuery InvoiceNumberApplicableQuery
		{
			get
			{
				ZQuery query = base.InvoiceNumberApplicableQuery;

				ZQuery additionalFilter = new ZQuery();
				additionalFilter.AddToFilter(JobChargeSchema.JR_E6, SQLComparisonOperator.NotEqual, null);
				additionalFilter.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_JH, SQLComparisonOperator.NotEqual, Parent.JR_JH);

				query.AddToFilter(additionalFilter, JoinCondition.And);
				return query;
			}
		}

		#endregion

		#region Cheque Number Applicable

		protected override ZQuery ChequeNoApplicableQuery
		{
			get
			{
				ZQuery query = base.ChequeNoApplicableQuery;

				ZQuery additionalFilter = new ZQuery();
				additionalFilter.AddToFilter(JobChargeSchema.JR_E6, SQLComparisonOperator.NotEqual, null);
				additionalFilter.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_JH, SQLComparisonOperator.NotEqual, Parent.JR_JH);

				query.AddToFilter(additionalFilter, JoinCondition.And);
				return query;
			}
		}

		protected override string ChequeNumberErrorMessage
		{
			get { return Res.GetString("fa4646fa-62e4-4cb5-ae1a-296df5f7ec2a", "The Check Number is already used on another job or apportionment."); }
		}

		#endregion

		#region HasChequeNumberBeenUsed

		protected override bool HasChequeNumberBeenUsed
		{
			get
			{
				if (Parent.BankAccount != null)
				{
					return Parent.BankAccount.HasChequeNumberBeenUsedOnAPayment(Parent.JR_ChequeNo, ZGuid.Empty);
				}
				else
				{
					return false;
				}
			}
		}

		#endregion

		#region Base Overrides

		protected override bool CheckForSellGSTRateMandatory()
		{
			return base.CheckForSellGSTRateMandatory() && (!IsCommentCharge || IsTaxTypeInvoice);
		}

		protected override bool IsCostGSTRateMandatory
		{
			get { return base.IsCostGSTRateMandatory && !IsCommentCharge; }
		}

		bool IsCommentCharge
		{
			get { return (Parent.ChargeCode != null && Parent.ChargeCode.IsComment); }
		}

		bool IsTaxTypeInvoice
		{
			get { return Parent.JR_InvoiceType == InvoiceTypesList.Codes.InvoicePerTaxCode || Parent.JR_InvoiceType == InvoiceTypesList.Codes.InvoicePerTaxCode_Batching; }
		}

		#endregion

		#region Error Messages

		protected override string InvoiceNumberErrorMessage
		{
			get { return Res.GetString("f59ad8ef-1546-4368-8bfc-b9b4501662f9", "The Invoice/Credit Note is already used on another job or apportionment."); }
		}

		protected override string GetDifferenceBetweenChargesForSameTransactionErrorMessage(ZPropertyInfo field, string expected)
		{
			return Res.GetString("ae5296cb-3e25-451c-91b9-c5d28fe3e09e", "Different to {0} entered on other charge for the same creditor and AP invoice number. Expected {1}", field.HumanReadableName, expected);
		}

		#endregion

		#region Rating Behavior Validation

		public void ValidateCostRatingBehavior()
		{
			ValidateCalculatedProperty(Parent.JR_Calc_CostRatingBehaviorInfo);
		}

		protected void CheckJR_Calc_CostRatingBehavior()
		{
			MandatoryValidation.CheckEntered(Parent.JR_Calc_CostRatingBehaviorInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JR_Calc_CostRatingBehaviorInfo, Parent.Lookups.RatingBehaviors_Cost);
		}

		public void ValidateSellRatingBehavior()
		{
			ValidateCalculatedProperty(Parent.JR_Calc_SellRatingBehaviorInfo);
		}

		protected void CheckJR_Calc_SellRatingBehavior()
		{
			MandatoryValidation.CheckEntered(Parent.JR_Calc_SellRatingBehaviorInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JR_Calc_SellRatingBehaviorInfo, Parent.Lookups.RatingBehaviors_Sell);
		}

		#endregion
	}
}

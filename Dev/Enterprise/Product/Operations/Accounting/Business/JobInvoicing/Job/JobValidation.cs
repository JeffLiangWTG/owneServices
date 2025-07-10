using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobValidation : JobHeaderValidation
	{
		public JobValidation(Job parent)
			: base(parent)
		{
		}

		public static string ContractDoesNotExistWarningMessage
		{
			get
			{
				return Res.GetString("933b14cf-8212-4abd-b0c4-64c1f172565d", "Client Contract with this number does not exist.");
			}
		}

		new Job Parent
		{
			get { return (Job)base.Parent; }
		}

		public override void ValidateAll()
		{
			using (new DisposableAction(() => IsRunningValidateAll = true, () => IsRunningValidateAll = false))
			{
				base.ValidateAll();
			}
			if (!(AccountingConfigurationRegistry.Instance.EnforceZeroBalanceDisbursements.Value.EnforceZeroBalanceDisbursementsValidationType == AccountingConstants.EnforceZeroBalanceDisbursementsOption.Default.Code)
					&& Parent.Charges.Where(e => e.IsDisbursementCharge).Any())
			{
				ValidateCostAndSellForDisbursementCharges();
			}
			ValidateJH_ProfitLoss();
			CheckGatewayConsolChildShipmentsJobsMutexesNotLocked();
			CheckHasCurrencyExchangeRate();
		}

		void ValidateCostAndSellForDisbursementCharges()
		{
			List<Charge> chargesFailingValidation = new List<Charge>();
			if (AccountingConfigurationRegistry.Instance.EnforceZeroBalanceDisbursements.Value.EnforceZeroBalanceDisbursementsValidationType == AccountingConstants.EnforceZeroBalanceDisbursementsOption.OSAmount.Code)
			{
				chargesFailingValidation.AddRange(GetChargesWithFailureForForeignAmount());
			}
			else if (AccountingConfigurationRegistry.Instance.EnforceZeroBalanceDisbursements.Value.EnforceZeroBalanceDisbursementsValidationType == AccountingConstants.EnforceZeroBalanceDisbursementsOption.LocalAmount.Code)
			{
				chargesFailingValidation.AddRange(GetChargesWithFailureForLocalAmount());
			}
			else if (AccountingConfigurationRegistry.Instance.EnforceZeroBalanceDisbursements.Value.EnforceZeroBalanceDisbursementsValidationType == AccountingConstants.EnforceZeroBalanceDisbursementsOption.Either.Code)
			{
				var result1 = GetChargesWithFailureForForeignAmount();
				var result2 = GetChargesWithFailureForLocalAmount();

				if (!result1.IsNullOrEmpty() && !result2.IsNullOrEmpty())
				{
					chargesFailingValidation.AddRange(result1.Where(c => !chargesFailingValidation.Contains(c)));
					chargesFailingValidation.AddRange(result2.Where(c => !chargesFailingValidation.Contains(c)));
				}
			}
			else if (AccountingConfigurationRegistry.Instance.EnforceZeroBalanceDisbursements.Value.EnforceZeroBalanceDisbursementsValidationType == AccountingConstants.EnforceZeroBalanceDisbursementsOption.Both.Code)
			{
				var result1 = GetChargesWithFailureForForeignAmount();
				var result2 = GetChargesWithFailureForLocalAmount();

				if (!result1.IsNullOrEmpty() || !result2.IsNullOrEmpty())
				{
					chargesFailingValidation.AddRange(result1.Where(c => !chargesFailingValidation.Contains(c)));
					chargesFailingValidation.AddRange(result2.Where(c => !chargesFailingValidation.Contains(c)));
				}
			}

			ZString message = Res.GetString("3888A400-8286-44BE-8BB9-4A89FB8851AD", @"For a disbursement charge, the sell amount and cost amount must be same.
Validation of disbursement charges is based on the configuration specified in Registry > Accounting > Job Invoicing > Enforce Zero Balance Disbursements");
			Parent.Charges.ForEach(c => c.ClearRowNotificationsContaining(message));
			if (!Parent.AllowSaveNonZeroBalanceDisbursements && (Parent.ShouldRaiseErrorForNonZeroBalanceDisbursementChargesEvenIfNoChanges || Parent.Charges.Any(e => e.HasChanges && ((BaseCharge)e).IsDisbursementCharge)))
			{
				chargesFailingValidation.ForEach(c => c.AddRowError(message));
			}
			else
			{
				chargesFailingValidation.ForEach(c => c.AddRowWarning(message));
			}
		}

		List<Charge> GetChargesWithFailureForForeignAmount()
		{
			var result = new List<Charge>();
			var groups1 = GetGroupsOfCharges(GroupingStrategyForCharges.ToCheckFailureForForeignAmountByCostCurrency);
			var groups2 = GetGroupsOfCharges(GroupingStrategyForCharges.ToCheckFailureForForeignAmountBySellCurrency);

			foreach (var group1 in groups1)
			{
				var group2 = groups2.FirstOrDefault(g => (g.Key.Equals(group1.Key)));

				if (group2.IsNullOrEmpty())
				{
					result.AddRange(group1.AsEnumerable().Where(c => !result.Contains(c)));
				}
				else if (!group1.Sum(e => e.JR_OSCostAmt).Equals(group2.Sum(e => e.JR_OSSellAmt)))
				{
					result.AddRange(group1.AsEnumerable().Where(c => !result.Contains(c)));
					result.AddRange(group2.AsEnumerable().Where(c => !result.Contains(c)));
				}
			}

			foreach (var group2 in groups2)
			{
				var group1 = groups1.FirstOrDefault(g => (g.Key.Equals(group2.Key)));

				if (group1.IsNullOrEmpty())
				{
					result.AddRange(group2.AsEnumerable().Where(c => !result.Contains(c)));
				}
			}

			return result;
		}

		List<Charge> GetChargesWithFailureForLocalAmount()
		{
			var result = new List<Charge>();
			var groups = GetGroupsOfCharges(GroupingStrategyForCharges.ToCheckFailureForLocalAmount);

			foreach (var group in groups)
			{
				var sumLocalCostAmt = ZDecimal.Zero;
				var sumLocalSellAmt = ZDecimal.Zero;
				foreach (var charge in group.AsEnumerable())
				{
					sumLocalCostAmt += charge.JR_LocalCostAmt;
					if (charge.IsSellLocal && charge.IsSellInvoiceForeign)
					{
						sumLocalSellAmt += charge.JR_LocalSellAmt;
					}
					else
					{
						sumLocalSellAmt += charge.JR_LocalSellAmt - charge.JR_CFXAmt;
					}
				}
				ZDecimal tolerance = AccountingConfigurationRegistry.Instance.EnforceZeroBalanceDisbursements.Value.MaximumVariance;
				if ((ZDecimal)Math.Abs(sumLocalSellAmt - sumLocalCostAmt) > tolerance)
				{
					result.AddRange(group.AsEnumerable());
				}
			}

			return result;
		}

#if DEBUG
		internal
#endif
		enum GroupingStrategyForCharges
		{
			ToCheckFailureForForeignAmountByCostCurrency,
			ToCheckFailureForForeignAmountBySellCurrency,
			ToCheckFailureForLocalAmount
		}

#if DEBUG
		internal
#endif
		IEnumerable<IGrouping<object, Charge>> GetGroupsOfCharges(GroupingStrategyForCharges strategy)
		{
			switch (strategy)
			{
				case GroupingStrategyForCharges.ToCheckFailureForForeignAmountByCostCurrency:
					return Parent.Charges.Where(e => e.IsDisbursementCharge).GroupBy(e => new { e.ChargeCode, e.Branch, e.Department, e.CostCurrency?.Code });

				case GroupingStrategyForCharges.ToCheckFailureForForeignAmountBySellCurrency:
					return Parent.Charges.Where(e => e.IsDisbursementCharge).GroupBy(e => new { e.ChargeCode, e.Branch, e.Department, e.SellCurrency?.Code });

				case GroupingStrategyForCharges.ToCheckFailureForLocalAmount:
					return Parent.Charges.Where(e => e.IsDisbursementCharge).GroupBy(e => new { e.ChargeCode, e.Branch, e.Department });

				default: return null;
			}
		}

		public void ValidateJH_ProfitLoss()
		{
			ValidateCalculatedProperty(Parent.JH_ProfitLossInfo);
		}

		void CheckGatewayConsolChildShipmentsJobsMutexesNotLocked()
		{
			var gatewayLockedChildShipmentsJobsErrorsMessage = GatewaySellToCostSynchroniser.GetGatewayApportionmentListingLockedChildShipmentsJobsErrorMessage(Parent);
			if (!gatewayLockedChildShipmentsJobsErrorsMessage.IsEmpty)
			{
				Parent.AddRowError(gatewayLockedChildShipmentsJobsErrorsMessage);
			}
		}

		#region JH_Status

		bool IsRunningValidateAll;

		protected override void CheckJH_Status()
		{
			base.CheckJH_Status();

			MandatoryValidation.CheckEntered(Parent.JH_StatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JH_StatusInfo, Parent.JobStatusList);
			ZString originalStatus = (ZString)Parent.JH_StatusInfo.OriginalValue;

			if (Parent.JH_Status == JobHeaderStatus.Closed.Code &&
				(!Parent.IsInDatabase || originalStatus != JobHeaderStatus.Closed.Code))
			{
				if (!Env.Security.CloseSingleJob.IsAllowed && !Env.Security.CloseMultipleJobs.IsAllowed)
				{
					Parent.JH_StatusInfo.AddError(ClosingJobSecurityErrorMessage);
				}
				else if (Parent.ContainsUnpostedApportionment)
				{
					Parent.JH_StatusInfo.AddError(GetClosingJobApportionedChargesPresenceErrorMessage(Parent.JH_JobNum));
				}
				else if (Parent.IsInDatabase && IsRunningValidateAll && ContainsUnpostedApportionmentInDataBase())
				{
					Parent.JH_StatusInfo.AddError(GetClosingJobApportionedChargesPresenceErrorMessage(Parent.JH_JobNum));
				}
				else if (CheckHasRelatedUnapprovedTransactions())
				{
					Parent.JH_StatusInfo.AddError(GetClosingJobUnapprovedTransactionErrorMessage(Parent.JH_JobNum));
				}
				else if (Parent.GetShouldJobBeClosedByDsbBatch())
				{
					Parent.JH_StatusInfo.AddError(GetClosingJobRelatedToDisbursementErrorMessage);
				}
			}

			var updateJobStatusMessage = JobStatusUpdateRestrictionRuleHelper.ValidateUpdateJobStatus(Parent);
			if (!string.IsNullOrEmpty(updateJobStatusMessage))
			{
				Parent.JH_StatusInfo.AddError(updateJobStatusMessage);
			}

			if (Parent.DoesChangingJobStatusTriggerRevenueRecognition)
			{
				Parent.JH_StatusInfo.AddWarning(Res.GetString("1fdeedb2-9bef-420b-b91c-ada52c173aa5",
									"Setting '{0}' status on the job {1} results in recognizing its revenue.\r\nYou would not be able to change this job status until you save or cancel the changes.", Parent.JH_Status, Parent.JH_JobNum));
			}

			if (!Parent.JH_StatusInfo.HasErrors() &&
				Parent.JH_ParentTableCode == JobConsolSchema.Constants.Prefix && !Parent.IsClosed &&
				Parent.PlugInData != null && !Parent.IsPluginDataDeleted &&
				Parent.PlugInData.InvoicingSupporter.ConsumerType != null &&
				Parent.PlugInData.InvoicingSupporter.ConsumerType == JobInvoicingConsumerTypes.ForwardingConsol)
			{
				Parent.JH_StatusInfo.AddError(Res.GetString("e0e47e8e-af1b-4e05-ac18-ef5e970decbd",
									"{0} is a legacy Gateway Billing Job which does not satisfy current requirements for Gateway Billing of a Gateway Agent Consol.  Please change the consol to be a valid Gateway Agent Consol or close this job.", Parent.JH_JobNum));
			}

			bool ContainsUnpostedApportionmentInDataBase()
			{
				var queryPayableLine = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
				queryPayableLine.AddToFilter(AccTransactionLinesSchema.AL_LineType, new[] { TransactionLineTypes.WIP, TransactionLineTypes.Accrual });

				var queryCheckingUnpostedPayable = new ZDBOnlyQuery(typeof(JobCharge));
				queryCheckingUnpostedPayable.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_AL_APLine, DBNull.Value);
				queryCheckingUnpostedPayable.AddSubQuery(JobChargeSchema.JR_AL_APLine, queryPayableLine, JoinCondition.Or);

				var queryJobCharge = new ZDBOnlyQuery(typeof(JobCharge));
				queryJobCharge.AddToFilter(JobChargeSchema.JR_JH, Parent.PK);
				queryJobCharge.AddToFilter(JobChargeSchema.JR_E6, SQLComparisonOperator.NotEqual, DBNull.Value);
				queryJobCharge.AddToFilter(queryCheckingUnpostedPayable, JoinCondition.And);
				return Parent.Factory.ExistsInDatabase(JobCharge.Schema.TableName, queryJobCharge);
			}
		}

		public bool CheckHasRelatedUnapprovedTransactions()
		{
			bool result = false;

			foreach (Charge charge in Parent.Charges)
			{
				if (!charge.JR_AL_APLine.IsEmpty && charge.APLine.AL_LineType == TransactionLineTypes.UnapprovedCost)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		public static string CompleteJobSecurityErrorMessage
		{
			get { return "\r\n\r\n" + Res.GetString("58eda6d1-f63c-46af-8091-00dec3728cf6", "You must revert the value of this field to its original value of 'CMP - Complete'"); }
		}

		public static string GetClosingJobUnapprovedTransactionErrorMessage(string jobNumber)
		{
			return Res.GetString("c04774ae-49da-47f1-aa45-8dc7d35d54cd", "You cannot close this Job {0} because it has related unapproved transactions. Please approve or delete these transactions.", jobNumber);
		}

		public static string GetClosingJobApportionedChargesPresenceErrorMessage(string jobNumber)
		{
			return Res.GetString("a19ffd8e-a0e6-4d10-a2b2-14c8cdabb996", "The job {0} contains apportioned charges and cannot be closed.\r\nPlease open consol and remove or post the apportionment(s) first and try again after closing and reopening the current form.", jobNumber);
		}

		public static string GetClosingJobRelatedToDisbursementErrorMessage
		{
			get { return Res.GetString("65A3BC87-36FB-44B3-B205-5C058C0D2D0A", $@"This job contains posted disbursement clearing balance and can only be closed via the Auto Job Closure process."); }
		}

		#endregion

		#region JH_GS_NKRepSales & JH_GS_NKRepOps

		protected override void CheckJH_GS_NKRepSales()
		{
			base.CheckJH_GS_NKRepSales();
			if (Parent.JH_GS_NKRepSales.IsEmpty && Parent.GetControllingCustomerForSaleRep() != null && Parent.GetDefaultSalesRep().IsEmpty)
			{
				Parent.JH_GS_NKRepSalesInfo.AddWarning(Res.GetString("29f5850a-2a4c-4c32-8a5c-423047efac31", "Controlling Customer does not have a Sales Rep assigned."));
			}
			else
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JH_GS_NKRepSalesInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.JH_GS_NKRepSalesInfo);
			if (!Parent.JH_GS_NKRepSales.IsEmpty && Parent.JH_GS_NKRepSalesInfo.OriginalValue.IsEmpty)
			{
				Parent.JH_GS_NKRepSalesInfo.AddWarning(Res.GetString("7a9bf616-9f0e-44ca-b12e-75a1344db301", "You must save this form to have this staff member recorded as the Sales Rep on this job. Currently this Sales Rep is NOT saved against this job."));
			}

			var originalValue = SalesRepValueWhenNotOverridden;
			if (Parent.JH_GS_NKRepSales != originalValue && !Parent.InvoicingAllowOverrideSalesRep)
			{
				var reportedValue = originalValue == string.Empty ? (ZString)Res.GetString("47619ec7-84b5-45d3-a29b-a2780fa3a5c3", "(none)") : originalValue;
				Parent.JH_GS_NKRepSalesInfo.AddError(Res.GetString("69C3BC45-1DCE-45CD-813A-B67E2036C773", "You do not have sufficient security rights to modify this field. You must reset the value to its previous value {0}", reportedValue));
			}
		}

		ZString SalesRepValueWhenNotOverridden
		{
			get
			{
				var originalValue = Parent.JH_GS_NKRepSalesInfo.OriginalValue;

				return (Parent.IsInDatabase && !Parent.JH_OA_LocalChargesAddrInfo.HasChanges && !originalValue.IsEmpty) ?
					(ZString)originalValue : Parent.GetDefaultSalesRep();
			}
		}

		protected override void CheckJH_GS_NKRepOps()
		{
			base.CheckJH_GS_NKRepOps();
			MandatoryValidation.CheckEntered(Parent.JH_GS_NKRepOpsInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JH_GS_NKRepOpsInfo);
		}

		#endregion

		#region JH_OA_LocalChargesAddr & AgentCollect

		string LocalClientText
		{
			get
			{
				if (Parent.CanCrossTradeDebtorDefaultingBeApplied)
				{
					return Parent.JobType?.PrepaidBillToPartyText ?? Res.GetString("DA6F1468-DD89-41D8-AB86-FF55CE4BE6AD", "Prepaid Bill-To Party");
				}
				else
				{
					return Parent.JobType?.LocalClientText ?? Res.GetString("3c5f5767-e44d-4b60-9339-42263cef710a", "Local Client");
				}
			}
		}

		string OverseasAgentText
		{
			get
			{
				if (Parent.CanCrossTradeDebtorDefaultingBeApplied)
				{
					return Parent.JobType?.CollectBillToPartyText ?? Res.GetString("E829B913-E540-4908-961A-D54CF13AAEE8", "Collect Bill-To Party");
				}
				else
				{
					return Parent.JobType?.OverseasAgentText ?? Res.GetString("34ade1d7-5cdc-4049-82b2-32057654e060", "Overseas Agent");
				}
			}
		}

#if DEBUG
		internal
#endif
 enum ValidationStartedBy
		{
			None,
			LocalChargesAddr,
			AgentCollectAddr
		}

#if DEBUG
		internal int CheckJH_OA_LocalChargesAddr_Count;
		internal int CheckJH_OA_AgentCollectAddr_Count;

		internal
#endif
 ValidationStartedBy validationStartedBy = ValidationStartedBy.None;

		protected override void CheckJH_OA_LocalChargesAddr()
		{
#if DEBUG
			CheckJH_OA_LocalChargesAddr_Count++;
#endif
			if (validationStartedBy == ValidationStartedBy.None)
			{
				validationStartedBy = ValidationStartedBy.LocalChargesAddr;
			}
			try
			{
				if (Parent.IsDeleted)
				{
					return;
				}

				base.CheckJH_OA_LocalChargesAddr();

				if (Parent.LocalCharges != null)
				{
					if (Parent.JH_OA_LocalChargesAddrInfo.OriginalValue.IsEmpty)
					{
						Parent.JH_OA_LocalChargesAddrInfo.AddWarning(Res.GetString("16c6b951-ed2f-4e99-9646-05dba4a17749", "You must save this form to have this organization recorded as the {0} on this job.  Currently this {0} is NOT saved against this job.", LocalClientText));
					}
					if (!Parent.LocalCharges.CompanyData.OB_IsDebtor)
					{
						Parent.JH_OA_LocalChargesAddrInfo.AddWarning(Res.GetString("0dcf0e99-ebfc-4856-814b-e788683e39e7", "In most cases, the {0} should be flagged as a Receivables organization.", LocalClientText));
					}
					if (!Parent.LocalCharges.OH_Code.IsEmpty && Parent.LocalCharges.CompanyData.OB_IsDebtor)
					{
						Parent.LocalCharges.CreditChecker.ValidateIsCreditLimitExceeded(Parent.JH_OA_LocalChargesAddrInfo, LedgerTypes.AccountsReceivable);
					}
				}

				var supporter = Parent.GetInvoicingSupporter();
				if (supporter != null)
				{
					var errormessage = supporter.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(Parent.LocalChargesPK);
					if (!errormessage.IsEmpty)
					{
						Parent.JH_OA_LocalChargesAddrInfo.AddError(errormessage);
					}
				}

				if (!Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed && Parent.JH_OA_LocalChargesAddr != (ZGuid)Parent.JH_OA_LocalChargesAddrInfo.OriginalValue && Parent.IsAnyRevenuePosted())
				{
					ZString originalOrganisationString = string.Empty;
					ZString originalAddressString = string.Empty;
					if (!Parent.JH_OA_LocalChargesAddrInfo.OriginalValue.IsEmpty)
					{
						var originalAddress = Parent.Factory.Load<OrgAddress>((ZGuid)Parent.JH_OA_LocalChargesAddrInfo.OriginalValue);
						if (originalAddress?.Header != null)
						{
							if (Parent.LocalCharges == null || Parent.LocalCharges.OH_Code != originalAddress.Header.OH_Code)
							{
								originalOrganisationString = originalAddress.Header.OH_Code;
							}
							originalAddressString = originalAddress.OA_Code;
						}
					}

					Parent.JH_OA_LocalChargesAddrInfo.AddError(Res.GetString("36dacb25-8fb9-4c5b-9976-6a8e2d82de23", "You cannot modify {0} address once a revenue charge is posted. {1}{2}\r\n{3}",
						LocalClientText,
						!originalOrganisationString.IsEmpty ? string.Format(CultureInfo.CurrentCulture, "\r\n{0}: '{1}'", originalResOrganisationString, originalOrganisationString) : string.Empty,
						!originalAddressString.IsEmpty ? string.Format(CultureInfo.CurrentCulture, "\r\n{0}: '{1}'", originalResAddressString, originalAddressString) : string.Empty,
						Env.Security.ModifyAddressContactAfterPostedCharge.ErrorMessageForNotAllowed));
				}

				ValidateOrganisation(Parent.JH_OA_LocalChargesAddrInfo);
				if (validationStartedBy != ValidationStartedBy.AgentCollectAddr)
				{
					ValidateJH_OA_AgentCollectAddr();
				}
			}
			finally
			{
				if (validationStartedBy == ValidationStartedBy.LocalChargesAddr)
				{
					validationStartedBy = ValidationStartedBy.None;
				}
			}
		}

		protected override void CheckJH_OA_AgentCollectAddr()
		{
#if DEBUG
			CheckJH_OA_AgentCollectAddr_Count++;
			if (Globals.IsTest && Parent.AddSomeErrorNotValidatedInSetParentCore_ForTestOnly)
			{
				Parent.JH_OA_AgentCollectAddrInfo.AddError("Error for test only");
			}
#endif
			if (validationStartedBy == ValidationStartedBy.None)
			{
				validationStartedBy = ValidationStartedBy.AgentCollectAddr;
			}
			try
			{
				if (Parent.IsDeleted)
				{
					return;
				}

				base.CheckJH_OA_AgentCollectAddr();

				if (Parent.AgentCollect != null)
				{
					if (Parent.JH_OA_AgentCollectAddrInfo.OriginalValue.IsEmpty)
					{
						Parent.JH_OA_AgentCollectAddrInfo.AddWarning(Res.GetString("ec348f9c-1df0-4e35-bc5e-85aed7bd06f4", "You must save this form to have this organization recorded as the {0} on this job.  Currently this {0} is NOT saved against this job.", OverseasAgentText));
					}
					if (!Parent.AgentCollect.CompanyData.OB_IsDebtor)
					{
						Parent.JH_OA_AgentCollectAddrInfo.AddWarning(Res.GetString("c1c83f36-ca26-4b8b-a67f-df344c0dbc33", "In most cases, the {0} should be flagged as a Receivables organization.", OverseasAgentText));
					}
					if (!Parent.AgentCollect.OH_Code.IsEmpty && Parent.AgentCollect.OH_IsDebtor)
					{
						Parent.AgentCollect.CreditChecker.ValidateIsCreditLimitExceeded(Parent.JH_OA_AgentCollectAddrInfo, LedgerTypes.AccountsReceivable);
					}
				}

				var supporter = Parent.GetInvoicingSupporter();
				if (supporter != null)
				{
					var errormessage = supporter.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(Parent.AgentCollectPK);
					if (!errormessage.IsEmpty)
					{
						Parent.JH_OA_AgentCollectAddrInfo.AddError(errormessage);
					}
				}

				if (!Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed && Parent.JH_OA_AgentCollectAddr != (ZGuid)Parent.JH_OA_AgentCollectAddrInfo.OriginalValue && Parent.IsAnyRevenuePosted())
				{
					ZString originalOrganisationString = string.Empty;
					ZString originalAddressString = string.Empty;
					if (!Parent.JH_OA_AgentCollectAddrInfo.OriginalValue.IsEmpty)
					{
						var originalAddress = Parent.Factory.Load<OrgAddress>((ZGuid)Parent.JH_OA_AgentCollectAddrInfo.OriginalValue);

						if (Parent.AgentCollect == null || Parent.AgentCollect.OH_Code != originalAddress.Header.OH_Code)
						{
							originalOrganisationString = originalAddress.Header.OH_Code;
						}
						originalAddressString = originalAddress.OA_Code;
					}

					Parent.JH_OA_AgentCollectAddrInfo.AddError(Res.GetString("43d4750e-af71-4ec3-a22b-4a67f1a341d2", "You cannot modify {0} address once a revenue charge is posted. {1}{2}\r\n{3}",
						OverseasAgentText,
						!originalOrganisationString.IsEmpty ? string.Format(CultureInfo.CurrentCulture, "\r\n{0}: '{1}'", originalResOrganisationString, originalOrganisationString) : string.Empty,
						!originalAddressString.IsEmpty ? string.Format(CultureInfo.CurrentCulture, "\r\n{0}: '{1}'", originalResAddressString, originalAddressString) : string.Empty,
						Env.Security.ModifyAddressContactAfterPostedCharge.ErrorMessageForNotAllowed));
				}

				ValidateOrganisation(Parent.JH_OA_AgentCollectAddrInfo);
				if (validationStartedBy != ValidationStartedBy.LocalChargesAddr)
				{
					ValidateJH_OA_LocalChargesAddr();
				}
			}
			finally
			{
				if (validationStartedBy == ValidationStartedBy.AgentCollectAddr)
				{
					validationStartedBy = ValidationStartedBy.None;
				}
			}
		}

		void ValidateOrganisation(ZPropertyInfo info)
		{
			if (!info.HasErrors())
			{
				var validateOrganisationResult = ValidateOrganisation(true);
				if (!validateOrganisationResult.IsEmpty)
				{
					info.AddError(validateOrganisationResult);
				}
			}
		}

		public ZString ValidateOrganisation(bool checkChargesCount)
		{
			var result = ZString.Empty;
			if (!Parent.LocalChargesPK.IsEmpty && !Parent.AgentCollectPK.IsEmpty &&
					Parent.LocalChargesPK == Parent.AgentCollectPK)
			{
				result = Res.GetString("960feb24-d4e1-4aee-8a14-c24c60b62b3e", "{0} and {1} must not be the same.", OverseasAgentText, LocalClientText);
			}

			// When creating a one off quote containing charges, the user can save even when the 'Local Client' field is empty.
			if (Parent.LocalChargesPK.IsEmpty && Parent.AgentCollectPK.IsEmpty
				&& (!checkChargesCount || Parent.Charges.Count > 0)
				&& !(Parent.JobType?.Code == JobInvoicingConsumerTypes.OneOffQuotationCode || Parent.OneOffQuote != null))
			{
				result = Res.GetString("8b756875-9d62-4c70-99b4-7b32ed23e6de", "Please enter {0} or {1}.", LocalClientText, OverseasAgentText);
			}
			return result;
		}

		#endregion

		#region JH_OC_LocalBillingContact

		protected override void CheckJH_OC_LocalBillingContact()
		{
			base.CheckJH_OC_LocalBillingContact();

			if (!Env.Security.ModifyAddressContactAfterPostedCharge.IsAllowed && Parent.JH_OC_LocalBillingContact != (ZGuid)Parent.JH_OC_LocalBillingContactInfo.OriginalValue && Parent.IsAnyRevenuePosted())
			{
				ZString originalOrganisationString = string.Empty;
				ZString originalContactString = string.Empty;
				if (!Parent.JH_OC_LocalBillingContactInfo.OriginalValue.IsEmpty)
				{
					var originalcontact = Parent.Factory.Load<OrgContact>((ZGuid)Parent.JH_OC_LocalBillingContactInfo.OriginalValue);

					if (Parent.LocalCharges == null || Parent.LocalCharges.OH_Code != originalcontact.Header.OH_Code)
					{
						originalOrganisationString = originalcontact.Header.OH_Code;
					}
					originalContactString = originalcontact.OC_ContactName;
				}

				Parent.JH_OC_LocalBillingContactInfo.AddError(Res.GetString("8a124154-c513-49f6-8ad0-dbbc32b07bfb", "You cannot modify {0} contact once a revenue charge is posted. {1}{2}\r\n{3}",
					LocalClientText,
					!originalOrganisationString.IsEmpty ? string.Format(CultureInfo.CurrentCulture, "\r\n{0}: '{1}'", originalResOrganisationString, originalOrganisationString) : string.Empty,
					!originalContactString.IsEmpty ? string.Format(CultureInfo.CurrentCulture, "\r\n{0}: '{1}'", originalResContactString, originalContactString) : string.Empty,
					Env.Security.ModifyAddressContactAfterPostedCharge.ErrorMessageForNotAllowed));
			}
		}

		#endregion

		string originalResOrganisationString
		{
			get
			{
				return Res.GetString("646f940e-b307-4ee3-a04e-792d01ee1ae7", "Original organization");
			}
		}

		string originalResAddressString
		{
			get
			{
				return Res.GetString("6d383718-93e4-46ff-adeb-406817a94e20", "Original address");
			}
		}

		string originalResContactString
		{
			get
			{
				return Res.GetString("e88ef231-b33e-440e-b69f-ad5954b7b33f", "Original contact");
			}
		}

		#region JH_GE

		protected override void CheckJH_GE()
		{
			base.CheckJH_GE();

			if (!Parent.IsInDatabase || Parent.JH_GEInfo.HasChanges)
			{
				ListValidation.ErrorIfInvalidPK(Parent.JH_GEInfo, Parent.Departments);

				if (!Parent.JH_GEInfo.HasErrors() &&
					(Parent.PlugInData == null || Parent.PlugInData.InvoicingSupporter.ConsumerType == null || Parent.PlugInData.InvoicingSupporter.ConsumerType.ValidateJobForMiscellaneousDepartment))
				{
					if (Parent.Department != null && Parent.Department.GE_Misc)
					{
						Parent.JH_GEInfo.AddError(Res.GetString("512e3889-d472-4965-bad4-1d4ae97e03a7", "Cannot issue job charges for a miscellaneous department."));
					}
				}
			}
		}

		#endregion

		#region JH_GB

		protected override void CheckJH_GB()
		{
			base.CheckJH_GB();
			ListValidation.ErrorIfInvalidPK(Parent.JH_GBInfo, Parent.Branches);
			GlbBranch brn = Parent.Factory.Load<GlbBranch>(Parent.JH_GB);
			if ((brn != null) && (!brn.GB_IsActive))
			{
				bool inDB = false;
				foreach (Charge ch in Parent.Charges)
				{
					if (ch.IsInDatabase)
					{
						inDB = true;
						break;
					}
				}
				if (!inDB)
				{
					if (!Parent.JH_GBInfo.HasErrors())
					{
						Parent.JH_GBInfo.AddError(Res.GetString("f28c66de-7932-4ce7-8618-5ca4e0f0e2ef", "This branch is inactive. Please select another branch"));
					}
				}
			}

			ValidateOnInvoicingSupporter(Parent.JH_GBInfo);
		}

		#endregion

		void ValidateOnInvoicingSupporter(ZPropertyInfo info)
		{
			var supporter = Parent.GetInvoicingSupporter();
			if (supporter != null)
			{
				supporter.ValidateJobProperty(info);
			}
		}

		#region JH_TH_NKQuoteNumber

		protected override void CheckJH_TH_NKQuoteNumber()
		{
			base.CheckJH_TH_NKQuoteNumber();

			if (Parent.JobType?.Code == JobInvoicingConsumerTypes.OneOffQuotationCode && (!Parent.JH_TH_NKQuoteNumber.IsEmpty && (Parent.JH_TH_NKQuoteNumberInfo.HasChanges || !Parent.IsInDatabase)))
			{
				Parent.JH_TH_NKQuoteNumberInfo.AddError(Res.GetString("74518fe8-262d-4dab-ae8f-19eb2afbeb32", "Quote Charges of subject One Off Quote could NOT be assigned with another One Off Quote."));
				return;
			}

			var parentOneOffQuote = Parent.OneOffQuote;
			if (!Parent.JH_TH_NKQuoteNumber.IsEmpty && parentOneOffQuote == null)
			{
				Parent.JH_TH_NKQuoteNumberInfo.AddError(Res.GetString("dfef1f37-e257-484c-84e9-de87219c8ae0", "Please enter a valid spot quotation number."));
			}

			if (parentOneOffQuote != null)
			{
				var query = new ZQuery(JobHeaderSchema.JH_TH_NKQuoteNumber, parentOneOffQuote.TH_QuoteNumber);
				query.AddToFilter(JobHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.AddToFilter(JobHeaderSchema.JH_GC, Parent.JH_GC);
				var existingOneOffQuoteJob = Parent.Factory.LoadTop1<JobHeader>(query);

				if (existingOneOffQuoteJob != null)
				{
					Parent.JH_TH_NKQuoteNumberInfo.AddError(Res.GetString("bcaa7726-d40e-4b98-858d-cba2e7398765", "This spot quote is already used on Job {0}.\r\nA Spot Quote can only be used on a single job for autorating purposes.", existingOneOffQuoteJob.JH_JobNum));
				}
				else if (!parentOneOffQuote.CurrentOneOffQuote.TT_QuoteApprovedByManager
					&& !Parent.ReadOnly
					&& !parentOneOffQuote.ReadOnly
					&& DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.Value)
				{
					Parent.JH_TH_NKQuoteNumberInfo.AddError(Res.GetString("2975d203-8630-452d-8c13-b9c100f5db74", "This spot quote must be approved before being used."));
				}
				else
				{
					var quoteJob = new Job.Loader(parentOneOffQuote).Load();

					if (quoteJob != null && Parent.LocalChargesPK != ZGuid.Empty && quoteJob.LocalChargesPK != Parent.LocalChargesPK)
					{
						Parent.JH_TH_NKQuoteNumberInfo.AddError(Res.GetString("1540afdf-bc24-47dc-8f22-c4e2909383aa",
							"This quote is invalid as it references a different Local Client."));
					}

					if (RatingDataRegistry.Instance.EnableOverseasAgentInOneOffQuote.Value
						&& quoteJob != null
						&& Parent.AgentCollectPK != ZGuid.Empty
						&& quoteJob.AgentCollectPK != Parent.AgentCollectPK)
					{
						Parent.JH_TH_NKQuoteNumberInfo.AddError(Res.GetString("8cfe360a-db53-4e54-abe9-9f0d9ba4d95c",
							"This quote is invalid as it references a different Overseas Agent."));
					}
				}
			}
		}

		#endregion

		#region Autopopulation

		public void ValidateInvoiceNum()
		{
			ValidateCalculatedProperty(Parent.InvoiceNumInfo);
		}

		protected virtual void CheckInvoiceNum()
		{
			if (Parent.InvoiceNum.IsDefault)
			{
				Parent.InvoiceNumInfo.AddError(Res.GetString("dd7b88e9-3d95-4a50-9426-91183fbd36c8", "Enter invoice number"));
			}
		}

		public void ValidateDocumentReceivedDate()
		{
			ValidateCalculatedProperty(Parent.DocumentReceivedDateInfo);
		}

		protected virtual void CheckDocumentReceivedDate()
		{
			if (Parent.DocumentReceivedDate.IsEmpty && !string.IsNullOrEmpty(Parent.InvoiceNum) && AccountingMasterFilesRegistry.Instance.DocumentReceivedDateMustBeEntered.Value)
			{
				Parent.DocumentReceivedDateInfo.AddError(Res.GetString("777CE0E3-2450-413A-A8BD-829774196092", "Enter document received date"));
			}
		}

		public void ValidateInvoiceDate()
		{
			ValidateCalculatedProperty(Parent.InvoiceDateInfo);
		}

		protected virtual void CheckInvoiceDate()
		{
			if (Parent.InvoiceDate.IsEmpty)
			{
				Parent.InvoiceDateInfo.AddError(Res.GetString("f553c474-f8c5-49fc-8d93-a527fe1531c5", "Enter invoice date"));
			}
		}

		public void ValidateInvoiceDueDate()
		{
			ValidateCalculatedProperty(Parent.InvoiceDueDateInfo);
		}

		protected virtual void CheckInvoiceDueDate()
		{
			if (Parent.InvoiceDueDate.IsEmpty)
			{
				Parent.InvoiceDueDateInfo.AddError(Res.GetString("e7b5c956-6652-4e21-94ad-f50d72a15871", "Enter payment date"));
			}
		}

		public void ValidateSupplierCostReference()
		{
			ValidateCalculatedProperty(Parent.SupplierCostReferenceInfo);
		}

		protected virtual void CheckSupplierCostReference()
		{
		}

		#endregion

		#region RevenueRecognitionDate

		public string GetRevenueRecognitionDateValidationErrors(bool showErrorForEmptyDate = true)
		{
			string errorMessages = string.Empty;
			string missedDateErrorPrefix = (NoResString)"missed date:";
			var errorList = new List<string>();

			Parent.InitializeParentFromGenericJobWithSettingDefaults();

			foreach (Charge charge in Parent.Charges)
			{
				if (charge.Validation.CheckEmptyRevenueRecognitionTypesForCost())
				{
					errorList.Add(BaseChargeValidation.EmptyRevenueRecognitionTypeErrorMessageForCost);
				}
				else if (!charge.IsCostRecognized)
				{
					string error = GetRevenueRecognitionDateValidationErrorCore(missedDateErrorPrefix + " {0}", charge.ChargeCode, charge.CostRecognition, false, charge.IsCostPosted, showErrorForEmptyDate);
					if (!string.IsNullOrEmpty(error))
					{
						errorList.Add(error);
					}
				}

				if (charge.Validation.CheckEmptyRevenueRecognitionTypesForSell())
				{
					errorList.Add(BaseChargeValidation.EmptyRevenueRecognitionTypeErrorMessageForSell);
				}
				else if (!charge.IsSellRecognized)
				{
					string error = GetRevenueRecognitionDateValidationErrorCore(missedDateErrorPrefix + " {0}", charge.ChargeCode, charge.SellRecognition, false, charge.IsRevenuePosted, showErrorForEmptyDate);
					if (!string.IsNullOrEmpty(error))
					{
						errorList.Add(error);
					}
				}
			}

			var unrecognisedLinesCollection = Parent.Factory.Load<TransactionLine>(new TransactionLinesCollection(Parent.Factory, Parent.GetUnrecognisedLinesQuery(null, null)).CompleteFilter);
			foreach (TransactionLine line in unrecognisedLinesCollection)
			{
				string error = line.AL_RevRecognitionType.IsEmpty ? EmptyRevenueRecognitionTypeOnJobRelatedLineErrorMessage :
					GetRevenueRecognitionDateValidationErrorCore(missedDateErrorPrefix + " {0}", line.ChargeCode, line.AL_RevRecognitionType, false, true, showErrorForEmptyDate);
				if (!string.IsNullOrEmpty(error))
				{
					errorList.Add(error);
				}
			}

			if (errorList.Count > 0)
			{
				var missedDateErrors = from warning in errorList
									   where warning.StartsWith(missedDateErrorPrefix)
									   orderby warning
									   select warning.Replace(missedDateErrorPrefix, string.Empty);
				var otherErrors = from warning in errorList
								  where !warning.StartsWith(missedDateErrorPrefix)
								  select warning;
				string missedDates = string.Join(",", missedDateErrors.Distinct().ToArray());

				ZStringBuilder errorToShow = new ZStringBuilder(otherErrors.Distinct());
				if (!string.IsNullOrEmpty(missedDates))
				{
					errorToShow.Append(Res.GetString("1A407BBA-9A18-48B7-8965-53DF69ED21F0", "The following information has not been recorded for this job. It is required for revenue recognition purposes:{0}.", missedDates));
				}

				if (!errorToShow.IsEmpty)
				{
					errorMessages = errorToShow.ToStringWithNewLineBetweenAppends();
				}
			}

			return errorMessages;
		}

		internal string GetRevenueRecognitionDateValidationError(string revenueRecognitionDateEmptyError, AccChargeCode chargeCode, ZString revenueRecognitionOptionOnlyAccepted)
		{
			return GetRevenueRecognitionDateValidationErrorCore(revenueRecognitionDateEmptyError, chargeCode, revenueRecognitionOptionOnlyAccepted);
		}

		internal string GetRevenueRecognitionDateValidationError(string revenueRecognitionDateEmptyError, AccChargeCode chargeCode, bool isDeferredRecognitionSupported = true, bool isPosting = false)
		{
			return GetRevenueRecognitionDateValidationErrorCore(revenueRecognitionDateEmptyError, chargeCode, isDeferredRecognitionSupported: isDeferredRecognitionSupported, isPosting: isPosting);
		}

		string GetRevenueRecognitionDateValidationErrorCore(
			string revenueRecognitionDateEmptyError,
			AccChargeCode chargeCode,
			string revenueRecognitionOptionOnlyAccepted = null,
			bool isDeferredRecognitionSupported = true,
			bool useStubIfRecognitionOptionIsNotExist = false,
			bool showErrorForEmptyDate = true,
			bool isPosting = false)
		{
			string validationError = string.Empty;

			if ((Parent.ConsumerTypeShouldCreateWIP(string.Empty) || Parent.ConsumerTypeShouldCreateAccrual(string.Empty) || isPosting) && chargeCode != null)
			{
				string recognitionType = revenueRecognitionOptionOnlyAccepted ?? Parent.GetRevenueRecognitionType(chargeCode);

				string revenueRecognitionOptionName = GetRevenueRecognitionOptionName(recognitionType);
				if (revenueRecognitionOptionName == NoRecognitionOptionFound)
				{
					if (string.IsNullOrEmpty(recognitionType) && string.IsNullOrEmpty(Parent.GetRevenueRecognitionType(chargeCode)))
					{
						validationError = InvalidRevenueRecognitionRegistrySetupErrorMessage;
					}
				}
				else
				{
					if (Parent.GetRevenueRecognitionDate(recognitionType).IsEmpty)
					{
						ZDateTime revenueRecognitionDate = Parent.AskShouldUseImmediateRevenueRecognisedDate(chargeCode, recognitionType, useStubIfRecognitionOptionIsNotExist);
						if (revenueRecognitionDate.IsEmpty)
						{
							bool recognitionDateCanBeEmpty = isDeferredRecognitionSupported &&
								Parent.JH_Status != JobHeaderStatus.Complete.Code && //JobHeaderStatus.Closed should not be here, because on posting we reopen closed job, so we can post unrecognized lines
								Parent.JH_Status != JobHeaderStatus.JobReadyForFinancialClosure.Code &&
								AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.Value;
							if (!recognitionDateCanBeEmpty && showErrorForEmptyDate)
							{
								validationError = string.Format(revenueRecognitionDateEmptyError, revenueRecognitionOptionName);
							}
						}
						else
						{
							validationError = GetRevenueRecognitionDateNotInGLPeriodError(revenueRecognitionDate);
						}
					}
				}
			}

			return validationError;
		}

		internal bool IsErrorsCanBeFixedBySettingNowDate(string errors)
		{
			return !errors.Contains(InvalidRevenueRecognitionRegistrySetupErrorMessage)
				&& !errors.Contains(EmptyRevenueRecognitionTypeOnJobRelatedLineErrorMessage)
				&& !errors.Contains(BaseChargeValidation.EmptyRevenueRecognitionTypeErrorMessageForCost)
				&& !errors.Contains(BaseChargeValidation.EmptyRevenueRecognitionTypeErrorMessageForSell);
		}

		string InvalidRevenueRecognitionRegistrySetupErrorMessage
		{
			get { return Res.GetString("4ED911A2-11E5-4D60-B3D1-06331B9AC84F", "You have not setup Revenue Recognition for this job type. Go to Registry -> {0} to configure Revenue Recognition.", ((IRegistryItemInternals)AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup).Location); }
		}

		internal static string EmptyRevenueRecognitionTypeOnJobRelatedLineErrorMessage
		{
			get { return Res.GetString("41da22f1-f1e1-4a89-95fb-2e09350aecd2", "The Job has related line with empty revenue recognition type. To fix it use menu item 'Recognize Revenue' form Job Invoicing menu."); }
		}

		internal string GetRevenueRecognitionDatePriorToExistingGLPeriodError(ZDateTime revenueRecognitionDate)
		{
			string resultError = "";
			if (revenueRecognitionDate > AccountingConstants.RevenueRecognitionDateConstants.Immediate &&
				revenueRecognitionDate < AccountingConstants.RevenueRecognitionDateConstants.MinSpecialDate)
			{
				ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_StartDate, SQLComparisonOperator.LessThanOrEqualTo, revenueRecognitionDate);
				filter.AddToFilter(JoinCondition.And, AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, Env.CurrentCompany.PK);
				AccPeriodManagement period = Parent.Factory.LoadTop1<AccPeriodManagement>(filter);
				resultError = period != null ? "" :
					Res.GetString("75E3B5AD-9FAA-4eda-ABBC-1CAB9923F94B", "The Revenue Recognition Date {0} cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date. A General Ledger Accounting Period cannot be created because it is earlier than the first period currently existing. Hit Yes to continue and Immediate revenue recognition treatment will be applied. Hit No to cancel saving and change the relevant operational dates.", revenueRecognitionDate.Date.ToShortDateString());
			}
			return resultError;
		}

		internal string GetRevenueRecognitionDateNotInGLPeriodError(ZDateTime revenueRecognitionDate)
		{
			string errorString = "";
			if (revenueRecognitionDate == AccountingConstants.RevenueRecognitionDateConstants.DateAfterLastPeriod)
			{
				errorString = Res.GetString("836E76D7-C5A7-4a4b-8D5C-1731CADDD3B3",
@"Please have your Accounting Department create Accounting Periods for the next year.
The Revenue Recognition Date cannot be set because an appropriate General Ledger Accounting Period has not been created.");
			}
			else if (revenueRecognitionDate != ZDateTime.Empty &&
					revenueRecognitionDate != AccountingConstants.RevenueRecognitionDateConstants.Immediate &&
					revenueRecognitionDate < AccountingConstants.RevenueRecognitionDateConstants.MinSpecialDate)
			{
				AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Parent.Factory);
				if (periodCalculator.GetPeriodManagementFromDate(revenueRecognitionDate) == null)
				{
					errorString = Res.GetString("891E662B-F6AC-4a52-AB0D-4B06023F716F",
@"Please have your Accounting Department create an appropriate Accounting Period.
The Revenue Recognition Date {0} cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date.",
						revenueRecognitionDate.Date.ToShortDateString());
				}
			}
			return errorString;
		}

		string GetRevenueRecognitionOptionName(string recognitionType)
		{
			string revenueRecognitionOptionName = NoRecognitionOptionFound;
			if (!string.IsNullOrEmpty(recognitionType))
			{
				ICodeDescription revenueRecognitionDateOption = RevenueRecognitionLookups.CompleteRecognitionDateOptionList[recognitionType];
				if (revenueRecognitionDateOption != null)
				{
					revenueRecognitionOptionName = string.Format("'{0}'", revenueRecognitionDateOption.Description);
				}
			}
			return revenueRecognitionOptionName;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not Visible To User")]
		const string NoRecognitionOptionFound = "No revenue recognition option";

		#endregion

		#region JH_ProfitLoss

		protected virtual void CheckJH_ProfitLoss()
		{
			if (AccountingMasterFilesRegistry.Instance.OrphanWIPOrACRDetection.Value
				&& Parent.HasOrphanWIPsorAccruals)
			{
				Parent.JH_ProfitLossInfo.AddWarning(Res.GetString("4cca37bb-396b-4e2c-be4e-974004abf8d6", "There are WIPs or Accruals linked to this Job which should be reversed but are not. As a result the Profit and Loss figure might not be accurate.\r\nPlease review the costs and revenues entered for this job."));
			}
		}

		#endregion

		#region JH_ProfitLossReasonCode

		protected override void CheckJH_ProfitLossReasonCode()
		{
			base.CheckJH_ProfitLossReasonCode();

			if (Parent.JobType == null || Parent.JobType.ProfitLossApplicable(Parent.PlugInData))
			{
				bool needCheckProfitLossReasonCodesEmptyError = false;
				if (IsProfitLossReasonCodeInvalidForThisJobStatus(Parent.JH_Status))
				{
					Parent.JH_ProfitLossReasonCodeInfo.AddError(Res.GetString("d8f227d3-2b8c-45fe-b4cc-4a9f436094ef", @"Please assign a Job Profit / Loss Reason Code to this job.
This job's Status requires a Reason Code because its Profit/Revenue Margin('{0}%') falls outside the tolerated margin threshold.", GetProfitMargin()));

					needCheckProfitLossReasonCodesEmptyError = true;
				}
				if (!Parent.JH_ProfitLossReasonCode.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.JH_ProfitLossReasonCodeInfo);

					needCheckProfitLossReasonCodesEmptyError = true;
				}
				if (needCheckProfitLossReasonCodesEmptyError && Parent.ProfitLossReasonCodes.Count == 0)
				{
					Parent.JH_ProfitLossReasonCodeInfo.AddError(Res.GetString("a07037ba-848a-40e7-9de5-5d26340df28f", "{0} registry item ({1}) is empty. Please set correct values.", AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.Caption, AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.Category));
				}
			}
		}

		public bool IsProfitLossReasonCodeInvalidForThisJobStatus(ZString status)
		{
			if (!Parent.JH_ProfitLossReasonCode.IsEmpty)
			{
				return false;
			}
			var pLRequiringReasonParameters = AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.GetFallBackValueAtAllLevels(Parent.JH_GC.IsValid ? Parent.JH_GC.ToGuid() : Guid.Empty, Parent.JH_GB.IsValid ? Parent.JH_GB.ToGuid() : Guid.Empty, Parent.JH_GE.IsValid ? Parent.JH_GE.ToGuid() : Guid.Empty);

			if (pLRequiringReasonParameters.JobStatusCollection.Count > 0 &&
				((ICodeDescriptionPairList)pLRequiringReasonParameters.JobStatusCollection).ContainsCode(status))
			{
				var profitMargin = GetProfitMargin();
				var isOverThreshold = profitMargin < pLRequiringReasonParameters.LossThreshold || profitMargin > pLRequiringReasonParameters.ProfitThreshold;
				return Parent.Charges.Count > 0 && isOverThreshold;
			}

			return false;
		}

		ZDecimal GetProfitMargin()
		{
			return Parent.JH_Status == JobHeaderStatus.Closed.Code ? Parent.JH_ProfitRevenueMarginPosted : Parent.JH_ProfitRevenueMargin;
		}

		#endregion

		#region Client contract number

		protected override void CheckJH_ClientContractNumber()
		{
			base.CheckJH_ClientContractNumber();

			if (!Parent.JH_ClientContractNumber.IsEmpty
				&& ObjectFactory.Get<IContractPermissions>().IsCarrierAndClientContractModulesEnabled())
			{
				var clientContractNumberValidation = ObjectFactory.Get<IRatingContractValidationHelper>();
				if (!clientContractNumberValidation.DoesClientContractNumberExist(Parent.Factory, Parent.JH_ClientContractNumber))
				{
					Parent.JH_ClientContractNumberInfo.AddWarning(ContractDoesNotExistWarningMessage);
				}
			}
		}

		#endregion

		#region ExchangeRates

		void CheckHasCurrencyExchangeRate()
		{
			Parent.ClearRowNotificationsContaining(AccountingUtils.GetElectronicProcessingChargeCurrencyNoExchangeRateErrorMessage(Parent));

			if (!ObjectFactory.Get<IElectronicProcessingChargeProvider>().HasElectronicProcessingChargeCurrencyExchangeRate(Parent))
			{
				Parent.AddRowError(AccountingUtils.GetElectronicProcessingChargeCurrencyNoExchangeRateErrorMessage(Parent));
			}
		}

		#endregion
	}
}

using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ApportionSplitChargeValidation : BaseChargeValidation
	{
		internal string ReopenClosedJobSecurityMessage
		{
			get { return Res.GetString("c23b2366-4426-43da-9d78-bea8486d03ed", "This job is currently closed. As you do not have the access right to re-open the Job, you will require authorization to proceed upon saving this apportionment."); }
		}

		internal string ReopenClosedJobWarningMessage
		{
			get { return Res.GetString("6c96992b-b25d-4c8c-b0c6-acdd92903ab2", "This job is currently closed. The job will be reopened after saving this apportionment."); }
		}

		internal string ApportionedAmountSumErrorMessage
		{
			get { return Res.GetString("a1df05c4-23b9-468d-a68b-e36332234c6a", "The sum of the apportioned OS amounts must be equal to the consol cost OS amount."); }
		}

		internal string ApportionedLocalAmountSumErrorMessage
		{
			get { return Res.GetString("23f8f2f2-cdc4-4000-93e3-e37450397616", "The sum of the apportioned local amounts must be equal to the consol cost local amount."); }
		}

		internal string GetNoOperationalJobWarningMessage(String jobNumber)
		{
			return Res.GetString("a818c29f-743a-46d8-bdec-cb521b47779c", "This posted apportioned charge belongs to job: {0} which is no longer attached to the consol or is inactive.", jobNumber);
		}

		internal string GetNoOperationalJobErrorMessage(String jobNumber)
		{
			return Res.GetString("0659b952-9ce7-4eb2-a8d3-62a8e5db50e4", "This apportioned charge belongs to job: {0} which is no longer attached to the consol or is inactive. Please untick Is Used or re-attach job to consol/activate job.", jobNumber);
		}

		public ApportionSplitChargeValidation(ApportionSplitCharge parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public void ValidateJR_JobNumber()
		{
			ValidateCalculatedProperty(Parent.JR_JobNumberInfo);
		}

		protected override void CheckJR_AC()
		{
			base.CheckJR_AC();

			var chargeCode = Parent.ChargeCode;
			if (!Parent.IsCostPosted && chargeCode != null)
			{
				var chargeTypeOverride = Job.GetChargeTypeInformation(chargeCode, Parent.InvoicingJob);
				var effectiveChargeType = chargeTypeOverride.AN_ChargeType;

				if (!AccChargeCode.IsValidInConsolCosting(effectiveChargeType))
				{
					Parent.JR_ACInfo.AddError(Res.GetString("b906b5fe-c138-4ab1-ac46-61f0a9576a01", "This charge code has a charge type of '{0}' and cannot be used in consol costing.", effectiveChargeType));
				}

				chargeTypeOverride = null;
			}
		}

		protected void CheckJR_JobNumber()
		{
			if (Parent.Job != null &&
				Parent.Job.JH_Status == JobHeaderStatus.Closed.Code &&
				Parent.JR_OSCostAmt != 0 &&
				Parent.JR_IsUsedForApportionment)
			{
				if (JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(Parent.Factory, Parent.InvoicingJob))
				{
					Parent.JR_JobNumberInfo.AddWarning(ReopenClosedJobWarningMessage);
				}
				else
				{
					Parent.JR_JobNumberInfo.AddWarning(ReopenClosedJobSecurityMessage);
				}
			}
		}

		protected override void CheckJR_GEIsNotEmpty()
		{
			base.CheckJR_GEIsNotEmpty();
			if (Parent.IsInDatabase && Parent.IsRevenuePosted)
			{
				if (Parent.JR_GE != (ZGuid)Parent.JR_GEInfo.OriginalValue)
				{
					Parent.JR_GEInfo.AddError(Res.GetString("8219b129-c162-47b5-8ea5-d65d2ea1893f", "The revenue for this charge line is posted. You cannot change the department."));
				}
			}
		}

		protected override void CheckJR_JH()
		{
			if (!Parent.JR_JH.IsValid || Parent.InvoicingJob == null)
			{
				Parent.JR_JHInfo.AddError(Res.GetString("8698a574-743a-4ad2-9fb1-3471b88c49c7", "No invoicing job is associated with this job charge. Invoicing job could be deleted while you were working on this form. Please close the form and reopen."));
			}

			base.CheckJR_JH();
		}

		protected override void CheckJR_GB()
		{
			base.CheckJR_GB();
			if (Parent.IsInDatabase && Parent.IsRevenuePosted)
			{
				if (Parent.JR_GB != (ZGuid)Parent.JR_GBInfo.OriginalValue)
				{
					Parent.JR_GBInfo.AddError(Res.GetString("c4d0df91-9401-4cab-a303-73d59baeb5e8", "The revenue for this charge line is posted. You cannot change the branch."));
				}
			}

			if (!Parent.JR_GBInfo.HasErrors())
			{
				if (AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.Value.EnableBranchLevelPosting)
				{
					if (!Parent.ParentConsolCost.IsPosted && !BranchLevelPostingHelper.DoesAllBranchesBelongToSamePostingGroup(AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting
						, Parent.ParentConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Where(x => x.JR_IsUsedForApportionment).Select(x => x.JR_GB).ToHashSet()))
					{
						Parent.JR_GBInfo.AddError(Res.GetString("0c97e439-c98b-41a4-89f0-8960316c1dac", @"Please review the Consol Cost allocations and ensure all charges within each apportionment are assigned branch from the same Posting Group. All charges posted in one transaction must be within the same Branch Posting Group.
Saving is prevented because charges for the same apportionment have been entered using a mix of branch Posting Groups."));
					}
				}
			}

			if (!Parent.JR_GBInfo.HasErrors())
			{
				var apportionmentCharges = Parent.ParentConsolCost?.ApportionmentCharges?.Cast<ApportionSplitCharge>().Where(x => x.JR_IsUsedForApportionment);

				if (AutoJRJRegistryStatusHelper.IsAutoJRJWithTaxRegistrationNumberEnabled()
					&& Parent.JR_IsUsedForApportionment
					&& Parent.ParentConsolCost != null
					&& !Parent.ParentConsolCost.IsPosted
					&& (Parent.ParentConsolCost.Creditor != null && Parent.ParentConsolCost.Creditor.IsProxyOrg(Parent.Company))
					&& !Parent.IsGatewaySellApportionmentCharge
					&& !Parent.AreInternalFieldsEmpty()
					&& !Parent.IsExcludedFromAutoJRJ(Parent.ParentConsolCost.Creditor)
					&& apportionmentCharges != null
					&& apportionmentCharges.Any(x => x.IsExcludedFromAutoJRJ(Parent.ParentConsolCost.Creditor))
					)
				{
					Parent.JR_GBInfo.AddError(Res.GetString("C8B9F407-B25E-4270-AADB-A891ACFE099E", @"At least one apportioned charge contains a Branch with a different tax registration to the Creditor.
Please clear the Internal Job/Branch/Department to allow posting this apportioned charge as an AP Invoice."));
				}
			}
		}

		protected override void CheckJR_AB()
		{
			if (!Parent.JR_ABInfo.HasErrors())
			{
				AccBankAccountCollection bankAccounts = Parent.BankAccounts;
				bankAccounts.Load();
				if (!bankAccounts.Contains(Parent.JR_AB))
				{
					ListValidation.ErrorIfInvalidPK(Parent.JR_ABInfo, Parent.BankAccounts, ResString.GetMultilingualString("a66848dd-b382-432e-907f-720564f7de6e", "Please select bank account with appropriate branch and the same currency as the Consol Costing's AP Invoice."));
				}
			}
		}

		protected override void CheckJR_ABIsValidZGuid()
		{
			if (!Parent.JR_AB.IsMissing)
			{
				base.CheckJR_ABIsValidZGuid();
			}
		}

		protected override void CheckJR_OSCostAmt()
		{
			base.CheckJR_OSCostAmt();
			if (Parent.ParentConsolCost != null && !Parent.ParentConsolCost.IsDeleted &&
				(!Parent.ParentConsolCost.IsPosted || Parent.ParentConsolCost.IsApprovingPosting))
			{
				if (Parent.ParentConsolCost.IsApprovingPosting &&
					Parent.JR_OSCostAmt == 0 && (ZDecimal)Parent.JR_OSCostAmtInfo.OriginalValue != 0)
				{
					Parent.JR_OSCostAmtInfo.AddError(Res.GetString("0f3e4e7f-086b-4479-9ec2-ecb5a53ea296", "The value can not be set to zero. The original value was not equal zero."));
				}
				else if (Parent.ParentConsolCost.UnApportionedAmount != 0 && Parent.JR_IsUsedForApportionment)
				{
					Parent.JR_OSCostAmtInfo.AddError(ApportionedAmountSumErrorMessage);
				}
			}
			ValidateJR_JobNumber();
		}

		protected override void CheckJR_LocalCostAmt()
		{
			base.CheckJR_LocalCostAmt();
			if (Parent.ParentConsolCost != null && !Parent.ParentConsolCost.IsDeleted &&
				(!Parent.ParentConsolCost.IsPosted || Parent.ParentConsolCost.IsApprovingPosting))
			{
				if (Parent.ParentConsolCost.ApportionmentCharges.JR_LocalCostAmtSum != Parent.ParentConsolCost.E6_LocalCostAmount)
				{
					Parent.JR_LocalCostAmtInfo.AddError(ApportionedLocalAmountSumErrorMessage);
				}
			}
		}

		protected override void CheckJR_CostRatingOverrideComment()
		{
		}

		public void ValidateJR_IsUsedForApportionment()
		{
			ValidateCalculatedProperty(Parent.JR_IsUsedForApportionmentInfo);
		}

		protected void CheckJR_IsUsedForApportionment()
		{
			if (Parent.ShouldValidateBranchAndDepartment)
			{
				ValidateJR_JobNumber();

				if (Parent.ShipmentInfo == null)
				{
					if (Parent.JR_IsCostPosted)
					{
						Parent.JR_IsUsedForApportionmentInfo.AddWarning(GetNoOperationalJobWarningMessage(Parent.JR_JobNumber));
					}
					else
					{
						Parent.JR_IsUsedForApportionmentInfo.AddError(GetNoOperationalJobErrorMessage(Parent.JR_JobNumber));
					}
				}
			}
			ValidateJR_GB();
		}

		protected override void CheckJR_GE()
		{
			base.CheckJR_GE();

			if (!Parent.IsInDatabase || Parent.JR_GEInfo.HasChanges || Parent.JR_ACInfo.HasChanges)
			{
				if (!Parent.JR_GEInfo.HasErrors() && Parent.ChargeCode != null && Parent.JR_IsUsedForApportionment)
				{
					CheckDepartmentIsValidForThisChargeCode(Parent.ChargeCode, Parent.Department, Parent.JR_GEInfo);
					ValidateMiscDepartment(Parent.Department, Parent.JR_GEInfo);
				}
			}
		}

		protected override void CheckJR_JH_InternalJob()
		{
			base.CheckJR_JH_InternalJob();

			if (!Parent.JR_JH_InternalJobInfo.HasErrors()
				&& Parent.JR_JH_InternalJob.IsValid
				&& Parent.InternalInvoicingJob != null
				&& Parent.InternalInvoicingJob.IsClosed
				&& Parent.ShouldCreateJRJ)
			{
				Parent.JR_JH_InternalJobInfo.AddError(Res.GetString("a6c04e17-5796-4150-93c6-2b9e91aa102e", "This job is closed. You cannot set the internal job to be a closed job."));
			}
		}

		public void ValidateIsFinal()
		{
			ValidateCalculatedProperty(Parent.IsFinalInfo);
		}

		protected virtual void CheckIsFinal()
		{
			if (!Parent.IsInDatabase)
			{
				if (Parent.IsFinal && !Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed)
				{
					Parent.IsFinalInfo.AddError(Res.GetString("44021F37-2CCA-49F7-A717-4D23D6C3AB7D", @"You do not have appropriate security rights to tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Tick Final Flag on Payables Invoice"));
				}

				if (Parent.ParentConsolCost != null)
				{
					var apInvoice = Parent.ParentConsolCost.ParentAPInvoice as APInvoice;
					if (apInvoice != null && apInvoice.CostVarianceApprovalHelper.AutoTickFinalFlag && Parent.IsFinalDefault && !Parent.IsFinal && !Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed)
					{
						Parent.IsFinalInfo.AddError(Res.GetString("4ACD0255-5C32-4541-8288-FB04C1A601B4", @"You do not have appropriate security rights to un-tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Untick Auto-ticked Final Flag"));
					}
				}
			}
		}

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateIsFinal();
			ValidateJR_IsUsedForApportionment();
		}

		protected override bool AdditionalCheckForMandatoryGovChargeCode() => Parent.JR_IsUsedForApportionment;

		protected new ApportionSplitCharge Parent;
	}
}

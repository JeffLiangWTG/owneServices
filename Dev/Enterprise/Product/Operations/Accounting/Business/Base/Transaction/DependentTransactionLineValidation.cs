using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public partial class DependentTransactionLineValidation : TransactionLineValidation
	{
		public DependentTransactionLineValidation(DependentTransactionLine parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAL_OSGSTAmount();
			if (DependentTransactionLine.IsMultiSubAccountsSupported)
			{
				ValidateAL_Calc_FirstSubClassParentId();
				ValidateAL_Calc_SecondSubClassParentId();
			}
		}

		#region AL_LocalTaxAmount

		protected override void CheckAL_LocalTaxAmount()
		{
			base.CheckAL_LocalTaxAmount();
			if (!AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency(DependentTransactionLine.AL_RX_NKTransactionCurrency, DependentTransactionLine.AL_OSTaxAmount,
				DependentTransactionLine.AL_LocalTaxAmount))
			{
				DependentTransactionLine.AL_LocalTaxAmountInfo.AddError(Res.GetString("970c4a4d-198c-40c7-8109-26defbc8de27", "The Local Tax Amount should be equal to OS Tax Amount when Local Currency is used"));
			}
		}

		#endregion

		#region AL_LocalExTaxAmount

		protected override void CheckAL_LocalExTaxAmount()
		{
			DependentTransactionLine.ClearRowNotifications();
			base.CheckAL_LocalExTaxAmount();
			if (DependentTransactionLine.AL_OSExTaxAmount != 0m &&
					DependentTransactionLine.AL_LocalExTaxAmount == 0m &&
					DependentTransactionLine.AL_ExchangeRate != 0m &&
					!DependentTransactionLine.AL_RX_NKTransactionCurrency.IsEmpty &&
					Env.CurrentCompany.ExchangeRate.ForeignToLocal(DependentTransactionLine.AL_OSExTaxAmount, DependentTransactionLine.AL_ExchangeRate) != 0m)
			{
				DependentTransactionLine.AddRowError(Res.GetString("08f05ade-1d9d-4688-b427-eb846b862fce", "Local Amount has been set to zero when foreign amount is valid."));
			}
			else if (!AccountingMasterFilesUtils.IsForeignAndLocalAmountSameWhenUsingLocalCurrency(DependentTransactionLine.AL_RX_NKTransactionCurrency, DependentTransactionLine.AL_OSExTaxAmount,
					DependentTransactionLine.AL_LocalExTaxAmount))
			{
				DependentTransactionLine.AL_LocalExTaxAmountInfo.AddError(Res.GetString("e0c5fbbb-7679-46c2-bc03-28c3a35c2c61", "The Local Amount should be equal to OS Amount when Local Currency is used"));
			}
		}

		#endregion

		protected static string AmountAndGSTAmountMustHaveSameSign
		{
			get { return Res.GetString("417774fe-6ba8-463b-ae9a-47d8634c96ab", "Amount and GST amount should have the same sign."); }
		}

		DependentTransactionLine DependentTransactionLine
		{
			get { return (DependentTransactionLine)Parent; }
		}

		#region AL_OSGSTAmount

		public void ValidateAL_OSGSTAmount()
		{
			ValidateCalculatedProperty(DependentTransactionLine.AL_OSGSTAmountInfo);
		}

		protected virtual void CheckAL_OSGSTAmount()
		{
			if (ShouldValidateTaxAmountSign)
			{
				if ((DependentTransactionLine.AL_OSExTaxAmount > 0 && DependentTransactionLine.AL_OSGSTAmount < 0) || (DependentTransactionLine.AL_OSExTaxAmount < 0 && DependentTransactionLine.AL_OSGSTAmount > 0))
				{
					DependentTransactionLine.AL_OSGSTAmountInfo.AddError(AmountAndGSTAmountMustHaveSameSign);
				}
			}
		}

		#endregion

		protected override void CheckAL_AG()
		{
			base.CheckAL_AG();

			if (DependentTransactionLine.GLHeader != null && (
					DependentTransactionLine.GenericTransactionCharge == null || (
						!DependentTransactionLine.GenericTransactionCharge.IsDeleted &&
						DependentTransactionLine.GenericTransactionCharge.VC_IsGLAccount)))
			{
				GLAccountHelper.CheckLocalAccountDescriptorHasMapping(DependentTransactionLine);
			}
		}

		#region AL_Calc_FirstSubClassParentId

		public void ValidateAL_Calc_FirstSubClassParentId()
		{
			ValidateCalculatedProperty(DependentTransactionLine.AL_Calc_FirstSubClassParentIdInfo);
		}

		protected void CheckAL_Calc_FirstSubClassParentId()
		{
			SubAccountHelper.ValidateSubClassParentId(DependentTransactionLine.AL_Calc_FirstSubClassParentIdInfo, DependentTransactionLine.SubAccounts.FirstSubAccount?.AL1_SubClassParentTableCode ?? ZString.Empty, DependentTransactionLine);
		}

		#endregion

		#region AL_Calc_SecondSubClassParentId

		public void ValidateAL_Calc_SecondSubClassParentId()
		{
			ValidateCalculatedProperty(DependentTransactionLine.AL_Calc_SecondSubClassParentIdInfo);
		}

		protected void CheckAL_Calc_SecondSubClassParentId()
		{
			SubAccountHelper.ValidateSubClassParentId(DependentTransactionLine.AL_Calc_SecondSubClassParentIdInfo, DependentTransactionLine.SubAccounts.SecondSubAccount?.AL1_SubClassParentTableCode ?? ZString.Empty, DependentTransactionLine);
		}

		#endregion

		protected override void CheckAL_GB()
		{
			base.CheckAL_GB();

			var transactionHeader = DependentTransactionLine.MasterTransactionHeader;
			var error = Res.GetString("d82b48ce-830c-43b2-9ec6-eb2a8bf2c6db", @"Please review the charge lines entered and ensure all charges have been entered belong to the same Posting Group. All charges posted in the one transaction must be in the same Posting Group.
Posting is prevented because charges have been entered using a mix of Posting Groups.");

			if (transactionHeader != null
				&& transactionHeader.Lines.Count > 0
				&& transactionHeader.EnforceBranchLevelPostingRegistryItem != null
				&& (bool)transactionHeader.EnforceBranchLevelPostingRegistryItem.Value.EnableBranchLevelPosting
				&& ((DependentTransactionLine)Parent).IsEnforceBranchLevelPostingValidationApplicable)
			{
				var branches = transactionHeader.Lines.Cast<DependentTransactionLine>().Where(x => x.IsEnforceBranchLevelPostingValidationApplicable).Select(x => x.AL_GB).ToHashSet();
				if (transactionHeader.HasContext(BusinessContext.OverrideTransactionBranchAndDepartment))
				{
					branches.Add(transactionHeader.AH_GB);

					error = Res.GetString("6be33c38-ff76-4150-9190-d0e7317dfa21", @"Please review the charge lines entered and overridden header and ensure all branches entered belong to the same Posting Group. Header and all charges posted in the one transaction must be in the same Posting Group.
Posting is prevented because header and charges have been entered using a mix of Posting Groups.");
				}

				if (!BranchLevelPostingHelper.DoesAllBranchesBelongToSamePostingGroup(transactionHeader.EnforceBranchLevelPostingRegistryItem, branches))
				{
					Parent.AL_GBInfo.AddError(error);
				}
			}
		}

		protected override void CheckAL_GB_TaxBranch()
		{
			base.CheckAL_GB_TaxBranch();

			if (DependentTransactionLine.MasterTransactionHeader != null
				&& DependentTransactionLine.MasterTransactionHeader.CanApplyTaxBranch)
			{
				MandatoryValidation.CheckEntered(DependentTransactionLine.AL_GB_TaxBranchInfo);
			}

			ListValidation.ErrorIfInvalidPK(DependentTransactionLine.AL_GB_TaxBranchInfo);
		}

		protected override void CheckAL_PlaceOfSupply()
		{
			base.CheckAL_PlaceOfSupply();

			if (!DependentTransactionLine.AL_PlaceOfSupply.IsEmpty &&
				(DependentTransactionLine.MasterTransactionHeader?.NeedPlaceOfSupplyAtHeaderLevel ?? false))
			{
				var header = DependentTransactionLine.MasterTransactionHeader;
				if (header != null && header.Lines
										.OfType<DependentTransactionLine>()
										.Any(l => l.AL_PlaceOfSupply != Parent.AL_PlaceOfSupply))
				{
					Parent.AL_PlaceOfSupplyInfo.AddError(Res.GetString("4637a620-22b7-4370-bd9b-2829aa384082", "Posting a {0} with more than one Place of Supply is not allowed. Make sure the Fixed place of supply selected on all the lines are same, or post separate transaction for each location.", header.HumanReadableName));
				}
			}
		}

		protected override void CheckAL_A9_VATClass()
		{
			base.CheckAL_A9_VATClass();
			if (ShouldValidateNoTaxMessage)
			{
				CountrySpecificValidationHelper.AddErrorOrWarningIfNoTaxMessage(DependentTransactionLine.AL_A9_VATClassInfo, DependentTransactionLine.AL_AT, () => DependentTransactionLine.AL_OSExTaxAmount, () => DependentTransactionLine.AL_OSTaxAmount, () => DependentTransactionLine.AL_OSExtraTaxAmount, IsAPTaxMessageMandatoryRegistry(), IsARTaxMessageMandatoryRegistry());
			}
		}

		protected virtual bool ShouldValidateNoTaxMessage => false;

		protected virtual bool IsAPTaxMessageMandatoryRegistry() => false;

		protected virtual bool IsARTaxMessageMandatoryRegistry() => false;

		#region Implementation

		protected void ValidateRevenueRecognition(string message)
		{
			Job job = DependentTransactionLine.InvoicingJob;
			if (job != null)
			{
				JobValidation jobValidation = job.Validation as JobValidation;
				if (jobValidation != null)
				{
					ZString error = jobValidation.GetRevenueRecognitionDateValidationError(message, Parent.ChargeCode, Parent.AL_RevRecognitionType);
					if (!error.IsEmpty)
					{
						Parent.AL_JHInfo.AddError(error);
					}
				}
			}
		}

		#endregion

		#region ValidateAL_LocalExtraTaxAmount

		public void ValidateAL_LocalExtraTaxAmount()
		{
			ValidateCalculatedProperty(DependentTransactionLine.AL_LocalExtraTaxAmountInfo);
		}

		protected void CheckAL_LocalExtraTaxAmount()
		{
			DependentTransactionLine.ClearRowNotifications();

			if (DependentTransactionLine.TaxRate != null && DependentTransactionLine.TaxRate.IsLocalExtraTaxAmountValuePersistent)
			{
				if (DependentTransactionLine.TaxRate.IsIndiaStateTax
					&& !DependentTransactionLine.AL_LocalGSTAmount.IsEmpty
						&& !DependentTransactionLine.AL_LocalExtraTaxAmount.IsEmpty)
				{
					if (Math.Abs(DependentTransactionLine.AL_LocalGSTAmount - DependentTransactionLine.AL_LocalExtraTaxAmount) >= 1)
					{
						DependentTransactionLine.AddRowError(Res.GetString("86481cc5-5ea5-4b20-98d3-9c9f1a009bee", "CGST and SGST can not differ by more than rounding error. Please check tax setup for '{0}'. If tax setup is correct then please contact CargoWise Support."
							, DependentTransactionLine.TaxRate.AT_Code));
					}
				}
			}
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.Business.JobInvoicing.BranchLevelPostingHelper;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public class InvoicingBaseTaxRecordParent : ITaxRecordParent
	{
		internal InvoicingBaseTaxRecordParent(InvoicingBase invoice)
		{
			parent = invoice;
			parent.OnSavedHander += Parent_OnSaved;
			chargeCreator_constructorInitializedOnly = new ChargeCreator();
		}

		IChargeCreator ChargeCreator => chargeCreator_constructorInitializedOnly;
		IChargeCreator chargeCreator_constructorInitializedOnly;
#if DEBUG
		public void SubstituteChargeCreator_ForTestOnly(IChargeCreator replacement) => chargeCreator_constructorInitializedOnly = replacement;
		public IChargeCreator ChargeCreator_ExposedForTestOnly => ChargeCreator;

#endif

		void Parent_OnSaved(bool saveSucceeded)
		{
			if (parent.IsPosted)
			{
				IsTaxTransactionsCalculatedBeforePosting = false;
			}
		}
		readonly InvoicingBase parent;

		public bool ShouldCalculateTaxTransactions => parent.IsBeingCreatedPostedAllocatedApprovedOrIncomplete && !parent.IsReversed && IsApplicableForTaxTransactions;

		public bool IsApplicableForTaxTransactions => new ZString[] { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.IncompleteInvoice, TransactionTypes.IncompleteCreditNote }.Contains(parent.AH_TransactionType)
			&& ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().HasAnyActiveAccTaxConfiguration(parent.Factory, parent.Company, ((ITaxRecordParentBase)this).Ledger);

		OrgHeader ITaxRecordParentBase.Org => parent.Header;

		ZDateTime ITaxRecordParentBase.PostDate => parent.AH_PostDate;

		ZString ITaxRecordParentBase.Ledger => parent.AH_Ledger == (ZString)LedgerTypes.IncompleteTransactions ? (ZString)LedgerTypes.AccountsPayable : parent.AH_Ledger;

		ZString ITaxRecordParentBase.Currency => parent.AH_RX_NKTransactionCurrency;

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		ZDecimal ITaxRecordParent.OSTaxAmount { get => parent.AH_OSTaxAmountOtherTaxes; set => parent.AH_OSTaxAmountOtherTaxes = value; }

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		ZDecimal ITaxRecordParent.LocalTaxAmount { get => parent.AH_LocalTaxAmountOtherTaxes; set => parent.AH_LocalTaxAmountOtherTaxes = value; }

		ZInt OSCurrencyDecimals => parent.OSCurrencyDecimals;

		ZInt LocalCurrencyDecimals => parent.LocalCurrencyDecimals;

		ZGuid ITaxRecordParentBase.PK => parent.PK;

		BusinessObjectFactory ITaxRecordParentBase.Factory => parent.Factory;

		GlbCompany ITaxRecordParentBase.Company => parent.Company;

		GlbBranch ITaxRecordParentBase.Branch => parent.TaxBranch ?? parent.Branch;

		GlbDepartment ITaxRecordParentBase.Department => parent.Department;

		bool ITaxRecordParentBase.IsPosted => parent.IsPosted;

		IReadOnlyList<ITaxableTransactionLine> ITaxRecordParent.GetLines()
		{
			return parent.Lines.Cast<InvoicingLineBase>()
				.Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(l)).Cast<ITaxableTransactionLine>()
				.Where(x => x.ChargeCode != null).ToList();
		}

		IReadOnlyList<ITaxableTransactionLineBase> ITaxRecordParentBase.GetLines()
		{
			return ((ITaxRecordParent)this).GetLines();
		}

		#region TaxRecoveryLine

		ITaxableTransactionLine ITaxRecordParent.AddTaxRecoveryLine(ZGuid chargeCodePK, ZGuid jobPK, ZGuid branchPK, ZGuid departmentPK, ZString currencyCode, ZDecimal lineAmount, ZDate taxDate, ZGuid orgPK, ZString supplyType)
		{
			var line = (InvoicingLineBase)parent.Lines.AddNew();
			taxRecoveryLinePKs = taxRecoveryLinePKs ?? new HashSet<ZGuid>();
			taxRecoveryLinePKs.Add(line.PK);
			line.GenericCharge = chargeCodePK;
			line.AL_GE = departmentPK;
			line.AL_JH = jobPK;
			line.AL_GB = branchPK;
			line.AL_RX_NKTransactionCurrency = currencyCode;
			line.AL_LineAmount = lineAmount;
			line.AL_LocalExTaxAmount = line.AL_LocalExTaxAmount; //to trigger OS and all tax amount calculations
			line.AL_TaxDate = taxDate;
			if (line.AL_JH.IsValid)
			{
				line.AL_OH = orgPK;
			}
			line.AL_SupplyType = supplyType;

			return TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line);
		}

		void ITaxRecordParent.DeleteAllAddedTaxRecoveryLines()
		{
			parent.Lines.Cast<InvoicingLineBase>().Where(x => IsTaxRecoveryLine(x)).ToArray().ForEach(x => x.Delete());
			taxRecoveryLinePKs = null;
		}

		public void RunOnSavingOperations()
		{
			if (!parent.IsInDatabase)
			{
				var isAnyChargeAdded = false;
				parent.Lines.Cast<InvoicingLineBase>().Where(x => IsTaxRecoveryLine(x)).ForEach(x => isAnyChargeAdded |= ChargeCreator.CreateChargeFromJobRelatedRevenueLine(x, parent, true) != null);
				if (isAnyChargeAdded)
				{
					parent.Factory.SetContext(NewChargeLoadActionOnPosting.RefreshChargesWhenPosted);
				}
			}
		}

		public bool IsTaxRecoveryLine(InvoicingLineBase x) => taxRecoveryLinePKs?.Contains(x.PK) ?? false;

		HashSet<ZGuid> taxRecoveryLinePKs;

		#endregion

		void ITaxRecordParent.SetTransactionHeaderBranch()
		{
			ObjectFactory.Get<IAccountingDependencyFactory>().GetBranchLevelPostingHelper().SetTransactionHeaderBranch(parent, InvoiceProcessingLevelIsAllowingToResetBranch.TaxTransactionCalculation);
		}

		public bool IsTaxTransactionsCalculatedBeforePosting
		{
			get => isOtherTaxesCalculatedBeforePosting;
			set
			{
				var prevValue = isOtherTaxesCalculatedBeforePosting;
				isOtherTaxesCalculatedBeforePosting = value;

				var hasChanged = prevValue != isOtherTaxesCalculatedBeforePosting;
				if (hasChanged && OnOtherTaxesCalculatedBeforePosting_Changed != null)
				{
					OnOtherTaxesCalculatedBeforePosting_Changed(this, EventArgs.Empty);
				}

				if (isOtherTaxesCalculatedBeforePosting)
				{
					parent.RegisterTaxTransactionCollectionAsEditableChild();
				}
			}
		}
		bool isOtherTaxesCalculatedBeforePosting;

		public event EventHandler OnOtherTaxesCalculatedBeforePosting_Changed;

#if DEBUG
		public int OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly => OnOtherTaxesCalculatedBeforePosting_Changed == null ? 0 : OnOtherTaxesCalculatedBeforePosting_Changed.GetInvocationList().Length;
#endif
	}
}

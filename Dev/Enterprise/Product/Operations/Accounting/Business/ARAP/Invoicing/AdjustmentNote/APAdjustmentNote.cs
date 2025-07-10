using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[CodeProperty(AccTransactionHeaderSchema.Constants.AH_ConsolidatedInvoiceRef)]
	[RestrictedFilteredItem()]
	public class APAdjustmentNote : AdjustmentNote, IDocManagerSupport, IEDocsParsingSupport
	{
		public APAdjustmentNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override Type DependentTransactionLineType
		{
			get { return typeof(APAdjustmentNoteLine); }
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.AccountsPayable; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return null; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForInternalRef
		{
			get { return AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceReferenceNumbers.Value.Value ? AccountingNumberFountainWrapperFactory.Instance.APInvoiceInternalRef : AccountingNumberFountainWrapperFactory.Instance.APAdjustmentNoteInternalRef; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("82a222ef-8f43-4051-af42-967f0ee71382", "Accounts Payable Adjustment Note"); }
		}

		protected override bool InvertSigns
		{
			get { return true; }
		}

		protected override Type TypeOfReverseTransaction
		{
			get { return typeof(APAdjustmentNote); }
		}

		protected override Type TypeOfTransaction
		{
			get { return typeof(APAdjustmentNote); }
		}

		protected override bool IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			if (Factory.HasContext(BusinessContext.SavingIncompleteTransaction))
			{
				return new IncompleteInvoicingBaseValidation(this);
			}

			return new APAdjustmentNoteValidation(this);
		}

		protected override bool ValidateExpectedInvoiceTotalSecurityIsAllowed
		{
			get
			{
				return Env.Security.AllowAPAdjustNoteChangeDefaultExpectedTotalValue.IsAllowed;
			}
		}

		protected override bool IsUseJobExchangeRateApplicable
		{
			get
			{
				return false;
			}
		}

		internal protected override BranchLevelPostingConfigurationRegistryItem EnforceBranchLevelPostingRegistryItem
		{
			get { return AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting; }
		}

		#region Invoice Amount Levels

		protected override AuthorizationModeAndSettingsRegistryItem AuthorizationModeAndSettingsRegistry => null;

		protected override SecurityCheckpoint RetrieveLevelApprovalCheckPoint(string levelCode) => null;

		#endregion

		#region IDocManagerSupport Members

		protected override InvoicingDocManagerInfo GetNewDocManagerInfo()
		{
			return docManagerInfo ?? (docManagerInfo = new InvoicingDocManagerInfo(this, Core.Constants.DocManagerCodes.PayableAdjustmentNote));
		}
		InvoicingDocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion
	}
}

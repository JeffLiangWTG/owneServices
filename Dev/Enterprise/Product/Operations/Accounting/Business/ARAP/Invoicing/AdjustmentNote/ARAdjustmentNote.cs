using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ARAdjustmentNote : AdjustmentNote, IDocManagerSupport, IEDocsParsingSupport
	{
		public ARAdjustmentNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool AH_InvoiceTerm_ReadOnly
		{
			get { return !AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.Value || !Env.Security.NewReceivablesAdjustmentNoteTerm.IsAllowed; }
		}

		protected override bool AH_InvoiceTermDays_ReadOnly
		{
			get { return AH_InvoiceTerm_ReadOnly || AH_InvoiceTerm == Constants.InvoiceTerms.CashOnDelivery; }
		}

		protected override bool AH_InvoiceDate_ReadOnly => base.AH_InvoiceDate_ReadOnly || !Env.Security.NewReceivablesAdjustmentNoteInvoiceDate.IsAllowed;

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("626dc035-5622-4b4f-a71d-0bf58126b0ba", "Accounts Receivable Adjustment Note"); }
		}

		public override Type DependentTransactionLineType
		{
			get { return typeof(ARAdjustmentNoteLine); }
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString Ledger
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsReceivable; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceTransactionNumbers.Value.Value)
				{
					return AccountingNumberFountainWrapperFactory.Instance.ARInvoiceNo;
				}
				else
				{
					return AccountingNumberFountainWrapperFactory.Instance.ARAdjustmentNoteNo;
				}
			}
		}

		public override bool CheckLevelSecurityRights()
		{
			bool result = true;
			if (!IsCreatedFromApprovalRequest && EnforceTwoApproversWhenPostingARCredit)
			{
				AmountBasedMultiLevelAuthorisationRequirement authorisationRequirement = AuthorisationRequired;
				if (Level1AuthorisationRequired(authorisationRequirement))
				{
					result = SecurityOverrideProvider.SecurityCertificates[FirstApprovalCheckpoint].IsAllowed;
				}
				if (Level2AuthorisationRequired(authorisationRequirement))
				{
					result &= SecurityOverrideProvider.SecurityCertificates[SecondApprovalCheckpoint].IsAllowed;
				}
			}
			else
			{
				result = base.CheckLevelSecurityRights();
			}
			return result;
		}

		protected override Type TypeOfReverseTransaction
		{
			get { return typeof(ARAdjustmentNote); }
		}

		protected override Type TypeOfTransaction
		{
			get { return typeof(ARAdjustmentNote); }
		}

		internal protected override BranchLevelPostingConfigurationRegistryItem EnforceBranchLevelPostingRegistryItem
		{
			get { return AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting; }
		}

		protected override bool IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		#region Invoice Amount Levels

		protected override AmountBasedMultiLevelAuthorisationRequirement AuthorisationRequiredCore()
		{
			return IsCreatingCreditNoteForReversal ? AuthorisationRequiredCore(AH_LocalTotalAmount) : (AH_LocalTotalAmount < 0 ? AuthorisationRequiredCore(-AH_LocalTotalAmount) : null);
		}

		protected override (ZGuid branchPK, ZGuid departmentPK) AuthorizationModeAndSettingsRegistryFallback =>
			(!IsCreatingCreditNoteForReversal && Job != null)
				? (Job.JH_GB, Job.JH_GE)
				: (AH_GB, AH_GE);

		protected override AuthorizationModeAndSettingsRegistryItem AuthorizationModeAndSettingsRegistry =>
			IsCreatingCreditNoteForReversal
				? AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings
				: AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings;

		protected override SecurityCheckpoint RetrieveLevelApprovalCheckPoint(string levelCode)
		{
			return GetCheckPointForLevel(levelCode);
		}

		#endregion

		#region IDocManagerSupport Members

		protected override InvoicingDocManagerInfo GetNewDocManagerInfo()
		{
			return docManagerInfo ?? (docManagerInfo = new InvoicingDocManagerInfo(this, Core.Constants.DocManagerCodes.ReceivableAdjustmentNote));
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

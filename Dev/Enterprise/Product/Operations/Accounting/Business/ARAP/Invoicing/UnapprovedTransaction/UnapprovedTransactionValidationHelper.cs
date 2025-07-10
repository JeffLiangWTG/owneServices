using System.Diagnostics.CodeAnalysis;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UnapprovedTransactionValidationHelper : LevelAuthorizationSecurityHelper<PaymentTwelveLevelAuthorisationSettings, PaymentTwelveLevelAuthorisationSettingsCollection>
	{
		public UnapprovedTransactionValidationHelper(bool intercompanyInvoiceApproval = false)
		{
			this.intercompanyInvoiceApproval = intercompanyInvoiceApproval;
		}

		readonly bool intercompanyInvoiceApproval;

		protected override PaymentTwelveLevelAuthorisationSettingsCollection RegistryValue
		{
			get { return AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.Value; }
		}

		public bool CheckLevelSecurityRights(ITransactionForApproval parent)
		{
			var requiredCheckPoint = RequiredSecurityCheckPoint(parent);
			return SecurityOverrideProviderSource.Get(parent).Provider.SecurityCertificates[requiredCheckPoint].IsAllowed;
		}

		public void CheckSecurityLevelsAndPromptForAuthorisation(TransactionHeader parent)
		{
			var errorMessage = Res.GetString("83c89d6f-bf69-4bab-b29e-503458b8c8f7", "This transaction requires a higher level of approval authority.");
			parent.RemoveRowError(errorMessage);
			if (!CheckLevelSecurityRights(parent))
			{
				parent.AddRowError(errorMessage);
			}
		}

		public SecurityCheckpoint RequiredSecurityCheckPoint(ITransactionForApproval parent)
		{
			return GetSecurityCheckPoint(parent.AH_LocalTotalAmount);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Don't see any complexity at all")]
		protected override SecurityCheckpoint GetCheckPointFromRegistrySetting(PaymentTwelveLevelAuthorisationSettings setting)
		{
			var checkpoint = Env.Security.None;
			if (setting != null)
			{
				var isApprovalRequestFunctionalityActivated = AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.Value;
				var useAPInvoiceSecurity = isApprovalRequestFunctionalityActivated && !intercompanyInvoiceApproval;
				switch (setting.AuthorisationRequirement)
				{
					case AuthorisationCodes.FirstApprovalRequiredOnly:
						checkpoint = useAPInvoiceSecurity ? Env.Security.APInvoiceApproval_FirstApproval : Env.Security.APUnapprovedInvoicesFirstApproval;
						break;
					case AuthorisationCodes.SecondApprovalRequiredOnly:
						checkpoint = useAPInvoiceSecurity ? Env.Security.APInvoiceApproval_SecondApproval : Env.Security.APUnapprovedInvoicesSecondApproval;
						break;
					case AuthorisationCodes.ThirdApprovalRequiredOnly:
						checkpoint = useAPInvoiceSecurity ? Env.Security.APInvoiceApproval_ThirdApproval : Env.Security.APUnapprovedInvoicesThirdApproval;
						break;
					case AuthorisationCodes.FourthApprovalRequiredOnly:
						checkpoint = useAPInvoiceSecurity ? Env.Security.APInvoiceApproval_FourthApproval : Env.Security.APUnapprovedInvoicesFourthApproval;
						break;
					case AuthorisationCodes.FifthApprovalRequiredOnly:
						checkpoint = useAPInvoiceSecurity ? Env.Security.APInvoiceApproval_FifthApproval : Env.Security.APUnapprovedInvoicesFifthApproval;
						break;
					case AuthorisationCodes.SixthApprovalRequiredOnly:
						checkpoint = useAPInvoiceSecurity ? Env.Security.APInvoiceApproval_SixthApproval : Env.Security.APUnapprovedInvoicesSixthApproval;
						break;
					case AuthorisationCodes.SeventhApprovalRequiredOnly:
						checkpoint = useAPInvoiceSecurity ? Env.Security.APInvoiceApproval_SeventhApproval : Env.Security.APUnapprovedInvoicesSeventhApproval;
						break;
					case AuthorisationCodes.EighthApprovalRequiredOnly:
						checkpoint = useAPInvoiceSecurity ? Env.Security.APInvoiceApproval_EighthApproval : Env.Security.APUnapprovedInvoicesEighthApproval;
						break;
					case AuthorisationCodes.NinthApprovalRequiredOnly:
						checkpoint = useAPInvoiceSecurity ? Env.Security.APInvoiceApproval_NinthApproval : Env.Security.APUnapprovedInvoicesNinthApproval;
						break;
					case AuthorisationCodes.TenthApprovalRequiredOnly:
						checkpoint = useAPInvoiceSecurity ? Env.Security.APInvoiceApproval_TenthApproval : Env.Security.APUnapprovedInvoicesTenthApproval;
						break;
					case AuthorisationCodes.EleventhApprovalRequiredOnly:
						checkpoint = useAPInvoiceSecurity ? Env.Security.APInvoiceApproval_EleventhApproval : Env.Security.APUnapprovedInvoicesEleventhApproval;
						break;
					case AuthorisationCodes.TwelfthApprovalRequiredOnly:
						checkpoint = useAPInvoiceSecurity ? Env.Security.APInvoiceApproval_TwelfthApproval : Env.Security.APUnapprovedInvoicesTwelfthApproval;
						break;
				}
			}
			return checkpoint;
		}
	}
}

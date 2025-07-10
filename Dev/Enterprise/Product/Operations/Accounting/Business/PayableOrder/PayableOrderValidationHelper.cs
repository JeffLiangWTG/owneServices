using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public class PayableOrderValidationHelper : LevelAuthorizationSecurityHelper<PaymentThreeLevelAuthorisationSettings, PaymentThreeLevelAuthorisationSettingsCollection>
	{
		protected override PaymentThreeLevelAuthorisationSettingsCollection RegistryValue
		{
			get { return AccountingConfigurationRegistry.Instance.PayableOrderAuthorizationSettings.Value; }
		}

		public bool CheckSecurityLevelsAndPromptForAuthorisation(AccPayableOrderHeader parent)
		{
			var requiredCheckPoint = RequiredSecurityCheckPoint(parent);
			return SecurityOverrideProviderSource.Get(parent).Provider.SecurityCertificates[requiredCheckPoint].IsAllowed;
		}

		SecurityCheckpoint RequiredSecurityCheckPoint(AccPayableOrderHeader parent)
		{
			return GetSecurityCheckPoint(parent.APH_Calc_TotalAmount);
		}

		protected override SecurityCheckpoint GetCheckPointFromRegistrySetting(PaymentThreeLevelAuthorisationSettings setting)
		{
			var checkpoint = Env.Security.None;
			if (setting != null)
			{
				switch (setting.AuthorisationRequirement)
				{
					case PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly:
						checkpoint = Env.Security.PayableOrderApprovalFirstLevelApproval;
						break;
					case PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.SecondApprovalRequiredOnly:
						checkpoint = Env.Security.PayableOrderApprovalSecondLevelApproval;
						break;
					case PaymentThreeLevelAuthorisationSettings.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly:
						checkpoint = Env.Security.PayableOrderApprovalThirdLevelApproval;
						break;
				}
			}
			return checkpoint;
		}
	}
}

using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	public abstract class CAEManifestForwarderMessageManagerTestCase<TForwarderMessageManager> : CAMessageManagerTestCase
		where TForwarderMessageManager : CAEManifestForwarderMessageManager
	{
		CAEManifestForwarderMessageManager manager;

		public void TestCanSendWithoutAdditionalWarning_MQWarningMessage()
		{
			var mQwarningText = CAMessageManager.MQWarningMessage;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CAMQWAR, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				using (CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INETCECPP"))
				{
					manager = GetMessageManager() as CAEManifestForwarderMessageManager;
					Assert("should contain mQwarningText", manager.GetNotificationsForSendingAnOriginal().ContainsWarning(mQwarningText));
				}

				LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
				using (CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INETCECPP"))
				{
					Assert("should contain mQwarningText", manager.GetNotificationsForSendingAnOriginal().ContainsWarning(mQwarningText));
				}

				using (CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "RCCECECPW"))
				{
					manager = GetMessageManager() as CAEManifestForwarderMessageManager;
					Assert("should not contain mQwarningText", !manager.GetNotificationsForSendingAnOriginal().ContainsWarning(mQwarningText));
				}

				LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
				using (CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "RCCECECPW"))
				{
					manager = GetMessageManager() as CAEManifestForwarderMessageManager;
					Assert("should not contain mQwarningText", !manager.GetNotificationsForSendingAnOriginal().ContainsWarning(mQwarningText));
				}
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CAMQWAR, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				manager = GetMessageManager() as CAEManifestForwarderMessageManager;
				Assert("Should contain WarningAndConfirmationWhenInTestModeText", manager.GetNotificationsForSendingAnOriginal().ContainsWarning(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));
				Assert("Should not contain mQwarningText", !manager.GetNotificationsForSendingAnOriginal().ContainsWarning(mQwarningText));

				LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
				manager = GetMessageManager() as CAEManifestForwarderMessageManager;
				Assert("Should not contain WarningAndConfirmationWhenInTestModeText", !manager.GetNotificationsForSendingAnOriginal().ContainsWarning(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));
				Assert("should not contain mQwarningText", !manager.GetNotificationsForSendingAnOriginal().ContainsWarning(mQwarningText));
			}
		}
	}
}

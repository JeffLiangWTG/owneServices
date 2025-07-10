using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Accounting.ElectronicMessaging.Israel;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Israel
{
	internal class IsraelEInvoicingCredentialSettingsTest : GlobalEInvoicingCredentialSettingsTest
	{
		protected override ZString ExpectedPasswordType => PasswordTypesList.Codes.EIM;

		protected override bool ExpectedIsCompanyCredentialsRequired => true;

		protected override IEInvoicingCredentialSettings GetBaseCredentialSettings()
			=> new IsraelEInvoicingCredentialSettings();
	}
}

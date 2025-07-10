using System.Collections.Generic;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Poland
{
	sealed class PolandCredentialLoader : ICredentialsLoader
	{
		public IEnumerable<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential> LoadForGEIRequest(GlbBranch branch, TransactionBatch batch, ICountryEInvoicingObjectFactory countryFactory)
		{
			var registryLoader = new RegistryCredentialLoader(AccountingElectronicMessagingRegistry.Instance.PolandEInvoicingCredentials);
			foreach (var credential in registryLoader.LoadForGEIRequest(branch, batch, countryFactory))
			{
				yield return credential;
			}

			var certificateLoader = new ObjectFactoryCredentialLoader();
			foreach (var credential in certificateLoader.LoadForGEIRequest(branch, batch, countryFactory))
			{
				yield return credential;
			}
		}
	}
}

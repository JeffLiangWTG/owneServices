using System.Collections;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public class EMCSLayoutProvider : IEMCSLayoutProvider
	{
		public static IEMCSLayoutProvider GetLayoutProvider(string countryOrGroupingCode)
		{
			object provider = null;

			var providers = ObjectFactory.Get<Hashtable>("EMCSLayoutProviders");
			if (!string.IsNullOrEmpty(countryOrGroupingCode))
			{
				var objectHandle = providers[countryOrGroupingCode] as ObjectHandle;
				provider = objectHandle?.GetObject();
			}
			if (provider == null)
			{
				var objectHandle = providers["Default"] as ObjectHandle;
				provider = objectHandle?.GetObject();
			}
			return (IEMCSLayoutProvider)provider;
		}

		public IPanelLayoutWithGridProvider GetInvoiceLineDetailsPanelLayoutWithGrid(ZString dataGroupingCode) => new InvoiceLineDetailsLayoutWithGrid(dataGroupingCode);

		public IPanelLayoutProvider DeclarationOrganizationsPanelLayout => new DeclarationOrganizationsLayout();
	}
}

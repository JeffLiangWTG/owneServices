using System.Collections;
using CargoWise.Application;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	class IntrastatLayoutProvider : IIntrastatLayoutProvider
	{
		public static IIntrastatLayoutProvider GetLayoutProvider(string countryOrGroupingCode)
		{
			object provider = null;

			var providers = ObjectFactory.Get<Hashtable>("IntrastatLayoutProviders");
			if (!string.IsNullOrEmpty(countryOrGroupingCode))
			{
				var objectHandle = (ObjectHandle)providers[countryOrGroupingCode];
				provider = objectHandle?.GetObject();
			}
			if (provider == null)
			{
				var objectHandle = (ObjectHandle)providers["Default"];
				provider = objectHandle?.GetObject();
			}
			return (IIntrastatLayoutProvider)provider;
		}

		public IPanelLayoutProvider OrganisationDetailsPanelLayout => new OrganisationDetailsLayout();

		public IPanelLayoutProvider TransactionDetailsPanelLayout => new TransactionDetailsLayout();

		public IPanelLayoutWithGridProvider TransactionLineDetailsWithGridLayout => new TransactionLineDetailsWithGridLayout();

		public IGridColumnLayoutProvider TransactionLinesGridColumnLayout => new TransactionLinesGridColumnLayout();
	}
}

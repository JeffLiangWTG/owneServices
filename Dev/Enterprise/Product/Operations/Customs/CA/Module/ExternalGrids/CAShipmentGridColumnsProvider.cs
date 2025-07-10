using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using IExternalGridColumnsProvider = Enterprise.Integration.Customs.CA.IExternalGridColumnsProvider;
using IShipmentGridColumnsProvider = Enterprise.Integration.Customs.CA.IShipmentGridColumnsProvider;

namespace Enterprise.Customs.CA.Module
{
	partial class CAShipmentGridColumnsProvider : CAExternalGridColumnsProvider, IShipmentGridColumnsProvider
	{
		#region Implementation of IShipmentGridColumnsProvider

		protected override IEnumerable<IExternalGridColumnsProvider> GetColumnsProviders()
		{
			List<IExternalGridColumnsProvider> columnsProviders = new List<IExternalGridColumnsProvider>();

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada)
			{
				columnsProviders.Add(new RNSGridColumnsProvider());
				columnsProviders.Add(new ArrivalGridColumnsProvider());
			}

			return columnsProviders.AsEnumerable();
		}

		#endregion
	}
}

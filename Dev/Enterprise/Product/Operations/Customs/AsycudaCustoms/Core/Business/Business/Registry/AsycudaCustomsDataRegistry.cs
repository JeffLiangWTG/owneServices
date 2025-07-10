using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public sealed class AsycudaCustomsDataRegistry : RegistryItemSet
		, Integration.Customs.AsycudaCustoms.IAsycudaCustomsRegistry
	{
		[ThreadStatic]
		static AsycudaCustomsDataRegistry instance;

		public static AsycudaCustomsDataRegistry Instance => instance ?? (instance = new AsycudaCustomsDataRegistry());

		AsycudaCustomsDataRegistry()
		{
		}

		public override bool IsForProductivityWise => false;

		protected override void SetDefaultsForNewItem(IRegistryItem item)
		{
			base.SetDefaultsForNewItem(item);
			item.CountryFilterPKs = AsycudaCustomsCountryPks;
		}

		IEnumerable<Guid> AsycudaCustomsCountryPks
		{
			get
			{
				if (asycudaCustomsCountryPks == null)
				{
					var countryProvider = ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>();
					asycudaCustomsCountryPks = GlbCompany.GetActiveCompanies(x => countryProvider.IsAsycudaCustomsCountry(x.GC_RN_NKCountryCode))
						.DistinctBy(x => x.GC_RN_NKCountryCode)
						.Select(x => x.Country.PK.ToGuid()).ToArray();
				}

				return asycudaCustomsCountryPks;
			}
		}

		Guid[] asycudaCustomsCountryPks;
	}
}

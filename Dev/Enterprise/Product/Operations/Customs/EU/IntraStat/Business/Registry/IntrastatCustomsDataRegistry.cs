using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.Intrastat.Business
{
	public sealed class IntrastatCustomsDataRegistry : RegistryItemSet, IEUIntrastatCustomsRegistry
	{
		public static IntrastatCustomsDataRegistry Instance => instance ?? (instance = new IntrastatCustomsDataRegistry());

		[ThreadStatic]
		static IntrastatCustomsDataRegistry instance;

		IntrastatCustomsDataRegistry()
		{
		}

		public override bool IsForProductivityWise => false;

		protected override void SetDefaultsForNewItem(IRegistryItem item)
		{
			base.SetDefaultsForNewItem(item);
			item.CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdiction;
		}

		public BooleanRegistryItem EnableIntrastatFunctions
		{
			get
			{
				return GetItem("EnableIntraStatFunctions", delegate
				{
					return new BooleanRegistryItem("EnableIntraStatFunctions",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_Intrastat,
						(NoResString)"Enable Intrastat Functions",
						(NoResString)"Set to YES to enable Intrastat Functions.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public bool IsIntrastatEnabled => EnableIntrastatFunctions.Value;
	}
}

using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class DENctsCustomsDataRegistry : RegistryItemSet
	{
		public static DENctsCustomsDataRegistry Instance => instance ??= new DENctsCustomsDataRegistry();

		[ThreadStatic]
		static DENctsCustomsDataRegistry instance;

		public override bool IsForProductivityWise => false;

		protected override void SetDefaultsForNewItem(IRegistryItem item)
		{
			base.SetDefaultsForNewItem(item);
			item.CountryFilterPKs = CountryFilterPKs.Germany;
		}

		public NctsFallbackConfigurationRegistryItem NctsFallbackConfiguration
		{
			get
			{
				return GetItem("NctsFallbackConfiguration", delegate
				{
					var result = new NctsFallbackConfigurationRegistryItem(
						"NctsFallbackConfiguration",
						RawDataRegistry.Categories.Customs_Germany,
						ResString.GetMultilingualString("2CAFF8B5-2AFD-4686-87B1-863DF070F71A", "NCTS Fallback Configuration"),
						ResString.GetMultilingualString("2CAFF8B5-2AFD-4686-87B1-863DF070F71A", "NCTS Fallback Configuration"),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new NctsFallbackConfiguration()
					);
					return result;
				});
			}
		}

		public static ZBool NctsFallbackIsActive
		{
			get
			{
				var nctsFallbackConfiguration = Instance.NctsFallbackConfiguration.Value;
				return (!nctsFallbackConfiguration.Start.IsEmpty && !nctsFallbackConfiguration.CustomsIncidentNumber.IsEmpty);
			}
		}
	}
}

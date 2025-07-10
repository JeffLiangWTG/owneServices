using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public sealed class COCustomsDataRegistry : RegistryItemSet, Integration.Customs.CO.ICOCustomsDataRegistry
	{
		#region Construction

		public static COCustomsDataRegistry Instance => instance ?? (instance = new COCustomsDataRegistry());

		[ThreadStatic]
		static COCustomsDataRegistry instance;

		public COCustomsDataRegistry()
		{
		}

		#endregion

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Colombia { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("5D11ED31-36BF-4B90-A4E0-2CBB3A0AE809", "Colombia")); } }
			public static MultilingualString Customs_Colombia_Manifest { get { return CombineCategories(Customs_Colombia, ResString.GetMultilingualString("B0105488-35A9-4A69-B3A7-DB34B232425A", "Manifest")); } }
		}

		#endregion

		#region ICOCustomsDataRegistry Members

		IRegistryItem Integration.Customs.CO.ICOCustomsDataRegistry.EnableCOManifests => EnableCOManifests;

		#endregion

		public override bool IsForProductivityWise => false;

		public BooleanRegistryItem EnableCOManifests
		{
			get
			{
				return GetItem("EnableCOManifests", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableCOManifests",
						Categories.Customs_Colombia,
						ResString.GetMultilingualString("61688E5E-CFA5-4444-97BE-8B6C03A31813", "Enable Colombia Manifest"),
						ResString.GetMultilingualString("286D2C78-D0D3-4D40-9186-9C9C8667C773", "Enable Colombia Manifest?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}
	}
}

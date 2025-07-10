using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class FreightPacksDataRegistry : RegistryItemSet
	{
		FreightPacksDataRegistry()
		{
		}

		#region Instance

		public static FreightPacksDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new FreightPacksDataRegistry();
				}
				return fInstance;
			}
		}
		[ThreadStatic]
		static FreightPacksDataRegistry fInstance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region PackUnits

		public StringRegistryItem InnerPackUnit
		{
			get
			{
				return GetItem<StringRegistryItem>("InnerPackUnit", delegate
				{
					return new StringRegistryItem(
						"InnerPackUnit",
						RawDataRegistry.Categories.Freight_Shipment_Packages,
						ResString.GetMultilingualString("68c5b1f1-6769-48ff-b83d-1504b953ce3d", "Inner Pack Unit"),
						ResString.GetMultilingualString("a66d38b3-6dcf-4c5e-9c75-549b003cfdbb", "Default Inner Pack Unit for Freight"),
						new StringRegistryDataType(CharacterCase.Upper, 0, 3),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						Core.Constants.PkgUnit.Carton);
				});
			}
		}

		public StringRegistryItem OuterPackUnit
		{
			get
			{
				return GetItem<StringRegistryItem>("OuterPackUnit", delegate
				{
					return new StringRegistryItem(
						"OuterPackUnit",
						RawDataRegistry.Categories.Freight_Shipment_Packages,
						ResString.GetMultilingualString("7def1529-116d-46a8-8ff3-3c7eecd4326f", "Outer Pack Unit"),
						ResString.GetMultilingualString("5a7e65ef-eedf-41fc-bf28-22807398e831", "Default Outer Pack Unit for Freight"),
						new StringRegistryDataType(CharacterCase.Upper, 0, 3),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						Core.Constants.PkgUnit.Pallet);
				});
			}
		}

		#endregion

		#region Dangerous Goods

		public BooleanRegistryItem ActivateIsCombustibleForDGItems
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ActivateIsCombustibleForDGItems", delegate
				{
					return new BooleanRegistryItem("ActivateIsCombustibleForDGItems",
					RawDataRegistry.Categories.Freight_Shipment_Packages,
					ResString.GetMultilingualString("6ca72203-4b43-4a86-b33e-2918ed28436e", "Activate Has Flash Point for DG items"),
					ResString.GetMultilingualString("6ca72203-4b43-4a86-b33e-2918ed28436e", "Activate Has Flash Point for DG items"),
					RegistryStorageFlags.System,
					false);
				});
			}
		}

		#endregion
	}
}

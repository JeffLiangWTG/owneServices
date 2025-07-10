using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class NctsCustomsDataRegistry : RegistryItemSet
	{
		public static NctsCustomsDataRegistry Instance => instance ??= new NctsCustomsDataRegistry();

		[ThreadStatic]
		static NctsCustomsDataRegistry instance;

		NctsCustomsDataRegistry()
		{
		}

		public override bool IsForProductivityWise => false;

		public BooleanRegistryItem EnableInventoryManagement
		{
			get
			{
				return GetItem("EnableInventoryManagementNCTS", delegate
				{
					return new BooleanRegistryItem("EnableInventoryManagementNCTS",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
						(NoResString)"Enable Inventory Management",
						(NoResString)"Enable Inventory Management",
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableMultipleMovements
		{
			get
			{
				return GetItem("EnableMultipleMovements", delegate
				{
					return new BooleanRegistryItem("EnableMultipleMovements",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
						(NoResString)"Enable Multiple Movements",
						(NoResString)"Enable Multiple Movements",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}
	}
}

using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Business
{
	public sealed class EUH7CustomsDataRegistry : RegistryItemSet
	{
		public static EUH7CustomsDataRegistry Instance => instance ?? (instance = new EUH7CustomsDataRegistry());

		[ThreadStatic]
		static EUH7CustomsDataRegistry instance;

		EUH7CustomsDataRegistry()
		{
		}

		public override bool IsForProductivityWise => false;

		#region Low Value (H7) Job Number Customization

		public BillCustomisationRegistryItem EUH7JobNumberCustomization
		{
			get
			{
				var dataType = new BillCustomisationRegistryDataType();
				dataType.Categories = NumberCustomisationElementCategories.Standard;
				dataType.GeneratedNumberName = ResString.GetMultilingualString("4723f535-615a-49f5-be1a-631f6717ca74", "Low Value (H7) Job Number");
				dataType.MaxLength = AsycudaManifestHeaderSchema.AMA_JobReference.MaxLength;

				var result = GetItem("EUH7JobNumberCustomization",
								() => new BillCustomisationRegistryItem(
									"EUH7JobNumberCustomization",
									RawDataRegistry.Categories.Customs_EuropeanUnionCommon_H7,
									ResString.GetMultilingualString("7eefdc60-f27d-4624-9212-2d26ce9fbe11", "Low Value (H7) Job Number Customization"),
									ResString.GetMultilingualString("442dcbd4-8a59-4ba2-b9d8-8f222cdb8b9a", "Override this value to customize how low value (H7) job numbers are formatted."),
									RegistryStorageFlags.Company,
									RegistryOptions.IsOnlyForDevelopers,
									dataType
								));

				result.CountryFilterPKs = EUH7CountryFilterPKs;
				return result;
			}
		}

		#endregion

		public static IEnumerable<Guid> EUH7CountryFilterPKs => CountryFilterPKs.EuropeanUnion;
	}
}

using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class AvsWebServiceUriRegistryItem : CodeDescriptionBoolRegistryItem
	{
		static readonly string AvsWebServicePrimaryUri = "https://webservices.wisegrid.net/addresscleansing/v2/";
		static readonly string AvsWebServiceSecondaryUri = "https://ordwebservices.wisegrid.net/addresscleansing/v2/";
		static readonly string AvsWebServiceBackgroundUri = "https://avsbackground.wisegrid.net/v2/";

		public AvsWebServiceUriRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new AddressValidationServiceURIRegistryItemImpl(name, category, caption, hint, storage, options), new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Sys to Sys Trust"))
		{
		}

		public new AvsWebServiceUriRegistryBusinessObjectCollection GetValueWithoutFallback(Guid companyPK, Guid branchPK, Guid departmentPK) => (AvsWebServiceUriRegistryBusinessObjectCollection)base.GetValueWithoutFallback(companyPK, branchPK, departmentPK);

		class AddressValidationServiceURIRegistryItemImpl : RegistryItemImpl
		{
			public AddressValidationServiceURIRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new AvsWebServiceUriCollectionRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return new AvsWebServiceUriRegistryBusinessObjectCollection
				{
					{ AvsWebServiceUriType.Primary, (NoResString)AvsWebServicePrimaryUri, false },
					{ AvsWebServiceUriType.Secondary, (NoResString)AvsWebServiceSecondaryUri, false },
					{ AvsWebServiceUriType.Background, (NoResString)AvsWebServiceBackgroundUri, false }
				};
			}
		}
	}

	public static class AvsWebServiceUriType
	{
		public static readonly string Primary = nameof(Primary);
		public static readonly string Secondary = nameof(Secondary);
		public static readonly string Background = nameof(Background);
	}
}

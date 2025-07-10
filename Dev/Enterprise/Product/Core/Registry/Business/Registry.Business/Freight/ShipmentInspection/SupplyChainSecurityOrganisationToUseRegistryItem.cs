using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class SupplyChainSecurityOrganisationToUseRegistryItem : TranslatableRegistryItem<SupplyChainSecurityOrganisationToUseCollection, SupplyChainSecurityOrganisationToUseCollection>, ICodeDescriptionPairListProvider
	{
		public SupplyChainSecurityOrganisationToUseRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, SupplyChainSecurityOrganisationToUseCollection defaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public SupplyChainSecurityOrganisationToUseRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, SupplyChainSecurityOrganisationToUseCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new SupplyChainSecurityOrganisationToUseRegistryDataType(defaultValue), storage, options, defaultValue))
		{
			this.defaultValue = defaultValue;
		}

		#region ICodeDescriptionPairListProvider

		CodeDescriptionPairList ICodeDescriptionPairListProvider.CodeDescriptionPairList
		{
			get { return this.Value.GetCodeDescriptionPairList(); }
		}

		#endregion

		public override bool IsTranslatable
		{
			get { return true; }
		}

		protected override SupplyChainSecurityOrganisationToUseCollection Convert(SupplyChainSecurityOrganisationToUseCollection value)
		{
			foreach (SupplyChainSecurityOrganisationToUse item in value)
			{
				item.Description = GetMultilingualString(item.EnglishDescription);
			}

			return value;
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				foreach (SupplyChainSecurityOrganisationToUse item in defaultValue)
				{
					yield return (ResourceString)item.Description;
				}
			}
		}

		public override IEnumerable<string> GetCaptions(SupplyChainSecurityOrganisationToUseCollection value)
		{
			foreach (SupplyChainSecurityOrganisationToUse item in value)
			{
				yield return item.EnglishDescription;
			}
		}

		public override int MaxLength
		{
			get { return 256; }
		}

		readonly SupplyChainSecurityOrganisationToUseCollection defaultValue;
	}

	#region Data Type

	[RegistryEditor("Enterprise.Registry.GUI.SupplyChainSecurityOrganisationToUseRegistryItemEditor, Enterprise.Registry.GUI")]
	public class SupplyChainSecurityOrganisationToUseRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SupplyChainSecurityOrganisationToUseCollection>
	{
		public SupplyChainSecurityOrganisationToUseRegistryDataType()
		{
		}

		public SupplyChainSecurityOrganisationToUseRegistryDataType(SupplyChainSecurityOrganisationToUseCollection defaultValue)
			: base(defaultValue)
		{
		}
	}

	#endregion
}

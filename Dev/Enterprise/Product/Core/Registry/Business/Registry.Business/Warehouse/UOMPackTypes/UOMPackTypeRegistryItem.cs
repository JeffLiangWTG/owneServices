using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class UOMPackTypeRegistryItem : TranslatableRegistryItem<UOMPackTypeCollection, UOMPackTypeCollection>
	{
		public UOMPackTypeRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions isOnlyForSupport,
			UOMPackTypeCollection defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new UOMPackTypeRegistryDataType(), storage, isOnlyForSupport, defaultValue))
		{
		}

		#region overrides

		public override bool IsTranslatable
		{
			get { return true; }
		}

		protected override UOMPackTypeCollection Convert(UOMPackTypeCollection value)
		{
			foreach (UOMPackType item in value)
			{
				item.Description = GetMultilingualString(item.EnglishDescription);
			}
			return value;
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				var collection = new UOMPackTypeCollection();

				foreach (UOMPackType item in collection)
				{
					yield return (ResourceString)item.Description;
				}
			}
		}

		public override int MaxLength { get { return UOMPackType.MaxDescriptionLength; } }

		public override IEnumerable<string> GetCaptions(UOMPackTypeCollection value)
		{
			return value.Cast<UOMPackType>()
						.Select(i => i.Description.ToString().Trim()).Distinct()
						.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();
		}

		#endregion
	}

	[RegistryEditor("Enterprise.Registry.GUI.UOMPackTypeRegistryItemEditor, Enterprise.Registry.GUI")]
	public class UOMPackTypeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<UOMPackTypeCollection>
	{
	}
}

using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	public class PickGroupRegistryItem : TranslatableRegistryItem<PickGroupCollection, PickGroupCollection>
	{
		public PickGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new PickGroupRegistryDataType(), storage))
		{
		}

		public override bool IsTranslatable
		{
			get { return true; }
		}

		protected override PickGroupCollection Convert(PickGroupCollection value)
		{
			foreach (PickGroup item in value)
			{
				item.Description = GetMultilingualString(item.EnglishDescription);
			}
			return value;
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get { return System.Array.Empty<ResourceString>(); }
		}

		public override IEnumerable<string> GetCaptions(PickGroupCollection value)
		{
			foreach (PickGroup item in value)
			{
				yield return item.Description;
			}
		}

		public override int MaxLength
		{
			get { return PickGroup.MaxDescriptionLength; }
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.PickGroupRegistryItemEditor, Enterprise.Registry.GUI")]
	class PickGroupRegistryDataType : NonPersistentBusinessObjectRegistryDataType<PickGroupCollection>
	{
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public abstract class TranslatableRegistryBusinessItemCollectionRegistryItem<TGet, TSet> : TranslatableRegistryItem<TGet, TSet> where TSet : RegistryBusinessObjectCollection
	{
		protected TranslatableRegistryBusinessItemCollectionRegistryItem(IRegistryItem item)
			: this(item, null)
		{ }

		protected TranslatableRegistryBusinessItemCollectionRegistryItem(IRegistryItem item, IRegistryEditorInfo editorInfo)
			: base(item)
		{
			this.EditorInfo = editorInfo;
			this.defaultValue = (RegistryBusinessObjectCollection)item.DefaultValue;
		}

		protected override TGet Convert(TSet value)
		{
			if (IsTranslatable)
			{
				foreach (RegistryBusinessObject item in value)
				{
					item.Description = GetMultilingualString(item.EnglishDescription);
				}
			}
			return (TGet)(object)value;
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				foreach (RegistryBusinessObject item in defaultValue)
				{
					yield return (ResourceString)item.Description;
				}
			}
		}

		public override IEnumerable<string> GetCaptions(TGet value)
		{
			foreach (RegistryBusinessObject item in (IEnumerable)value)
			{
				yield return item.EnglishDescription;
			}
		}

		public override bool IsTranslatable
		{
			get
			{
				return defaultValue.Count == 0 || defaultValue.Cast<RegistryBusinessObject>().All(item => (item.Description is ResourceString));
			}
		}

		readonly RegistryBusinessObjectCollection defaultValue;
	}
}

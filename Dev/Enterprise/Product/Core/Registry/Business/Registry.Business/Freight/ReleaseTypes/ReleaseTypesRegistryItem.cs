using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ReleaseTypesRegistryItem : TranslatableRegistryItem<ReleaseTypes, ReleaseTypes>, ICodeDescriptionPairListProvider
	{
		public ReleaseTypesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ReleaseTypes defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ReleaseTypesRegistryDataType(defaultValue), null, storage, RegistryOptions.Default, defaultValue, false))
		{
			this.defaultValue = defaultValue;
		}

		#region ICodeDescriptionPairListProvider

		CodeDescriptionPairList ICodeDescriptionPairListProvider.CodeDescriptionPairList
		{
			get { return this.Value.Types.GetCodeDescriptionPairList(); }
		}

		#endregion

		public override bool IsTranslatable
		{
			get { return true; }
		}

		protected override ReleaseTypes Convert(ReleaseTypes value)
		{
			foreach (ReleaseType item in value.Types)
			{
				item.Description = GetMultilingualString(item.EnglishDescription);
			}
			return value;
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				foreach (ReleaseType item in defaultValue.Types)
				{
					yield return (ResourceString)item.Description;
				}
			}
		}

		public override IEnumerable<string> GetCaptions(ReleaseTypes value)
		{
			foreach (ReleaseType item in value.Types)
			{
				yield return item.EnglishDescription;
			}
		}

		public override int MaxLength
		{
			get { return 256; }
		}

		readonly ReleaseTypes defaultValue;
	}

	[RegistryEditor("Enterprise.Registry.GUI.ReleaseTypesRegistryItemEditor, Enterprise.Registry.GUI")]
	class ReleaseTypesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ReleaseTypes>
	{
		public ReleaseTypesRegistryDataType()
		{
		}

		public ReleaseTypesRegistryDataType(ReleaseTypes defaultValue) : base(defaultValue)
		{
		}

		protected override ReleaseTypes DeserialiseCore(byte[] value)
		{
			ReleaseTypes result = base.DeserialiseCore(value);
			result.Types.SystemDefinedReleaseTypeCollection = this.DefaultValue.Types;
			return result;
		}
	}
}

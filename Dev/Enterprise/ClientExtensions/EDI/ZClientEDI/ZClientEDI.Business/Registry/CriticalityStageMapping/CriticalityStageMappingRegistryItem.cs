using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry
{
	public class CriticalityStageMappingRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<CriticalityStageMappingCollection, CriticalityStageMappingCollection>
	{
		public CriticalityStageMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage,
			CriticalityStageMappingRegistryEditorInfo editorInfo,
			CriticalityStageMappingCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CriticalityStageMappingRegistryDataType(defaultValue), editorInfo, storage, RegistryOptions.Default, defaultValue), editorInfo)
		{
			this.defaultValue = defaultValue;

			if (defaultValue.MaxDepth <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(defaultValue), "MaxDepth must be set on the default value");
			}
		}

		protected readonly CriticalityStageMappingCollection defaultValue;

		public override int MaxLength
		{
			get { return 250; }
		}
	}

	public class CriticalityStageMappingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CriticalityStageMappingCollection>
	{
		public CriticalityStageMappingRegistryDataType(CriticalityStageMappingCollection defaultValue)
			: base(defaultValue)
		{
		}

		protected override CriticalityStageMappingCollection DeserialiseCore(byte[] value)
		{
			var result = base.DeserialiseCore(value);
			if (DefaultValue != null)
			{
				result.MaxDepth = DefaultValue.MaxDepth;
				result.AllDescriptions = DefaultValue.AllDescriptions;
				result.CodeLists = DefaultValue.CodeLists;

				if (result.AllDescriptions != null || result.CodeLists != null)
				{
					var resultAllDescriptions = result.AllDescriptions?.ToArray();
					var resultCodeLists = result.CodeLists?.ToArray();

					foreach (CriticalityStageMapping node in result)
					{
						int depthIndex = result.GetDepth(node) - 1;
						if (result.AllDescriptions != null)
						{
							if (node.IsSystemAll)
							{
								node.Description = resultAllDescriptions[depthIndex];
							}
						}

						if (result.CodeLists != null)
						{
							node.CodeList = resultCodeLists[depthIndex];
						}
					}
				}
			}
			return result;
		}
	}
}


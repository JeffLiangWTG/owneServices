using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry
{
	public class ResolutionAndClosureBehaviourRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<ResolutionAndClosureBehaviourCollection, ResolutionAndClosureBehaviourCollection>
	{
		public ResolutionAndClosureBehaviourRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage,
			ResolutionAndClosureBehaviourRegistryEditorInfo editorInfo,
			ResolutionAndClosureBehaviourCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ResolutionAndClosureBehaviourRegistryDataType(defaultValue), editorInfo, storage, RegistryOptions.Default, defaultValue), editorInfo)
		{
			this.defaultValue = defaultValue;

			if (defaultValue.MaxDepth <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(defaultValue), "MaxDepth must be set on the default value");
			}
		}

		protected readonly ResolutionAndClosureBehaviourCollection defaultValue;

		public override int MaxLength
		{
			get { return 250; }
		}
	}

	public class ResolutionAndClosureBehaviourRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ResolutionAndClosureBehaviourCollection>
	{
		public ResolutionAndClosureBehaviourRegistryDataType(ResolutionAndClosureBehaviourCollection defaultValue)
			: base(defaultValue)
		{
		}

		protected override ResolutionAndClosureBehaviourCollection DeserialiseCore(byte[] value)
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

					foreach (ResolutionAndClosureBehaviour node in result)
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


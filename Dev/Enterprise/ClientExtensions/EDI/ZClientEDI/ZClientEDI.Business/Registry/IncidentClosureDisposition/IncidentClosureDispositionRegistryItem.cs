using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry
{
	public class IncidentClosureDispositionRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<IncidentClosureDispositionCollection, IncidentClosureDispositionCollection>
	{
		public IncidentClosureDispositionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, IncidentClosureDispositionRegistryEditorInfo editorInfo, IncidentClosureDispositionCollection defaultValues)
			: base(new RegistryItemImpl(name, category, caption, hint, new IncidentClosureDispositionRegistryDataType(defaultValues), editorInfo, storage, RegistryOptions.Default, defaultValues), editorInfo)
		{ }

		public override int MaxLength => 250;
	}

	public class IncidentClosureDispositionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<IncidentClosureDispositionCollection>
	{
		public IncidentClosureDispositionRegistryDataType(IncidentClosureDispositionCollection defaultValue)
			: base(defaultValue)
		{
		}

		protected override IncidentClosureDispositionCollection DeserialiseCore(byte[] value)
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

					foreach (IncidentClosureDisposition node in result)
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

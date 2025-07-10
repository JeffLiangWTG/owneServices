using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ServiceLevelVisibilityRegistryItem : StronglyTypedRegistryItem<RegistryServiceLevelCollection>
	{
		public ServiceLevelVisibilityRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new ServiceLevelVisibilityRegistryDataType(), RegistryStorageFlags.System, options, RegistryServiceLevelCollection.GetDefaultLevels()))
		{
			EditorInfo = new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("5936a433-6562-4382-9092-ca5d3d8acf43", "Published"), true, true, true);
		}
	}

	class ServiceLevelVisibilityRegistryDataType : NonPersistentBusinessObjectRegistryDataType<RegistryServiceLevelCollection>
	{
		protected override RegistryServiceLevelCollection DeserialiseCore(byte[] value)
		{
			RegistryServiceLevelCollection deserialisedCollection = base.DeserialiseCore(value);
			RegistryServiceLevelCollection result = RegistryServiceLevelCollection.GetDefaultLevels();
			foreach (RegistryServiceLevel level in result)
			{
				RegistryServiceLevel foundLevel = deserialisedCollection.FindByRefServiceLevelPK(level.RefServiceLevelPK);
				if (foundLevel != null)
				{
					level.Bool = foundLevel.Bool;
				}
			}

			return result;
		}
	}
}

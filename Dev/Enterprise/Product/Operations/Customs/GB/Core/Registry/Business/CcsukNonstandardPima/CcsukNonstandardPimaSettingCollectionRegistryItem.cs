using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	public class CcsukNonstandardPimaSettingCollectionRegistryItem : StronglyTypedRegistryItem<CcsukNonstandardPimaSettingCollection>
	{
		public CcsukNonstandardPimaSettingCollectionRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage, CcsukNonstandardPimaSettingCollection defaultCollection)
			: base(new RegistryItemImpl(name, category, (NoResString)caption, (NoResString)hint, new CcsukNonstandardPimaRegistryDataType(), storage, defaultCollection))
		{
		}
	}
}

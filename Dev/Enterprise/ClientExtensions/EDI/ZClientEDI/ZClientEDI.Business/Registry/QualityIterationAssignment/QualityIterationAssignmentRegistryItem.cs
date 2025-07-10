using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class QualityIterationAssignmentRegistryItem : StronglyTypedRegistryItem<QualityIterationAssignmentHeader>
	{
		public QualityIterationAssignmentRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, new QualityIterationAssignmentHeader())
		{
		}

		public QualityIterationAssignmentRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryBusinessObjectTemplate defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new QualityIterationAssignmentRegistryDataType(), storage, defaultValue))
		{
		}
	}
}


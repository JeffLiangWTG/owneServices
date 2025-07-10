using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business
{
	public class CNDeclarationDeadlineWarningThresholdRegistryItem : StronglyTypedRegistryItem<CNDeclarationDeadlineWarningThresholdCollection>
	{
		public CNDeclarationDeadlineWarningThresholdRegistryItem(
					string name,
					MultilingualString category,
					MultilingualString caption,
					MultilingualString hint,
					RegistryStorageFlags storage,
					CNDeclarationDeadlineWarningThresholdCollection defaultValue)
						: base(new RegistryItemImpl(
						name, category, caption, hint, new CNDeclarationDeadlineWarningThresholdRegistryDataType(), storage, defaultValue))
		{
		}
	}
}

using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItem : StronglyTypedRegistryItem<AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection>
	{
		public AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItemEditor, Enterprise.Registry.GUI")]
	class AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection>
	{
		public AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryDataType()
		{
		}
	}
}


using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class TrainingZoneRateRegistryItem : StronglyTypedRegistryItem<TrainingZoneRateCollection>
	{
		public TrainingZoneRateRegistryItem(string category)
			: base(new RegistryItemImpl(
				"TrainingZoneRate",
				(NoResString)category,
				(NoResString)"Training Zone Rates",
				(NoResString)"Please specify rates for training zones",
				new TrainingZoneRateDataType(),
				RegistryStorageFlags.System))
		{
		}
	}
}


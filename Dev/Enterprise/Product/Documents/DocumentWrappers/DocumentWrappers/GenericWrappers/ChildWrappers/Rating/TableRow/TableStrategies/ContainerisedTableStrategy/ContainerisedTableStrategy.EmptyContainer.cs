using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	partial class ContainerisedTableStrategy
	{
		sealed class EmptyContainer : IRefContainer
		{
			public ZGuid PK => ZGuid.Empty;
			public ZString RC_Code { get; set; }
			public ZString RC_ContainerType { get; set; }
			public ZString RC_Description { get; set; }
			public ZString RC_ISOType { get; set; }
			public ZString RC_StorageClass { get; set; }
			public ZString RC_FreightRateClass { get; set; }
		}
	}
}

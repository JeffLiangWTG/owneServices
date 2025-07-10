using System;
using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICustomsDataRegistry
			{
				IRegistryItem AlwaysAssumeLocalOrganisationIsBothImporterAndExporter { get; }
				ImmutableList<ZString> ASNRefreshOptionsFieldTypes(Guid companyPK);
				IRegistryItem EnableExactMatchForProduct { get; }
				bool IsAUSeaCargoHouseEnabled { get; }
				ZString LocalCountryCustomsInterfaceSubmissionType
				{
					get;
#if DEBUG
					set;
#endif
				}
				IRegistryItem MemoryThresholdForMessageProcessing { get; }
				IRegistryItem OrdersPerDeclarationLimit { get; }
				IRegistryItem OrdersPerDeclarationLimitIntroductionTimeUTC { get; }
				IRegistryItem ShowHeaderTariffData { get; }
				IRegistryItem DeclarationNumberCustomisation { get; }
			}
		}
	}
}

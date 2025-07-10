using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public delegate RoadRunnerDetails GetAndCacheSingleResourceRoadRunnerDetails(GlbStaff resource, BusinessObjectFactory factory);

	public class ChannelFactoryParameters
	{
		public ChannelFactoryParameters(
			PropertyCache cache,
			bool isPreview,
			bool isBuffer,
			bool isInConstrainedMode,
			bool hideCapabilityTasksFromResourceChannels,
			bool hideResourceTasksFromCapabilityChannels,
			ZGuid sectionPK,
			BoardFactoryProvider factoryProvider,
			IEnumerable<ZGuid> capabilityChannelEntityPKs,
			Func<ComponentGrid> getComponentGrid = null,
			GetAndCacheSingleResourceRoadRunnerDetails getAndCacheSingleResourceRoadRunnerDetails = null)
		{
			Cache = cache;
			IsPreview = isPreview;
			IsBuffer = isBuffer;
			IsInConstrainedMode = isInConstrainedMode;
			HideCapabilityTasksFromResourceChannels = hideCapabilityTasksFromResourceChannels;
			HideResourceTasksFromCapabilityChannels = hideResourceTasksFromCapabilityChannels;
			SectionPK = sectionPK;
			FactoryProvider = factoryProvider;
			CapabilityChannelEntityPKs = capabilityChannelEntityPKs;
			GetComponentGrid = getComponentGrid;
			GetAndCacheSingleResourceRoadRunnerDetails = getAndCacheSingleResourceRoadRunnerDetails;
		}

		public PropertyCache Cache { get; }
		public bool IsPreview { get; }
		public bool IsBuffer { get; }
		public bool IsInConstrainedMode { get; }
		public bool HideCapabilityTasksFromResourceChannels { get; }
		public bool HideResourceTasksFromCapabilityChannels { get; }
		public ZGuid SectionPK { get; }
		public BoardFactoryProvider FactoryProvider { get; }
		public IEnumerable<ZGuid> CapabilityChannelEntityPKs { get; }
		public Func<ComponentGrid> GetComponentGrid { get; }
		public GetAndCacheSingleResourceRoadRunnerDetails GetAndCacheSingleResourceRoadRunnerDetails { get; }
	}
}

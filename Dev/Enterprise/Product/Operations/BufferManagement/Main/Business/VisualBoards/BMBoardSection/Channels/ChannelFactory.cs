using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public static class ChannelFactory
	{
		public static IEnumerable<IVisualBoardChannel> CreatePrimaryChannels(BMBoardSectionViewModel viewModel, BMBoardSection section)
		{
			return CreatePrimaryChannels(CreateParameters(viewModel, section), section);
		}

		public static IEnumerable<IVisualBoardChannel> CreatePrimaryChannels(ChannelFactoryParameters parameters, BMBoardSection section)
		{
			if (section.SectionConfiguration.IsReleaseScheduler)
			{
				return CreateReleaseSchedulerPrimaryChannels(parameters, section);
			}
			else
			{
				return CreateChannels(parameters, section.SectionConfiguration.OrderedPrimaryAxisChannels, section.Factory);
			}
		}

		public static IEnumerable<IVisualBoardChannel> CreateSecondaryChannels(BMBoardSectionViewModel viewModel, BMBoardSection section)
		{
			return CreateSecondaryChannels(CreateParameters(viewModel, section), section);
		}

		public static IEnumerable<IVisualBoardChannel> CreateSecondaryChannels(ChannelFactoryParameters parameters, BMBoardSection section)
		{
			if (section.SectionConfiguration.IsReleaseScheduler)
			{
				return CreateReleaseSchedulerSecondaryChannels();
			}
			else
			{
				return CreateChannels(parameters, section.SectionConfiguration.OrderedSecondaryAxisChannels, section.Factory);
			}
		}

		static ChannelFactoryParameters CreateParameters(BMBoardSectionViewModel viewModel, BMBoardSection section)
		{
			return new ChannelFactoryParameters(
				viewModel.Cache,
				viewModel.IsPreview,
				viewModel.IsBuffer,
				viewModel.IsInConstrainedMode,
				viewModel.HideCapabilityTasksFromResourceChannels,
				viewModel.HideResourceTasksFromCapabilityChannels,
				viewModel.SectionPK,
				viewModel.FactoryProvider,
				section?.CapabilityChannelEntityPKs,
				() => viewModel.ComponentGrid,
				viewModel.GetAndCacheSingleResourceRoadRunnerDetails
			);
		}

		static IEnumerable<IVisualBoardChannel> CreateReleaseSchedulerSecondaryChannels()
		{
			yield return new ReleaseSchedulerReleasedChannel();
			yield return new ReleaseSchedulerUnReleasedChannel();
		}

		static IEnumerable<IVisualBoardChannel> CreateReleaseSchedulerPrimaryChannels(ChannelFactoryParameters parameters, BMBoardSection section)
		{
			var channels = new List<IVisualBoardChannel>();

			var releaseGroup = section.ReleaseGroup;
			var component = section.Component;

			if (component != null)
			{
				if (releaseGroup != null)
				{
					var constrainedResources = releaseGroup.Staff.Cast<GlbStaff>()
						.Where(staff => staff.GS_IsActive && ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(staff, component))
						.OrderBy(s => s.GS_FullName)
						.ToArray();

					channels.AddRange(constrainedResources.Select(staff => new ResourceChannel(staff, parameters)));
					channels.Add(new NonConstrainedResourcesChannel(constrainedResources));
				}
				else
				{
					channels.Add(new NonConstrainedResourcesChannel(Enumerable.Empty<GlbStaff>()));
				}
			}

			return channels;
		}

		static IEnumerable<IVisualBoardChannel> CreateChannels(ChannelFactoryParameters parameters, IEnumerable<BMBoardSectionChannel> channels, BusinessObjectFactory factory)
		{
			var hasCurrentUser = channels.Any(c => c.IsCurrentUser);

			return channels
				.Select(channel => CreateChannel(parameters, channel, factory, hasCurrentUser))
				.Where(channel => channel != null)
				.ToArray();
		}

		static IVisualBoardChannel CreateChannel(ChannelFactoryParameters parameters, BMBoardSectionChannel channel, BusinessObjectFactory factory, bool hasCurrentUser)
		{
			if (channel == null || (hasCurrentUser && !channel.IsCurrentUser && channel.MSC_ParentID == EnvProxy.Instance.CurrentUser.PK))
			{
				return null;
			}

			if (channel.IsUnChanneled && !channel.MSC_ChannelType.IsEmpty)
			{
				return new UnchanneledChannel(channel.MSC_ChannelType);
			}

			if (channel.IsCurrentUser)
			{
				var currentUserChannel = BMBoardSectionChannel.GetCurrentUserChannel(channel);

				return CreateChannel(currentUserChannel.GetChannelBusinessObject(factory), parameters);
			}

			return CreateChannel(channel.GetChannelBusinessObject(factory), parameters);
		}

		static VisualBoardChannel CreateChannel(BusinessObject channelEntity, ChannelFactoryParameters parameters)
		{
			if (channelEntity == null)
			{
				return null;
			}

			if (channelEntity is GlbStaff staff)
			{
				return new ResourceChannel(staff, parameters);
			}

			if (channelEntity is GlbGroup group)
			{
				return new GroupChannel(group, () => parameters.Cache, () => parameters.FactoryProvider);
			}

			if (channelEntity is GlbCapability capability)
			{
				return new CapabilityChannel(capability, parameters.HideResourceTasksFromCapabilityChannels, () => parameters.Cache, () => parameters.FactoryProvider);
			}

			if (channelEntity is WorkQueue queue)
			{
				return new WorkQueueChannel(queue, () => parameters.Cache, () => parameters.FactoryProvider);
			}

			if (channelEntity is TagMagnitude tag)
			{
				return new TagMagnitudeChannel(tag, () => parameters.Cache, () => parameters.FactoryProvider);
			}

			return null;
		}

		#region Tests
#if DEBUG

		public static IVisualBoardChannel CreateChannelForTest(this BMBoardSectionViewModel viewModel, BMBoardSectionChannel channel, BusinessObjectFactory factory) => CreateChannel(CreateParameters(viewModel, channel?.Section), channel, factory, false);

		public static IVisualBoardChannel CreateChannelForTest(this BMBoardSectionViewModel viewModel, BusinessObject channelEntity)
		{
			var channel = CreateChannel(channelEntity, CreateParameters(viewModel, null));
			viewModel.AddChannelForTest(channel);
			return channel;
		}

#endif
		#endregion
	}
}

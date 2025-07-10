using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	internal class CapabilityChannel : VisualBoardChannel
	{
		internal CapabilityChannel(GlbCapability capability, bool hideResourceTasksFromCapabilityChannels, Func<PropertyCache> getCache, Func<BoardFactoryProvider> getFactoryProvider)
			: base(capability.PK, capability.G4_Code, ChannelTypeList.Codes.Capability, getCache, getFactoryProvider)
		{
			HideResourceTasksFromCapabilityChannels = hideResourceTasksFromCapabilityChannels;
		}

		readonly bool HideResourceTasksFromCapabilityChannels;

		#region Status

		protected override ChannelStatus GetChannelStatusCore(BusinessObjectFactory factory, PropertyCache propertyCache)
		{
			return new ChannelStatus();
		}

		#endregion

		#region Descriptor

		protected override ChannelDescriptor GetChannelDescriptorCore(BusinessObjectFactory factory)
		{
			var bizo = factory.Load<GlbCapability>(EntityPK);
			if (bizo != null)
			{
				return new ChannelDescriptor
				{
					ChannelNames = new ChannelNameBuilder(bizo.G4_Description, bizo.G4_Code).Build(),
				};
			}
			else
			{
				return new ChannelDescriptor();
			}
		}

		#endregion

		#region Matcher

		protected override ChannelMatcher GetChannelMatcherCore(BusinessObjectFactory factory)
		{
			return new ChannelMatcher(task => IsInChannel(task));
		}

		bool IsInChannel(IProcessTask task)
		{
			return (EntityPK == task.P9_G4_RequiredCapability)
				&& (task.P9_GS_NKAssignedStaffMember.IsEmpty || !HideResourceTasksFromCapabilityChannels);
		}

		#endregion
	}
}

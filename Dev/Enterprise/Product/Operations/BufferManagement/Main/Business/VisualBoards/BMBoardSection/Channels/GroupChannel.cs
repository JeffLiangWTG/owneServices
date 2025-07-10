using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	internal class GroupChannel : VisualBoardChannel
	{
		internal GroupChannel(GlbGroup group, Func<PropertyCache> getCache, Func<BoardFactoryProvider> getFactoryProvider)
			: base(group.PK, group.GG_Code, ChannelTypeList.Codes.Group, getCache, getFactoryProvider)
		{
		}

		#region Status

		protected override ChannelStatus GetChannelStatusCore(BusinessObjectFactory factory, PropertyCache propertyCache)
		{
			return new ChannelStatus();
		}

		#endregion

		#region ChannelDescriptor

		protected override ChannelDescriptor GetChannelDescriptorCore(BusinessObjectFactory factory)
		{
			var bizo = factory.Load<GlbGroup>(EntityPK);

			if (bizo != null)
			{
				return new ChannelDescriptor
				{
					ChannelNames = new ChannelNameBuilder(bizo.GG_Desc, bizo.GG_Code).Build(),
				};
			}
			else
			{
				return new ChannelDescriptor();
			}
		}

		#endregion

		#region ChannelMatcher

		protected override ChannelMatcher GetChannelMatcherCore(BusinessObjectFactory factory)
		{
			var bizo = factory.Load<GlbGroup>(EntityPK);
			if (bizo != null)
			{
				var resources = bizo.Staff.Cast<GlbStaff>().Select(s => (s.PK, s.GS_Code)).Where(s => !string.IsNullOrEmpty(s.GS_Code)).ToHashSet();

				return new ChannelMatcher(task => IsInChannel(task, resources.Select(r => r.GS_Code).ToHashSet()));
			}
			else
			{
				return ChannelMatcher.Empty;
			}
		}

		bool IsInChannel(IProcessTask task, HashSet<ZString> resourceCodes)
		{
			var concreteTask = (ProcessTask)task;
			return concreteTask.P9_GG_AssignedGroup == EntityPK
				|| concreteTask.AssignedStaffMember != null && resourceCodes.Contains(concreteTask.P9_GS_NKAssignedStaffMember)
				|| concreteTask.ProcessHeader != null && concreteTask.ProcessHeader.FH_GG_ReleaseGroup == EntityPK;
		}

		#endregion
	}
}


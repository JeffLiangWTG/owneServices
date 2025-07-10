using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	internal class WorkQueueChannel : VisualBoardChannel
	{
		internal WorkQueueChannel(WorkQueue queue, Func<PropertyCache> getCache, Func<BoardFactoryProvider> getFactoryProvider)
			: base(queue.PK, queue.TGM_Code, ChannelTypeList.Codes.Tag, getCache, getFactoryProvider)
		{
		}

		#region Status

		protected override ChannelStatus GetChannelStatusCore(BusinessObjectFactory factory, PropertyCache propertyCache)
		{
			return new ChannelStatus
			{
				IsHighRisk = false,
			};
		}

		#endregion

		#region Colors

		protected override ChannelDescriptor GetChannelDescriptorCore(BusinessObjectFactory factory)
		{
			var bizo = factory.Load<WorkQueue>(EntityPK);

			if (bizo != null)
			{
				return new ChannelDescriptor
				{
					ChannelNames = new ChannelNameBuilder(bizo.DisplayText, bizo.TGM_Description, bizo.TGM_Code).Build(),
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
			return ((ProcessTask)task).GetApplicableTags().Any(t => t.PK == EntityPK);
		}

		#endregion
	}
}

using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	internal class TagMagnitudeChannel : VisualBoardChannel
	{
		internal TagMagnitudeChannel(TagMagnitude tag, Func<PropertyCache> getCache, Func<BoardFactoryProvider> getFactoryProvider)
			: base(tag.PK, tag.TGM_Code, ChannelTypeList.Codes.Tag, getCache, getFactoryProvider)
		{
			tag.Factory.AddFetchHint(TagDefinitionSchema.PK, tag.TGM_TGD_Tag);
		}

		#region Status

		protected override ChannelStatus GetChannelStatusCore(BusinessObjectFactory factory, PropertyCache propertyCache)
		{
			return new ChannelStatus();
		}

		#endregion

		#region Descriptor

		protected override ChannelDescriptor GetChannelDescriptorCore(BusinessObjectFactory factory)
		{
			var bizo = factory.Load<TagMagnitude>(EntityPK);

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

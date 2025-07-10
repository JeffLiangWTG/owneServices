using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSectionChannelValidation : AutoBMBoardSectionChannelValidation
	{
		public BMBoardSectionChannelValidation(AutoBMBoardSectionChannel channel)
			: base(channel)
		{
		}

		protected new BMBoardSectionChannel Parent => (BMBoardSectionChannel)base.Parent;

		BMBoardSectionChannelCollection GetChannelsCollection()
		{
			if (Parent.MSC_Axis == ChannelAxisCodeList.Codes.Primary)
			{
				return Parent?.Section?.SectionConfiguration?.PrimaryAxisChannels;
			}
			else
			{
				return Parent?.Section?.SectionConfiguration?.SecondaryAxisChannels;
			}
		}

		protected override void CheckMSC_ChannelType()
		{
			base.CheckMSC_ChannelType();

			MandatoryValidation.CheckEntered(Parent.MSC_ChannelTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.MSC_ChannelTypeInfo);

			if (!Parent.IsUnChanneled)
			{
				var channels = GetChannelsCollection();
				if (channels != null)
				{
					var unchanneledChannel = channels.FirstOrDefault(c => c.IsUnChanneled);

					if (unchanneledChannel != null && !unchanneledChannel.MSC_ChannelType.IsEmpty && unchanneledChannel.MSC_ChannelType != Parent.MSC_ChannelType)
					{
						Parent.MSC_ChannelTypeInfo.AddError(Res.GetString("1832b3ec-0247-46e0-bd03-58a040ae108e", "When showing un-channeled work, only one type of channel is allowed."));
					}
				}

				if (Parent.IsCurrentUser)
				{
					CheckPropertyIsUniqueInChannelCollection(Parent.MSC_ChannelTypeInfo);
				}
			}
		}

		protected override void CheckMSC_ParentID()
		{
			base.CheckMSC_ParentID();

			ListValidation.ErrorIfInvalidPK(Parent.MSC_ParentIDInfo);
			ListValidation.ErrorIfCancelledAndEditable(Parent.MSC_ParentIDInfo);

			if (!Parent.IsUnChanneled && !Parent.IsCurrentUser)
			{
				MandatoryValidation.CheckEntered(Parent.MSC_ParentIDInfo);
			}

			CheckPropertyIsUniqueInChannelCollection(Parent.MSC_ParentIDInfo);
		}

		void CheckPropertyIsUniqueInChannelCollection(ZPropertyInfo propertyInfo)
		{
			var config = Parent.Section.SectionConfiguration;
			var allChannels = config.PrimaryAxisChannels.Concat(config.SecondaryAxisChannels);

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(propertyInfo, allChannels, Res.GetString("aaa55871-1e79-430e-b5a7-43cd6d57c469", "Cannot have the same channel more than once on one board section."));
		}
	}
}

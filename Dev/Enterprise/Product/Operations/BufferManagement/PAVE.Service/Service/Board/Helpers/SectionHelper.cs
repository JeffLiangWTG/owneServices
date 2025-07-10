using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Model = CargoWise.PAVE.Common.Model;

namespace Enterprise.BufferManagement.Service
{
	public static class SectionHelper
	{
		#region Model To DTO

		public static FlowDirection CreateFlowDirection(string flowDirection)
		{
			switch (flowDirection)
			{
				case FlowDirectionList.Codes.Left:
					return FlowDirection.Left;
				case FlowDirectionList.Codes.Right:
					return FlowDirection.Right;
				case FlowDirectionList.Codes.Up:
					return FlowDirection.Up;
				case FlowDirectionList.Codes.Down:
				default:
					return FlowDirection.Down;
			}
		}

		public static SectionLayoutDTO ToLayoutDTO(this Model.SectionLayout model)
		{
			return new SectionLayoutDTO
			{
				ColSpan = model.ColSpan,
				Column = model.Column,
				ColWidthPercent = model.ColWidthPercent,
				Row = model.Row,
				RowHeightPercent = model.RowHeightPercent,
				RowSpan = model.RowSpan
			};
		}

		public static ChannelType ToChannelTypeDTO(string channelType, bool overrideChannels = false)
		{
			if (overrideChannels)
			{
				return ChannelType.Override;
			}

			switch (channelType)
			{
				case ChannelTypeList.Codes.Capability:
					return ChannelType.Capability;
				case ChannelTypeList.Codes.Group:
					return ChannelType.Group;
				case ChannelTypeList.Codes.Resource:
					return ChannelType.Resource;
				case ChannelTypeList.Codes.Tag:
					return ChannelType.Tag;
				case ChannelTypeList.Codes.ReleaseSchedulerChannels:
					return ChannelType.ReleaseScheduler;
				case BMConstants.ChannelByTimeCode:
					return ChannelType.Time;
				case ChannelTypeList.Codes.NotChanneled:
				default:
					return ChannelType.NotChanneled;
			}
		}

		#endregion
	}
}

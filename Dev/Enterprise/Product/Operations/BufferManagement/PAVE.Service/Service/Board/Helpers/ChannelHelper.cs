using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.PAVE.Common.DTO;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Model = CargoWise.PAVE.Common.Model;

namespace Enterprise.BufferManagement.Service
{
	internal static class ChannelHelper
	{
		#region Model To DTO

		public static IEnumerable<ChannelContentDTO> ToChannelDTOs(this IEnumerable<IChannelMatcher> channelMatchers,
			IEnumerable<Model.Task> tasks,
			IEnumerable<Model.Workflow> workflows)
		{
			Model.Workflow GetWorkflow(Guid pk) => workflows.FirstOrDefault(w => w.PK == pk);

			foreach (var matcher in channelMatchers)
			{
				var channelDTO = new ChannelContentDTO()
				{
					EntityPK = matcher.Channel.EntityPK
				};

				channelDTO.Tasks = tasks.Where(task => matcher.IsInChannel(task, GetWorkflow(task.WorkflowPK))).Select(t => t.PK);

				yield return channelDTO;
			}
		}

		#endregion

		#region BusinessObject to DTO

		public static IEnumerable<ChannelContentDTO> ToChannelsDTO(this IEnumerable<IVisualBoardChannel> channels, Func<IVisualBoardChannel, IEnumerable<Guid>> getTaskPKs)
		{
			return channels.Select(channel => new ChannelContentDTO()
			{
				EntityPK = channel.EntityPK.IsValid ? channel.EntityPK.ToGuid() : Guid.Empty,
				Tasks = getTaskPKs(channel)
			}).ToArray();
		}

		#endregion
	}
}

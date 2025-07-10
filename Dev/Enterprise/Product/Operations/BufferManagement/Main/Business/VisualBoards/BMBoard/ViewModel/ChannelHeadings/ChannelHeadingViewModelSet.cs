using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	/// <summary>
	/// This class exists to ensure the dictionary inside it compares keys based on something other than instance.
	/// </summary>
	public class ChannelHeadingViewModelSet
	{
		public ChannelHeadingViewModelSet(ImmutableDictionary<IVisualBoardChannel, ChannelHeadingViewModel> channelViewModels, ImmutableDictionary<ZString, RoadRunnerDetails> roadRunnerDetails)
		{
			this.channelViewModels = channelViewModels;
			this.roadRunnerDetails = roadRunnerDetails;
		}

		public IDictionary<IVisualBoardChannel, ChannelHeadingViewModel> ChannelViewModels => channelViewModels;
		public IDictionary<ZString, RoadRunnerDetails> RoadRunnerDetails => roadRunnerDetails;

		readonly ImmutableDictionary<IVisualBoardChannel, ChannelHeadingViewModel> channelViewModels;
		readonly ImmutableDictionary<ZString, RoadRunnerDetails> roadRunnerDetails;
	}
}

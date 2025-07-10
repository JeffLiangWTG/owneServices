using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	public partial class SecondaryChannelsTabPageControl : ChannelsTabPageControlBase
	{
		public SecondaryChannelsTabPageControl()
		{
			InitializeComponent();
		}

		protected override ChannelAssignmentControl ChannelsControl
		{
			get { return SecondaryChannelsControl; }
		}

		protected override BMBoardSectionChannelsViewModel GetChannelsViewModel(BMBoardSection section)
		{
			return section.SectionConfiguration.SecondaryChannelsViewModel;
		}
	}
}

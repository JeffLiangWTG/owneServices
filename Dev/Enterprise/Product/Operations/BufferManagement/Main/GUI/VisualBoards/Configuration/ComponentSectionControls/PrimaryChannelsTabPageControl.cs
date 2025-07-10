using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	public partial class PrimaryChannelsTabPageControl : ChannelsTabPageControlBase
	{
		public PrimaryChannelsTabPageControl()
		{
			InitializeComponent();
		}

		protected override ChannelAssignmentControl ChannelsControl
		{
			get { return PrimaryChannelsControl; }
		}

		protected override BMBoardSectionChannelsViewModel GetChannelsViewModel(BMBoardSection section)
		{
			return section.SectionConfiguration.PrimaryChannelsViewModel;
		}
	}
}

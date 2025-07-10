namespace Enterprise.BufferManagement.GUI
{
	public class ChannelStatusPictureBox : OptimisticPictureBox
	{
		public ChannelStatusPictureBox()
			: base(shouldDisposeImageOnControlDispose: false) // Image life cycle is handled through the board refresh process.
		{
			Name = nameof(ChannelStatusPictureBox);
		}

		public bool IsClickable { get; internal set; }
	}
}

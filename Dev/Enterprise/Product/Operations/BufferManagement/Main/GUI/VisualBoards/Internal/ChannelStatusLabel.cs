namespace Enterprise.BufferManagement.GUI
{
	public class ChannelStatusLabel : DirectionalLabel
	{
		public ChannelStatusLabel(bool isVertical)
			: base(isVertical)
		{
			Name = nameof(ChannelStatusLabel);
		}
		public bool IsClickable { get; internal set; }
	}
}

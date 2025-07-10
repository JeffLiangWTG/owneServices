namespace Enterprise.BufferManagement.GUI
{
	public partial class DirectionalLabel : FadeLabel
	{
		protected override string TextStyleString => (isVertical) ? $"position: absolute; text-align: {base.CssTextAlign}; top: 0; right: 0; height: 90%; transform: rotate(180deg);writing-mode: vertical-lr; word-break: break-all;" : $"{base.TextStyleString} white-space: nowrap;";
	}
}

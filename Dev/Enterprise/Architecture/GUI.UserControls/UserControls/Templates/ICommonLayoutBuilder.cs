namespace Enterprise.ZArchitecture.GUI
{
	public interface ICommonLayoutBuilder
	{
		ColumnLayoutBuilderCaptionWidthSize CaptionWidth { get; }

		bool NarrowColumnForMediumControls { get; }
	}
}

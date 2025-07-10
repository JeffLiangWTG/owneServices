using System.ComponentModel;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Indicates where, relative to the control, a label caption should be rendered.
	/// </summary>
	public enum LabelCaptionAlignment
	{
		Auto,
		Left,
		Top,
		[Browsable(false)] // don't show this in the designer
		Default = Left,
	}
}

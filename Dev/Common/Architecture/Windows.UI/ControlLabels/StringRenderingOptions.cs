using System;
using System.ComponentModel;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Specifies how to measure and render a label caption.
	/// </summary>
	[Flags]
	public enum StringRenderingOptions
	{
		None = 1,
		Wrap = 2,
		Truncate = 4,
		[Browsable(false)] // don't show this in the designer
		Default = Truncate,
	}
}

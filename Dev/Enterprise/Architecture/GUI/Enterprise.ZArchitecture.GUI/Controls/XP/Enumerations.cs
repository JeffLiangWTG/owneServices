namespace Enterprise.Core.Forms
{
	/// <summary>
	/// The style to draw the button.
	/// </summary>
	public enum Style
	{
		XpStyle, Normal
	}

	/// <summary>
	/// The type of the button.
	/// </summary>
	[WTG.StaticAnalysis.Annotation.CodeAlive("The enum contains meaningful values used by multiple places.")]
	internal enum ButtonType
	{
		PushButton = 1, RadioButton,
		CheckBox, GroupBox,
		UserButton
	}

	/// <summary>
	/// The state of the button.
	/// </summary>
	public enum XPButtonState
	{
		Normal = 1, Hot,
		Pressed, Disabled,
		Defaulted
	}
}

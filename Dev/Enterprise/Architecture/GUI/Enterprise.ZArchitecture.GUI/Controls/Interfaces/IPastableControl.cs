namespace Enterprise.Core.Forms
{
	/// <summary>
	/// Implemented on a control that needs special handling of pasting rather than simply sending WM_PASTE to the control.
	/// </summary>
	public interface IPastableControl
	{
		#if !WINZOR

		/// <summary>
		/// returns if the paste was successfull based on number of things please also check if the control is readonly.
		/// </summary>
		bool TryPaste();

		#endif
	}
}

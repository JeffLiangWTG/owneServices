using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// A source for auto-list items.
	/// </summary>
	public interface IAutoListSource
	{
		/// <summary>
		/// Get the ITypeDescriptorContext object.
		/// </summary>
		ITypeDescriptorContext Context { get; }

		/// <summary>
		/// Create a new, unpopulated list control to be used to show as the drop down list. This control
		/// will not have focus while the user has the control visible, but it can receive mouse move
		/// events. You should update the currently selected item on mouse move events.
		/// </summary>
		Control NewListControl();

		/// <summary>
		/// Populate a list control with the auto-list items. If the items havn't changed since the last
		/// call to this method you can just leave it as it is. You should also set the selected item that
		/// is shown.
		/// </summary>
		/// <param name="listControl">The list control that was returned by NewListControl().</param>
		/// <param name="textBox">The text box that is used.</param>
		/// <returns>True if the list control should be shown now.</returns>
		bool UpdateListControl(Control listControl, TextBoxBase textBox);

		/// <summary>
		/// Replace the text on the text box after the user has indicated they want the currently selected
		/// item to be pasted into the text box.
		/// </summary>
		/// <param name="listControl">The list control that was returned by NewListControl().</param>
		/// <param name="textBox">
		/// The text box that is used.
		/// </param>
		void ReplaceText(Control listControl, TextBoxBase textBox);

		/// <summary>
		/// Does the given list control have a selection that should replace the text in the text box?
		/// </summary>
		bool HasSelection(Control listControl);

		/// <summary>
		/// When the value is to be committed.
		/// </summary>
		event EventHandler ValueCommitRequired;

		/// <summary>
		/// The KeyDown event of the TextBox.
		/// </summary>
		void OnTextBoxKeyDown(Control listControl, KeyEventArgs e);

		/// <summary>
		/// The KeyUp event of the TextBox.
		/// </summary>
		void OnTextBoxKeyUp(Control listControl, KeyEventArgs e);

		/// <summary>
		/// When the text box has left or lost focus. You should clean up any resources here.
		/// </summary>
		void OnTextBoxLostFocus();
	}
}

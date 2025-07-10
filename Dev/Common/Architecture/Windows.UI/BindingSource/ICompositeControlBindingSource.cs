using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Implemented on either a uer control to implement data binding of child controls, or on a BindingSource
	/// component that supports binding of a number of controls.
	/// </summary>
	public interface ICompositeControlBindingSourceProvider
	{
		ICompositeControlBindingSource BindingSource { get; }
	}

	/// <summary>
	/// Implemented on a component that manages the DataBindings for the controls of
	/// a composite control.
	/// </summary>
	public interface ICompositeControlBindingSource
	{
		/// <summary>
		/// The top level data source.
		/// </summary>
		Type DataSourceType { get; set; }

		/// <summary>
		/// The top level data source.
		/// </summary>
		object DataSource { get; }

		/// <summary>
		/// When the DataSource property has changed.
		/// </summary>
		event EventHandler DataSourceChanged;

		/// <summary>
		/// Get the binding member of a control.
		/// </summary>
		string GetBindingMember(Control control);

		/// <summary>
		/// Set the binding member of a control.
		/// </summary>
		void SetBindingMember(Control control, string value);

		/// <summary>
		/// Get the binding member of a control.
		/// </summary>
		string GetFullBindingMember(Control control);

		/// <summary>
		/// Get all binding members of the controls managed by the component.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IEnumerable<KeyValuePair<Control, string>> FullBindingMembers { get; }

		/// <summary>
		/// When one or more binding members change.
		/// </summary>
		event EventHandler<ControlBindingMemberChangedEventArgs> FullBindingMemberChanged;
	}

	/// <summary>
	/// Event information for the ICompositeControlBindingSource.FullBindingMemberChanged event.
	/// </summary>
	public sealed class ControlBindingMemberChangedEventArgs : EventArgs
	{
		public ControlBindingMemberChangedEventArgs(Control control, string bindingMember)
		{
			this.Control = control;
			this.BindingMember = bindingMember;
		}

		/// <summary>
		/// The control whose binding member has changed. If null, all binding members of all
		/// controls have changed.
		/// </summary>
		public Control Control { get; private set; }

		/// <summary>
		/// The new binding member for the control.
		/// </summary>
		public string BindingMember { get; private set; }
	}
}

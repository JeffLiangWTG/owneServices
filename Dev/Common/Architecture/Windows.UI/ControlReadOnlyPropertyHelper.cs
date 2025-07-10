using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Enumeration;

namespace CargoWise.Windows.UI
{
	public interface IEditableInViewMode
	{
		bool EditableInViewMode { get; }
	}

	/// <summary>
	/// A helper class to implement the ReadOnly meta-data property on a control.
	/// </summary>
	public class ControlReadOnlyPropertyHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
		public delegate bool GetReadOnlyDelegate();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
		public delegate void SetReadOnlyDelegate(bool value);

		public ControlReadOnlyPropertyHelper(Control control, GetReadOnlyDelegate getReadOnlyValue, SetReadOnlyDelegate setReadOnlyValue)
		{
			this.control = control;
			this.getReadOnlyValue = getReadOnlyValue;
			this.setReadOnlyValue = setReadOnlyValue;
		}

		public bool ReadOnlyForBinding
		{
			get
			{
				return readOnlyForBinding;
			}
			set
			{
				if (value != ReadOnlyForBinding)
				{
					using (SuppressOnReadOnlyChanged())
					{
						readOnlyForBinding = value;
						if (readOnlyForBinding)
						{
							readOnly = ControlReadOnly;
							ControlReadOnly = !EditableInViewMode;
						}
						else
						{
							ControlReadOnly = !EditableInViewMode && readOnly;
						}
					}
					OnReadOnlyForBindingChanged(EventArgs.Empty);
				}
			}
		}
		bool readOnlyForBinding;

		bool EditableInViewMode
			=> ZEnumerable.Iterate(control, c => c.Parent, null).OfType<IEditableInViewMode>().Any(c => c.EditableInViewMode);

		public event EventHandler ReadOnlyForBindingChanged;

		void OnReadOnlyForBindingChanged(EventArgs e)
		{
			if (ReadOnlyForBindingChanged != null)
			{
				ReadOnlyForBindingChanged(this, e);
			}
		}

		public void OnReadOnlyChanged()
		{
			if (suppressOnReadOnlyChangedIndex == 0)
			{
				using (SuppressOnReadOnlyChanged())
				{
					if (ReadOnlyForBinding)
					{
						ControlReadOnly = true;
					}
					readOnly = ControlReadOnly;
				}
			}
		}
		bool readOnly;

		#region Implementation

		int suppressOnReadOnlyChangedIndex;
		readonly Control control;
		readonly GetReadOnlyDelegate getReadOnlyValue;
		readonly SetReadOnlyDelegate setReadOnlyValue;

		IDisposable SuppressOnReadOnlyChanged()
		{
			suppressOnReadOnlyChangedIndex++;
			return new DisposableAction(delegate
			{
				suppressOnReadOnlyChangedIndex--;
			});
		}

		bool ControlReadOnly
		{
			get { return setReadOnlyTruePending || getReadOnlyValue(); }
			set
			{
#if !WINZOR
				if (value && control.IsHandleCreated && control.Focused)
				{
					if (!setReadOnlyTruePending)
					{
						setReadOnlyTruePending = true;
						// prevent re-entrancy problems when focus changes while setting ReadOnly=true
						control.BeginInvoke(new MethodInvoker(delegate
						{
							if (setReadOnlyTruePending)
							{
								using (SuppressOnReadOnlyChanged())
								{
									setReadOnlyTruePending = false;
									setReadOnlyValue(true);
								}
							}
						}));
					}
				}
				else
#endif
				{
					setReadOnlyValue(value);
					setReadOnlyTruePending = false;
				}
			}
		}
		bool setReadOnlyTruePending;

		#endregion
	}
}

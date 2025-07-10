using System;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// A helper class for exposing the BindingMember of a control from the KBindingSource component.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class ControlBindingMemberHelper : IDisposable
	{
		protected ControlBindingMemberHelper(Control control)
		{
			this.control = control;
		}

		public static ControlBindingMemberHelper Get(Control control)
		{
			ControlBindingMemberHelper result = control.GetUserData(ControlUserDataKey) as ControlBindingMemberHelper;
			try
			{
				if (result == null)
				{
					result = new ControlBindingMemberHelper(control);
					control.SetUserData(ControlUserDataKey, result);
				}
			}
			catch (ArgumentException ex)
			{
				var controlName = "";
				var controlTemp = control;
				while (controlTemp != null)
				{
					controlName += $"at Type:{controlTemp.GetType().FullName} Name:{controlTemp.Name}\r\n";
					controlTemp = controlTemp.Parent;
				}
				ErrorReporter.ReportOnce("ArgumentException in ControlBindingMemberHelper.Get", "Control:" + controlName, ex);
			}

			return result;
		}
		static readonly int ControlUserDataKey = ControlExtensions.CreateUserDataKey();

		public string BindingMember
		{
			get
			{
				if (!control.IsDisposed)
				{
					ICompositeControlBindingSource bindingSource = GetBindingSource();
					if (bindingSource != null)
					{
						return bindingSource.GetBindingMember(control);
					}
				}
				return bindingMember;
			}
			set
			{
				if (!control.IsDisposed)
				{
					bindingMember = value;
					currentBindingSource = GetBindingSource();
					if (currentBindingSource != null)
					{
						currentBindingSource.SetBindingMember(control, value);
					}
					if (string.IsNullOrEmpty(bindingMember))
					{
						currentBindingSource = null;
						AncestorChangeEvent.AncestorChanged -= Control_AncestorChanged;
					}
					else
					{
						AncestorChangeEvent.AncestorChanged += Control_AncestorChanged;
					}
				}
			}
		}
		string bindingMember = "";

		#region IDisposable

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
		public void Dispose()
		{
			BindingMember = "";
			control.SetUserData(ControlUserDataKey, null);
		}

		#endregion

		#region Implementation

		readonly Control control;
		ICompositeControlBindingSource currentBindingSource;

		ICompositeControlBindingSource GetBindingSource()
		{
			return KBindingSource.GetBindingSource(control);
		}

		ControlAncestorChangedEvent AncestorChangeEvent
		{
			get { return ancestorChangeEvent ?? (ancestorChangeEvent = ControlAncestorChangedEvent.Get(control)); }
		}
		ControlAncestorChangedEvent ancestorChangeEvent;

		void Control_AncestorChanged(object sender, EventArgs e)
		{
			ICompositeControlBindingSource newBindingSource = GetBindingSource();
			if (newBindingSource != currentBindingSource)
			{
				if (currentBindingSource != null)
				{
					currentBindingSource.SetBindingMember(control, "");
					currentBindingSource = null;
				}
				if (!string.IsNullOrEmpty(bindingMember))
				{
					currentBindingSource = GetBindingSource();
					if (currentBindingSource != null)
					{
						currentBindingSource.SetBindingMember(control, bindingMember);
					}
				}
			}
		}

		#endregion
	}
}

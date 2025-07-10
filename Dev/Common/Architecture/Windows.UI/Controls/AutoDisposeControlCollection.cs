using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// A ControlCollection subclass that automatically disposes controls that are removed to prevent resource leaks
	/// </summary>
	/// <summary><see cref="Control.ControlCollection"/></summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers")]
	public class AutoDisposeControlCollection : Control.ControlCollection
	{
		public AutoDisposeControlCollection(Control owner)
			: base(owner)
		{
			owner.Disposed += Owner_Disposed;
		}

		public override void Add(Control value)
		{
			try
			{
				base.Add(value);
			}
			catch (ArgumentException) //WI00258260
			{
				if (value is ButtonBase button)
				{
					button.ImageList = null;
					button.Image = null;
				}
				try
				{
					base.Add(value);
				}
				catch (ArgumentException)
				{
					//give up
				}
			}
			controlsToDispose.Remove(value);
		}

		public override void Remove(Control value)
		{
			try
			{
				base.Remove(value);
			}
			catch (NullReferenceException ex)
			{
				var message =
					(NoResString)"Null reference in ControlCollection.Remove()." +
					(NoResString)"\r\nOwner control: " + GetControlInfo(Owner, true) +
					(NoResString)"\r\nRemoved control: " + GetControlInfo(value, false) +
					(NoResString)"\r\nControl was " + (Contains(value) ? "" : (NoResString)"not ") + "removed.";

				ErrorReporter.ReportOnce(message, ex);
			}

			if (!value.IsDisposed && !value.Disposing)
			{
				controlsToDispose.Add(value);
				value.Disposed += Control_Disposed;
			}
		}

		string GetControlInfo(Control control, bool includeParents)
		{
			if (control == null)
			{
				return "null.";
			}

			var sb = new StringBuilder();
			sb.Append(control.Name + "(" + control.GetType().FullName + "), ");

			if (includeParents)
			{
				sb.Append((NoResString)"Parents: ");
				var parent = control.Parent;
				while (parent != null)
				{
					sb.Append(parent.Name);
					if (parent.Parent != null)
					{
						sb.Append(" : ");
					}
					parent = parent.Parent;
				}
			}

			sb.Append("IsHandleCreated = " + control.IsHandleCreated + ", ");
			sb.Append("IsDisposed = " + control.IsDisposed + ".");

			return sb.ToString();
		}

		void Control_Disposed(object sender, EventArgs e)
		{
			var control = sender as Control;
			if (control != null)
			{
				control.Disposed -= Control_Disposed;
				if (controlsToDispose.Contains(control))
				{
					controlsToDispose.Remove(control);
				}
			}
		}

		void Owner_Disposed(object sender, EventArgs e)
		{
			foreach (var control in controlsToDispose.ToArray())
			{
				if (control.Parent == null)
				{
					control.Dispose();
				}
			}

			controlsToDispose.Clear();
		}

		readonly HashSet<Control> controlsToDispose = new HashSet<Control>();
	}
}

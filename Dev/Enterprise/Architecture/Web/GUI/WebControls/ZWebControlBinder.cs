using System;
using System.Collections;
using System.Web.UI;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Summary description for ZWebControlBinder.
	/// </summary>
	public class ZWebControlBinder
	{
		readonly IBusiness DataSource;

		public ZWebControlBinder(IBusiness dataSource)
		{
			this.DataSource = dataSource;
		}

		public void Bind(ICollection controls, bool ignoreChanges)
		{
			AddFetchHints(controls);
			BindAllControls(controls, ignoreChanges);
		}

		public virtual void Bind(ICollection controls)
		{
			Bind(controls, false);
		}

		#region binding implementaiton

		void BindAllControls(ICollection controls, bool ignoreChanges)
		{
			var controlsArray = new Control[controls.Count];
			controls.CopyTo(controlsArray, 0);
			foreach (var control in controlsArray)
			{
				if (control is ISelfBindingWebControl)
				{
					BindControl(control, ignoreChanges);
				}
				else
				{
					BindAllControls(control.Controls, ignoreChanges);
				}
			}
		}

		void BindControl(Control control, bool ignoreChanges)
		{
			if (!(control is IBindTo))
			{
				throw new ArgumentException("The ZBinder should only be used to bind ZControls; '" + control.ID + "' is not a valid ZControl!");
			}

			if (ignoreChanges && (control is ISelfBindingPostbackWebControl))
			{
				((ISelfBindingPostbackWebControl)control).HasChanges = false;
			}
			if (control is ISelfBindingWebControl)
			{
				BindSelfBindingControl((ISelfBindingWebControl)control);
			}
		}

		void BindSelfBindingControl(ISelfBindingWebControl control)
		{
			if (control.IsBindable(DataSource))
			{
				control.Bind(DataSource);
			}
		}

		#endregion

		#region Fetch hints

		protected void AddFetchHints(ICollection controls)
		{
			foreach (Control control in controls)
			{
				if (control is IFetchHintGenerator)
				{
					((IFetchHintGenerator)control).AddFetchHint(DataSource, "");
				}
				else
				{
					AddFetchHints(control.Controls);
				}
			}
		}

		#endregion

	}
}

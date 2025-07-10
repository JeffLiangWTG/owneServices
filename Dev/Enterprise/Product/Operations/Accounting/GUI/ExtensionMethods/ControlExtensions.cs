using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Windows.UI.Layout;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public class ControlBasedVisibiltyProvider : IVisibilityProvider
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="independentControl">Control which determines the visibility of other controls</param>
		/// <param name="whenToShowTheDependentControl">delegate which will hold the function that determines whether Dependent Control will be hidden/visible</param>
		/// <param name="EventNameOfindependentControlWhenVisibilityChangeEventWillbeFired">Event Name Of independent Control When VisibleChanged Event will be Fired</param>
		public ControlBasedVisibiltyProvider(Control independentControl, Func<bool> whenToShowTheDependentControl, params string[] eventNameOfindependentControl)
		{
			this.WhenToShowTheDependentControl = whenToShowTheDependentControl;
			var evntHandler = this.GetType().GetMethod("RaiseVisibilityChanged", BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (var evntName in eventNameOfindependentControl)
			{
				var evntInfo = independentControl.GetType().GetEvent(evntName);
				var firingDelegate = evntInfo.EventHandlerType;
				var del = Delegate.CreateDelegate(firingDelegate, this, evntHandler);

				MethodInfo addHandler = evntInfo.GetAddMethod();
				Object[] addHandlerArgs = { del };
				addHandler.Invoke(independentControl, addHandlerArgs);
			}
		}

		#region IVisibilityDependencyProvider

		public bool Visible
		{
			get
			{
				return WhenToShowTheDependentControl();
			}
		}

		public event EventHandler VisibleChanged;
		#endregion

		#region Implementation
		readonly Func<bool> WhenToShowTheDependentControl;

		void RaiseVisibilityChanged(object sender, EventArgs e)
		{
			if (VisibleChanged != null)
			{
				VisibleChanged(sender, e);
			}
		}
		#endregion

	}

	public static class ControlExtensions
	{
		public static void SetVisibilityController(this Control control, IVisibilityProvider visibilityProvider, Action doThisExtraBitAsWell)
		{
			if (control is ZTabPage)
			{
				(control as ZTabPage).TabVisible = visibilityProvider.Visible;
			}
			else
			{
				control.Visible = visibilityProvider.Visible;
			}
			new VisibilityController(visibilityProvider, control, doThisExtraBitAsWell);
		}

		public static void SetEnabler(this Control control, IVisibilityProvider visibilityProvider, Action doThisExtraBitAsWell)
		{
			control.Enabled = visibilityProvider.Visible;
			new EnablementController(visibilityProvider, control, doThisExtraBitAsWell);
		}

		class VisibilityController
		{
			public VisibilityController(IVisibilityProvider visibilityProvider, Control control, Action doThisExtraBitAsWell)
			{
				this.visibilityProvider = visibilityProvider;
				this.visibilityProvider.VisibleChanged += visibilityProvider_VisibleChanged;
				this.control = control;
				this.DoThisExtraBitAsWell = doThisExtraBitAsWell;
			}

			void visibilityProvider_VisibleChanged(object sender, EventArgs e)
			{
				if (control is ZTabPage)
				{
					(control as ZTabPage).TabVisible = visibilityProvider.Visible;
				}
				else
				{
					control.Visible = visibilityProvider.Visible;
				}

				if (DoThisExtraBitAsWell != null)
				{
					DoThisExtraBitAsWell();
				}
			}

			readonly Control control;
			readonly Action DoThisExtraBitAsWell;
			readonly IVisibilityProvider visibilityProvider;
		}

		class EnablementController
		{
			public EnablementController(IVisibilityProvider visibilityProvider, Control control, Action doThisExtraBitAsWell)
			{
				this.visibilityProvider = visibilityProvider;
				this.visibilityProvider.VisibleChanged += visibilityProvider_VisibleChanged;
				this.control = control;
				this.DoThisExtraBitAsWell = doThisExtraBitAsWell;
			}

			void visibilityProvider_VisibleChanged(object sender, EventArgs e)
			{
				control.Enabled = visibilityProvider.Visible;
				if (DoThisExtraBitAsWell != null)
				{
					DoThisExtraBitAsWell();
				}
			}

			readonly Control control;
			readonly Action DoThisExtraBitAsWell;
			readonly IVisibilityProvider visibilityProvider;
		}
	}
}

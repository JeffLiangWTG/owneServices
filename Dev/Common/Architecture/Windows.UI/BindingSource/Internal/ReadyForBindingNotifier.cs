using System;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Windows.UI
{
	sealed class ReadyForBindingNotifier : IDisposable
	{
		public ReadyForBindingNotifier(Control control)
		{
			Control = control;
			UpdateReady();
			if (!ready)
			{
				var kControl = Control as IKControl;
				if (kControl != null)
				{
					kControl.HandleFullyCreated += UpdateReady;
				}
				else
				{
					Control.HandleCreated += UpdateReady;
				}

				Control.ParentChanged += UpdateReady;
				Control.VisibleChanged += UpdateReady;
			}
		}

		public Control Control { get; private set; }

		public bool Ready
		{
			get { return ready; }
			private set
			{
				if (ready != value)
				{
					ready = value;
					OnReadyChanged(EventArgs.Empty);
				}
			}
		}

		bool ready;

		public void Force()
		{
			Unhook();

			if (!ready)
			{
				HookUpdateBindingOnNonContainerControlCreatedIfNeeded();
			}

			Ready = true;
		}

		#region WI00435379
		void HookUpdateBindingOnNonContainerControlCreatedIfNeeded()
		{
			if (!(Control is ContainerControl) && !Control.Created && !Control.IsHandleCreated)
			{
				HookUpdateBindingOnCreated();
			}
		}

		void UpdateBindingOnCreated(object sender, EventArgs e)
		{
			UnhookUpdateBindingOnCreated();

			if (Control.Created)
			{
				foreach (Binding binding in Control.DataBindings)
				{
					BindingContext.UpdateBinding(Control.BindingContext, binding);
				}
			}
		}

		void HookUpdateBindingOnCreated()
		{
			var kControl = Control as IKControl;
			if (kControl != null)
			{
				kControl.HandleFullyCreated -= UpdateBindingOnCreated;
				kControl.HandleFullyCreated += UpdateBindingOnCreated;
			}
			else
			{
				Control.HandleCreated -= UpdateBindingOnCreated;
				Control.HandleCreated += UpdateBindingOnCreated;
			}
		}

		void UnhookUpdateBindingOnCreated()
		{
			var kControl = Control as IKControl;
			if (kControl != null)
			{
				kControl.HandleFullyCreated -= UpdateBindingOnCreated;
			}
			else
			{
				Control.HandleCreated -= UpdateBindingOnCreated;
			}
		}
		#endregion

		public event EventHandler ReadyChanged;

		void OnReadyChanged(EventArgs e)
		{
			if (ReadyChanged != null)
			{
				ReadyChanged(this, e);
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			Unhook();
			UnhookUpdateBindingOnCreated();
		}

		#endregion

		#region Implementation

		bool CalculateReady()
		{
			return Control.Created && Control.Visible;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void UpdateReady(object sender, EventArgs e)
		{
			try
			{
				UpdateReady();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce(ex.Message, ex);
			}
		}

		void UpdateReady()
		{
			Ready = CalculateReady();
			if (Ready)
			{
				Unhook();
			}
		}

		void Unhook()
		{
			var kControl = Control as IKControl;
			if (kControl != null)
			{
				kControl.HandleFullyCreated -= UpdateReady;
			}

			Control.HandleCreated -= UpdateReady;
			Control.ParentChanged -= UpdateReady;
			Control.VisibleChanged -= UpdateReady;
		}

		#endregion
	}
}

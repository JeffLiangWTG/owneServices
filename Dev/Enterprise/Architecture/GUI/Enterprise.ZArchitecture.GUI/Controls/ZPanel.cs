using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public class ZPanel : KPanel, IIsVisibleForBindingControl, IExtendedControl
	{
		public ZPanel()
		{
			UserEventTracker.Instance.AddUserEventToControl(this);
			DisposableLeakListener.Instance.RegisterDisposable(this);
			Extensions = NewExtensionCollection();
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);
			if (isNotFinalizing)
			{
				Extensions.Dispose();
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
		}

#if WINZOR
		protected override Cursor DefaultCursor => Cursors.Default;
#endif

		#region IsVisibleForBinding

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZBool IsVisibleForBinding
		{
			get { return Visible; }
			set
			{
				if (!fInVisibleForBinding)
				{
					fInVisibleForBinding = true;
					try
					{
						if (Visible != value)
						{
							Visible = value;
							OnIsVisibleForBindingChanged();
						}
					}
					finally
					{
						fInVisibleForBinding = false;
					}
				}
			}
		}

		bool fInVisibleForBinding;

		public event EventHandler IsVisibleForBindingChanged;

		protected virtual void OnIsVisibleForBindingChanged()
		{
			if (IsVisibleForBindingChanged != null)
			{
				IsVisibleForBindingChanged(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Drag and Drop

		[DefaultValue(true)]
		public override bool AllowDrop
		{
			get { return true; }
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			DragDropManager.HandleDragDrop(this, drgevent);
			base.OnDragDrop(drgevent);
		}

		protected override void OnDragOver(DragEventArgs drgevent)
		{
			DragDropManager.HandleDragOver(this, drgevent);
			base.OnDragOver(drgevent);
		}

		#endregion

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);
			NotificationBroadcaster.Instance.BroadcastVisibilityChange(this);
		}

#if !WINZOR
		protected override void WndProc(ref Message m)
		{
			try
			{
				base.WndProc(ref m);
			}
			catch (ArgumentException ex) when (m.Msg == WindowsMessage.WM_CONTEXTMENU)
			{
				ErrorReporter.ReportOnce("ArgumentException_CONTEXTMENU", $"ContextMenu cannot be shown on an invisible control. IsHandleCreated:{IsHandleCreated}, Visible:{Visible}.", ex);
			}
		}
#endif

		#region Metadata

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZPanel>()
			.Property("Text", string.Empty, true) // Property name
			.Property("IsVisibleForBinding", ZBool.True, true)
			.Result;
		}

		#endregion

		#region IExtendedControl

		Control IExtendedControl.Host => this;

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		IControlExtensionCollection NewExtensionCollection()
		{
			return new ControlExtensionCollection(this);
		}
	}
}

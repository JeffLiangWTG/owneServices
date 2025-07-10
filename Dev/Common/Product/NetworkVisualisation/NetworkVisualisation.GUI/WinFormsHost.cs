using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	public static class WinFormsHost
	{
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public static NetworkUserControl HostNetworkUserControlInWinForms(ElementHost hostControl, IDiagramEntity diagramEntity, INetworkRefresher networkProvider)
		{
			return HostNetworkUserControlInWinForms(hostControl, diagramEntity, networkProvider, null, null, null);
		}
#pragma warning restore CS0618 // Restore the warning for obsolete usage

#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public static NetworkUserControl HostNetworkUserControlInWinForms(ElementHost hostControl, IDiagramEntity diagramEntity, INetworkRefresher networkProvider, DataTemplateSelector selector, NodeViewModelProvider viewModelProvider, IRibbonDataProvider ribbonDataProvider)
		{
			var control = new NetworkUserControl(diagramEntity, networkProvider, selector, viewModelProvider, ribbonDataProvider);
			HostControlInWinForms(hostControl, control);
			control.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (!hostControl.IsDisposed && hostControl.IsHandleCreated)
				{
					control.SetDataContext(networkProvider.GetReloadedNetwork(), isReloading: false);
					RedrawHostControl(hostControl);
					control.ScrollToShape();
					control.SetInitialDrawingComplete();
					hostControl.TabStop = true;
				}
				SetNetworkUserControlHostedEvent(control);
			}));
			hostControl.Disposed += AddUnbindOnDispose(hostControl, control);
			return control;
		}
#pragma warning restore CS0618 // Restore the warning for obsolete usage

#pragma warning disable CS0618 // Disable the warning for obsolete usage
		static EventHandler AddUnbindOnDispose(ElementHost hostControl, NetworkUserControl control)
		{
			return (s, e) =>
			{
				/*
				 * HACK: There is an issue where event subscriptions cause memory leaks in ElementHost controls.
				 * This is a dirty way to remove all the subscriptions in Network.Connections.
				 * (Note: NodeViewModels are also cleared on dispose because the network.Entities collection is unbound.)
				 */
				control.ViewModel.NetworkViewModel.ScheduledConnections.Clear();
				control.ViewModel.NetworkViewModel.NonScheduledConnections?.Clear();
				control.CloseAllChildren();
				WorkaroundElementHostMemoryLeaks(hostControl, control);
				control.Dispose();
			};
		}

#pragma warning restore CS0618 // Restore the warning for obsolete usage
		// https://stackoverflow.com/questions/6373874/wpf-element-host-memory-leak
		// https://social.msdn.microsoft.com/Forums/vstudio/en-US/ef9c78a5-ee14-44b4-bc37-aef1b430f1da/memory-leak-involving-avalonadapter-and-elementhost?forum=wpf
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		static void WorkaroundElementHostMemoryLeaks(ElementHost hostControl, NetworkUserControl control)
		{
			SizeChangedEventHandler handler = (SizeChangedEventHandler)Delegate.CreateDelegate(typeof(SizeChangedEventHandler), hostControl, "childFrameworkElement_SizeChanged");
			control.SizeChanged -= handler;
		}
#pragma warning restore CS0618 // Restore the warning for obsolete usage

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "The width change here doesnt need to consider DPI.")]
		[SuppressMessage("CargoWiseOne", "CW1042", Justification = "The width change here doesnt need to consider DPI.")]
		static void RedrawHostControl(ElementHost hostControl)
		{
			/*
			 * HACK: 
			 * As per this link, "http://stackoverflow.com/questions/470846/winforms-wpf-interop-wpf-content-fails-to-paint".
			 * except using width=0 so that extra MemoryStream + buffer are not created by the ElementHost
			 */
			var width = hostControl.Width;
			hostControl.Width = 0;
			hostControl.Width = width;
		}

#if DEBUG
		public static void RedrawHostControl_ForTest(ElementHost hostControl)
		{
			RedrawHostControl(hostControl);
		}
#endif

#pragma warning disable CS0618 // Disable the warning for obsolete usage
		static void SetNetworkUserControlHostedEvent(NetworkUserControl control)
		{
#if DEBUG
			control.NetworkUserControlHosted.Set();
#endif
		}
#pragma warning restore CS0618 // Restore the warning for obsolete usage

#pragma warning disable CS0618 // Disable the warning for obsolete usage
		static void HostControlInWinForms(ElementHost hostControl, NetworkUserControl wpfControl)
		{
			hostControl.Child = wpfControl;
		}
#pragma warning restore CS0618 // Restore the warning for obsolete usage
	}
}

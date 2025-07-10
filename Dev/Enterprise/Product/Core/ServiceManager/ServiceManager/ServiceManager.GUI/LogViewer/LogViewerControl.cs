using System;
using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ServiceManager.GUI
{
	public interface ILogViewerControl
	{
		void RefreshLogs();
	}

	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class LogViewerControl : ZUserControl
	{
		public LogViewerControl()
		{
			InitializeComponent();
		}

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<LogViewerControl>().Result;
		}

		public ILogViewerControl StrategyLogViewerControl
		{
			get
			{
				return viewerControl as ILogViewerControl;
			}
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);
			var serviceTaskLogViewer = (Business.ServiceTaskLogViewer)BindingSource.Current;
			if (serviceTaskLogViewer != null && viewerControl == null)
			{
				InitializeViewerControl(serviceTaskLogViewer);
			}
		}
	}
}

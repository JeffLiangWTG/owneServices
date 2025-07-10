using System;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	public partial class ClientLinkModal : ZChildForm, ICaptionRenderingSupport
	{
		readonly Action onModalClose;

		public ClientLinkModal(string windowTitle, Action onModalClose = null)
		{
			Text = Res.GetString("f8ed50c5-a431-4bb1-4205-4cc98b504932", "Client Link - {0}", windowTitle);
			this.onModalClose = onModalClose;
			InitializeComponent();
		}

		public void CloseButton_Click(object sender, EventArgs e)
		{
			onModalClose?.Invoke();
			Close();
		}

		#region Disable status bar

		protected override bool ShowStatusBar => false;

		protected override void UpdateStatusBar(string notification, INotificationType state)
		{
			// this method needs to be overridden to avoid NRE during runtime
		}

		#endregion

		#region ICaptionRenderingSupport

		bool? ICaptionRenderingSupport.CaptionRenderingEnabled => true;

		event EventHandler ICaptionRenderingSupport.CaptionRenderingEnabledChanged
		{
			add { }
			remove { }
		}

		#endregion
	}
}

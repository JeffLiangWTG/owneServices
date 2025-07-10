using System;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	internal class ThumbnailSizeContextMenuExposed : ThumbNailSizeContextMenu
	{
		public void CallMenuItemClicked(object sender, EventArgs e)
		{
			base.OnMenuItemClicked(sender, e);
		}

		protected override void SaveSettings()
		{
			SaveSettingsHook?.Invoke();
			base.SaveSettings();
		}

#if !WINZOR

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);
			IsDisposed = true;
		}

		public
#if NET8_0_OR_GREATER
		new
#endif
		bool IsDisposed { get; set; }

#endif

		public Action SaveSettingsHook { get; set; }

		public void SaveSettingsForTest() => SaveSettings();
	}
}

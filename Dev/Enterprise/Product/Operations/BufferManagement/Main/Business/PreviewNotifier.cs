using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.BufferManagement.Business
{
	public interface IPreviewReceiver
	{
		void UpdatePreview();
	}

	public interface IPreviewNotifier : IPreviewNotifierChild
	{
		new IPreviewReceiver PreviewReceiver { get; set; }
	}

	public interface IPreviewNotifierChild
	{
		IPreviewReceiver PreviewReceiver { get; }
	}

	public static class PreviewNotifierExtensions
	{
		public static IDisposable LayoutConfigUpdatedOnDisposeIfValueChanged<T>(this IPreviewNotifierChild notifier, T oldValue, T newValue)
		{
			if (notifier.PreviewReceiver == null || EqualityComparer<T>.Default.Equals(oldValue, newValue))
			{
				return DisposableAction.NoAction;
			}
			else
			{
				return new DisposableAction(() =>
				{
					notifier.PreviewReceiver.UpdatePreview();
				});
			}
		}
	}
}

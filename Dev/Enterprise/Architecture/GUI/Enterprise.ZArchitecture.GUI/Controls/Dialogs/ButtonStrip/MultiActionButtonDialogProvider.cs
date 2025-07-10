using System;
using System.Linq;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public static class MultiActionButtonDialogProvider
	{
		public static T ShowDialog<T>(string message, string caption, params ButtonStripAction<T>[] buttonStripActions) where T : struct, IConvertible
		{
			return ShowDialog(message, caption, default(T), buttonStripActions);
		}

		public static T ShowDialog<T>(string message, string caption, T defaultValue, params ButtonStripAction<T>[] buttonStripActions) where T : struct, IConvertible
		{
			ZForm form = null;
			using (form = new MultiActionButtonDialogForm(caption, message, new MultiActionProvidingButtonStrip<T>(() => form.Close(), buttonStripActions)))
			{
				ZFormModaliser.ShowDialogAndDispose(form);

				if (form.Tag is T)
				{
					return (T)form.Tag;
				}
			}

			return defaultValue;
		}
	}

	public class MultiActionButtonDialogWrapper<T> : IMultiActionButtonDialogWrapper<T> where T : struct, IConvertible
	{
		public T ShowDialog(string message, string caption, params ButtonStripAction<T>[] buttonStripActions)
		{
			return ShowDialog(message, caption, default(T), buttonStripActions);
		}

		public T ShowDialog(string message, string caption, T defaultValue, params ButtonStripAction<T>[] buttonStripActions)
		{
			LogLastShow(message, caption, buttonStripActions);

#if DEBUG
			var action = buttonStripActions.FirstOrDefault(b => b.Response.Equals(ResponseToFireForTest));
			if (Globals.IsTest && action != null)
			{
				if (action.FireAction != null)
				{
					action.FireAction(null, EventArgs.Empty);
				}

				return action.Response;
			}
#endif

			return MultiActionButtonDialogProvider.ShowDialog(message, caption, defaultValue, buttonStripActions);
		}

#if DEBUG

		public T ResponseToFireForTest { get; set; }

#endif

		void LogLastShow(string message, string caption, params ButtonStripAction<T>[] buttonStripActions)
		{
			LastMessage = message;
			LastCaption = caption;
			ButtonStripActions = buttonStripActions;
		}

		public string LastMessage { get; private set; }
		public string LastCaption { get; private set; }
		public ButtonStripAction<T>[] ButtonStripActions { get; private set; }
	}
}

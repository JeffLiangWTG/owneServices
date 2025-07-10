using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Common;

#if WINZOR
using CargoWise.Windows.UI;
#endif

namespace CargoWise.Main.Navigation.ViewModels
{
#nullable disable
	public abstract class ViewModelWithNotification : INotifyPropertyChanged
	{
		#region Notification

		event PropertyChangedEventHandler propertyChanged;
		public event PropertyChangedEventHandler PropertyChanged
		{
			add
			{
				if (value != null)
				{
					if (propertyChanged == null || !propertyChanged.GetInvocationList().Select(i => i.Method).Contains(value.Method))
					{
						propertyChanged += value;
					}
				}
			}
			remove
			{
				propertyChanged -= value;
			}
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));
			if (propertyChanged != null)
			{
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		protected void BeginInvokeOnUIThread(Action action)
		{
#if DEBUG
			if (IsInUnitTesting)
			{
				action?.Invoke();
				return;
			}
#endif

#if WINZOR
			// We need to make sure that we have this code running on the main thread in the winzor environment,
			// Otherwise it will cause MultilingualText to not get the correct language text.
			var mainForm = ZApplication.GetOpenForms().FirstOrDefault();
			mainForm?.Invoke(action);
#else
			ApplicationDispatcher.Current.BeginInvoke(action);
#endif
		}

#if DEBUG
		/// <summary>
		/// For unit testing purposes only
		/// </summary>
		public static bool IsInUnitTesting { get; set; }

		protected bool IsInDesignMode
		{
			get
			{
#if WINZOR
				return false;
#else
				// Check if in design mode (this is a simple example, adapt as needed)
				return System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());
#endif
			}
		}
#endif
	}
}

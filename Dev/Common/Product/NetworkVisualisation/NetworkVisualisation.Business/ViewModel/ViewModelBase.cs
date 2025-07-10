using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MissingFrom.Net;

namespace CargoWise.NetworkVisualisation.Business
{
	public abstract class ViewModelBase : INotifyPropertyChanged
	{
		readonly WeakEventManager manager = new WeakEventManager();

		protected ViewModelBase()
		{
		}

		protected ViewModelBase(params INotifyPropertyChanged[] childNotifiers)
		{
			foreach (var child in childNotifiers.Where(c => c != null))
			{
				/*
				 * This bit of memory leak preventing magic comes from here:
				 * https://msdn.microsoft.com/en-us/library/aa970850%28v=vs.110%29.aspx
				 * 
				 * A third party library is being used to implement the weak event listener pattern as it is not part of .net core.
				 */
				manager.AddWeakEventListener(child, OnPropertyOfChildChanged);
			}
		}

		void OnPropertyOfChildChanged(object sender, PropertyChangedEventArgs e)
		{
			if (sender != this)
			{
				OnPropertyChanged(e.PropertyName);
			}
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			NotifyRelatedPropertiesChanged(propertyName);
		}

		void NotifyRelatedPropertiesChanged(string propertyName)
		{
			NotifyPropertyChanged(propertyName);

			foreach (var wrappingProperty in GetWrappingProperties(propertyName))
			{
				NotifyPropertyChanged(wrappingProperty);
			}
		}

		protected virtual void NotifyPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual IEnumerable<string> GetWrappingProperties(string propertyName)
		{
			yield break;
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}

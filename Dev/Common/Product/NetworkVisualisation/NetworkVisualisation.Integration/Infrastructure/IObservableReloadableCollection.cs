using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface IObservableReloadableCollection<T> :
			INotifyCollectionChanged,
			INotifyPropertyChanged,
			IList<T>,
			ICollection<T>,
			IEnumerable<T>,
			IEnumerable
	{
		void Reload();
		event EventHandler Reloaded;
	}
}

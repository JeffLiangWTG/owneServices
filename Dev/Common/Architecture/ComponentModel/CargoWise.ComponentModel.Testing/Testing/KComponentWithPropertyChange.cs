#if DEBUG
using System.ComponentModel;

namespace CargoWise.ComponentModel.Testing
{
	public class KComponentWithPropertyChange : KComponent, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void FirePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#endif

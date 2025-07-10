using System.ComponentModel;
using CargoWise.Main.Navigation.ViewModels;

#if WINZOR
using System.Runtime.CompilerServices;
#endif

namespace CargoWise.Main.Navigation;

public abstract class ViewModelBase :
#if !WINZOR
	ViewModelWithNotification,
#endif
	INotifyPropertyChanged
{
#if WINZOR
	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
#endif

	bool? isDesignMode { get; set; }
	protected bool IsDesignMode
	{
		get
		{
			if (isDesignMode.HasValue)
			{
				return isDesignMode.Value;
			}

#if !WINZOR
			isDesignMode = DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());
#endif
			return isDesignMode ?? false;
		}
	}
}

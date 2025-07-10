using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CargoWise.Main.Navigation;

#nullable disable
public class Snapshot : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	public Guid Id { get; set; }
	public SnapshotModuleFilter ModuleFilter { get; set; }

	int _order;
	public int Order
	{
		get => _order;
		set
		{
			if (_order != value)
			{
				_order = value;
				NotifyPropertyChanged();
			}
		}
	}

	string _value;
	public string Value
	{
		get => _value;
		set
		{
			if (_value != value)
			{
				_value = value;
				NotifyPropertyChanged();
			}
		}
	}

	public Guid Owner { get; set; }

	public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

	string _errorMessage;
	public string ErrorMessage
	{
		get => _errorMessage;
		set
		{
			if (_errorMessage != value)
			{
				_errorMessage = value;
				NotifyPropertyChanged();
			}
		}
	}

	void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

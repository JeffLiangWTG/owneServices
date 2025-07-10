using System;
using System.Windows.Input;

namespace CargoWise.Main.Navigation;

#nullable disable

public class RelayCommand : ICommand
{
	readonly Action<object> execute;
	readonly Func<object, bool> canExecute;

	public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
	{
		this.execute = execute;
		this.canExecute = canExecute;
	}

	public bool CanExecute(object parameter)
	{
		return canExecute == null || canExecute(parameter);
	}

	public void Execute(object parameter)
	{
		if (CanExecute(parameter))
		{
			execute(parameter);
		}
	}

	public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

	public event EventHandler CanExecuteChanged;
}

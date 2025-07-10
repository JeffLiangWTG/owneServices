using System;
using System.Windows.Input;

namespace CargoWise.Main.Navigation;

#nullable disable
public class MenuCommand : ICommand
{
	readonly Action _action;
	readonly Func<bool> _canExecute;

	public event EventHandler CanExecuteChanged;

	public MenuCommand(Action action, Func<bool> canExecute = null)
	{
		_action = action ?? throw new ArgumentNullException(nameof(action));
		_canExecute = canExecute;
	}

	public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
	public void Execute(object parameter)
	{
		if (CanExecute(parameter))
		{
			_action();
		}
	}

	public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

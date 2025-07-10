using System;
using System.Windows.Input;

namespace CargoWise.Main.Navigation;

#nullable disable
public class ClickCommand : ICommand
{
	public ClickCommand(Action executeMethod)
	{
		executeCommand = executeMethod;
	}

	public bool CanExecute(object parameter)
	{
		return executeCommand != null;
	}

	public void Execute(object parameter)
	{
		if (executeCommand != null)
		{
			executeCommand();
		}
	}

	public void OnCanExecuteChanged()
	{
		if (CanExecuteChanged != null)
		{
			CanExecuteChanged(this, EventArgs.Empty);
		}
	}

	public void Clear()
	{
		executeCommand = null;
	}

	Action executeCommand;
	public event EventHandler CanExecuteChanged;
}

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CargoWise.Main.Navigation;

#nullable disable
public class PopupMenu : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	string title;
	public string Title
	{
		get => title;
		set
		{
			if (title != value)
			{
				title = value;
				NotifyPropertyChanged();
			}
		}
	}

	bool selected;
	public bool IsSelected
	{
		get => selected;
		set
		{
			if (selected != value)
			{
				selected = value;
				NotifyPropertyChanged();
			}
		}
	}

	bool isChecked;
	public bool IsChecked
	{
		get => isChecked;
		set
		{
			if (isChecked != value)
			{
				isChecked = value;
				NotifyPropertyChanged();
			}
		}
	}

	string inputGestureText;
	public string InputGestureText
	{
		get => inputGestureText;
		set
		{
			if (inputGestureText != value)
			{
				inputGestureText = value;
				NotifyPropertyChanged();
			}
		}
	}

	public object ToolStripDropDown { get; set; }
	public object IconGeometry { get; set; }
	public Point DropDownOffset { get; set; }
	public ObservableCollection<PopupMenu> SubMenuItems { get; set; } = [];
	public ICommand Command { get; set; }
	public bool IsSeparator { get; set; }

	void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public PopupMenu FindPopupMenuByRelatedToolStripItem(object toolStripItem)
	{
		if (ToolStripDropDown != null && toolStripItem == ToolStripDropDown)
		{
			return this;
		}

		foreach (var menu in SubMenuItems)
		{
			var item = menu.FindPopupMenuByRelatedToolStripItem(toolStripItem);
			if (item != null)
			{
				return item;
			}
		}

		return null;
	}

	public PopupMenuEvent PopupMenuEvent { get; set; } = new PopupMenuEvent();
}

public class PopupMenuEvent
{
	public void RefreshPopupMenus()
	{
		OnSubMenusChanged?.Invoke(this, EventArgs.Empty);
	}

	public event EventHandler OnSubMenusChanged;
}
